using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Utils;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Library.Posts;
using VKClient.Common.Profiles.Shared.Views;
using VKClient.Common.Stickers.Views;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using Windows.System;

namespace VKClient.Library
{
    public class NavigatorImpl : INavigator
    {
        private static readonly Regex _friendsReg = new Regex("/friends(\\?id=[0-9])?");
        private static readonly Regex _communitiesReg = new Regex("/groups(\\s|$)");
        private static readonly Regex _dialogsReg = new Regex("/(im|mail)(\\s|$)");
        private static readonly Regex _dialogReg = new Regex("/write[-0-9]+");
        private static readonly Regex _wallReg = new Regex("/wall[-0-9]+_[0-9]+");
        private static readonly Regex _feedWallReg = new Regex("/feed?w=wall[-0-9]+_[0-9]+");
        private static readonly Regex _audiosReg = new Regex("/audios[-0-9]+");
        private static readonly Regex _newsReg = new Regex("/feed(\\s|$)");
        private static readonly Regex _feedbackReg = new Regex("/feed?section=notifications");
        private static readonly Regex _profileReg = new Regex("/(id|wall)[0-9]+");
        private static readonly Regex _communityReg = new Regex("/(club|event|public|wall)[-0-9]+");
        private static readonly Regex _photosReg = new Regex("/(photos|albums)[-0-9]+");
        private static readonly Regex _photoReg = new Regex("/photo[-0-9]+_[0-9]+");
        private static readonly Regex _albumReg = new Regex("/album[-0-9]+_[0-9]+");
        private static readonly Regex _tagReg = new Regex("/tag[0-9]+");
        private static readonly Regex _videosReg = new Regex("/videos[-0-9]+");
        private static readonly Regex _videoReg = new Regex("/video[-0-9]+_[0-9]+");
        private static readonly Regex _boardReg = new Regex("/board[0-9]+");
        private static readonly Regex _topicReg = new Regex("/topic[-0-9]+_[0-9]+");
        private static readonly Regex _stickersSettingsReg = new Regex("/stickers/settings(\\s|$)");
        private static readonly Regex _settingsReg = new Regex("/settings(\\s|$)");
        private static readonly Regex _stickersReg = new Regex("/stickers(\\s|$)");
        private static readonly Regex _stickersPackReg = new Regex("/stickers([\\/A-Za-z0-9]+)");
        private static readonly Regex _faveReg = new Regex("/fave(\\s|$)");
        private static readonly Regex _appsReg = new Regex("/apps(\\s|$)");
        private static readonly Regex _marketAlbumReg = new Regex("/market[-0-9]+\\?section=album_[-0-9]+");
        private static readonly Regex _marketReg = new Regex("/market[-0-9]+");
        private static readonly Regex _productReg = new Regex("/product[-0-9]+_[0-9]+");
        private static readonly Regex _namedObjReg = new Regex("/[A-Za-z0-9\\\\._-]+");
        private readonly List<NavigatorImpl.NavigationTypeMatch> _navTypesList = new List<NavigatorImpl.NavigationTypeMatch>()
    {
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._friendsReg, NavigatorImpl.NavType.friends),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._communitiesReg, NavigatorImpl.NavType.communities),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._dialogsReg, NavigatorImpl.NavType.dialogs),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._dialogReg, NavigatorImpl.NavType.dialog),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._wallReg, NavigatorImpl.NavType.wallPost),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._feedWallReg, NavigatorImpl.NavType.wallPost),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._audiosReg, NavigatorImpl.NavType.audios),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._newsReg, NavigatorImpl.NavType.news),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._feedbackReg, NavigatorImpl.NavType.feedback),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._profileReg, NavigatorImpl.NavType.profile),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._communityReg, NavigatorImpl.NavType.community),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._photosReg, NavigatorImpl.NavType.albums),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._photoReg, NavigatorImpl.NavType.photo),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._albumReg, NavigatorImpl.NavType.album),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._tagReg, NavigatorImpl.NavType.tagPhoto),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videosReg, NavigatorImpl.NavType.videos),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videoReg, NavigatorImpl.NavType.video),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._boardReg, NavigatorImpl.NavType.board),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._topicReg, NavigatorImpl.NavType.topic),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersSettingsReg, NavigatorImpl.NavType.stickersSettings),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._settingsReg, NavigatorImpl.NavType.settings),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._faveReg, NavigatorImpl.NavType.fave),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._appsReg, NavigatorImpl.NavType.apps),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._marketAlbumReg, NavigatorImpl.NavType.marketAlbum),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._marketReg, NavigatorImpl.NavType.market),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._productReg, NavigatorImpl.NavType.product),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersReg, NavigatorImpl.NavType.stickers),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersPackReg, NavigatorImpl.NavType.stickersPack),
      new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._namedObjReg, NavigatorImpl.NavType.namedObject)
    };
        private List<string> _history = new List<string>();
        private const string VK_ME_DOMAIN = "vk.me/";
        private bool _isResolvingScreenName;

        private static Frame NavigationService
        {
            get
            {
                return (Frame)Application.Current.RootVisual;
            }
        }

        public List<string> History
        {
            get
            {
                return this._history.Skip<string>(Math.Max(0, this._history.Count<string>() - 10)).Take<string>(10).ToList<string>();
            }
        }

        public void GoBack()
        {
            Logger.Instance.Info("Navigator.GoBack");
            this._history.Add("Back");
            if (!NavigatorImpl.NavigationService.CanGoBack)
            {
                FramePageUtils.CurrentPage.SwitchNavigationEffects();
                ParametersRepository.SetParameterForId("SwitchNavigationEffects", (object)true);
                Navigator.Current.NavigateToMainPage();
            }
            else
                NavigatorImpl.NavigationService.GoBack();
        }

        public void NavigateToWebUri(string uri, bool forceWebNavigation = false, bool fromPush = false)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return;
            if (!uri.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !uri.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                uri = "http://" + uri;
            Logger.Instance.Info("Navigator.NavigateToWebUri, uri={0}, forceWebNavigation={1}", (object)uri, (object)forceWebNavigation);
            bool flag = false;
            if (!forceWebNavigation)
                flag = this.GetWithinAppNavigationUri(uri, fromPush);
            if (flag)
                return;
            uri = this.PrepareWebUri(uri);
            new WebBrowserTask()
            {
                Uri = new Uri(uri, UriKind.Absolute)
            }.Show();
        }

        private string PrepareWebUri(string uri)
        {
            if (!NavigatorImpl.IsVKUri(uri))
                return "http://m.vk.com/away.php?to=" + WebUtility.UrlEncode(uri);
            return uri;
        }

        private bool GetWithinAppNavigationUri(string uri, bool fromPush = false)
        {
            if (!NavigatorImpl.IsVKUri(uri))
                return false;
            string uri1 = uri;
            int num = uri1.IndexOf("://", StringComparison.InvariantCulture);
            if (num > -1)
                uri1 = uri1.Remove(0, num + 3);
            int count = uri1.IndexOf("/", StringComparison.InvariantCulture);
            if (count > -1)
                uri1 = uri1.Remove(0, count);
            if (uri1.StartsWith("dev/") || uri1.StartsWith("dev") && uri1.Length == 3)
                return false;
            Dictionary<string, string> queryString = HttpUtils.ParseQueryString(uri);
            if (uri1.StartsWith("/feed") && queryString.ContainsKey("section") && queryString["section"] == "search")
            {
                this.NavigateToNewsSearch(HttpUtility.UrlDecode(queryString.ContainsKey("q") ? queryString["q"] : ""));
                return true;
            }
            long id1;
            string id1String;
            long id2;
            string id2String;
            string objName;
            string objSub;
            NavigatorImpl.NavType navigationType = this.GetNavigationType(uri1, out id1, out id1String, out id2, out id2String, out objName, out objSub);
            if (navigationType == NavigatorImpl.NavType.none)
                return false;
            if (id1 == 0L)
                id1 = AppGlobalStateManager.Current.LoggedInUserId;
            bool flag = true;
            switch (navigationType)
            {
                case NavigatorImpl.NavType.friends:
                    this.NavigateToFriends(id1, "", false, FriendsPageMode.Default);
                    break;
                case NavigatorImpl.NavType.communities:
                    this.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", false, 0L, 0L, "", false, "");
                    break;
                case NavigatorImpl.NavType.dialogs:
                    this.NavigateToConversations();
                    break;
                case NavigatorImpl.NavType.news:
                    this.NavigateToNewsFeed(0, false);
                    break;
                case NavigatorImpl.NavType.tagPhoto:
                    this.NavigateToPhotoAlbum(Math.Abs(id1), id1 < 0L, "2", "0", "", 0, "", "", false, 0);
                    break;
                case NavigatorImpl.NavType.albums:
                    this.NavigateToPhotoAlbums(false, Math.Abs(id1), id1 < 0L, 0);
                    break;
                case NavigatorImpl.NavType.profile:
                    this.NavigateToUserProfile(id1, "", "", false);
                    break;
                case NavigatorImpl.NavType.dialog:
                    this.NavigateToConversation(id1, false, false, "", 0L, false);
                    break;
                case NavigatorImpl.NavType.community:
                    this.NavigateToGroup(id1, "", false);
                    break;
                case NavigatorImpl.NavType.board:
                    this.NavigateToGroupDiscussions(id1, "", 0, false, false);
                    break;
                case NavigatorImpl.NavType.album:
                    long albumIdLong = AlbumTypeHelper.GetAlbumIdLong(id2String);
                    AlbumType albumType = AlbumTypeHelper.GetAlbumType(albumIdLong);
                    this.NavigateToPhotoAlbum(Math.Abs(id1), id1 < 0L, albumType.ToString(), albumIdLong.ToString(), "", 0, "", "", false, 0);
                    break;
                case NavigatorImpl.NavType.video:
                    this.NavigateToVideoWithComments((VKClient.Common.Backend.DataObjects.Video)null, id1, id2, "");
                    break;
                case NavigatorImpl.NavType.audios:
                    this.NavigateToAudio(0, Math.Abs(id1), id1 < 0L, 0L, 0L, "");
                    break;
                case NavigatorImpl.NavType.topic:
                    flag = false;
                    break;
                case NavigatorImpl.NavType.photo:
                    this.NavigateToPhotoWithComments(null, (PhotoWithFullInfo)null, id1, id2, "", false, false);
                    break;
                case NavigatorImpl.NavType.wallPost:
                    this.NavigateToWallPostComments(id2, id1, false, 0L, 0L, "");
                    break;
                case NavigatorImpl.NavType.namedObject:
                    this.ResolveScreenNameNavigationObject(uri, objName, fromPush);
                    break;
                case NavigatorImpl.NavType.stickersSettings:
                    this.NavigateToStickersManage();
                    break;
                case NavigatorImpl.NavType.settings:
                    this.NavigateToSettings();
                    break;
                case NavigatorImpl.NavType.feedback:
                    this.NavigateToFeedback();
                    break;
                case NavigatorImpl.NavType.videos:
                    this.NavigateToVideo(false, Math.Abs(id1), id1 < 0L, false);
                    break;
                case NavigatorImpl.NavType.fave:
                    this.NavigateToFavorites();
                    break;
                case NavigatorImpl.NavType.apps:
                    if (AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled)
                    {
                        this.NavigateToGames(0L, fromPush);
                        break;
                    }
                    flag = false;
                    break;
                case NavigatorImpl.NavType.marketAlbum:
                    this.NavigateToMarketAlbumProducts(id1, id2, null);
                    break;
                case NavigatorImpl.NavType.market:
                    this.NavigateToMarket(id1);
                    break;
                case NavigatorImpl.NavType.product:
                    this.NavigateToProduct(id1, id2);
                    break;
                case NavigatorImpl.NavType.stickers:
                    this.NavigateToStickersStore();
                    break;
                case NavigatorImpl.NavType.stickersPack:
                    NavigatorImpl.ShowStickersPack(objSub);
                    break;
            }
            return flag;
        }

        private static bool IsVKUri(string uri)
        {
            uri = uri.ToLowerInvariant();
            uri = uri.Replace("http://", "").Replace("https://", "");
            if (uri.StartsWith("m.") || uri.StartsWith("t.") || uri.StartsWith("0."))
                uri = uri.Remove(0, 2);
            if (uri.StartsWith("www.") || uri.StartsWith("new."))
                uri = uri.Remove(0, 4);
            if (!uri.StartsWith("vk.com/") && !uri.StartsWith("vkontakte.ru/"))
                return uri.StartsWith("vk.me/");
            return true;
        }

        private NavigatorImpl.NavType GetNavigationType(string uri, out long id1, out string id1String, out long id2, out string id2String, out string obj, out string objSub)
        {
            id1 = id2 = 0L;
            id1String = id2String = "";
            obj = objSub = "";
            foreach (NavigatorImpl.NavigationTypeMatch navTypes1 in this._navTypesList)
            {
                if (navTypes1.Check(uri))
                {
                    if (navTypes1.SubTypes.Count > 0)
                    {
                        foreach (string subType in navTypes1.SubTypes)
                        {
                            foreach (NavigatorImpl.NavigationTypeMatch navTypes2 in this._navTypesList)
                            {
                                if (navTypes2.Check(subType))
                                {
                                    id1 = navTypes2.Id1;
                                    id2 = navTypes2.Id2;
                                    id1String = navTypes2.Id1String;
                                    id2String = navTypes2.Id2String;
                                    obj = navTypes2.ObjName;
                                    objSub = navTypes2.ObjSubName;
                                    return navTypes2.MatchType;
                                }
                            }
                        }
                    }
                    id1 = navTypes1.Id1;
                    id2 = navTypes1.Id2;
                    id1String = navTypes1.Id1String;
                    id2String = navTypes1.Id2String;
                    obj = navTypes1.ObjName;
                    objSub = navTypes1.ObjSubName;
                    return navTypes1.MatchType;
                }
            }
            return NavigatorImpl.NavType.none;
        }

        private static void ShowStickersPack(string stickersPackName)
        {
            stickersPackName = stickersPackName.Replace("/", "");
            if (string.IsNullOrWhiteSpace(stickersPackName))
                return;
            CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.message;
            StickersPackView.Show(stickersPackName, "link");
        }

        private void ResolveScreenNameNavigationObject(string uri, string objName, bool fromPush)
        {
            if (this._isResolvingScreenName)
                return;
            this._isResolvingScreenName = true;
            AccountService.Instance.ResolveScreenName(objName.Replace("/", ""), (Action<BackendResult<ResolvedData, ResultCode>>)(res =>
            {
                this._isResolvingScreenName = false;
                if (res.ResultCode == ResultCode.Succeeded)
                    Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        ResolvedData resultData = res.ResultData;
                        if ((resultData != null ? resultData.resolvedObject : (ResolvedObject)null) != null)
                        {
                            ResolvedObject resolvedObject = res.ResultData.resolvedObject;
                            bool flag = false;
                            int num = uri.IndexOf("://", StringComparison.InvariantCulture);
                            if (num > -1)
                            {
                                string str = uri.Remove(0, num + "://".Length);
                                if (!string.IsNullOrEmpty(str) && str.StartsWith("vk.me/"))
                                    flag = true;
                            }
                            if (resolvedObject.type == "user")
                            {
                                if (flag)
                                    this.NavigateToConversation(resolvedObject.object_id, false, false, "", 0L, false);
                                else
                                    this.NavigateToUserProfile(resolvedObject.object_id, "", "", false);
                            }
                            else if (resolvedObject.type == "group")
                            {
                                if (flag)
                                    this.NavigateToConversation(-resolvedObject.object_id, false, false, "", 0L, false);
                                else
                                    this.NavigateToGroup(resolvedObject.object_id, "", false);
                            }
                            else if (resolvedObject.type == "application" && res.ResultData.app != null && AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled)
                            {
                                if (this.TryOpenGame(new List<Game>()
                {
                  res.ResultData.app
                }, fromPush))
                                    return;
                                this.NavigateToWebUri(uri, true, false);
                            }
                            else
                                this.NavigateToWebUri(uri, true, false);
                        }
                        else
                            this.NavigateToWebUri(uri, true, false);
                    }));
                else
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", (VKRequestsDispatcher.Error)null);
            }));
        }

        private bool TryOpenGame(List<Game> games, bool fromPush)
        {
            bool flag = false;
            if (games.Count > 0)
            {
                Game game = games[0];
                if (!string.IsNullOrEmpty(game.platform_id) && game.is_in_catalog == 1)
                {
                    flag = true;
                    Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        PageBase currentPage = FramePageUtils.CurrentPage;
                        if (currentPage == null || currentPage is OpenUrlPage)
                        {
                            this.NavigateToGames(game.id, false);
                        }
                        else
                        {
                            Grid grid = currentPage.Content as Grid;
                            FrameworkElement frameworkElement = null;
                            if ((grid != null ? grid.Children : (UIElementCollection)null) != null && grid.Children.Count > 0)
                                frameworkElement = grid.Children[0] as FrameworkElement;
                            PageBase page = currentPage;
                            List<object> games1 = new List<object>();
                            games1.Add((object)game);
                            int num = fromPush ? 4 : 3;
                            string requestName = "";
                            int selectedIndex = 0;
                            FrameworkElement root = frameworkElement;
                            page.OpenGamesPopup(games1, (GamesClickSource)num, requestName, selectedIndex, root);
                        }
                    }));
                }
            }
            return flag;
        }

        private static string GetNavigateToUserProfileString(long uid, string userName = "", string source = "")
        {
            if (((App)Application.Current).RootFrame.Content is ProfilePage && (((App)Application.Current).RootFrame.Content as ProfilePage).ViewModel.Id == Math.Abs(uid))
                return null;
            if (uid < 0L)
                return null;
            return NavigatorImpl.GetNavigateToUserProfileNavStr(uid, userName, false, source);
        }

        public bool NavigateToUserProfile(long uid, string userName = "", string source = "", bool needClearStack = false)
        {
            string navStr = NavigatorImpl.GetNavigateToUserProfileString(uid, userName, source);
            if (navStr == null)
                return false;
            if (needClearStack)
                navStr = (!navStr.Contains("?") ? navStr + "?" : navStr + "&") + "ClearBackStack=true";
            this.Navigate(navStr);
            return true;
        }

        public static string GetNavigateToUserProfileNavStr(long uid, string userName = "", bool forbidOverrideGoBack = false, string source = "")
        {
            if (userName == null)
                userName = "";
            return string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}&Name={1}&ForbidOverrideGoBack={2}&Source={3}", (object)uid, (object)Extensions.ForURL(userName), (object)forbidOverrideGoBack, (object)source);
        }

        private static string GetNavigateTrGroupString(long groupId, string name = "")
        {
            if (((App)Application.Current).RootFrame.Content is ProfilePage && (((App)Application.Current).RootFrame.Content as ProfilePage).ViewModel.Id == -Math.Abs(groupId))
                return null;
            return NavigatorImpl.GetNavigateToGroupNavStr(groupId, name, false);
        }

        public bool NavigateToGroup(long groupId, string name = "", bool needClearStack = false)
        {
            string navStr = NavigatorImpl.GetNavigateTrGroupString(groupId, name);
            if (navStr == null)
                return false;
            if (needClearStack)
                navStr = (!navStr.Contains("?") ? navStr + "?" : navStr + "&") + "ClearBackStack=true";
            this.Navigate(navStr);
            return true;
        }

        public static string GetNavigateToGroupNavStr(long groupId, string name = "", bool forbidOverrideGoBack = false)
        {
            groupId = -Math.Abs(groupId);
            return string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}&Name={1}&ForbidOverrideGoBack={2}&Source={3}", (object)groupId, (object)Extensions.ForURL(name), (object)forbidOverrideGoBack, (object)CurrentCommunitySource.ToString(CurrentCommunitySource.Source));
        }

        public void NavigateToPostsSearch(long ownerId, string nameGen = "")
        {
            this.Navigate(string.Format("/VKClient.Common;component/Profiles/Shared/Views/PostsSearchPage.xaml?OwnerId={0}&NameGen={1}", ownerId, (object)nameGen));
        }

        private static string GetNavigateToPhotoAlbumsString(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            string str = string.Format("/VKClient.Photos;component/PhotosMainPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&AdminLevel={3}", (object)pickMode, (object)userOrGroupId, isGroup, (object)adminLevel);
            if (pickMode)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToPhotoAlbums(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0)
        {
            this.Navigate(NavigatorImpl.GetNavigateToPhotoAlbumsString(pickMode, userOrGroupId, isGroup, adminLevel));
        }

        public void NavigateToNewWallPost(long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0, bool isPublicPage = false, bool isNewTopicMode = false, bool isPostponed = false)
        {
            this.Navigate(NavigatorImpl.GetNavToNewPostStr(userOrGroupId, isGroup, adminLevel, isPublicPage, isNewTopicMode, isPostponed));
        }

        public static string GetNavToNewPostStr(long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0, bool isPublicPage = false, bool isNewTopicMode = false, bool isPostponed = false)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            WallPostViewModel.Mode mode1 = WallPostViewModel.Mode.NewWallPost;
            if (isNewTopicMode)
                mode1 = WallPostViewModel.Mode.NewTopic;
            WallPostViewModel.Mode mode2 = WallPostViewModel.Mode.EditDiscussionComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditDiscussionComment;
            mode2 = WallPostViewModel.Mode.EditPhotoComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditPhotoComment;
            mode2 = WallPostViewModel.Mode.EditVideoComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditVideoComment;
            mode2 = WallPostViewModel.Mode.EditProductComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditProductComment;
            mode2 = WallPostViewModel.Mode.EditWallComment;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditWallComment;
            mode2 = WallPostViewModel.Mode.EditWallPost;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.EditWallPost;
            mode2 = WallPostViewModel.Mode.PublishWallPost;
            if (ParametersRepository.Contains(mode2.ToString()))
                mode1 = WallPostViewModel.Mode.PublishWallPost;
            bool flag = FramePageUtils.CurrentPage is PostCommentsPage;
            return string.Format("/VKClient.Common;component/NewPost.xaml?UserOrGroupId={0}&IsGroup={1}&AdminLevel={2}&IsPublicPage={3}&IsNewTopicMode={4}&Mode={5}&FromWallPostPage={6}&IsPostponed={7}&IsPopupNavigation=True", (object)userOrGroupId, isGroup, (object)adminLevel, (object)isPublicPage, (object)isNewTopicMode, (object)mode1, (object)flag, (object)isPostponed);
        }

        public static string GetShareExternalContentpageNavStr()
        {
            return "/VKClient.Common;component/ShareExternalContentPage.xaml";
        }

        private static string GetNavigateToVideoString(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, bool forceAllowVideoUpload = false)
        {
            if (!pickMode && isGroup)
                return string.Format("/VKClient.Video;component/VideoCatalog/GroupVideosPage.xaml?OwnerId={0}", (object)-userOrGroupId);
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            if (!pickMode && userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId)
                return NavigatorImpl.GetNavigateToVideoCatalogString();
            string str = string.Format("/VKClient.Video;component/VideoPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&ForceAllowVideoUpload={3}", (object)pickMode.ToString(), (object)(userOrGroupId == 0L ? AppGlobalStateManager.Current.LoggedInUserId : userOrGroupId), isGroup, (object)forceAllowVideoUpload);
            if (pickMode)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToVideo(bool pickMode = false, long userOrGroupId = 0, bool isGroup = false, bool forceAllowVideoUpload = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToVideoString(pickMode, userOrGroupId, isGroup, forceAllowVideoUpload));
        }

        public void NavigateToVideoAlbum(long albumId, string albumName, bool pickMode = false, long userOrGroupId = 0, bool isGroup = false)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            string navStr = string.Format("/VKClient.Video;component/VideoPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&AlbumId={3}&AlbumName={4}", pickMode.ToString(), userOrGroupId, isGroup, albumId.ToString(), Extensions.ForURL(albumName));
            if (pickMode)
                navStr += "&IsPopupNavigation=True";
            this.Navigate(navStr);
        }

        private static string GetNavigateToAudioString(int pickMode = 0, long userOrGroupId = 0, bool isGroup = false, long albumId = 0, long excludeAlbumId = 0, string albumName = "")
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            string str = string.Format("/VKClient.Audio;component/AudioPage.xaml?PageMode={0}&UserOrGroupId={1}&IsGroup={2}&AlbumId={3}&ExcludeAlbumId={4}&AlbumName={5}", pickMode, userOrGroupId, isGroup, albumId, (object)excludeAlbumId, (object)Extensions.ForURL(albumName));
            if (pickMode != 0)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToAudio(int pickMode = 0, long userOrGroupId = 0, bool isGroup = false, long albumId = 0, long excludeAlbumId = 0, string albumName = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToAudioString(pickMode, userOrGroupId, isGroup, albumId, excludeAlbumId, albumName));
        }

        public void NavigateToDocuments(long ownerId = 0, bool isOwnerCommunityAdmined = false)
        {
            this.Navigate(string.Format("/VKClient.Common;component/DocumentsPage.xaml?OwnerId={0}&IsOwnerCommunityAdmined={1}", ownerId, (object)isOwnerCommunityAdmined));
        }

        public void NavigateToPostponedPosts(long groupId = 0)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PostponedPostsPage.xaml"));
        }

        public void NavigateToWallPostComments(long postId, long ownerId, bool focusCommentsField, long pollId = 0, long pollOwnerId = 0, string adData = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToPostCommentsNavStr(postId, ownerId, focusCommentsField, pollId, pollOwnerId, adData));
        }

        public static string GetNavigateToPostCommentsNavStr(long postId, long ownerId, bool focusCommentsField, long pollId = 0, long pollOwnerId = 0, string adData = "")
        {
            return string.Format("/VKClient.Common;component/PostCommentsPage.xaml?PostId={0}&OwnerId={1}&FocusComments={2}&PollId={3}&PollOwnerId={4}&AdData={5}", (object)postId, ownerId, (object)focusCommentsField, (object)pollId, (object)pollOwnerId, (object)adData);
        }

        public void NavigateToFriendsList(long lid, string listName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FriendsPage.xaml?ListId={0}&ListName={1}", (object)lid.ToString(), (object)Extensions.ForURL(listName)));
        }

        public void NavigateToFollowers(long userOrGroupId, bool isGroup, string name)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FollowersPage.xaml?UserOrGroupId={0}&IsGroup={1}&Name={2}", (object)userOrGroupId, isGroup, (object)Extensions.ForURL(name)));
        }

        public void NavigateToSubscriptions(long userId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Profiles/Users/Views/SubscriptionsPage.xaml?UserId={0}", (object)userId));
        }

        private static string GetNavigateToFriendsString(long userId, string name, bool mutual, FriendsPageMode mode = FriendsPageMode.Default)
        {
            return string.Format("/VKClient.Common;component/FriendsPage.xaml?UserId={0}&Name={1}&Mutual={2}&Mode={3}", (object)userId, (object)Extensions.ForURL(name), (object)mutual.ToString(), (object)mode.ToString());
        }

        public void NavigateToFriends(long userId, string name, bool mutual, FriendsPageMode mode = FriendsPageMode.Default)
        {
            this.Navigate(NavigatorImpl.GetNavigateToFriendsString(userId, name, mutual, mode));
        }

        public void NavigateToFriendRequests(bool areSuggestedFriends)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FriendRequestsPage.xaml?AreSuggestedFriends={0}", (object)areSuggestedFriends));
        }

        public static string GetNavigateToSettingsStr()
        {
            return "/VKClient.Common;component/SettingsNewPage.xaml";
        }

        public void NavigateToSettings()
        {
            this.Navigate(NavigatorImpl.GetNavigateToSettingsStr());
        }

        public void NavigateToImageViewer(string aid, int albumType, long userOrGroupId, bool isGroup, int photosCount, int selectedPhotoIndex, List<Photo> photos, Func<int, Image> getImageByIdFunc)
        {
            ImageViewerDecoratorUC.ShowPhotosFromAlbum(aid, albumType, userOrGroupId, isGroup, photosCount, selectedPhotoIndex, photos, getImageByIdFunc);
        }

        public void NavigateToImageViewerPhotosOrGifs(int selectedIndex, List<PhotoOrDocument> photosOrDocuments, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false, FrameworkElement currentViewControl = null, Action<int> setContextOnCurrentViewControl = null, Action<int, bool> showHideOverlay = null, bool shareButtonOnly = false)
        {
            ImageViewerDecoratorUC.ShowPhotosOrGifsById(selectedIndex, photosOrDocuments, fromDialog, friendsOnly, getImageByIdFunc, page, hideActions, currentViewControl, setContextOnCurrentViewControl, showHideOverlay, shareButtonOnly);
        }

        public void NavigateToImageViewer(int photosCount, int initialOffset, int selectedPhotoIndex, List<long> photoIds, List<long> ownerIds, List<string> accessKeys, List<Photo> photos, string viewerMode, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false)
        {
            ImageViewerDecoratorUC.ShowPhotosById(photosCount, initialOffset, selectedPhotoIndex, photoIds, ownerIds, accessKeys, photos, fromDialog, friendsOnly, getImageByIdFunc, page, hideActions);
        }

        public void NaviateToImageViewerPhotoFeed(long userOrGroupId, bool isGroup, string aid, int photosCount, int selectedPhotoIndex, int date, List<Photo> photos, string mode, Func<int, Image> getImageByIdFunc)
        {
            ImageViewerDecoratorUC.ShowPhotosFromFeed(userOrGroupId, isGroup, aid, photosCount, selectedPhotoIndex, date, photos, mode, getImageByIdFunc);
        }

        private static string GetNavigateToPhotoWithComments(Photo photo, PhotoWithFullInfo photoWithFullInfo, long ownerId, long pid, string accessKey, bool fromDialog = false, bool friendsOnly = false)
        {
            ParametersRepository.SetParameterForId("Photo", (object)photo);
            ParametersRepository.SetParameterForId("PhotoWithFullInfo", (object)photoWithFullInfo);
            return string.Format("/VKClient.Photos;component/PhotoCommentsPage.xaml?ownerId={0}&pid={1}&accessKey={2}&FromDialog={3}&FriendsOnly={4}", ownerId, (object)pid, (object)Extensions.ForURL(accessKey), (object)fromDialog, (object)friendsOnly);
        }

        public void NavigateToPhotoWithComments(Photo photo, PhotoWithFullInfo photoWithFullInfo, long ownerId, long pid, string accessKey, bool fromDialog = false, bool friendsOnly = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToPhotoWithComments(photo, photoWithFullInfo, ownerId, pid, accessKey, fromDialog, friendsOnly));
        }

        public void NavigateToLikesPage(long ownerId, long itemId, int type, int knownCount = 0)
        {
            this.Navigate(string.Format("/VKClient.Common;component/LikesPage.xaml?OwnerId={0}&ItemId={1}&Type={2}&knownCount={3}", ownerId, (object)itemId, (object)type, (object)knownCount));
        }

        public void PickAlbumToMovePhotos(long userOrGroupId, bool isGroup, string excludeAid, List<long> list, int adminLevel = 0)
        {
            userOrGroupId = userOrGroupId != 0L ? userOrGroupId : AppGlobalStateManager.Current.LoggedInUserId;
            this.Navigate(string.Format("/VKClient.Photos;component/PhotosMainPage.xaml?UserOrGroupId={0}&IsGroup={1}&SelectForMove=True&ExcludeId={2}&SelectedPhotos={3}&AdminLevel={4}", (object)userOrGroupId, isGroup, (object)excludeAid, (object)list.GetCommaSeparated(), (object)adminLevel) + "&IsPopupNavigation=True");
        }

        private static string GetNavigateToGroupsString(long userId, string name = "", bool pickManaged = false, long owner_id = 0, long pic_id = 0, string text = "", bool isGif = false, string accessKey = "")
        {
            ParametersRepository.SetParameterForId("ShareText", (object)text);
            string str = string.Format("/VKClient.Groups;component/GroupsListPage.xaml?UserId={0}&Name={1}&PickManaged={2}&OwnerId={3}&PicId={4}&IsGif={5}&AccessKey={6}", (object)userId, (object)name, (object)pickManaged, (object)owner_id, pic_id, (object)isGif, (object)Extensions.ForURL(accessKey));
            if (pickManaged)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToGroups(long userId, string name = "", bool pickManaged = false, long owner_id = 0, long pic_id = 0, string text = "", bool isGif = false, string accessKey = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToGroupsString(userId, name, pickManaged, owner_id, pic_id, text, isGif, accessKey));
        }

        public void NavigateToMap(bool pick, double latitude, double longitude)
        {
            this.Navigate(string.Format("/VKClient.Common;component/MapAttachmentPage.xaml?latitude={0}&longitude={1}&Pick={2}", (object)latitude.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)longitude.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)pick.ToString()));
        }

        public void NavigateToPostSchedule(DateTime? dateTime = null)
        {
            long num = 0;
            if (dateTime.HasValue)
                num = dateTime.Value.Ticks;
            this.Navigate(string.Format("/VKClient.Common;component/PostSchedulePage.xaml?PublishDateTime={0}", (object)num) + "&IsPopupNavigation=True");
        }

        private static string GetNavigateToGroupDiscussionsString(long gid, string name, int adminLevel, bool isPublicPage, bool canCreateTopic)
        {
            return string.Format("/VKClient.Groups;component/GroupDiscussionsPage.xaml?GroupId={0}&Name={1}&AdminLevel={2}&IsPublicPage={3}&CanCreateTopic={4}", gid, (object)name, (object)adminLevel, (object)isPublicPage, (object)canCreateTopic);
        }

        public void NavigateToGroupDiscussions(long gid, string name, int adminLevel, bool isPublicPage, bool canCreateTopic)
        {
            this.Navigate(NavigatorImpl.GetNavigateToGroupDiscussionsString(gid, name, adminLevel, isPublicPage, canCreateTopic));
        }

        public void NavigateToGroupDiscussion(long gid, long tid, string topicName, int knownCommentsCount, bool loadFromEnd, bool canComment)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/GroupDiscussionPage.xaml?GroupId={0}&TopicId={1}&TopicName={2}&KnownCommentsCount={3}&LoadFromTheEnd={4}&CanComment={5}", gid, (object)tid, (object)Extensions.ForURL(topicName), (object)knownCommentsCount, (object)loadFromEnd, (object)canComment));
        }

        public static string GetNavToFeedbackStr()
        {
            return "/VKClient.Common;component/FeedbackPage.xaml";
        }

        public void NavigateToFeedback()
        {
            this.Navigate(NavigatorImpl.GetNavToFeedbackStr());
        }

        private static string GetNavigateToVideoWithCommentsString(VKClient.Common.Backend.DataObjects.Video video, long ownerId, long videoId, string accessKey = "")
        {
            ParametersRepository.SetParameterForId("Video", video);
            StatisticsActionSource videoSource = CurrentMediaSource.VideoSource;
            string videoContext = CurrentMediaSource.VideoContext;
            return string.Format("/VKClient.Video;component/VideoCommentsPage.xaml?OwnerId={0}&VideoId={1}&AccessKey={2}&VideoSource={3}&VideoContext={4}", ownerId, videoId, Extensions.ForURL(accessKey), videoSource, Extensions.ForURL(videoContext));
        }

        public void NavigateToVideoWithComments(VKClient.Common.Backend.DataObjects.Video video, long ownerId, long videoId, string accessKey = "")
        {
            this.Navigate(NavigatorImpl.GetNavigateToVideoWithCommentsString(video, ownerId, videoId, accessKey));
        }

        public void NavigateToConversationsSearch()
        {
            this.Navigate("/VKMessenger;component/Views/ConversationsSearch.xaml" + "?IsPopupNavigation=True");
        }

        public static string GetNavToConversationStr(long userOrChatId, bool isChat, bool fromLookup = false, string newMessageContents = "", long messageId = 0, bool isContactSellerMode = false)
        {
            return string.Format("/VKMessenger;component/Views/ConversationPage.xaml?UserOrChatId={0}&IsChat={1}&FromLookup={2}&NewMessageContents={3}&MessageId={4}&IsContactProductSellerMode={5}", (object)userOrChatId, (object)isChat, (object)fromLookup, (object)!string.IsNullOrEmpty(newMessageContents), (object)messageId, (object)isContactSellerMode);
        }

        public void NavigateToConversation(long userOrChatId, bool isChat, bool fromLookup = false, string newMessageContents = "", long messageId = 0, bool isContactSellerMode = false)
        {
            if (!string.IsNullOrEmpty(newMessageContents))
                ParametersRepository.SetParameterForId("NewMessageContents", (object)newMessageContents);
            this.Navigate(NavigatorImpl.GetNavToConversationStr(userOrChatId, isChat, fromLookup, newMessageContents, messageId, isContactSellerMode));
        }

        public void NavigateToWelcomePage()
        {
            this.Navigate("/VKClient;component/WelcomePage.xaml");
        }

        private void Navigate(string navStr)
        {
            Logger.Instance.Info("Navigator.Navigate, navStr={0}", (object)navStr);
            this._history.Add(navStr);
            if (this._history.Count > 100)
                this._history = this._history.Skip<string>(Math.Max(0, this._history.Count<string>() - 10)).Take<string>(10).ToList<string>();
            this.HandleIsPopupAnimationIfNeeded(navStr);
            Execute.ExecuteOnUIThread((Action)(() => NavigatorImpl.NavigationService.Navigate(new Uri(navStr, UriKind.Relative))));
        }

        private void HandleIsPopupAnimationIfNeeded(string navStr)
        {
        }

        public void NavigateToPickUser(bool createChat, long initialUserId, bool goBackOnResult, int currentCountInChat = 0, PickUserMode mode = PickUserMode.PickForMessage, string customTitle = "", int sexFilter = 0)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PickUserForNewMessagePage.xaml?CreateChat={0}&InitialUserId={1}&GoBackOnResult={2}&CurrentCountInChat={3}&PickMode={4}&CustomTitle={5}&SexFilter={6}", (object)createChat, (object)initialUserId, (object)goBackOnResult, (object)currentCountInChat, (object)mode, (object)customTitle, (object)sexFilter) + "&IsPopupNavigation=True");
        }

        public void NavigateToPickConversation()
        {
            this.Navigate("/VKMessenger;component/Views/PickConversationPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToAudioPlayer(bool startPlaying = false)
        {
            this.Navigate(string.Format("/VKClient.Audio;component/Views/AudioPlayer.xaml?startPlaying={0}", (object)startPlaying) + "&IsPopupNavigation=True");
        }

        public void NavigateToGroupInvitations()
        {
            this.Navigate("/VKClient.Groups;component/GroupInvitationsPage.xaml");
        }

        private static string GetNavigateToPhotoAlbumString(long userOrGroupId, bool isGroup, string type, string aid, string albumName = "", int photosCount = 0, string title = "", string description = "", bool pickMode = false, int adminLevel = 0)
        {
            string str = string.Format("/VKClient.Photos;component/PhotoAlbumPage.xaml?userOrGroupId={0}&isGroup={1}&albumType={2}&albumId={3}&albumName={4}&photosCount={5}&pageTitle={6}&albumDesc={7}&PickMode={8}&AdminLevel={9}", (object)userOrGroupId, isGroup, (object)type, (object)aid, (object)Extensions.ForURL(albumName), (object)photosCount, (object)Extensions.ForURL(title), (object)Extensions.ForURL(description), (object)pickMode, (object)adminLevel);
            if (pickMode)
                str += "&IsPopupNavigation=True";
            return str;
        }

        public void NavigateToPhotoAlbum(long userOrGroupId, bool isGroup, string type, string aid, string albumName = "", int photosCount = 0, string title = "", string description = "", bool pickMode = false, int adminLevel = 0)
        {
            this.Navigate(NavigatorImpl.GetNavigateToPhotoAlbumString(userOrGroupId, isGroup, type, aid, albumName, photosCount, title, description, pickMode, adminLevel));
        }

        public void NavigateToMainPage()
        {
            this.Navigate("/VKClient.Common;component/NewsPage.xaml?ClearBackStack=true");
        }

        private static string GetNavigateToFavoritesString()
        {
            return "/VKClient.Common;component/FavoritesPage.xaml";
        }

        public void NavigateToFavorites()
        {
            this.Navigate(NavigatorImpl.GetNavigateToFavoritesString());
        }

        public void NavigateToGames(long gameId = 0, bool fromPush = false)
        {
            this.Navigate(NavigatorImpl.GetGamesNavStr(gameId, fromPush));
        }

        public static string GetGamesNavStr(long gameId = 0, bool fromPush = false)
        {
            return string.Format("/VKClient.Common;component/GamesMainPage.xaml?GameId={0}&FromPush={1}", (object)gameId, (object)fromPush);
        }

        public void NavigateToMyGames()
        {
            this.Navigate("/VKClient.Common;component/GamesMyPage.xaml");
        }

        public void NavigateToGamesFriendsActivity(long gameId = 0, string gameName = "")
        {
            this.Navigate(string.Format("/VKClient.Common;component/GamesFriendsActivityPage.xaml?GameId={0}&GameName={1}", (object)gameId, (object)gameName));
        }

        public void NavigateToGamesInvites()
        {
            this.Navigate("/VKClient.Common;component/GamesInvitesPage.xaml");
        }

        public void OpenGame(Game game)
        {
            if (game == null)
                return;
            if (InstalledPackagesFinder.Instance.IsPackageInstalled(game.platform_id))
                this.OpenGame(game.id);
            else
                new MarketplaceDetailTask()
                {
                    ContentIdentifier = game.platform_id,
                    ContentType = MarketplaceContentType.Applications
                }.Show();
        }

        public async void OpenGame(long gameId)
        {
            if (gameId <= 0)
                return;
            await Launcher.LaunchUriAsync(new Uri(string.Format("vk{0}://", (object)gameId)));
        }

        public void NavigateToGameSettings(long gameId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/GameSettingsPage.xaml?GameId={0}", (object)gameId));
        }

        public void NavigateToManageSources(ManageSourcesMode mode = ManageSourcesMode.ManageHiddenNewsSources)
        {
            this.Navigate(string.Format("/VKClient.Common;component/ManageSourcesPage.xaml?Mode={0}", (object)mode));
        }

        public void NavigateToPhotoPickerPhotos(int maxAllowedToSelect, bool ownPhotoPick = false, bool pickToStorageFile = false)
        {
            this.Navigate(string.Format("/VKClient.Photos;component/PhotoPickerPhotos.xaml?MaxAllowedToSelect={0}&OwnPhotoPick={1}&PickToStorageFile={2}&IsPopupNavigation=True", (object)maxAllowedToSelect, (object)ownPhotoPick, (object)pickToStorageFile));
        }

        public void NavigationToValidationPage(string validationUri)
        {
            this.Navigate(string.Format("/VKClient.Common;component/ValidatePage.xaml?ValidationUri={0}", (object)Extensions.ForURL(validationUri)) + "&IsPopupNavigation=True");
        }

        public void NavigateTo2FASecurityCheck(string username, string password, string phoneMask, string validationType, string validationSid)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Auth2FAPage.xaml?username={0}&password={1}&phoneMask={2}&validationType={3}&validationSid={4}&IsPopupNavigation=True", (object)Extensions.ForURL(username), (object)Extensions.ForURL(password), (object)Extensions.ForURL(phoneMask), (object)Extensions.ForURL(validationType), (object)Extensions.ForURL(validationSid)));
        }

        public void NavigateToAddNewVideo(string filePath, long ownerId)
        {
            //this.Navigate(string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/AddEditVideoPage.xaml?VideoToUploadPath={0}&OwnerId={1}", (object)Extensions.ForURL(filePath), ownerId) + "&IsPopupNavigation=True");
            this.Navigate(string.Format("/VKClient.Video;component/AddEditVideoPage.xaml?VideoToUploadPath={0}&OwnerId={1}", (object)Extensions.ForURL(filePath), ownerId) + "&IsPopupNavigation=True");
        }

        public void NavigateToEditVideo(long ownerId, long videoId, VKClient.Common.Backend.DataObjects.Video video = null)
        {
            if (video != null)
                ParametersRepository.SetParameterForId("VideoForEdit", (object)video);///VKClient.Video;component/VideoCatalog/
            //this.Navigate(string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/AddEditVideoPage.xaml?OwnerId={0}&VideoId={1}", ownerId, videoId) + "&IsPopupNavigation=True");
            this.Navigate(string.Format("/VKClient.Video;component/AddEditVideoPage.xaml?OwnerId={0}&VideoId={1}", ownerId, videoId) + "&IsPopupNavigation=True");
        }

        private static string GetNavigateToNewsFeedString(int newsSourceId = 0, bool photoFeedMoveTutorial = false)
        {
            return string.Format("/VKClient.Common;component/NewsPage.xaml?NewsSourceId={0}&PhotoFeedMoveTutorial={1}", (object)newsSourceId, (object)photoFeedMoveTutorial);
        }

        public void NavigateToNewsFeed(int newsSourceId = 0, bool photoFeedMoveTutorial = false)
        {
            this.Navigate(NavigatorImpl.GetNavigateToNewsFeedString(newsSourceId, photoFeedMoveTutorial));
        }

        private string GetNavigateToNewsSearchString(string query = "")
        {
            return string.Format("/VKClient.Common;component/NewsSearchPage.xaml?Query={0}", (object)HttpUtility.UrlEncode(query));
        }

        public void NavigateToNewsSearch(string query = "")
        {
            this.Navigate(this.GetNavigateToNewsSearchString(query));
        }

        private static string GetNavigateToConversationsString()
        {
            return "/VKMessenger;component/ConversationsPage.xaml";
        }

        public void NavigateToConversations()
        {
            this.Navigate(NavigatorImpl.GetNavigateToConversationsString());
        }

        public IPhotoPickerPhotosViewModel GetPhotoPickerPhotosViewModelInstance(int maxAllowedToSelect)
        {
            return (IPhotoPickerPhotosViewModel)new PhotoPickerPhotosViewModel(maxAllowedToSelect, false);
        }

        public void NavigateToBlackList()
        {
            this.Navigate("/VKClient.Common;component/BannedUsersPage.xaml");
        }

        public void NavigateToBirthdaysPage()
        {
            this.Navigate("/VKClient.Common;component/BirthdaysPage.xaml");
        }

        public void NavigateToSuggestedPostponedPostsPage(long userOrGroupId, bool isGroup, int mode)
        {
            this.Navigate(string.Format("/VKClient.Common;component/SuggestedPostponedPostsPage.xaml?UserOrGroupId={0}&IsGroup={1}&Mode={2}", (object)userOrGroupId, isGroup, (object)mode));
        }

        public void NavigateToHelpPage()
        {
            this.Navigate(string.Format("/VKClient.Common;component/HelpPage.xaml"));
        }

        public void NavigateToAboutPage()
        {
            this.Navigate(string.Format("/VKClient.Common;component/AboutPage.xaml"));
        }

        public void NavigateFromSDKAuthPage(string callbackUriToLaunch)
        {
            ParametersRepository.SetParameterForId("CallbackUriToLaunch", (object)callbackUriToLaunch);
            this.GoBack();
        }

        public void NavigateToEditPrivacy(EditPrivacyPageInputData inputData)
        {
            ParametersRepository.SetParameterForId("EditPrivacyInputData", (object)inputData);
            this.Navigate("/VKClient.Common;component/EditPrivacyPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToSettingsPrivacy()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsPrivacyPage.xaml"));
        }

        public void NavigateToSettingsGeneral()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsGeneralPage.xaml"));
        }

        public void NavigateToSettingsAccount()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsAccountPage.xaml"));
        }

        public void NavigateToChangePassword()
        {
            this.Navigate(string.Format("/VKClient.Common;component/ChangePasswordPage.xaml?IsPopupNavigation=True"));
        }

        public void NavigateToChangeShortName(string currentShortName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsChangeShortNamePage.xaml?CurrentShortName={0}&IsPopupNavigation=True", (object)Extensions.ForURL(currentShortName)));
        }

        public void NavigateToSettingsNotifications()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsNotifications.xaml"));
        }

        public void NavigateToEditProfile()
        {
            this.Navigate(string.Format("/VKClient.Common;component/SettingsEditProfilePage.xaml"));
        }

        public void NavigateToCreateEditPoll(long ownerId, long pollId = 0, Poll poll = null)
        {
            if (poll != null)
                ParametersRepository.SetParameterForId("Poll", (object)poll);
            this.Navigate(string.Format("/VKClient.Common;component/CreateEditPollPage.xaml?OwnerId={0}&PollId={1}", ownerId, (object)pollId) + "&IsPopupNavigation=True");
        }

        public void NavigateToPollVoters(long ownerId, long pollId, long answerId, string answerText)
        {
            this.Navigate(string.Format("/VKClient.Common;component/PollVotersPage.xaml?OwnerId={0}&PollId={1}&AnswerId={2}&AnswerText={3}", ownerId, (object)pollId, (object)answerId, (object)answerText));
        }

        public void NavigateToUsersSearch(string query = "")
        {
            this.Navigate("/VKClient.Common;component/UsersSearchResultsPage.xaml?IsPopupNavigation=True&Query=" + query);
        }

        public void NavigateToUsersSearchNearby()
        {
            this.Navigate("/VKClient.Common;component/UsersSearchNearbyPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToUsersSearchParams()
        {
            this.Navigate("/VKClient.Common;component/UsersSearchParamsPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToFriendsSuggestions()
        {
            this.Navigate("/VKClient.Common;component/FriendsSuggestionsPage.xaml");
        }

        public void NavigateToRegistrationPage()
        {
            this.Navigate(string.Format("/VKClient.Common;component/RegistrationPage.xaml?SessionId={0}", (object)Guid.NewGuid().ToString()));
        }

        public void NavigateToFriendsImportFacebook()
        {
            this.Navigate("/VKClient.Common;component/FriendsImportFacebookPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToFriendsImportGmail()
        {
            this.Navigate("/VKClient.Common;component/FriendsImportGmailPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToFriendsImportTwitter(string oauthToken, string oauthVerifier)
        {
            this.Navigate(string.Format("/VKClient.Common;component/FriendsImportTwitterPage.xaml?oauthToken={0}&oauthVerifier={1}&IsPopupNavigation=True", (object)oauthToken, (object)oauthVerifier));
        }

        public void NavigateToFriendsImportContacts()
        {
            this.Navigate("/VKClient.Common;component/FriendsImportContactsPage.xaml?IsPopupNavigation=True");
        }

        public void NavigateToGroupRecommendations(int categoryId, string categoryName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/RecommendedGroupsPage.xaml?CategoryId={0}&CategoryName={1}", (object)categoryId, (object)Extensions.ForURL(categoryName)));
        }

        private static string GetNavigateToVideoCatalogString()
        {
            //return string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/VideoCatalog/VideoCatalogPage.xaml");
            return string.Format("/VKClient.Video;component/VideoCatalog/VideoCatalogPage.xaml");//VKClient.Video\videocatalog\videocatalogpage.xaml
        }

        public void NavigateToVideoCatalog()
        {
            this.Navigate(NavigatorImpl.GetNavigateToVideoCatalogString());
        }

        public static string GetOpenUrlPageStr(string uriToOpen)
        {
            return string.Format("/VKClient.Common;component/OpenUrlPage.xaml?Uri={0}", (object)Extensions.ForURL(uriToOpen));
        }

        public static string GetWebViewPageNavStr(string uri)
        {
            return "/VKClient.Common;component/WebViewPage.xaml?Uri=" + HttpUtility.UrlEncode(uri);
        }

        public void NavigateToWebViewPage(string uri)
        {
            this.Navigate(NavigatorImpl.GetWebViewPageNavStr(uri));
        }

        private static string GetNavigateToMarketString(long ownerId)
        {
            return string.Format("/VKClient.Common;component/Market/Views/MarketMainPage.xaml?OwnerId={0}", ownerId);
        }

        public void NavigateToMarket(long ownerId)
        {
            this.Navigate(NavigatorImpl.GetNavigateToMarketString(ownerId));
        }

        public void NavigateToProduct(Product product)
        {
            if (product == null)
                return;
            ParametersRepository.SetParameterForId("Product", (object)product);
            this.NavigateToProduct(product.owner_id, product.id);
        }

        private static string GetNavigateToProductString(long ownerId, long productId)
        {
            return string.Format("/VKClient.Common;component/Market/Views/ProductPage.xaml?OwnerId={0}&ProductId={1}", ownerId, (object)productId);
        }

        public void NavigateToProduct(long ownerId, long productId)
        {
            this.Navigate(NavigatorImpl.GetNavigateToProductString(ownerId, productId));
        }

        public void NavigateToProductsSearchParams(long priceFrom, long priceTo, int currencyId, string currencyName)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Market/Views/ProductsSearchParamsPage.xaml?PriceFrom={0}&PriceTo={1}&CurrencyId={2}&CurrencyName={3}", (object)priceFrom, (object)priceTo, (object)currencyId, (object)currencyName));
        }

        public void NavigateToMarketAlbums(long ownerId)
        {
            this.Navigate(string.Format("/VKClient.Common;component/Market/Views/MarketAlbumsPage.xaml?OwnerId={0}", ownerId));
        }

        private static string GetNavigateToMarketAlbumProductsString(long ownerId, long albumId, string albumName)
        {
            return string.Format("/VKClient.Common;component/Market/Views/MarketAlbumProductsPage.xaml?OwnerId={0}&AlbumId={1}&AlbumName={2}", ownerId, albumId, (object)albumName);
        }

        public void NavigateToMarketAlbumProducts(long ownerId, long albumId, string albumName)
        {
            this.Navigate(NavigatorImpl.GetNavigateToMarketAlbumProductsString(ownerId, albumId, albumName));
        }

        public void NavigateToVideoAlbumsList(long ownerId, bool forceAllowCreateAlbum = false)
        {
            //this.Navigate(string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/VideoCatalog/VideoAlbumsListPage.xaml?OwnerId={0}&ForceAllowCreateAlbum={1}", ownerId, (object)forceAllowCreateAlbum));
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/VideoAlbumsListPage.xaml?OwnerId={0}&ForceAllowCreateAlbum={1}", ownerId, (object)forceAllowCreateAlbum));
        }

        public void NavigateToVideoList(VKList<VideoCatalogItem> catalogItems, int source, string context, string sectionId = "", string next = "", string name = "")
        {
            ParametersRepository.SetParameterForId("CatalogItemsToShow", (object)catalogItems);
            //this.Navigate(string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/VideoCatalog/VideoListPage.xaml?SectionId={0}&Next={1}&Name={2}&Source={3}&Context={4}", (object)Extensions.ForURL(sectionId), (object)Extensions.ForURL(next), (object)Extensions.ForURL(name), (object)source, (object)Extensions.ForURL(context)));
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/VideoListPage.xaml?SectionId={0}&Next={1}&Name={2}&Source={3}&Context={4}", (object)Extensions.ForURL(sectionId), (object)Extensions.ForURL(next), (object)Extensions.ForURL(name), (object)source, (object)Extensions.ForURL(context)));
        }

        public void NavigateToCreateEditVideoAlbum(long albumId = 0, long groupId = 0, string name = "", PrivacyInfo pi = null)
        {
            if (pi != null)
                ParametersRepository.SetParameterForId("AlbumPrivacyInfo", (object)pi);
            //this.Navigate(string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/VideoCatalog/CreateEditVideoAlbumPage.xaml?AlbumId={0}&GroupId={1}&Name={2}", albumId, (object)groupId, (object)Extensions.ForURL(name)));
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/CreateEditVideoAlbumPage.xaml?AlbumId={0}&GroupId={1}&Name={2}", albumId, (object)groupId, (object)Extensions.ForURL(name)));
        }

        public void NavigateToAddVideoToAlbum(long ownerId, long videoId)
        {
            //this.Navigate(string.Format("/VKClient.VKClient.Common.Backend.DataObjects.Video;component/VideoCatalog/AddToAlbumPage.xaml?VideoId={0}&OwnerId={1}", videoId, ownerId));
            this.Navigate(string.Format("/VKClient.Video;component/VideoCatalog/AddToAlbumPage.xaml?VideoId={0}&OwnerId={1}", videoId, ownerId));
        }

        public void NavigateToConversationMaterials(long peerId)
        {
            this.Navigate("/VKMessenger;component/Views/ConversationMaterialsPage.xaml?PeerId=" + (object)peerId);
        }

        public void NavigateToDocumentsPicker()
        {
            this.Navigate("/VKClient.Common;component/DocumentsPickerPage.xaml?IsPopupNavigation=true");
        }

        public void NavigateToCommunityCreation()
        {
            this.Navigate("/VKClient.Groups;component/CommunityCreationPage.xaml?IsPopupNavigation=true");
        }

        public void NavigateToCommunitySubscribers(long communityId, GroupType communityType, bool isManagement = false, bool isPicker = false, bool isBlockingPicker = false)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/CommunitySubscribersPage.xaml?CommunityId={0}&CommunityType={1}&IsManagement={2}&IsPicker={3}&IsBlockingPicker={4}", communityId, (int)communityType, isManagement, isPicker, isBlockingPicker));
        }

        private static string GetNavigateToStickersStoreString()
        {
            return "/VKClient.Common;component/Stickers/Views/StickersStorePage.xaml";
        }

        public void NavigateToStickersStore()
        {
            this.Navigate(NavigatorImpl.GetNavigateToStickersStoreString());
        }

        private static string GetNavigateToStickersManageString()
        {
            return "/VKClient.Common;component/Stickers/Views/StickersManagePage.xaml";
        }

        public void NavigateToStickersManage()
        {
            this.Navigate(NavigatorImpl.GetNavigateToStickersManageString());
        }

        public void NavigateToBalance()
        {
            this.Navigate("/VKClient.Common;component/Balance/Views/BalancePage.xaml");
        }

        public void NavigateToCustomListPickerSelection(CustomListPicker parentPicker)
        {
            ParametersRepository.SetParameterForId("ParentPicker", (object)parentPicker);
            this.Navigate("/VKClient.Common;component/UC/CustomListPicker/SelectionPage.xaml");
        }

        public void NavigateToGraffitiDrawPage(long userOrChatId, bool isChat, string title) // NEW: 4.8.0
        {
            title = HttpUtility.UrlEncode(title);
            this.Navigate(string.Format("/VKClient.Common;component/Graffiti/Views/GraffitiDrawPage.xaml?UserOrChatId={0}&IsChat={1}&Title={2}", (object)userOrChatId, (object)isChat, (object)title));
        }

        public void NavigateToCommunityManagement(long communityId, GroupType communityType, bool isAdministrator)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/MainPage.xaml?CommunityId={0}&CommunityType={1}&IsAdministrator={2}", communityId, (int)communityType, isAdministrator));
        }

        public void NavigateToCommunityManagementInformation(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/Information/InformationPage.xaml?CommunityId=" + communityId);
        }

        public void NavigateToCommunityManagementPlacementSelection(long communityId, Place place)
        {
            ParametersRepository.SetParameterForId("PlacementSelectionPlace", (object)place);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/Information/PlacementSelectionPage.xaml?CommunityId={0}&IsPopupNavigation=true", communityId));
        }

        public void NavigateToCommunityManagementServices(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/ServicesPage.xaml?CommunityId=" + (object)communityId);
        }

        public void NavigateToCommunityManagementServiceSwitch(CommunityService service, CommunityServiceState currentState)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ServiceSwitchPage.xaml?Service={0}&CurrentState={1}&IsPopupNavigation=true", (object)service, (object)currentState));
        }

        public void NavigateToCommunityManagementManagers(long communityId, GroupType communityType)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ManagersPage.xaml?CommunityId={0}&CommunityType={1}", communityId, (int)communityType));
        }

        public void NavigateToCommunityManagementManagerAdding(long communityId, User user, bool fromPicker)
        {
            ParametersRepository.SetParameterForId("CommunityManager", (object)user);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ManagerEditingPage.xaml?CommunityId={0}&FromPicker={1}&IsPopupNavigation=true", (object)communityId, (object)fromPicker));
        }

        public void NavigateToCommunityManagementManagerEditing(long communityId, User manager, bool isContact, string position, string email, string phone)
        {
            ParametersRepository.SetParameterForId("CommunityManager", (object)manager);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/ManagerEditingPage.xaml?CommunityId={0}&IsContact={1}&Position={2}&Phone={3}&Email={4}&IsPopupNavigation=true", (object)communityId, (object)isContact, (object)Extensions.ForURL(position), (object)Extensions.ForURL(phone), (object)Extensions.ForURL(email)));
        }

        public void NavigateToCommunityManagementRequests(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/RequestsPage.xaml?CommunityId=" + (object)communityId);
        }

        public void NavigateToCommunityManagementInvitations(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/InvitationsPage.xaml?CommunityId=" + (object)communityId);
        }

        public void NavigateToCommunityManagementBlacklist(long communityId, GroupType communityType)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlacklistPage.xaml?CommunityId={0}&CommunityType={1}", (object)communityId, (int)communityType));
        }

        public void NavigateToCommunityManagementBlockAdding(long communityId, User user, bool isOpenedWithoutPicker = false)
        {
            ParametersRepository.SetParameterForId("CommunityManagementBlockEditingUser", (object)user);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlockEditingPage.xaml?CommunityId={0}&IsEditing={1}&IsOpenedWithoutPicker={2}&IsPopupNavigation=true", (object)communityId, (object)false, (object)isOpenedWithoutPicker));
        }

        public void NavigateToCommunityManagementBlockEditing(long communityId, User user, User manager)
        {
            ParametersRepository.SetParameterForId("CommunityManagementBlockEditingUser", (object)user);
            ParametersRepository.SetParameterForId("CommunityManagementBlockEditingManager", (object)manager);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlockEditingPage.xaml?CommunityId={0}&IsEditing={1}&IsOpenedWithoutPicker={2}&IsPopupNavigation=true", (object)communityId, (object)true, (object)false));
        }

        public void NavigateToCommunityManagementBlockDurationPicker(int durationUnixTime)
        {
            this.Navigate(string.Format("/VKClient.Groups;component/Management/BlockDurationPicker.xaml?DurationUnixTime={0}&IsPopupNavigation=true", (object)durationUnixTime));
        }

        public void NavigateToCommunityManagementLinks(long communityId)
        {
            this.Navigate("/VKClient.Groups;component/Management/LinksPage.xaml?CommunityId=" + (object)communityId);
        }

        public void NavigateToCommunityManagementLinkCreation(long communityId, GroupLink link)
        {
            if (link != null)
                ParametersRepository.SetParameterForId("CommunityLink", (object)link);
            this.Navigate(string.Format("/VKClient.Groups;component/Management/LinkCreationPage.xaml?CommunityId={0}&IsPopupNavigation=true", (object)communityId));
        }

        public class NavigationTypeMatch
        {
            private readonly Regex _idsRegEx = new Regex("\\-?[0-9]+");
            private readonly Regex _queryParamsRegex = new Regex("(\\?|\\&)([^=]+)\\=([^&]+)");
            private readonly Regex _regEx;

            public NavigatorImpl.NavType MatchType { get; private set; }

            public long Id1 { get; private set; }

            public string Id1String { get; private set; }

            public long Id2 { get; private set; }

            public string Id2String { get; private set; }

            public string ObjName { get; private set; }

            public string ObjSubName { get; private set; }

            public List<string> SubTypes { get; private set; }

            public NavigationTypeMatch(Regex regExp, NavigatorImpl.NavType navType)
            {
                this._regEx = regExp;
                this.MatchType = navType;
            }

            public bool Check(string uri)
            {
                MatchCollection matchCollection1 = this._regEx.Matches(uri);
                if (matchCollection1.Count == 0)
                    return false;
                Match match1 = matchCollection1[0];
                this.ObjName = match1.Value;
                if (match1.Groups.Count > 0)
                    this.ObjSubName = match1.Groups[match1.Groups.Count - 1].Value;
                MatchCollection matchCollection2 = this._idsRegEx.Matches(this.ObjName);
                if (matchCollection2.Count > 0)
                {
                    this.Id1String = matchCollection2[0].Value;
                    long result;
                    this.Id1 = long.TryParse(matchCollection2[0].Value, out result) ? result : 0L;
                }
                if (matchCollection2.Count > 1)
                {
                    this.Id2String = matchCollection2[1].Value;
                    long result;
                    this.Id2 = long.TryParse(matchCollection2[1].Value, out result) ? result : 0L;
                }
                MatchCollection matchCollection3 = this._queryParamsRegex.Matches(uri);
                this.SubTypes = new List<string>();
                foreach (Match match2 in matchCollection3)
                {
                    if (match2.Groups.Count == 4 && match2.Groups[2].Value == "w")
                        this.SubTypes.Add("/" + match2.Groups[match2.Groups.Count - 1].Value);
                }
                return true;
            }
        }

        public class NavigationSubtypeMatch
        {
        }

        public enum NavType
        {
            none,
            friends,
            communities,
            dialogs,
            news,
            tagPhoto,
            albums,
            profile,
            dialog,
            community,
            board,
            album,
            video,
            audios,
            topic,
            photo,
            wallPost,
            namedObject,
            stickersSettings,
            settings,
            feedback,
            videos,
            fave,
            apps,
            marketAlbum,
            market,
            product,
            stickers,
            stickersPack,
        }
    }
}
