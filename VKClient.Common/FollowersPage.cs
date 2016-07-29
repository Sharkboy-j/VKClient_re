using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FollowersPage : PageBase
  {
    private bool _isInitialized;
    private bool _subscriptions;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    private FollowersViewModel FollowersVM
    {
      get
      {
        return this.DataContext as FollowersViewModel;
      }
    }

    public FollowersPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBox);
      this.listBox.OnRefresh = (Action) (() =>
      {
        if (this._subscriptions)
          this.FollowersVM.SubscriptionsVM.LoadData(true, false, (Action<BackendResult<UsersAndGroups, ResultCode>>) null, false);
        else
          this.FollowersVM.FollowersVM.LoadData(true, false, (Action<BackendResult<List<User>, ResultCode>>) null, false);
      });
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      string name = "";
      if (this.NavigationContext.QueryString.ContainsKey("Name"))
        name = this.NavigationContext.QueryString["Name"];
      if (this.NavigationContext.QueryString.ContainsKey("Mode") && this.NavigationContext.QueryString["Mode"] == "Subscriptions")
      {
        this._subscriptions = true;
        this.listBox.SetBinding(FrameworkElement.DataContextProperty, new Binding("SubscriptionsVM"));
      }
      FollowersViewModel followersViewModel = new FollowersViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, name, this._subscriptions);
      this.DataContext = (object) followersViewModel;
      if (!this._subscriptions)
        followersViewModel.FollowersVM.LoadData(false, false, (Action<BackendResult<List<User>, ResultCode>>) null, false);
      else
        followersViewModel.SubscriptionsVM.LoadData(false, false, (Action<BackendResult<UsersAndGroups, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      if (!this._subscriptions)
        this.FollowersVM.FollowersVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
      else
        this.FollowersVM.SubscriptionsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      UserGroupHeader userGroupHeader = this.listBox.SelectedItem as UserGroupHeader;
      if (userGroupHeader == null)
        return;
      if (userGroupHeader.UserHeader != null)
        Navigator.Current.NavigateToUserProfile(userGroupHeader.UserHeader.UserId, userGroupHeader.UserHeader.User.Name, "", false);
      else if (userGroupHeader.GroupHeader != null)
        Navigator.Current.NavigateToGroup(userGroupHeader.GroupHeader.Group.id, userGroupHeader.GroupHeader.Group.name, false);
      this.listBox.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/FollowersPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
      this.listBox = (ExtendedLongListSelector) this.FindName("listBox");
    }
  }
}
