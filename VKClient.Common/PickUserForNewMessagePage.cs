using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base;
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
  public class PickUserForNewMessagePage : PageBase
  {
    private readonly ApplicationBarIconButton _appBarButtonSearch = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
      Text = CommonResources.FriendsPage_AppBar_Search
    };
    private readonly ApplicationBarIconButton _appBarButtonEnableSelection = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.manage.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_SelectSeveral
    };
    private readonly ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/check.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Save
    };
    private readonly ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private bool _isInitialized;
    private DialogService _dialogService;
    private bool _groupViewOpened;
    private ApplicationBar _mainAppBar;
    private ApplicationBar _selectionAppBar;
    private bool _createChat;
    private long _initialUserId;
    private bool _goBackOnResult;
    private int _currentCountInChat;
    private bool _creatingChat;
    internal ProgressIndicator progressIndicator;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector allFriendsListBox;
    internal PivotItem pivotItemLists;
    internal ExtendedLongListSelector friendListsListBox;
    private bool _contentLoaded;

    private PickUserViewModel PickUserVM
    {
      get
      {
        return this.DataContext as PickUserViewModel;
      }
    }

    private bool ForbidGlobalSearch
    {
      get
      {
        if (this._currentCountInChat <= 0)
          return this._createChat;
        return true;
      }
    }

    public PickUserForNewMessagePage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.allFriendsListBox.JumpListOpening += (EventHandler) ((s, e) => this._groupViewOpened = true);
      this.allFriendsListBox.JumpListClosed += (EventHandler) ((s, e) => this._groupViewOpened = false);
      this.Header.HideSandwitchButton = true;
      this.SuppressMenu = true;
    }

    private void BuildAppBar()
    {
      this._mainAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._selectionAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonSearch);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonEnableSelection);
      this._selectionAppBar.Buttons.Add((object) this._appBarButtonCheck);
      this._selectionAppBar.Buttons.Add((object) this._appBarButtonCancel);
      this._appBarButtonSearch.Click += new EventHandler(this.AppBarButtonSearch_Click);
      this._appBarButtonEnableSelection.Click += new EventHandler(this.AppBarButtonEnableSelection_Click);
      this._appBarButtonCheck.Click += new EventHandler(this.AppBarButtonCheck_Click);
      this._appBarButtonCancel.Click += new EventHandler(this.AppBarButtonCancel_Click);
      this.ApplicationBar = (IApplicationBar) this._mainAppBar;
    }

    private void AppBarButtonEnableSelection_Click(object sender, EventArgs e)
    {
      this.PickUserVM.IsInSelectionMode = true;
      this.UpdateAppBar();
    }

    private void AppBarButtonCancel_Click(object sender, EventArgs e)
    {
      if (this.PickUserVM.PickUserMode == PickUserMode.PickForMessage)
      {
        this.PickUserVM.IsInSelectionMode = false;
        this.UpdateAppBar();
      }
      else
        Navigator.Current.GoBack();
    }

    private void AppBarButtonCheck_Click(object sender, EventArgs e)
    {
      List<FriendHeader> allSelected = this.PickUserVM.GetAllSelected();
      if (allSelected.Count <= 0)
        return;
      this.RespondToSelection(allSelected.Where<FriendHeader>((Func<FriendHeader, bool>) (fh => fh.User != null)).Select<FriendHeader, User>((Func<FriendHeader, User>) (fh => fh.User)).ToList<User>(), allSelected.Where<FriendHeader>((Func<FriendHeader, bool>) (fh => fh.FriendsList != null)).Select<FriendHeader, FriendsList>((Func<FriendHeader, FriendsList>) (fh => fh.FriendsList)).ToList<FriendsList>());
    }

    private void UpdateAppBar()
    {
      if (this.PickUserVM.IsInSelectionMode)
        this.ApplicationBar = (IApplicationBar) this._selectionAppBar;
      else
        this.ApplicationBar = (IApplicationBar) this._mainAppBar;
      if (this.PickUserVM.PickUserMode == PickUserMode.PickForPartner)
        this.ApplicationBar = (IApplicationBar) null;
      if (this.PickUserVM.PickUserMode == PickUserMode.PickForPrivacy)
        this._mainAppBar.Buttons.Remove((object) this._appBarButtonEnableSelection);
      this._appBarButtonCheck.IsEnabled = this.PickUserVM.SelectedCount > 0;
    }

    private void AppBarButtonSearch_Click(object sender, EventArgs e)
    {
      DialogService dialogService = new DialogService();
      dialogService.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      int num1 = 1;
      dialogService.HideOnNavigation = num1 != 0;
      int num2 = 6;
      dialogService.AnimationType = (DialogService.AnimationTypes) num2;
      this._dialogService = dialogService;
      UsersSearchDataProvider searchDataProvider = new UsersSearchDataProvider(this.PickUserVM.AllFriendsRaw.Select<User, FriendHeader>((Func<User, FriendHeader>) (f => new FriendHeader(f, false))), !this.ForbidGlobalSearch);
      DataTemplate itemTemplate = (DataTemplate) Application.Current.Resources["FriendItemTemplate"];
      GenericSearchUC searchUC = new GenericSearchUC();
      searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 77.0, 0.0, 0.0);
      searchUC.Initialize<User, FriendHeader>((ISearchDataProvider<User, FriendHeader>) searchDataProvider, new Action<object, object>(this.HandleSelectedItem), itemTemplate);
      searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler) ((s, ev) => this.pivot.Visibility = searchUC.SearchTextBox.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
      this._dialogService.Child = (FrameworkElement) searchUC;
      this._dialogService.Show((UIElement) this.pivot);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        bool flag;
        if (this.NavigationContext.QueryString.ContainsKey("GoBackOnResult"))
        {
          string str = this.NavigationContext.QueryString["GoBackOnResult"];
          flag = true;
          string @string = flag.ToString();
          if (str == @string)
          {
            this._goBackOnResult = true;
            goto label_7;
          }
        }
        if (this.NavigationContext.QueryString.ContainsKey("CreateChat"))
        {
          string str = this.NavigationContext.QueryString["CreateChat"];
          flag = true;
          string @string = flag.ToString();
          if (str == @string)
          {
            this._createChat = true;
            this._initialUserId = long.Parse(this.NavigationContext.QueryString["InitialUserId"]);
          }
        }
label_7:
        PickUserMode mode = (PickUserMode) Enum.Parse(typeof (PickUserMode), this.NavigationContext.QueryString["PickMode"]);
        this._currentCountInChat = int.Parse(this.NavigationContext.QueryString["CurrentCountInChat"]);
        int sexFilter = int.Parse(this.NavigationContext.QueryString["SexFilter"]);
        PickUserViewModel pickUserViewModel = new PickUserViewModel(mode, sexFilter);
        if (this.NavigationContext.QueryString.ContainsKey("CustomTitle"))
          pickUserViewModel.CustomTitle = this.NavigationContext.QueryString["CustomTitle"];
        if (mode == PickUserMode.PickForMessage || mode == PickUserMode.PickForPartner)
          this.pivot.Items.Remove((object) this.pivotItemLists);
        this.DataContext = (object) pickUserViewModel;
        pickUserViewModel.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
        pickUserViewModel.Friends.LoadData(false, false, (Action<BackendResult<List<User>, ResultCode>>) null, false);
        pickUserViewModel.Lists.LoadData(false, false, (Action<BackendResult<List<FriendsList>, ResultCode>>) null, false);
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedCount"))
        return;
      this.UpdateAppBar();
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (!this.PickUserVM.IsInSelectionMode || this._groupViewOpened || this.PickUserVM.PickUserMode != PickUserMode.PickForMessage)
        return;
      e.Cancel = true;
      this.PickUserVM.IsInSelectionMode = false;
      this.UpdateAppBar();
    }

    private void FriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      object selectedItem = longListSelector.SelectedItem;
      this.HandleSelectedItem((object) longListSelector, selectedItem);
      longListSelector.SelectedItem = null;
    }

    private void HandleSelectedItem(object listBox, object selectedItem)
    {
      FriendHeader friendHeader = selectedItem as FriendHeader;
      int maxAllowedCount;
      if (this._createChat)
      {
        maxAllowedCount = VKConstants.MaxChatCount;
        if (this._initialUserId != 0L)
          --maxAllowedCount;
      }
      else
        maxAllowedCount = VKConstants.MaxChatCount - this._currentCountInChat;
      if (friendHeader == null)
        return;
      if (!this.PickUserVM.IsInSelectionMode)
      {
        List<User> users = new List<User>();
        users.Add(friendHeader.User);
        this.RespondToSelection(users, null);
      }
      else
      {
        friendHeader.IsSelected = !friendHeader.IsSelected;
        if (this.PickUserVM.PickUserMode != PickUserMode.PickForMessage || this.PickUserVM.SelectedCount <= maxAllowedCount)
          return;
        this.ShowMessageBoxCannotAdd(maxAllowedCount);
        friendHeader.IsSelected = false;
      }
    }

    private void ShowMessageBoxCannotAdd(int maxAllowedCount)
    {
      int num = (int) MessageBox.Show(UIStringFormatterHelper.FormatNumberOfSomething(maxAllowedCount, CommonResources.YouCanSelectNoMoreOneFrm, CommonResources.YouCanSelectNoMoreTwoFourFrm, CommonResources.YouCanSelectNoMoreFiveFrm, true, null, false));
    }

    private void RespondToSelection(List<User> users, List<FriendsList> lists)
    {
      if (users.IsNullOrEmpty() && lists.IsNullOrEmpty())
        return;
      if (this._goBackOnResult)
      {
        ParametersRepository.SetParameterForId("SelectedUsers", (object) users);
        if (lists.NotNullAndHasAtLeastOneNonNullElement())
          ParametersRepository.SetParameterForId("SelectedLists", (object) lists);
        this.NavigationService.GoBackSafe();
      }
      else if (this._createChat || users.Count > 1)
        this.CreateChatAndProceed(users.Select<User, long>((Func<User, long>) (u => u.uid)).ToList<long>());
      else
        Navigator.Current.NavigateToConversation(users.First<User>().uid, false, true, "", 0L, false);
    }

    private void CreateChatAndProceed(List<long> uids)
    {
      long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
      if (this._creatingChat)
        return;
      this._creatingChat = true;
      this.ShowProgressIndicator(true, CommonResources.FriendsAndContactsSearchPage_CreatingChat);
      List<long> userIds = new List<long>()
      {
        loggedInUserId
      };
      if (this._initialUserId != 0L)
        userIds.Add(this._initialUserId);
      userIds.AddRange((IEnumerable<long>) uids);
      MessagesService.Instance.CreateChat(userIds, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (res =>
      {
        this._creatingChat = false;
        this.ShowProgressIndicator(false, "");
        if (res.ResultCode != ResultCode.Succeeded)
          ExtendedMessageBox.ShowSafe(CommonResources.FriendsAndContactsSearchPage_FailedToCreateChat);
        else
          Navigator.Current.NavigateToConversation(res.ResultData.response, true, true, "", 0L, false);
      }));
    }

    private void ShowProgressIndicator(bool show, string text = "")
    {
      if (this.Dispatcher.CheckAccess())
        this.DoShowProgressIndicator(show, text);
      else
        this.Dispatcher.BeginInvoke((Action) (() => this.DoShowProgressIndicator(show, text)));
    }

    private void DoShowProgressIndicator(bool show, string text)
    {
      this.progressIndicator.IsIndeterminate = show;
      this.progressIndicator.IsVisible = show;
      if (!show)
        return;
      this.progressIndicator.Text = text;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/PickUserForNewMessagePage.xaml", UriKind.Relative));
      this.progressIndicator = (ProgressIndicator) this.FindName("progressIndicator");
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemAll = (PivotItem) this.FindName("pivotItemAll");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.allFriendsListBox = (ExtendedLongListSelector) this.FindName("allFriendsListBox");
      this.pivotItemLists = (PivotItem) this.FindName("pivotItemLists");
      this.friendListsListBox = (ExtendedLongListSelector) this.FindName("friendListsListBox");
    }
  }
}
