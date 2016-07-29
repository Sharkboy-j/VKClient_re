using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class LikesPage : PageBase
  {
    private bool _isInitialized;
    private const int _countToLoad = 30;
    private const int _countToReload = 100;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal ExtendedLongListSelector listBoxAll;
    internal PivotItem pivotItemShared;
    internal ExtendedLongListSelector listBoxShared;
    internal PivotItem pivotItemFriends;
    internal ExtendedLongListSelector listBoxFriends;
    private bool _contentLoaded;

    private LikesViewModel LikesVM
    {
      get
      {
        return this.DataContext as LikesViewModel;
      }
    }

    public LikesPage()
    {
      this.InitializeComponent();
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      LikesViewModel vm = new LikesViewModel(long.Parse(this.NavigationContext.QueryString["OwnerId"]), long.Parse(this.NavigationContext.QueryString["ItemId"]), (LikeObjectType) int.Parse(this.NavigationContext.QueryString["Type"]), int.Parse(this.NavigationContext.QueryString["knownCount"]));
      this.DataContext = (object) vm;
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxAll);
      this.listBoxAll.OnRefresh = (Action) (() => vm.All.LoadData(true, false, (Action<BackendResult<LikesList, ResultCode>>) null, false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxShared);
      this.listBoxShared.OnRefresh = (Action) (() => vm.Shared.LoadData(true, false, (Action<BackendResult<LikesList, ResultCode>>) null, false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxFriends);
      this.listBoxFriends.OnRefresh = (Action) (() => vm.Friends.LoadData(true, false, (Action<BackendResult<LikesList, ResultCode>>) null, false));
      vm.All.LoadData(false, false, (Action<BackendResult<LikesList, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      FriendHeader friendHeader = longListSelector.SelectedItem as FriendHeader;
      if (friendHeader == null)
        return;
      if (friendHeader.IsGroupHeader)
        Navigator.Current.NavigateToGroup(friendHeader.GroupId, friendHeader.FullName, false);
      else
        Navigator.Current.NavigateToUserProfile(friendHeader.UserId, friendHeader.User.Name, "", false);
      longListSelector.SelectedItem = null;
    }

    private void All_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.LikesVM.All.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Shared_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.LikesVM.Shared.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Friends_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.LikesVM.Friends.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedItem == this.pivotItemFriends)
        this.LikesVM.Friends.LoadData(false, false, (Action<BackendResult<LikesList, ResultCode>>) null, false);
      if (this.pivot.SelectedItem != this.pivotItemShared)
        return;
      this.LikesVM.Shared.LoadData(false, false, (Action<BackendResult<LikesList, ResultCode>>) null, false);
    }

    private void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemAll && this.LikesVM.All.Collection.Any<FriendHeader>())
        this.listBoxAll.ScrollTo((object) this.LikesVM.All.Collection.First<FriendHeader>());
      else if (this.pivot.SelectedItem == this.pivotItemFriends && this.LikesVM.Friends.Collection.Any<FriendHeader>())
      {
        this.listBoxFriends.ScrollTo((object) this.LikesVM.Friends.Collection.First<FriendHeader>());
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemShared || !this.LikesVM.Shared.Collection.Any<FriendHeader>())
          return;
        this.listBoxShared.ScrollTo((object) this.LikesVM.Shared.Collection.First<FriendHeader>());
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/LikesPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemAll = (PivotItem) this.FindName("pivotItemAll");
      this.listBoxAll = (ExtendedLongListSelector) this.FindName("listBoxAll");
      this.pivotItemShared = (PivotItem) this.FindName("pivotItemShared");
      this.listBoxShared = (ExtendedLongListSelector) this.FindName("listBoxShared");
      this.pivotItemFriends = (PivotItem) this.FindName("pivotItemFriends");
      this.listBoxFriends = (ExtendedLongListSelector) this.FindName("listBoxFriends");
    }
  }
}
