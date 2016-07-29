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
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
    public partial class GroupsListPage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonSearch = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
      Text = CommonResources.FriendsPage_AppBar_Search
    };
    private ApplicationBarIconButton _appBarButtonGlobe = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/New/globe.png", UriKind.Relative),
      Text = CommonResources.RecommendedGroups_Recommendations.ToLowerInvariant()
    };
    private ApplicationBarIconButton _appBarButtonCreate = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.NewFriendsList_Create.ToLowerInvariant()
    };
    private ApplicationBar _defaultAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private bool _isInitialized;
    private DialogService _dialogService;
    private bool _pickManaged;
    private long _ownerId;
    private long _picId;
    private string _text;
    private IShareContentDataProvider _shareContentDataProvider;
    private bool _isGif;
    private string _accessKey;

    private GroupsListViewModel GroupsListVM
    {
      get
      {
        return this.DataContext as GroupsListViewModel;
      }
    }

    public GroupsListPage()
    {
      this.InitializeComponent();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.communitiesListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.eventsListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.manageListBox);
      this.communitiesListBox.OnRefresh = (Action) (() => this.GroupsListVM.AllGroupsVM.LoadData(true, false, (Action<BackendResult<List<Group>, ResultCode>>) null, false));
      this.eventsListBox.OnRefresh = (Action) (() => this.GroupsListVM.EventsVM.LoadData(true, false, (Action<BackendResult<List<Group>, ResultCode>>) null, false));
      this.manageListBox.OnRefresh = (Action) (() => this.GroupsListVM.ManagedVM.LoadData(true, false, (Action<BackendResult<List<Group>, ResultCode>>) null, false));
      this.Header.OnHeaderTap = (Action) (() => this.OnHeaderTap());
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonSearch.Click += new EventHandler(this._appBarButtonSearch_Click);
      this._appBarButtonGlobe.Click += new EventHandler(this._appBarButtonGlobe_Click);
      this._appBarButtonCreate.Click += new EventHandler(GroupsListPage._appBarButtonCreate_Click);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonSearch);
    }

    private void _appBarButtonGlobe_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToGroupRecommendations(0, "");
    }

    private void UpdateAppBar()
    {
      if (this.IsMenuOpen)
        return;
      this.ApplicationBar = (IApplicationBar) this._defaultAppBar;
      this._defaultAppBar.Opacity = 0.9;
      if (this._pickManaged || this._shareContentDataProvider != null || this._ownerId != 0L && this._ownerId != AppGlobalStateManager.Current.LoggedInUserId || this._defaultAppBar.Buttons.Contains((object) this._appBarButtonGlobe))
        return;
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonGlobe);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonCreate);
    }

    private void _appBarButtonSearch_Click(object sender, EventArgs e)
    {
      DialogService dialogService = new DialogService();
      dialogService.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      int num1 = 0;
      dialogService.HideOnNavigation = num1 != 0;
      int num2 = 6;
      dialogService.AnimationType = (DialogService.AnimationTypes) num2;
      this._dialogService = dialogService;
      GroupsSearchDataProvider searchDataProvider = new GroupsSearchDataProvider((IEnumerable<GroupHeader>) this.GroupsListVM.AllGroupsVM.Collection);
      DataTemplate itemTemplate = (DataTemplate) Application.Current.Resources["VKGroupTemplate"];
      GenericSearchUC searchUC = new GenericSearchUC();
      searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 77.0, 0.0, 0.0);
      searchUC.Initialize<Group, GroupHeader>((ISearchDataProvider<Group, GroupHeader>) searchDataProvider, new Action<object, object>(this.HandleSelectedItem), itemTemplate);
      searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler) ((s, ev) => this.pivot.Visibility = searchUC.SearchTextBox.Text != "" ? Visibility.Collapsed : Visibility.Visible);
      this._dialogService.Child = (FrameworkElement) searchUC;
      this._dialogService.Show((UIElement) this.pivot);
      CommunityOpenSource lastCurrentCommunitySource = CurrentCommunitySource.Source;
      this._dialogService.Closed += (EventHandler) ((p, f) => CurrentCommunitySource.Source = lastCurrentCommunitySource);
      CurrentCommunitySource.Source = CommunityOpenSource.Search;
    }

    private static void _appBarButtonCreate_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToCommunityCreation();
    }

    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      this.HandleSelectedItem((object) longListSelector, longListSelector.SelectedItem);
      longListSelector.SelectedItem = null;
    }

    private void HandleSelectedItem(object listBox, object selectedItem)
    {
      GroupHeader groupHeader = selectedItem as GroupHeader;
      if (groupHeader == null)
        return;
      if (this._pickManaged)
      {
        if (this._ownerId != 0L && this._picId != 0L)
          this.Share(this._text ?? "", groupHeader.Group.id, groupHeader.Group.name ?? "");
        ParametersRepository.SetParameterForId("PickedGroupForRepost", (object) groupHeader.Group);
        EventAggregator.Current.Publish((object) new PhotoIsRepostedInGroup());
        this.NavigationService.GoBackSafe();
      }
      else if (this._shareContentDataProvider != null)
      {
        this._shareContentDataProvider.StoreDataToRepository();
        ShareContentDataProviderManager.StoreDataProvider(this._shareContentDataProvider);
        Navigator.Current.NavigateToNewWallPost(groupHeader.Group.id, true, groupHeader.Group.admin_level, groupHeader.Group.GroupType == GroupType.PublicPage, false, false);
      }
      else
        Navigator.Current.NavigateToGroup(groupHeader.Group.id, groupHeader.Group.name, false);
    }

    public void Share(string text, long gid = 0, string groupName = "")
    {
      if (!this._isGif)
      {
        WallService.Current.Repost(this._ownerId, this._picId, text, RepostObject.photo, gid, (Action<BackendResult<RepostResult, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Photo, gid, groupName);
          else
            new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
        }))));
      }
      else
      {
        string str = string.IsNullOrWhiteSpace(this._accessKey) ? string.Format("doc{0}_{1}", (object) this._ownerId, (object) this._picId) : string.Format("doc{0}_{1}_{2}", (object) this._ownerId, (object) this._picId, (object) this._accessKey);
        WallService current = WallService.Current;
        WallPostRequestData postData = new WallPostRequestData();
        postData.owner_id = -gid;
        postData.message = text;
        postData.AttachmentIds = new List<string>() { str };
        Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Doc, gid, groupName);
          else
            new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
        })));
        current.Post(postData, callback);
      }
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        string str = "";
        if (this.NavigationContext.QueryString.ContainsKey("Name"))
          str = this.NavigationContext.QueryString["Name"];
        this._pickManaged = this.NavigationContext.QueryString["PickManaged"] == bool.TrueString;
        this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
        this._ownerId = long.Parse(this.NavigationContext.QueryString["OwnerId"]);
        this._picId = long.Parse(this.NavigationContext.QueryString["PicId"]);
        this._isGif = bool.Parse(this.NavigationContext.QueryString["IsGif"]);
        this._accessKey = this.NavigationContext.QueryString["AccessKey"];
        if (this._pickManaged || this._shareContentDataProvider != null)
        {
          this.Header.HideSandwitchButton = true;
          this.SuppressMenu = true;
          this.pivot.Items.Remove((object) this.pivotItemAll);
          this.pivot.Items.Remove((object) this.pivotItemEvents);
        }
        if (this._shareContentDataProvider is ShareExternalContentDataProvider)
          this.NavigationService.ClearBackStack();
        long userId = this.CommonParameters.UserId;
        string userName = str;
        int num = this._pickManaged ? 1 : 0;
        GroupsListViewModel groupsListViewModel = new GroupsListViewModel(userId, userName, num != 0);
        this.DataContext = (object) groupsListViewModel;
        groupsListViewModel.LoadGroups(false, false);
        long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
        if (userId != loggedInUserId)
          this.pivot.Items.Remove((object) this.pivotItemManage);
        this._isInitialized = true;
      }
      if (this._dialogService == null || !this._dialogService.IsOpen)
        this.UpdateAppBar();
      this._text = ParametersRepository.GetParameterForIdAndReset("ShareText") as string;
    }

    private void communitiesListBox_Link_1(object sender, LinkUnlinkEventArgs e)
    {
    }

    private void eventsListBox_Link(object sender, LinkUnlinkEventArgs e)
    {
    }

    private void managedListBox_Link_1(object sender, LinkUnlinkEventArgs e)
    {
    }

    private void Canvas_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
    }

    private void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemAll && this.GroupsListVM.AllGroupsVM.Collection.Any<GroupHeader>())
        this.communitiesListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemEvents && this.GroupsListVM.EventsVM.Collection.Any<Group<GroupHeader>>())
      {
        this.eventsListBox.ScrollToTop();
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemManage || !this.GroupsListVM.ManagedVM.Collection.Any<GroupHeader>())
          return;
        this.manageListBox.ScrollToTop();
      }
    }
  }
}
