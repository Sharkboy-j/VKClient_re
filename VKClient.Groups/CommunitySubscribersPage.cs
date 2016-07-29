using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

using VKClient.Audio.Base.Extensions;

namespace VKClient.Groups
{
  public partial class CommunitySubscribersPage : PageBase
  {
    private bool _isInitialized;
    private bool _isManagement;
    private bool _isPicker;
    private bool _isBlockingPicker;
    private long _communityId;

    public CommunitySubscribersViewModel ViewModel
    {
      get
      {
        return this.DataContext as CommunitySubscribersViewModel;
      }
    }

    public CommunitySubscribersPage()
    {
      this.InitializeComponent();
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.AllList);
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.UnsureList);
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.FriendsList);
      this.Header.OnHeaderTap = (Action) (() =>
      {
        if (this.Pivot.SelectedItem == this.PivotItemAll)
          this.AllList.ScrollToTop();
        if (this.Pivot.SelectedItem == this.PivotItemUnsure)
          this.UnsureList.ScrollToTop();
        if (this.Pivot.SelectedItem != this.PivotItemFriends)
          return;
        this.FriendsList.ScrollToTop();
      });
      this.AllList.OnRefresh = (Action) (() => this.ViewModel.All.LoadData(true, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false));
      this.UnsureList.OnRefresh = (Action) (() => this.ViewModel.Unsure.LoadData(true, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false));
      this.FriendsList.OnRefresh = (Action) (() => this.ViewModel.Friends.LoadData(true, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      GroupType toEnum = this.NavigationContext.QueryString["CommunityType"].ParseToEnum<GroupType>();
      this._communityId = long.Parse(this.NavigationContext.QueryString["CommunityId"]);
      this._isManagement = this.NavigationContext.QueryString["IsManagement"].ToLower() == "true";
      this._isPicker = this.NavigationContext.QueryString["IsPicker"].ToLower() == "true";
      this._isBlockingPicker = this.NavigationContext.QueryString["IsBlockingPicker"].ToLower() == "true";
      if (toEnum != GroupType.Event)
        this.Pivot.Items.Remove((object) this.PivotItemUnsure);
      CommunitySubscribersViewModel subscribersViewModel = new CommunitySubscribersViewModel(this._communityId, toEnum, this._isManagement);
      this.DataContext = (object) subscribersViewModel;
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton()
      {
        IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
        Text = CommonResources.FriendsPage_AppBar_Search
      };
      applicationBarIconButton.Click += new EventHandler(this.SearchButton_OnClicked);
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton);
      if (this._isPicker)
      {
        this.Header.HideSandwitchButton = true;
        this.SuppressMenu = true;
      }
      subscribersViewModel.All.LoadData(false, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.Pivot.SelectedItem == this.PivotItemUnsure)
        this.ViewModel.Unsure.LoadData(false, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false);
      if (this.Pivot.SelectedItem != this.PivotItemFriends)
        return;
      this.ViewModel.Friends.LoadData(false, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false);
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      if (sender == this.AllList)
        this.ViewModel.All.LoadMoreIfNeeded(e.ContentPresenter.Content);
      if (sender == this.UnsureList)
        this.ViewModel.Unsure.LoadMoreIfNeeded(e.ContentPresenter.Content);
      if (sender != this.FriendsList)
        return;
      this.ViewModel.Friends.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = (ExtendedLongListSelector) sender;
      LinkHeader item = longListSelector.SelectedItem as LinkHeader;
      if (item == null)
        return;
      longListSelector.SelectedItem = null;
      if (!this._isPicker)
        Navigator.Current.NavigateToUserProfile(item.Id, item.User.Name, "", false);
      else if (item.Id != AppGlobalStateManager.Current.LoggedInUserId && (this.ViewModel.Managers == null || this.ViewModel.Managers.All<User>((Func<User, bool>) (m => m.id != item.Id))))
      {
        if (!this._isBlockingPicker)
          Navigator.Current.NavigateToCommunityManagementManagerAdding(this.ViewModel.CommunityId, item.User, true);
        else
          Navigator.Current.NavigateToCommunityManagementBlockAdding(this.ViewModel.CommunityId, item.User, false);
      }
      else
        new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
    }

    private void AddToManagers_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader linkHeader = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (linkHeader == null)
        return;
      Navigator.Current.NavigateToCommunityManagementManagerAdding(this.ViewModel.CommunityId, linkHeader.User, false);
    }

    private void Edit_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader linkHeader = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (linkHeader == null)
        return;
      this.ViewModel.NavigateToManagerEditing(linkHeader);
    }

    private void RemoveFromCommunity_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader user = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (user == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.RemovingFromCommunity, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ViewModel.RemoveFromCommunity(user);
    }

    private void Block_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader linkHeader = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (linkHeader == null)
        return;
      Navigator.Current.NavigateToCommunityManagementBlockAdding(this._communityId, linkHeader.User, true);
    }

    private void SearchButton_OnClicked(object sender, EventArgs e)
    {
      if (this.ViewModel.Managers == null)
        return;
      DialogService dialogService = new DialogService();
      dialogService.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      int num = 0;
      dialogService.HideOnNavigation = num != 0;
      DataTemplate itemTemplate = (DataTemplate) this.Resources["ItemTemplate"];
      CommunitySubscribersSearchDataProvider searchDataProvider = new CommunitySubscribersSearchDataProvider(this._communityId, this.ViewModel.CommunityType, this.ViewModel.Managers, this._isManagement, this.Pivot.SelectedItem == this.PivotItemFriends);
      GenericSearchUC searchUC = new GenericSearchUC();
      searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 77.0, 0.0, 0.0);
      searchUC.Initialize<User, LinkHeader>((ISearchDataProvider<User, LinkHeader>) searchDataProvider, (Action<object, object>) ((p, f) => this.List_OnSelectionChanged(p, (SelectionChangedEventArgs) null)), itemTemplate);
      searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler) ((s, ev) => this.Pivot.Visibility = searchUC.SearchTextBox.Text != "" ? Visibility.Collapsed : Visibility.Visible);
      EventHandler eventHandler = (EventHandler) ((p, f) =>
      {
        this.Pivot.Visibility = Visibility.Visible;
        this.ViewModel.SearchViewModel = (GenericCollectionViewModel2<VKList<User>, LinkHeader>) null;
      });
      dialogService.Closed += eventHandler;
      GenericSearchUC genericSearchUc = searchUC;
      dialogService.Child = (FrameworkElement) genericSearchUc;
      Pivot pivot = this.Pivot;
      dialogService.Show((UIElement) pivot);
      this.InitializeAdornerControls();
      this.ViewModel.SearchViewModel = ((GenericSearchViewModel<User, LinkHeader>) searchUC.ViewModel).SearchVM;
    }
  }
}
