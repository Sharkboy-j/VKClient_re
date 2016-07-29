using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public partial class ManagersPage : PageBase
  {
    private bool _isInitialized;
    private ManagersViewModel ViewModel
    {
      get
      {
        return this.DataContext as ManagersViewModel;
      }
    }

    public ManagersPage()
    {
      this.InitializeComponent();
      this.Header.OnHeaderTap += (Action) (() => this.List.ScrollToTop());
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.List);
      this.List.OnRefresh = (Action) (() => this.ViewModel.Managers.LoadData(true, false, (Action<BackendResult<CommunityManagers, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long communityId = long.Parse(this.NavigationContext.QueryString["CommunityId"]);
      GroupType communityType = (GroupType) int.Parse(this.NavigationContext.QueryString["CommunityType"]);
      ManagersViewModel managersViewModel = new ManagersViewModel(communityId);
      this.DataContext = (object) managersViewModel;
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton()
      {
        IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
        Text = CommonResources.AppBar_Add
      };
      applicationBarIconButton.Click += (EventHandler) ((p, f) => Navigator.Current.NavigateToCommunitySubscribers(this.ViewModel.CommunityId, communityType, false, true, false));
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton);
      managersViewModel.Managers.LoadData(true, false, (Action<BackendResult<CommunityManagers, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.Managers.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      LinkHeader linkHeader = this.List.SelectedItem as LinkHeader;
      if (linkHeader == null)
        return;
      this.List.SelectedItem = null;
      Navigator.Current.NavigateToUserProfile(linkHeader.Id, "", "", false);
    }

  }
}