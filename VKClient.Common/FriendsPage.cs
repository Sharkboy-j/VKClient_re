using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class FriendsPage : PageBase
  {
    private readonly ApplicationBarIconButton _appBarButtonCreateList = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.FriendsPage_CreateList
    };
    private readonly ApplicationBarIconButton _appBarButtonAddToList = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.FriendsPage_AppBar_AddToList
    };
    private readonly ApplicationBarIconButton _appBarButtonSearch = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
      Text = CommonResources.FriendsPage_AppBar_Search
    };
    private readonly ApplicationBarIconButton _appBarButtonAdd = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.FriendsPage_AppBar_Add
    };
    private bool _isInitialized;
    private NewFriendsListUC _createFriendsListUC;
    private DialogService _dialogService;
    private FriendsPageMode _mode;
    private ApplicationBar _friendsListAppBar;
    private ApplicationBar _mainAppBar;
    private bool _mutualNavigationPerformed;
    private bool _loadedLists;
    private bool _loadedOnline;
    private bool _loadedCommon;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal ExtendedLongListSelector allFriendsListBox;
    internal PivotItem pivotItemOnline;
    internal ExtendedLongListSelector onlineFriendsListBox;
    internal PivotItem pivotItemLists;
    internal ExtendedLongListSelector friendListsListBox;
    internal PivotItem pivotItemMutualFriends;
    internal ExtendedLongListSelector mutualFriendsListBox;
    private bool _contentLoaded;

    private FriendsViewModel FriendsVM
    {
      get
      {
        return this.DataContext as FriendsViewModel;
      }
    }

    public FriendsPage()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.FriendsPage_Loaded);
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    private void BuildAppBar(bool isCurrentUser)
    {
      this._friendsListAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._mainAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._appBarButtonCreateList.Click += new EventHandler(this._appBarButtonCreateList_Click);
      this._appBarButtonAddToList.Click += new EventHandler(this._appBarButtonAddToList_Click);
      this._appBarButtonSearch.Click += new EventHandler(this._appBarButtonSearch_Click);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonSearch);
      if (isCurrentUser)
      {
        this._appBarButtonAdd.Click += new EventHandler(this._appBarButtonAdd_Click);
        this._mainAppBar.Buttons.Add((object) this._appBarButtonAdd);
      }
      this._friendsListAppBar.Buttons.Add((object) this._appBarButtonCreateList);
    }

    private void UpdateAppBar()
    {
      if (this.pivot.SelectedItem == this.pivotItemLists)
      {
        this.ApplicationBar = (IApplicationBar) null;
      }
      else
      {
        if (this.pivot.SelectedItem == this.pivotItemMutualFriends)
          return;
        if (this.FriendsVM.FriendsMode == FriendsViewModel.Mode.Lists)
          this._mainAppBar.Buttons.Contains((object) this._appBarButtonAddToList);
        this.ApplicationBar = (IApplicationBar) this._mainAppBar;
      }
    }

    private void _appBarButtonAddToList_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToFriends(AppGlobalStateManager.Current.LoggedInUserId, "", false, FriendsPageMode.PickAndBack);
    }

    private void _appBarButtonCreateList_Click(object sender, EventArgs e)
    {
      this._dialogService = new DialogService();
      this._dialogService.SetStatusBarBackground = true;
      this._createFriendsListUC = new NewFriendsListUC();
      this._createFriendsListUC.Initialize(true);
      this._dialogService.Child = (FrameworkElement) this._createFriendsListUC;
      this._createFriendsListUC.buttonCreate.Click += new RoutedEventHandler(this.buttonCreate_Click);
      this._dialogService.Show(null);
    }

    private void _appBarButtonSearch_Click(object sender, EventArgs e)
    {
      DialogService dialogService = new DialogService();
      dialogService.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      int num = 0;
      dialogService.HideOnNavigation = num != 0;
      this._dialogService = dialogService;
      UsersSearchDataProvider searchDataProvider = new UsersSearchDataProvider(this.FriendsVM.AllFriendsRaw.Select<User, FriendHeader>((Func<User, FriendHeader>) (f => new FriendHeader(f, false))), this._mode == FriendsPageMode.Default);
      DataTemplate itemTemplate = (DataTemplate) Application.Current.Resources["FriendItemTemplate"];
      GenericSearchUC searchUC = new GenericSearchUC();
      searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 77.0, 0.0, 0.0);
      searchUC.Initialize<User, FriendHeader>((ISearchDataProvider<User, FriendHeader>) searchDataProvider, new Action<object, object>(this.HandleSearchSelectionChanged), itemTemplate);
      searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler) ((s, ev) => this.pivot.Visibility = searchUC.SearchTextBox.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
      this._dialogService.Child = (FrameworkElement) searchUC;
      this._dialogService.Show((UIElement) this.pivot);
    }

    private void HandleSearchSelectionChanged(object listBox, object selectedItem)
    {
      FriendHeader friendHeader = selectedItem as FriendHeader;
      if (friendHeader == null)
        return;
      Navigator.Current.NavigateToUserProfile(friendHeader.UserId, friendHeader.User.Name, "", false);
    }

    private void _appBarButtonAdd_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToFriendsSuggestions();
    }

    private void buttonCreate_Click(object sender, RoutedEventArgs e)
    {
    }

    private void FriendsPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (!this.NavigationContext.QueryString.ContainsKey("Mutual") || !(this.NavigationContext.QueryString["Mutual"] == bool.TrueString) || this._mutualNavigationPerformed)
        return;
      this.pivot.SelectedItem = (object) this.pivotItemMutualFriends;
      this._mutualNavigationPerformed = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      bool isCurrentUser = false;
      FriendsViewModel vm;
      if (this.NavigationContext.QueryString.ContainsKey("ListId"))
      {
        vm = new FriendsViewModel(long.Parse(this.NavigationContext.QueryString["ListId"]), this.NavigationContext.QueryString["ListName"], true);
      }
      else
      {
        long userId = this.CommonParameters.UserId;
        string name = "";
        if (this.NavigationContext.QueryString.ContainsKey("Name"))
          name = this.NavigationContext.QueryString["Name"];
        vm = new FriendsViewModel(userId, name);
        isCurrentUser = userId == AppGlobalStateManager.Current.LoggedInUserId;
      }
      this.BuildAppBar(isCurrentUser);
      if (this.NavigationContext.QueryString.ContainsKey("Mode"))
        this._mode = (FriendsPageMode) Enum.Parse(typeof (FriendsPageMode), this.NavigationContext.QueryString["Mode"], true);
      this.DataContext = (object) vm;
      vm.LoadFriends();
      if (vm.FriendsMode == FriendsViewModel.Mode.Friends)
      {
        if (this._mode == FriendsPageMode.Default)
        {
          if (vm.OwnFriends)
            this.pivot.Items.Remove((object) this.pivotItemMutualFriends);
          else
            this.pivot.Items.Remove((object) this.pivotItemLists);
        }
        if (this._mode == FriendsPageMode.PickAndBack)
        {
          this.pivot.Items.Remove((object) this.pivotItemLists);
          this.pivot.Items.Remove((object) this.pivotItemMutualFriends);
        }
      }
      if (vm.FriendsMode == FriendsViewModel.Mode.Lists)
      {
        this.pivot.Items.Remove((object) this.pivotItemLists);
        this.pivot.Items.Remove((object) this.pivotItemMutualFriends);
      }
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.allFriendsListBox);
      this.allFriendsListBox.OnRefresh = (Action) (() => vm.RefreshFriends(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.onlineFriendsListBox);
      this.onlineFriendsListBox.OnRefresh = (Action) (() => vm.RefreshFriends(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.mutualFriendsListBox);
      this.mutualFriendsListBox.OnRefresh = (Action) (() => vm.RefreshFriends(false));
      this._isInitialized = true;
    }

    private void friendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selected = this.allFriendsListBox.SelectedItem as FriendHeader;
      if (selected == null)
        return;
      this.HandleUserSelection(selected);
      this.allFriendsListBox.SelectedItem = null;
    }

    private void onlineFriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selected = this.onlineFriendsListBox.SelectedItem as FriendHeader;
      if (selected == null)
        return;
      this.HandleUserSelection(selected);
      this.onlineFriendsListBox.SelectedItem = null;
    }

    private void mutualFriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selected = this.mutualFriendsListBox.SelectedItem as FriendHeader;
      if (selected == null)
        return;
      this.HandleUserSelection(selected);
      this.mutualFriendsListBox.SelectedItem = null;
    }

    private void HandleUserSelection(FriendHeader selected)
    {
      if (this._mode == FriendsPageMode.Default)
        Navigator.Current.NavigateToUserProfile(selected.UserId, selected.User.Name, "", false);
      if (this._mode != FriendsPageMode.PickAndBack)
        return;
      ParametersRepository.SetParameterForId("PickedUser", (object) selected);
      this.NavigationService.GoBackSafe();
    }

    private void friendListsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader friendHeader = this.friendListsListBox.SelectedItem as FriendHeader;
      if (friendHeader == null)
        return;
      FriendsList friendsList = friendHeader.FriendsList;
      if (friendsList == null)
        return;
      if (friendsList.lid == -1L)
        Navigator.Current.NavigateToBirthdaysPage();
      else
        Navigator.Current.NavigateToFriendsList(friendsList.lid, friendsList.name);
      this.friendListsListBox.SelectedItem = null;
    }

    private void pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void pivot_LoadedPivotItem_1(object sender, PivotItemEventArgs e)
    {
      if (e.Item == this.pivotItemOnline && !this._loadedOnline)
      {
        this._loadedOnline = true;
        this.FriendsVM.OnlineFriendsVM.LoadData(false, false, (Action<BackendResult<List<User>, ResultCode>>) null, false);
      }
      if (e.Item == this.pivotItemLists && !this._loadedLists)
      {
        this._loadedLists = true;
        this.FriendsVM.FriendListsVM.LoadData(false, false, (Action<BackendResult<List<FriendsList>, ResultCode>>) null, false);
      }
      if (e.Item != this.pivotItemMutualFriends || this._loadedCommon)
        return;
      this._loadedCommon = true;
      this.FriendsVM.CommonFriendsVM.LoadData(false, false, (Action<BackendResult<List<User>, ResultCode>>) null, false);
    }

    private void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemAll && this.FriendsVM.AllFriendsVM.Collection.Any<Group<FriendHeader>>())
        this.allFriendsListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemOnline && this.FriendsVM.OnlineFriendsVM.Collection.Any<FriendHeader>())
        this.onlineFriendsListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemMutualFriends && this.FriendsVM.CommonFriendsVM.Collection.Any<FriendHeader>())
      {
        this.mutualFriendsListBox.ScrollToTop();
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemLists || !this.FriendsVM.FriendListsVM.Collection.Any<FriendHeader>())
          return;
        this.friendListsListBox.ScrollToTop();
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/FriendsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemAll = (PivotItem) this.FindName("pivotItemAll");
      this.allFriendsListBox = (ExtendedLongListSelector) this.FindName("allFriendsListBox");
      this.pivotItemOnline = (PivotItem) this.FindName("pivotItemOnline");
      this.onlineFriendsListBox = (ExtendedLongListSelector) this.FindName("onlineFriendsListBox");
      this.pivotItemLists = (PivotItem) this.FindName("pivotItemLists");
      this.friendListsListBox = (ExtendedLongListSelector) this.FindName("friendListsListBox");
      this.pivotItemMutualFriends = (PivotItem) this.FindName("pivotItemMutualFriends");
      this.mutualFriendsListBox = (ExtendedLongListSelector) this.FindName("mutualFriendsListBox");
    }
  }
}
