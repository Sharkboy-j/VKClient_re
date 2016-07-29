using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public partial class NewsPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBar _appBarNews;
    private ApplicationBarMenuItem _gcMenuItem;
    private ApplicationBarIconButton _appBarButtonRefreshNews;
    private ApplicationBarIconButton _appBarButtonAddNews;
    private ApplicationBarIconButton _appBarButtonAttachImage;
    private ApplicationBarIconButton _appBarButtonLists;
    private static double _scrollPosition;
    private bool _needToScrollToOffset;
    private bool _photoFeedMoveTutorial;
    private readonly HideHeaderHelper _hideHelper;
    private ObservableCollection<PickableNewsfeedSourceItemViewModel> _newsSources;
    private ListPickerUC2 _picker;
    private DialogService _photoFeedMoveTutorialDialog;
    private DialogService _newsfeedTopPromoDialog;

    private bool CanShowNewsfeedTopPromo
    {
      get
      {
        if (FramePageUtils.CurrentPage.GetType() == this.GetType() && MenuUC.Instance.Opacity <= 0.0 && !this.ImageViewerDecorator.IsShown)
          return this._photoFeedMoveTutorialDialog == null;
        return false;
      }
    }

    public NewsPage()
    {
        ApplicationBar expr_06 = new ApplicationBar();
        expr_06.BackgroundColor=VKConstants.AppBarBGColor;
        expr_06.ForegroundColor=VKConstants.AppBarFGColor;
        expr_06.Opacity=0.9;
        this._appBarNews = expr_06;
        ApplicationBarMenuItem expr_36 = new ApplicationBarMenuItem();
        expr_36.Text="garbage collect";
        this._gcMenuItem = expr_36;


      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      applicationBarIconButton1.Text = CommonResources.MainPage_News_Refresh;
      Uri uri1 = new Uri("/Resources/appbar.refresh.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      this._appBarButtonRefreshNews = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      applicationBarIconButton2.Text = CommonResources.MainPage_News_AddNews;
      Uri uri2 = new Uri("/Resources/AppBarNewPost-WXGA.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      this._appBarButtonAddNews = applicationBarIconButton2;
      this._appBarButtonAttachImage = new ApplicationBarIconButton()
      {
        IconUri = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative),
        Text = CommonResources.NewPost_AppBar_AddPhoto
      };
      this._appBarButtonLists = new ApplicationBarIconButton()
      {
        IconUri = new Uri("/Resources/lists.png", UriKind.Relative),
        Text = CommonResources.AppBar_Lists
      };
      this.InitializeComponent();
      this.panelNews.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollNews), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelNews);
      this.scrollNews.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.BuildAppBar();
      this.Loaded += new RoutedEventHandler(this.NewsPage_Loaded);
      this.Header.OnFreshNewsTap = new Action(this.OnFreshNewsTap);
      GenericHeaderUC genericHeaderUc = this.Header.ucHeader;
      Action action1 = (Action) (() => this.OnHeaderTap(true));
      genericHeaderUc.OnHeaderTap = action1;
      Action action2 = new Action(this.OpenNewsSourcePicker);
      genericHeaderUc.OnTitleTap = action2;
      genericHeaderUc.borderMenuOpenIcon.Visibility = Visibility.Visible;
      this._hideHelper = new HideHeaderHelper(this.Header, this.scrollNews, (PhoneApplicationPage) this);
      this.ucNewPost.DataContext = (object) new MenuViewModel();
    }

    private void BuildAppBar()
    {
      this._appBarButtonAttachImage.Click += new EventHandler(this._appBarButtonAttachImage_Click);
      this._appBarButtonAddNews.Click += new EventHandler(this._appBarMenuItemAddNews_Click);
      this._appBarButtonRefreshNews.Click += new EventHandler(this._appBarMenuItemRefreshNews_Click);
      this._appBarButtonLists.Click += new EventHandler(this._appBarButtonLists_Click);
    }

    private void _appBarButtonAttachImage_Click(object sender, EventArgs e)
    {
      ParametersRepository.SetParameterForId("GoPickImage", (object) true);
      Navigator.Current.NavigateToNewWallPost(0L, false, 0, false, false, false);
    }

    private void _appBarMenuItemAddNews_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToNewWallPost(0L, false, 0, false, false, false);
    }

    private void _appBarMenuItemRefreshNews_Click(object sender, EventArgs e)
    {
      NewsViewModel.Instance.NewsFeedVM.LoadData(true, false, (Action<BackendResult<NewsFeedData, ResultCode>>) null, false);
    }

    private void _appBarButtonLists_Click(object sender, EventArgs e)
    {
    }

    private void _gcMenuItem_Click(object sender, EventArgs e)
    {
      GC.Collect();
    }

    public static void Reset()
    {
      NewsPage._scrollPosition = 0.0;
    }

    private void NewsPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._needToScrollToOffset)
      {
        this.panelNews.ScrollTo(NewsPage._scrollPosition);
        this.panelNews.Opacity = 1.0;
        this._needToScrollToOffset = false;
      }
      if (!this._photoFeedMoveTutorial)
        return;
      this.ShowPhotoFeedMoveTutorial();
    }

    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
    {
      base.OnRemovedFromJournal(e);
      this.Header.ucHeader.CleanupBinding();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        bool flag = false;
        long newsSourceId = 0;
        if (this.NavigationContext.QueryString.ContainsKey("NewsSourceId"))
          newsSourceId = (long) int.Parse(this.NavigationContext.QueryString["NewsSourceId"]);
        if (this.NavigationContext.QueryString.ContainsKey("PhotoFeedMoveTutorial"))
          this._photoFeedMoveTutorial = this.NavigationContext.QueryString["PhotoFeedMoveTutorial"] == bool.TrueString;
        if (newsSourceId == 0L && NewsViewModel.Instance.ForceNewsFeedUpdate)
          newsSourceId = NewsSources.NewsFeed.PickableItem.ID;
        if (newsSourceId != 0L)
        {
          NewsViewModel instance = NewsViewModel.Instance;
          PickableNewsfeedSourceItemViewModel sourceItemViewModel = NewsSources.GetAllPredefinedNewsSources().FirstOrDefault<PickableNewsfeedSourceItemViewModel>((Func<PickableNewsfeedSourceItemViewModel, bool>) (item => item.PickableItem.ID == newsSourceId));
          PickableItem pickableItem = sourceItemViewModel != null ? sourceItemViewModel.PickableItem : (PickableItem) null;
          instance.NewsSource = pickableItem;
          flag = true;
        }
        this.DataContext = (object) NewsViewModel.Instance;
        NewsViewModel.Instance.EnsureUpToDate();
        NewsViewModel.Instance.FreshNewsStateChangedCallback = new Action<FreshNewsState>(this.FreshNewsStateChangedCallback);
        if (e.NavigationMode == NavigationMode.New && NewsPage._scrollPosition != 0.0 && !flag)
        {
          this._needToScrollToOffset = true;
          this.panelNews.Opacity = 0.0;
        }
        this.AskToast();
        this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelNews);
        this.panelNews.OnRefresh = (Action) (() => NewsViewModel.Instance.ReloadNews(false, true, false));
        this._isInitialized = true;
      }
      this.UpdateKeepScrollPosition();
      NewsViewModel.Instance.KeepScrollPositionChanged = new Action(this.UpdateKeepScrollPosition);
      NewsViewModel.Instance.ShowNewsfeedTopPromoAction = new Action<UserNotification>(this.ShowNewsfeedTopPromo);
      CurrentMediaSource.AudioSource = StatisticsActionSource.news;
      CurrentMediaSource.VideoSource = StatisticsActionSource.news;
      CurrentMediaSource.GifPlaySource = StatisticsActionSource.news;
      CurrentMarketItemSource.Source = MarketItemSource.feed;
      CurrentNewsFeedSource.Source = ViewPostSource.NewsFeed;
      CurrentCommunitySource.Source = CommunityOpenSource.Newsfeed;
      NewsViewModel.Instance.UpdateCurrentNewsFeedSource();
      this.ProcessInputParameters();
      this.HandleProtocolLaunchIfNeeded();
      this.CheckFreshNews();
    }

    private void UpdateKeepScrollPosition()
    {
      this.panelNews.KeepScrollPositionWhenAddingItems = NewsViewModel.Instance.KeepScrollPosition;
    }

    private void CheckFreshNews()
    {
      NewsViewModel.Instance.CheckForFreshNewsIfNeeded(this.scrollNews.Viewport.Y - 176.0 + 32.0);
    }

    private void FreshNewsStateChangedCallback(FreshNewsState state)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (state == FreshNewsState.ForcedReload)
        {
          NewsViewModel.Instance.ReplaceAllWithPendingFreshNews();
          this.OnHeaderTap(false);
          state = NewsViewModel.Instance.FreshNewsState;
        }
        this.Header.IsLoadingFreshNews = false;
        this._hideHelper.UpdateFreshNewsState(state);
        if (state == FreshNewsState.NoNews)
          return;
        this._hideHelper.ShowFreshNews();
      }));
    }

    private void HandleProtocolLaunchIfNeeded()
    {
      if (AppGlobalStateManager.Current.IsUserLoginRequired() || !(PageBase.ProtocolLaunchAfterLogin != null))
        return;
      this.NavigationService.Navigate(PageBase.ProtocolLaunchAfterLogin);
      PageBase.ProtocolLaunchAfterLogin = (Uri) null;
    }

    protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      base.HandleOnNavigatingFrom(e);
      NewsPage._scrollPosition = this.scrollNews.Viewport.Y;
      NewsViewModel.Instance.NavigatedFromNewsfeedTime = DateTime.Now;
    }

    private void AskToast()
    {
      if (!AppGlobalStateManager.Current.GlobalState.AllowToastNotificationsQuestionAsked)
      {
        if (MessageBox.Show(CommonResources.Toast_AllowQuestion, CommonResources.Settings_AllowToast, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
          AppGlobalStateManager.Current.GlobalState.PushSettings.EnableAll();
          AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled = true;
        }
        else
        {
          AppGlobalStateManager.Current.GlobalState.PushSettings.EnableAll();
          AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled = false;
        }
        AppGlobalStateManager.Current.GlobalState.AllowToastNotificationsQuestionAsked = true;
      }
      PushNotificationsManager.Instance.Initialize();
    }

    private void ProcessInputParameters()
    {
      Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
      if (group == null)
        return;
      foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>) NewsViewModel.Instance.NewsFeedVM.Collection)
      {
        WallPostItem wallPostItem = virtualizable as WallPostItem;
        if (wallPostItem == null && virtualizable is NewsFeedAdsItem)
          wallPostItem = (virtualizable as NewsFeedAdsItem).WallPostItem;
        if ((wallPostItem != null ? wallPostItem.LikesAndCommentsItem : (LikesAndCommentsItem) null) != null && wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(group.id, group.name))
          break;
        VideosNewsItem videosNewsItem = virtualizable as VideosNewsItem;
        if (videosNewsItem != null)
          videosNewsItem.LikesAndCommentsItem.ShareInGroupIfApplicable(group.id, group.name);
      }
    }

    private void OnHeaderTap(bool scrollAnimated = true)
    {
      if (scrollAnimated)
        this.panelNews.ScrollToBottom(false);
      else
        this.panelNews.ScrollTo(0.0);
      if (this._hideHelper == null)
        return;
      NewsViewModel instance = NewsViewModel.Instance;
      if (instance.FreshNewsState == FreshNewsState.Insert)
        instance.FreshNewsState = FreshNewsState.NoNews;
      this._hideHelper.Show(false);
    }

    private void OnFreshNewsTap()
    {
      if (this._hideHelper == null)
        return;
      NewsViewModel instance = NewsViewModel.Instance;
      switch (instance.FreshNewsState)
      {
        case FreshNewsState.Insert:
          this.OnHeaderTap(false);
          break;
        case FreshNewsState.Reload:
          if (instance.AreFreshNewsUpToDate && instance.HasFreshNewsToInsert)
          {
            NewsViewModel.Instance.ReplaceAllWithPendingFreshNews();
            this.OnHeaderTap(false);
            break;
          }
          this.Header.IsLoadingFreshNews = true;
          NewsViewModel.Instance.ReloadNews(false, false, true);
          break;
      }
    }

    private void OnHeaderFixedTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.OnHeaderTap(true);
    }

    private void OpenNewsSourcePicker()
    {
      this._newsSources = NewsViewModel.Instance.GetSectionsAndLists();
      if (this._newsSources == null)
        return;
      this.SelectNewsSourceItem(this._newsSources.FirstOrDefault<PickableNewsfeedSourceItemViewModel>((Func<PickableNewsfeedSourceItemViewModel, bool>) (item => item.PickableItem.ID == NewsViewModel.Instance.NewsSource.ID)));
      this._picker = new ListPickerUC2()
      {
        ItemsSource = (IList) this._newsSources,
        PickerMaxWidth = 408.0,
        PickerMaxHeight = 768.0,
        BackgroundColor = (Brush) Application.Current.Resources["PhoneCardOverlayBrush"],
        PickerMargin = new Thickness(0.0, 0.0, 0.0, 64.0),
        ItemTemplate = (DataTemplate) this.Resources["NewsSourceItemTemplate"]
      };
      this._picker.ItemTapped += (EventHandler<object>) ((sender, item) =>
      {
        PickableNewsfeedSourceItemViewModel newsSource = item as PickableNewsfeedSourceItemViewModel;
        if (newsSource == null)
          return;
        this.SelectNewsSourceItem(newsSource);
        NewsViewModel.Instance.NewsSource = newsSource.PickableItem;
      });
      bool flag1 = false;
      DialogService dialogService1 = this._newsfeedTopPromoDialog;
      if ((dialogService1 != null ? (dialogService1.IsOpen ? 1 : 0) : 0) != 0)
      {
        this._newsfeedTopPromoDialog.Hide();
        flag1 = true;
      }
      bool flag2 = false;
      DialogService dialogService2 = this._photoFeedMoveTutorialDialog;
      if ((dialogService2 != null ? (dialogService2.IsOpen ? 1 : 0) : 0) != 0)
      {
        this._photoFeedMoveTutorialDialog.Hide();
        flag2 = true;
      }
      PickableNewsfeedSourceItemViewModel feedNewsSource = this._newsSources.FirstOrDefault<PickableNewsfeedSourceItemViewModel>((Func<PickableNewsfeedSourceItemViewModel, bool>) (source => source == NewsSources.NewsFeed));
      if (flag1 && feedNewsSource != null)
      {
        feedNewsSource.FadeOutToggleEnabled = true;
        this._picker.Closed += (EventHandler) ((sender, args) => feedNewsSource.FadeOutToggleEnabled = false);
      }
      PickableNewsfeedSourceItemViewModel photosNewsSource = this._newsSources.FirstOrDefault<PickableNewsfeedSourceItemViewModel>((Func<PickableNewsfeedSourceItemViewModel, bool>) (source => source == NewsSources.Photos));
      if (flag2 && photosNewsSource != null)
      {
        photosNewsSource.FadeOutEnabled = true;
        this._picker.Closed += (EventHandler) ((sender, args) => photosNewsSource.FadeOutEnabled = false);
      }
      this._picker.Show(new Point(8.0, 32.0), (FrameworkElement) FramePageUtils.CurrentPage);
    }

    private void SelectNewsSourceItem(PickableNewsfeedSourceItemViewModel newsSource)
    {
      if (this._newsSources == null || newsSource == null)
        return;
      foreach (PickableNewsfeedSourceItemViewModel newsSource1 in (Collection<PickableNewsfeedSourceItemViewModel>) this._newsSources)
      {
        int num = newsSource1.PickableItem.ID == newsSource.PickableItem.ID ? 1 : 0;
        newsSource1.IsSelected = num != 0;
      }
    }

    private async void ToggleControl_OnTap(object sender, EventArgs e)
    {
      this._picker.DisableContent();
      this.SelectNewsSourceItem(NewsSources.NewsFeed);
      await Task.Delay(200);
      ListPickerUC2 listPickerUc2 = this._picker;
      if (listPickerUc2 == null)
        return;
      listPickerUc2.Hide();
    }

    private async void ShowPhotoFeedMoveTutorial()
    {
      if (AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown)
        return;
      AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown = true;
      PhotoFeedMoveTutorialUC childUC = new PhotoFeedMoveTutorialUC();
      GenericHeaderUC header = this.Header.ucHeader;
      childUC.SetCutArea(header.GetTitleMarginLeft(), header.GetTitleWidth());
      DialogService dialogService = new DialogService();
      dialogService.Child = (FrameworkElement) childUC;
      dialogService.BackgroundBrush = (Brush) null;
      int num1 = 0;
      dialogService.IsOverlayApplied = num1 != 0;
      int num2 = 5;
      dialogService.AnimationType = (DialogService.AnimationTypes) num2;
      int num3 = 6;
      dialogService.AnimationTypeChild = (DialogService.AnimationTypes) num3;
      int num4 = 0;
      dialogService.IsBackKeyOverride = num4 != 0;
      this._photoFeedMoveTutorialDialog = dialogService;
      this._photoFeedMoveTutorialDialog.Closing += (EventHandler) ((sender, args) => header.HideNewsfeedPromoOverlay());
      this._photoFeedMoveTutorialDialog.Show(null);
      header.ShowNewsfeedPromoOverlay();
      await Task.Delay(600);
      childUC.BackgroundTapCallback = (Action) (() =>
      {
        if (this._photoFeedMoveTutorialDialog == null || !this._photoFeedMoveTutorialDialog.IsOpen)
          return;
        this._photoFeedMoveTutorialDialog.Hide();
      });
    }

    private async void ShowNewsfeedTopPromo(UserNotification notification)
    {
      if (!this.CanShowNewsfeedTopPromo || notification == null || (notification.Type != UserNotificationType.bubble_newsfeed || notification.bubble_newsfeed == null))
        return;
      NewsfeedTopPromoUC newsfeedTopPromoUc = new NewsfeedTopPromoUC();
      NewsfeedTopPromoViewModel topPromoViewModel = new NewsfeedTopPromoViewModel(notification.bubble_newsfeed);
      newsfeedTopPromoUc.DataContext = (object) topPromoViewModel;
      NewsfeedTopPromoUC childUC = newsfeedTopPromoUc;
      GenericHeaderUC header = this.Header.ucHeader;
      childUC.SetCutArea(header.GetTitleMarginLeft(), header.GetTitleWidth());
      DialogService dialogService = new DialogService();
      dialogService.Child = (FrameworkElement) childUC;
      dialogService.BackgroundBrush = (Brush) null;
      int num1 = 0;
      dialogService.IsOverlayApplied = num1 != 0;
      int num2 = 5;
      dialogService.AnimationType = (DialogService.AnimationTypes) num2;
      int num3 = 6;
      dialogService.AnimationTypeChild = (DialogService.AnimationTypes) num3;
      int num4 = 0;
      dialogService.IsBackKeyOverride = num4 != 0;
      this._newsfeedTopPromoDialog = dialogService;
      childUC.ButtonPrimaryTapCallback = (Action) (() =>
      {
        NewsViewModel.Instance.TopFeedPromoAnswer = new bool?(true);
        this.OpenNewsSourcePicker();
      });
      childUC.ButtonSecondaryTapCallback = (Action) (() => this._newsfeedTopPromoDialog.Hide());
      this._newsfeedTopPromoDialog.Closing += (EventHandler) ((sender, args) => header.HideNewsfeedPromoOverlay());
      this.OnHeaderTap(false);
      this._newsfeedTopPromoDialog.Show(null);
      header.ShowNewsfeedPromoOverlay();
      NewsViewModel.Instance.TopFeedPromoAnswer = new bool?(false);
      NewsViewModel.Instance.TopFeedPromoId = notification.id;
      await Task.Delay(600);
      childUC.BackgroundTapCallback = (Action) (() =>
      {
        if (this._newsfeedTopPromoDialog == null || !this._newsfeedTopPromoDialog.IsOpen)
          return;
        this._newsfeedTopPromoDialog.Hide();
      });
    }
  }
}
