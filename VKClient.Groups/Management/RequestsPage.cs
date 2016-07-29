using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    public partial class RequestsPage : PageBase
  {
    private bool _isInitialized;

    private RequestsViewModel ViewModel
    {
      get
      {
        return this.DataContext as RequestsViewModel;
      }
    }

    public RequestsPage()
    {
      this.InitializeComponent();
      this.Header.OnHeaderTap += (Action) (() => this.List.ScrollToTop());
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.List);
      this.List.OnRefresh = (Action) (() => this.ViewModel.Requests.LoadData(true, false, (Action<BackendResult<VKList<User>, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      RequestsViewModel requestsViewModel = new RequestsViewModel(long.Parse(this.NavigationContext.QueryString["CommunityId"]));
      this.DataContext = (object) requestsViewModel;
      requestsViewModel.Requests.LoadData(true, false, (Action<BackendResult<VKList<User>, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.Requests.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = (ExtendedLongListSelector) sender;
      if (!(longListSelector.SelectedItem is FriendHeader))
        return;
      longListSelector.SelectedItem = null;
    }

    private void Request_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FriendHeader friendHeader = ((FrameworkElement) sender).DataContext as FriendHeader;
      if (friendHeader == null)
        return;
      Navigator.Current.NavigateToUserProfile(friendHeader.UserId, "", "", false);
    }

    private void Button_OnAcceptClicked(object sender, RoutedEventArgs e)
    {
      FriendHeader friendHeader = ((FrameworkElement) sender).DataContext as FriendHeader;
      if (friendHeader == null)
        return;
      this.ViewModel.HandleRequest(friendHeader, true);
    }

    private void Button_OnDeclineClicked(object sender, RoutedEventArgs e)
    {
      FriendHeader friendHeader = ((FrameworkElement) sender).DataContext as FriendHeader;
      if (friendHeader == null)
        return;
      this.ViewModel.HandleRequest(friendHeader, false);
    }

    private void Button_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    private void Separator_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

  }
}
