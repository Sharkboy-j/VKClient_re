using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public partial class InvitationsPage : PageBase
  {
    private bool _isInitialized;

    private InvitationsViewModel ViewModel
    {
      get
      {
        return this.DataContext as InvitationsViewModel;
      }
    }

    public InvitationsPage()
    {
      this.InitializeComponent();
      this.Header.OnHeaderTap += (Action) (() => this.List.ScrollToTop());
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.List);
      this.List.OnRefresh = (Action) (() => this.ViewModel.Invitations.LoadData(true, false, (Action<BackendResult<VKList<User>, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      InvitationsViewModel invitationsViewModel = new InvitationsViewModel(long.Parse(this.NavigationContext.QueryString["CommunityId"]));
      this.DataContext = (object) invitationsViewModel;
      invitationsViewModel.Invitations.LoadData(true, false, (Action<BackendResult<VKList<User>, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.Invitations.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = (ExtendedLongListSelector) sender;
      LinkHeader linkHeader = longListSelector.SelectedItem as LinkHeader;
      if (linkHeader == null)
        return;
      longListSelector.SelectedItem = null;
      Navigator.Current.NavigateToUserProfile(linkHeader.Id, "", "", false);
    }
  }
}
