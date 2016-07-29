using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;

namespace VKClient.Common.UC
{
  public class MenuUC : UserControl
  {
    private SolidColorBrush _textNotSelectedBrush = new SolidColorBrush(Colors.White);
    private DateTime _lastDateTimeMenuNavigation = DateTime.MinValue;
    public static MenuUC Instance;
    private bool _isNavigating;
    internal Grid LayoutRoot;
    internal ScrollViewer scrollViewer;
    internal TextBlock textBlockWatermarkText;
    internal Grid gridMenu;
    internal Border borderNews;
    internal TextBlock textBlockNews;
    internal Border borderFeedback;
    internal TextBlock textBlockFeedback;
    internal Border borderMessages;
    internal TextBlock textBlockMessages;
    internal Border borderFriends;
    internal TextBlock textBlockFriends;
    internal Border borderGroups;
    internal TextBlock textBlockGroups;
    internal Border borderPhotos;
    internal TextBlock textBlockPhotos;
    internal Border borderVideos;
    internal TextBlock textBlockVideos;
    internal Border borderAudios;
    internal TextBlock textBlockAudios;
    internal Border borderGames;
    internal TextBlock textBlockGames;
    internal Border borderFavorites;
    internal TextBlock textBlockFavorites;
    internal Border borderSettings;
    internal TextBlock textBlockSettings;
    internal BirthdaysUC ucBirthdays;
    internal MiniPlayerUC ucMiniPlayer;
    private bool _contentLoaded;

    public PageBase ParentPage
    {
      get
      {
        return (Application.Current.RootVisual as PhoneApplicationFrame).Content as PageBase;
      }
    }

    public bool IsOnNewsPage
    {
      get
      {
        return this.ParentPage is NewsPage;
      }
    }

    public bool IsOnFeedbackPage
    {
      get
      {
        return this.ParentPage is FeedbackPage;
      }
    }

    public bool IsOnSettingsPage
    {
      get
      {
        return this.CheckOnPage("SettingsNewPage");
      }
    }

    public bool IsOnMessagesPage
    {
      get
      {
        return this.CheckOnPage("ConversationsPage");
      }
    }

    public bool IsOnFriendsPage
    {
      get
      {
        return this.CheckOnPage("FriendsPage");
      }
    }

    public bool IsOnGroupsPage
    {
      get
      {
        return this.CheckOnPage("GroupsListPage");
      }
    }

    public bool IsOnPhotosPage
    {
      get
      {
        return this.CheckOnPage("PhotosMainPage");
      }
    }

    public bool IsOnVideosPage
    {
      get
      {
        return this.CheckOnPage("VideoCatalogPage");
      }
    }

    public bool IsOnAudiosPage
    {
      get
      {
        return this.CheckOnPage("AudioPage");
      }
    }

    public bool IsOnFavoritesPage
    {
      get
      {
        return this.ParentPage is FavoritesPage;
      }
    }

    public bool IsOnGamesPage
    {
      get
      {
        return this.ParentPage is GamesMainPage;
      }
    }

    public bool IsOnLoggedInUserPage
    {
      get
      {
        return this.CheckOnProfilePage(AppGlobalStateManager.Current.LoggedInUserId);
      }
    }

    public bool IsOnFriendRequestsPage
    {
      get
      {
        return this.ParentPage is FriendRequestsPage;
      }
    }

    public bool IsOnGroupInvitationsPage
    {
      get
      {
        return this.CheckOnPage("GroupInvitationsPage");
      }
    }

    public bool IsOnBirthdaysPage
    {
      get
      {
        return this.ParentPage is BirthdaysPage;
      }
    }

    public bool IsOnAudioPlayerPage
    {
      get
      {
        return this.CheckOnPage("AudioPlayer");
      }
    }

    public bool IsOnHelpPage
    {
      get
      {
        return this.ParentPage is HelpPage;
      }
    }

    private SolidColorBrush SelectedBrush
    {
      get
      {
        return Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"] as SolidColorBrush;
      }
    }

    private SolidColorBrush IconNotSelectedBrush
    {
      get
      {
        return Application.Current.Resources["PhoneSidebarIconBackgroundBrush"] as SolidColorBrush;
      }
    }

    private SolidColorBrush TextNotSelectedBrush
    {
      get
      {
        return this._textNotSelectedBrush;
      }
    }

    public MenuUC()
    {
      MenuUC.Instance = this;
      this.InitializeComponent();
      this.Visibility = Visibility.Collapsed;// UPDATE: 4.8.0
      this.CacheMode = (CacheMode) new BitmapCache();
      this.DataContext = (object) new MenuViewModel();
      this.UpdateState();
      this.ucMiniPlayer.stackPanelTrackTitle.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.stackPanelTrackTitle_Tap);
    }

    private bool CheckOnPage(string pageName)
    {
      PageBase parentPage = this.ParentPage;
      return parentPage != null && (parentPage.CommonParameters.UserOrGroupId == 0L || parentPage.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId) && (!parentPage.CommonParameters.IsGroup && (parentPage.CommonParameters.UserId == 0L || parentPage.CommonParameters.UserId == AppGlobalStateManager.Current.LoggedInUserId)) && this.ParentPage.GetType().Name.Contains(pageName);
    }

    private bool CheckOnProfilePage(long uid)
    {
      PageBase parentPage = this.ParentPage;
      return parentPage != null && (parentPage.CommonParameters.UserOrGroupId == uid || uid == AppGlobalStateManager.Current.LoggedInUserId && parentPage.CommonParameters.UserOrGroupId == 0L) && (this.ParentPage.GetType().Name.Contains("ProfilePage") && !this.ParentPage.GetType().Name.Contains("EditProfile"));
    }

    private void stackPanelTrackTitle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.NavigateToAudioPlayerPage();
    }

    public void UpdateState()
    {
      this.UpdateItem(this.IsOnNewsPage, this.textBlockNews, this.borderNews);
      this.UpdateItem(this.IsOnFeedbackPage, this.textBlockFeedback, this.borderFeedback);
      this.UpdateItem(this.IsOnMessagesPage, this.textBlockMessages, this.borderMessages);
      this.UpdateItem(this.IsOnFriendsPage, this.textBlockFriends, this.borderFriends);
      this.UpdateItem(this.IsOnGroupsPage, this.textBlockGroups, this.borderGroups);
      this.UpdateItem(this.IsOnPhotosPage, this.textBlockPhotos, this.borderPhotos);
      this.UpdateItem(this.IsOnVideosPage, this.textBlockVideos, this.borderVideos);
      this.UpdateItem(this.IsOnAudiosPage, this.textBlockAudios, this.borderAudios);
      this.UpdateItem(this.IsOnFavoritesPage, this.textBlockFavorites, this.borderFavorites);
      this.UpdateItem(this.IsOnSettingsPage, this.textBlockSettings, this.borderSettings);
      this.UpdateItem(this.IsOnGamesPage, this.textBlockGames, this.borderGames);
    }

    private void UpdateItem(bool isHighlighted, TextBlock textBlock, Border border)
    {
      if (isHighlighted)
      {
        textBlock.Foreground = (Brush) this.SelectedBrush;
        border.Background = (Brush) this.SelectedBrush;
      }
      else
      {
        textBlock.Foreground = (Brush) this.TextNotSelectedBrush;
        border.Background = (Brush) this.IconNotSelectedBrush;
      }
    }

    private void NavigateOnMenuClick(Action navigateAction, bool needClearStack = true)
    {
      if ((DateTime.Now - this._lastDateTimeMenuNavigation).TotalMilliseconds < 700.0 || this._isNavigating)
        return;
      this._isNavigating = true;
      this._lastDateTimeMenuNavigation = DateTime.Now;
      PageBase parentPage = this.ParentPage;
      parentPage.PrepareForMenuNavigation((Action) (() =>
      {
        this._isNavigating = false;
        if (needClearStack)
          WallPostVMCacheManager.ResetInstance();
        navigateAction();
        if (needClearStack)
          return;
        Execute.ExecuteOnUIThread((Action) (async () =>
        {
          await Task.Delay(1);
          this.HandleSamePageNavigation(parentPage, true);
        }));
      }), needClearStack);
    }

    public void NavigateToUserProfile(long uid, string userName, bool isHoldEvent)
    {
      if (this.CheckOnProfilePage(uid))
        this.HandleSamePageNavigation((PageBase) null, false);
      else
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToUserProfile(uid, userName, "", false)), !isHoldEvent);
    }

    public void NavigateToAudioPlayerPage()
    {
      if (this.IsOnAudioPlayerPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        PageBase parentPage = this.ParentPage;
        if (parentPage == null)
          return;
        if (parentPage.IsMenuOpen)
          parentPage.OpenCloseMenu(false, (Action) (() => Navigator.Current.NavigateToAudioPlayer(false)), false);
        else
          Navigator.Current.NavigateToAudioPlayer(false);
      }
    }

    private void Button_Status_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnLoggedInUserPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("self");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToUserProfile(AppGlobalStateManager.Current.LoggedInUserId, AppGlobalStateManager.Current.GlobalState.LoggedInUser.Name, "", false)), !flag);
      }
    }

    private void Button_Status_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Button_Status_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void NewsButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      bool flag1 = e == null;
      bool flag2 = this.IsOnNewsPage;
      if (!flag2 & flag1)
      {
        foreach (JournalEntry back in (Application.Current.RootVisual as PhoneApplicationFrame).BackStack)
        {
          if (back.Source.OriginalString.Contains("NewsPage.xaml"))
          {
            flag2 = true;
            break;
          }
        }
      }
      if (flag2)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        this.PublishMenuClickEvent("news");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToNewsFeed(0, false)), !flag1);
      }
    }

    private void NewsButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.NewsButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void PublishMenuClickEvent(string itemName)
    {
      EventAggregator.Current.Publish((object) new MenuClickEvent()
      {
        item = itemName
      });
    }

    private void Feedback_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnFeedbackPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("feedback");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToFeedback()), !flag);
      }
    }

    private void Feedback_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Feedback_Tap(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void Conversations_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      bool flag1 = e == null;
      bool flag2 = this.IsOnMessagesPage;
      if (!flag2 & flag1)
      {
        foreach (JournalEntry back in (Application.Current.RootVisual as PhoneApplicationFrame).BackStack)
        {
          if (back.Source.OriginalString.Contains("ConversationsPage.xaml"))
          {
            flag2 = true;
            break;
          }
        }
      }
      if (flag2)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        this.PublishMenuClickEvent("messages");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToConversations()), !flag1);
      }
    }

    private void Conversations_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Conversations_Tap(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void FriendsButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnFriendsPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("friends");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToFriends(AppGlobalStateManager.Current.LoggedInUserId, "", false, FriendsPageMode.Default)), !flag);
      }
    }

    private void FriendsButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.FriendsButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void GroupsButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnGroupsPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("groups");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", false, 0L, 0L, "", false, "")), !flag);
      }
    }

    private void GroupsButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GroupsButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void PhotosButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnPhotosPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("photos");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToPhotoAlbums(false, 0L, false, 0)), !flag);
      }
    }

    private void PhotosButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.PhotosButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void VideosButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnVideosPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("videos");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToVideoCatalog()), !flag);
      }
    }

    private void VideosButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VideosButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void AudioButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnAudiosPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("audios");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToAudio(0, 0L, false, 0L, 0L, "")), !flag);
      }
    }

    private void AudioButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.AudioButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void FavoritesButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnFavoritesPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("favorites");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToFavorites()), !flag);
      }
    }

    private void FavoritesButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.FavoritesButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void GamesButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnGamesPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("games");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToGames(0L, false)), !flag);
      }
    }

    private void GamesButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GamesButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void FriendRequests_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnFriendRequestsPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("friends_requests");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToFriendRequests(false)), !flag);
      }
    }

    private void FriendRequests_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.FriendRequests_Tap(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void GroupRequests_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnGroupInvitationsPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("group_requests");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToGroupInvitations()), !flag);
      }
    }

    private void GroupRequests_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GroupRequests_Tap(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void GamesRequests_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnGamesPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("games");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToGames(0L, false)), !flag);
      }
    }

    private void GamesRequests_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GamesRequests_Tap(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    private void HandleSamePageNavigation(PageBase parentPage = null, bool withoutAnimation = false)
    {
      if (parentPage == null)
        parentPage = this.ParentPage;
      if (parentPage == null)
        return;
      parentPage.OpenCloseMenu(false, null, withoutAnimation);
    }

    private void SettingsButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.IsOnSettingsPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        bool flag = e == null;
        this.PublishMenuClickEvent("settings");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToSettings()), !flag);
      }
    }

    private void SettingsButton_Hold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.SettingsButton_Click(sender, (System.Windows.Input.GestureEventArgs) null);
    }

    internal void NavigateToBirthdays(bool isHoldEvent)
    {
      if (this.IsOnBirthdaysPage)
      {
        this.HandleSamePageNavigation((PageBase) null, false);
      }
      else
      {
        this.PublishMenuClickEvent("birthdays");
        this.NavigateOnMenuClick((Action) (() => Navigator.Current.NavigateToBirthdaysPage()), !isHoldEvent);
      }
    }

    private void SearchPanel_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SearchHintsUC.ShowPopup();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/MenuUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.scrollViewer = (ScrollViewer) this.FindName("scrollViewer");
      this.textBlockWatermarkText = (TextBlock) this.FindName("textBlockWatermarkText");
      this.gridMenu = (Grid) this.FindName("gridMenu");
      this.borderNews = (Border) this.FindName("borderNews");
      this.textBlockNews = (TextBlock) this.FindName("textBlockNews");
      this.borderFeedback = (Border) this.FindName("borderFeedback");
      this.textBlockFeedback = (TextBlock) this.FindName("textBlockFeedback");
      this.borderMessages = (Border) this.FindName("borderMessages");
      this.textBlockMessages = (TextBlock) this.FindName("textBlockMessages");
      this.borderFriends = (Border) this.FindName("borderFriends");
      this.textBlockFriends = (TextBlock) this.FindName("textBlockFriends");
      this.borderGroups = (Border) this.FindName("borderGroups");
      this.textBlockGroups = (TextBlock) this.FindName("textBlockGroups");
      this.borderPhotos = (Border) this.FindName("borderPhotos");
      this.textBlockPhotos = (TextBlock) this.FindName("textBlockPhotos");
      this.borderVideos = (Border) this.FindName("borderVideos");
      this.textBlockVideos = (TextBlock) this.FindName("textBlockVideos");
      this.borderAudios = (Border) this.FindName("borderAudios");
      this.textBlockAudios = (TextBlock) this.FindName("textBlockAudios");
      this.borderGames = (Border) this.FindName("borderGames");
      this.textBlockGames = (TextBlock) this.FindName("textBlockGames");
      this.borderFavorites = (Border) this.FindName("borderFavorites");
      this.textBlockFavorites = (TextBlock) this.FindName("textBlockFavorites");
      this.borderSettings = (Border) this.FindName("borderSettings");
      this.textBlockSettings = (TextBlock) this.FindName("textBlockSettings");
      this.ucBirthdays = (BirthdaysUC) this.FindName("ucBirthdays");
      this.ucMiniPlayer = (MiniPlayerUC) this.FindName("ucMiniPlayer");
    }
  }
}
