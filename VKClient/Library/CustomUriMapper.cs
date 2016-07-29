using Facebook.Client;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Social;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace VKClient.Library
{
    public class CustomUriMapper : UriMapperBase
    {
        public bool NeedHandleActivation;

        public override Uri MapUri(Uri uri)
        {
            Logger.Instance.Info("Requested uri " + uri.ToString());
            string originalString = uri.OriginalString;
            bool flag1 = this.NeedHandleActivation;
            this.NeedHandleActivation = false;
            if (AppGlobalStateManager.Current.LoggedInUserId == 0L)
            {
                if (originalString.StartsWith("/Protocol"))
                    PageBase.ProtocolLaunchAfterLogin = this.MapProtocolLaunchUri(uri);
                return uri;
            }
            if (originalString.StartsWith("/Protocol"))
            {
                if (flag1)
                    StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                    {
                        Reason = AppActivationReason.other_app,
                        ReasonSubtype = "unknown"
                    });
                return this.MapProtocolLaunchUri(uri);
            }
            if (originalString.Contains("NewsPage.xaml"))
            {
                Dictionary<string, string> paramDict = uri.ParseQueryString();
                if (paramDict.ContainsKey("msg_id") && paramDict.ContainsKey("uid"))
                {
                    string s = paramDict["uid"];
                    if (flag1)
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.push,
                            ReasonSubtype = "message"
                        });
                    long userOrChatId;
                    if (long.TryParse(s, out userOrChatId))
                    {
                        bool isChat = false;
                        if (userOrChatId - 2000000000L > 0L)
                        {
                            userOrChatId -= 2000000000L;
                            isChat = true;
                        }
                        return new Uri(NavigatorImpl.GetNavToConversationStr(userOrChatId, isChat, false, "", 0L, false) + "&ClearBackStack=true", UriKind.Relative);
                    }
                }
                if (paramDict.ContainsKey("type"))
                {
                    string str = paramDict["type"];
                    int num = AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled ? 1 : 0;
                    if (flag1)
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.push,
                            ReasonSubtype = str
                        });
                    if (num != 0 && (str == "sdk_open" || str == "sdk_request" || str == "sdk_invite"))
                    {
                        long result = 0;
                        if (paramDict.ContainsKey("app_id"))
                            long.TryParse(paramDict["app_id"], out result);
                        if (str == "sdk_open")
                            Navigator.Current.OpenGame(result);
                        if (str == "sdk_invite")
                            return new Uri(NavigatorImpl.GetGamesNavStr(0L, true), UriKind.Relative);
                        return new Uri(NavigatorImpl.GetGamesNavStr(result, true), UriKind.Relative);
                    }
                    if (str == "open_url" && paramDict.ContainsKey("url"))
                        return new Uri(NavigatorImpl.GetOpenUrlPageStr(paramDict["url"]), UriKind.Relative);
                    if (str == "friend_found")
                        return new Uri(NavigatorImpl.GetNavigateToUserProfileNavStr(long.Parse(paramDict["uid"]), "", false, "") + "?ClearBackStack=true", UriKind.Relative);
                    if (str == "friend")
                        return new Uri("/VKClient.Common;component/FriendRequestsPage.xaml" + "?ClearBackStack=true", UriKind.Relative);
                    return new Uri(NavigatorImpl.GetNavToFeedbackStr() + "?ClearBackStack=true", UriKind.Relative);
                }
                if (paramDict.ContainsKey("place"))
                {
                    if (flag1 && !paramDict.ContainsKey("type"))
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.push,
                            ReasonSubtype = "place"
                        });
                    string str = paramDict["place"];
                    long ownerId = long.Parse(str.Remove(str.IndexOf('_')).Remove(0, 4));
                    return new Uri(NavigatorImpl.GetNavigateToPostCommentsNavStr(long.Parse(str.Remove(0, str.IndexOf('_') + 1)), ownerId, false, 0L, 0L, "") + "&ClearBackStack=true", UriKind.Relative);
                }
                if (paramDict.ContainsKey("group_id"))
                {
                    long result = 0;
                    if (flag1 && !paramDict.ContainsKey("type"))
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.push,
                            ReasonSubtype = "group"
                        });
                    if (long.TryParse(paramDict["group_id"], out result))
                        return new Uri(NavigatorImpl.GetNavigateToGroupNavStr(result, "", false) + "&ClearBackStack=true", UriKind.Relative);
                }
                if (paramDict.ContainsKey("uid"))
                {
                    long result = 0;
                    if (flag1 && !paramDict.ContainsKey("type"))
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.push,
                            ReasonSubtype = "user"
                        });
                    if (long.TryParse(paramDict["uid"], out result))
                        return new Uri(NavigatorImpl.GetNavigateToUserProfileNavStr(result, "", false, "") + "&ClearBackStack=true", UriKind.Relative);
                }
                if (paramDict.ContainsKey("from_id"))
                {
                    long result = 0;
                    if (long.TryParse(paramDict["from_id"], out result))
                    {
                        if (result < 0L)
                        {
                            if (flag1 && !paramDict.ContainsKey("type"))
                                StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                                {
                                    Reason = AppActivationReason.push,
                                    ReasonSubtype = "group"
                                });
                            return new Uri(NavigatorImpl.GetNavigateToGroupNavStr(-result, "", false) + "&ClearBackStack=true", UriKind.Relative);
                        }
                        if (flag1 && !paramDict.ContainsKey("type"))
                            StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                            {
                                Reason = AppActivationReason.push,
                                ReasonSubtype = "user"
                            });
                        return new Uri(NavigatorImpl.GetNavigateToUserProfileNavStr(result, "", false, "") + "&ClearBackStack=true", UriKind.Relative);
                    }
                }
                if (paramDict.ContainsKey("device_token") && paramDict.ContainsKey("url"))
                    return new Uri(NavigatorImpl.GetWebViewPageNavStr(HttpUtility.UrlDecode(paramDict["url"])) + "&ClearBackStack=true", UriKind.Relative);
                if (paramDict.ContainsKey("confirm_hash"))
                    Execute.ExecuteOnUIThread((Action)(() => VKRequestsDispatcher.DispatchRequestToVK<object>("account.validateAction", new Dictionary<string, string>()
          {
            {
              "confirm",
              MessageBox.Show(HttpUtility.UrlDecode(paramDict["confirm"]), CommonResources.VK, MessageBoxButton.OKCancel) == MessageBoxResult.OK ? "1" : "0"
            },
            {
              "hash",
              paramDict["confirm_hash"]
            }
          }, (Action<BackendResult<object, ResultCode>>)(e => { }), (Func<string, object>)null, false, true, new CancellationToken?())));
                ShareOperation shareOperation = (Application.Current as IAppStateInfo).ShareOperation;
                if (shareOperation != null)
                {
                    DataPackageView data = shareOperation.Data;
                    if (data.Contains(StandardDataFormats.StorageItems) || data.Contains(StandardDataFormats.WebLink) || data.Contains(StandardDataFormats.Text))
                    {
                        ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)new ShareExternalContentDataProvider(shareOperation));
                        return new Uri(NavigatorImpl.GetShareExternalContentpageNavStr(), UriKind.Relative);
                    }
                }
                if (originalString.Contains("ShareContent") && originalString.Contains("FileId"))
                {
                    this.SetChoosenPhoto(HttpUtility.UrlDecode(paramDict["FileId"]));
                    ParametersRepository.SetParameterForId("FromPhotoPicker", (object)true);
                    return new Uri(NavigatorImpl.GetNavToNewPostStr(0L, false, 0, false, false, false) + "&ClearBackStack=true", UriKind.Relative);
                }
                if (originalString.Contains("Action=WallPost") && paramDict.ContainsKey("PostId") && (paramDict.ContainsKey("OwnerId") && paramDict.ContainsKey("FocusComments")))
                {
                    if (flag1 && !paramDict.ContainsKey("type"))
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.other_app,
                            ReasonSubtype = "contacts"
                        });
                    long postId = long.Parse(paramDict["PostId"]);
                    long num1 = long.Parse(paramDict["OwnerId"]);
                    bool flag2 = bool.Parse(paramDict["FocusComments"]);
                    long num2 = long.Parse(paramDict["PollId"]);
                    long num3 = long.Parse(paramDict["PollOwnerId"]);
                    long ownerId = num1;
                    int num4 = flag2 ? 1 : 0;
                    long pollId = num2;
                    long pollOwnerId = num3;
                    string adData = "";
                    return new Uri(NavigatorImpl.GetNavigateToPostCommentsNavStr(postId, ownerId, num4 != 0, pollId, pollOwnerId, adData) + "&ClearBackStack=True", UriKind.Relative);
                }
                if (originalString.Contains("Action=ShowPhotos"))
                {
                    if (flag1 && !paramDict.ContainsKey("type"))
                        StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                        {
                            Reason = AppActivationReason.other_app,
                            ReasonSubtype = "contacts"
                        });
                    return new Uri(originalString.Replace("/VKClient.Common;component/NewsPage.xaml", "/VKClient.Common;component/ImageViewerBasePage.xaml") + "&ClearBackStack=True", UriKind.Relative);
                }
            }
            if (originalString.Contains("PeopleExtension"))
            {
                Dictionary<string, string> queryString = uri.ParseQueryString();
                if (flag1)
                    StatsEventsTracker.Instance.Handle(new AppActivatedEvent()
                    {
                        Reason = AppActivationReason.other_app,
                        ReasonSubtype = "contacts"
                    });
                if (queryString.ContainsKey("accountaction") && queryString["accountaction"] == "manage")
                    return new Uri(NavigatorImpl.GetNavigateToSettingsStr() + "?ClearBackStack=true", UriKind.Relative);
                if (queryString.ContainsKey("action"))
                {
                    string[] strArray = new string[0];
                    if (queryString.ContainsKey("contact_ids"))
                        strArray = queryString["contact_ids"].FromURL().Split(',');
                    string str = queryString["action"];
                    if (!(str == "Show_Contact"))
                    {
                        if (str == "Post_Update")
                            return new Uri(NavigatorImpl.GetNavToNewPostStr(0L, false, 0, false, false, false) + "&ClearBackStack=True", UriKind.Relative);
                    }
                    else
                    {
                        ((IEnumerable<string>)strArray).FirstOrDefault<string>();
                        long itemIdByRemoteId = RemoteIdHelper.GetItemIdByRemoteId(strArray[0]);
                        return new Uri((itemIdByRemoteId <= 0L ? NavigatorImpl.GetNavigateToGroupNavStr(-itemIdByRemoteId, "", true) : NavigatorImpl.GetNavigateToUserProfileNavStr(itemIdByRemoteId, "", true, "")) + "&ClearBackStack=True", UriKind.Relative);
                    }
                }
            }
            return uri;
        }

        private Uri MapProtocolLaunchUri(Uri uri)
        {
            string originalString = uri.OriginalString;
            Dictionary<string, string> parametersAsDict = QueryStringHelper.GetParametersAsDict(originalString);
            string str1 = "/VKClient.Common;component/NewsPage.xaml";
            string str2;
            if (parametersAsDict.ContainsKey("encodedLaunchUri"))
            {
                str2 = HttpUtility.UrlDecode(parametersAsDict["encodedLaunchUri"]);
                if (parametersAsDict.ContainsKey("sourceAppIdentifier"))
                {
                    string str3 = parametersAsDict["sourceAppIdentifier"];
                    if (str2.Contains("/?"))
                        str2 = str2.Replace("/?", "?");
                    string oldValue = "vkappconnect://authorize";
                    if (!str2.StartsWith(oldValue))
                    {
                        if (str2.StartsWith("vk://"))
                            return new Uri(NavigatorImpl.GetOpenUrlPageStr(str2.Replace("vk://", "https://vk.com/")), UriKind.Relative);
                        int num = (int)MessageBox.Show("Unsupported protocol: " + str2);
                        str2 = str1;
                    }
                    else if (str2.Contains("RedirectUri=vkc"))
                    {
                        int num = (int)MessageBox.Show("Unsupported redirect uri. Please, use the latest version of WP SDK.");
                        str2 = str1;
                    }
                    else
                        str2 = str2.Replace(oldValue, "/VKClient.Common;component/SDKAuthPage.xaml") + "&SDKGUID=" + str3.ToLowerInvariant();
                }
                else if (str2.StartsWith("fb128749580520227://authorize"))
                {
                    if (new FacebookUriMapper().MapUri(uri).OriginalString.StartsWith("/SuccessfulFacebookLogin.xaml"))
                        return new Uri("/VKClient.Common;component/FriendsImportFacebookPage.xaml", UriKind.Relative);
                }
                else if (str2.StartsWith("com.vk.vkclient:/gmail-oauth/code"))
                {
                    Dictionary<string, string> queryString = CustomUriMapper.ParseQueryString(str2);
                    if (queryString != null && queryString.ContainsKey("code"))
                    {
                        string str3 = queryString["code"];
                        if (!string.IsNullOrEmpty(str3))
                            return new Uri(string.Format("/VKClient.Common;component/FriendsImportGmailPage.xaml?code={0}", (object)str3), UriKind.Relative);
                    }
                }
                else if (str2.StartsWith("com.vk.vkclient://twitter-oauth/callback"))
                {
                    Dictionary<string, string> queryString = CustomUriMapper.ParseQueryString(str2);
                    if (queryString != null && queryString.ContainsKey("oauth_token") && queryString.ContainsKey("oauth_verifier"))
                    {
                        string str3 = queryString["oauth_token"];
                        string str4 = queryString["oauth_verifier"];
                        if (!string.IsNullOrEmpty(str3) && !string.IsNullOrEmpty(str4))
                            return new Uri(string.Format("/VKClient.Common;component/FriendsImportTwitterPage.xaml?oauthToken={0}&oauthVerifier={1}", (object)str3, (object)str4), UriKind.Relative);
                    }
                }
            }
            else
            {
                int num = (int)MessageBox.Show("Unable to identify source app or uri launch from uri " + originalString);
                str2 = str1;
            }
            return new Uri(str2, UriKind.Relative);
        }

        private static Dictionary<string, string> ParseQueryString(string uri)
        {//wtf
            //IEnumerable<string[]> source = ((IEnumerable<string>) uri.Substring(uri.LastIndexOf('?') == -1 ? 0 : uri.LastIndexOf('?') + 1).Split('&')).Select<string, string[]>((Func<string, string[]>) (piece => piece.Split('=')));
            //Func<string[], string> func = (Func<string[], string>) (pair => pair[0]);
            //Func<string[], string> keySelector;
            //return source.ToDictionary<string[], string, string>(keySelector, (Func<string[], string>) (pair => pair[1]));



            /*
            internal string[] <ParseQueryString>b__3_0(string piece)
            {
                return piece.Split('=');
            }
        
             internal string <ParseQueryString>b__3_1(string[] pair)
            {
                return pair[0];
            }
             * internal string <ParseQueryString>b__3_2(string[] pair)
    {
        return pair[1];
    }
             */



            IEnumerable<string> arg_4D_0 = uri.Substring((uri.LastIndexOf('?') == -1) ? 0 : (uri.LastIndexOf('?') + 1)).Split('&');
            Func<string, string[]> arg_4D_1 = new Func<string, string[]>(piece => piece.Split('='));

            IEnumerable<string[]> arg_90_0 = Enumerable.Select<string, string[]>(arg_4D_0, arg_4D_1);
            Func<string[], string> arg_90_1 = new Func<string[], string>(pair => pair[0]);
            Func<string[], string> arg_90_2 = new Func<string[], string>(pair => pair[1]);
            return Enumerable.ToDictionary<string[], string, string>(arg_90_0, arg_90_1, arg_90_2);
        }

        private void SetChoosenPhoto(string fileId)
        {
            using (MediaLibrary mediaLibrary = new MediaLibrary())
            {
                using (Picture pictureFromToken = mediaLibrary.GetPictureFromToken(fileId))
                {
                    ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", (object)new List<Stream>()
          {
            pictureFromToken.GetThumbnail()
          });
                    ParametersRepository.SetParameterForId("ChoosenPhotos", (object)new List<Stream>()
          {
            pictureFromToken.GetImage()
          });
                }
            }
        }
    }
}
