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
using VKClient.Common.UC;
using VKClient.Groups.Library;
using VKClient.Groups.Localization;
using VKClient.Groups.UC;

namespace VKClient.Groups
{
    public partial class GroupInvitationsPage : PageBase
  {
    private bool _isInitialized;

    private GroupInvitationsViewModel GroupInvitationsVM
    {
      get
      {
        return this.DataContext as GroupInvitationsViewModel;
      }
    }

    public GroupInvitationsPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = GroupResources.GroupInvitationsPage_Title;
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxRequests.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxRequests);
      this.listBoxRequests.OnRefresh = (Action) (() => this.GroupInvitationsVM.InvitationsVM.LoadData(true, false, (Action<BackendResult<CommunityInvitationsList, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      GroupInvitationsViewModel invitationsViewModel = new GroupInvitationsViewModel();
      invitationsViewModel.ParentCommunityInvitationsUC = (CommunityInvitationsUC) ParametersRepository.GetParameterForIdAndReset("CommunityInvitationsUC");
      this.DataContext = (object) invitationsViewModel;
      invitationsViewModel.LoadInvitations();
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.GroupInvitationsVM.InvitationsVM.LoadMoreIfNeeded((object) (e.ContentPresenter.Content as GroupHeader));
    }
  }
}
