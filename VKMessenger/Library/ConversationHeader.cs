using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
    public class ConversationHeader : ViewModelBase, IBinarySerializable, IHandle<NotificationSettingsChangedEvent>, IHandle
    {
        private static readonly Thickness _onlineMargin = new Thickness(12.0, 0.0, 12.0, 0.0);
        private static readonly Thickness _onlineMobileMargin = new Thickness(12.0, 0.0, 20.0, 0.0);
        private static readonly Thickness _offlineMargin = new Thickness(12.0, 0.0, 0.0, 0.0);
        private static readonly Thickness _mutedMargin = new Thickness(12.0, 0.0, 24.0, 0.0);
        private static readonly Thickness _dateTextMarginUserThumb = new Thickness(0.0, 61.0, 12.0, 0.0);
        private static readonly Thickness _dateTextMarginNoUserThumb = new Thickness(0.0, 53.0, 12.0, 0.0);
        private string _uiTitle = string.Empty;
        private string _uiBody = string.Empty;
        private string _uiDate = string.Empty;
        private bool _isRead = true;
        private ConversationAvatarViewModel _conversationAvatarVM = new ConversationAvatarViewModel();
        public Message _message;
        public List<User> _associatedUsers;
        private int _unread;
        private bool _isOnline;
        private bool _isOnlineMobile;
        private bool _changingNotifications;

        public ConversationAvatarViewModel ConversationAvatarVM
        {
            get
            {
                return this._conversationAvatarVM;
            }
        }

        public int Unread
        {
            get
            {
                return this._unread;
            }
            set
            {
                this._unread = value;
                this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>)(() => this.Unread));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.HaveUnreadMessagesVisibility));
            }
        }

        public Visibility HaveUnreadMessagesVisibility
        {
            get
            {
                return this.Unread <= 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public SolidColorBrush HaveUnreadMessagesBackground
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources[(object)((!this.AreNotificationsDisabled ? "Phone" : "PhoneMuted") + "ConversationNewMessagesCountBackgroundBrush")];
            }
        }

        public string UITitle
        {
            get
            {
                return this._uiTitle;
            }
            set
            {
                if (!(this._uiTitle != value))
                    return;
                this._uiTitle = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UITitle));
            }
        }

        public string UIBody
        {
            get
            {
                return this._uiBody;
            }
            set
            {
                if (!(this._uiBody != value))
                    return;
                this._uiBody = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIBody));
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIBodyNoUserThumb));
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIBodyUserThumb));
            }
        }

        public string UIBodyNoUserThumb
        {
            get
            {
                if (this.NoUserThumbVisibility != Visibility.Visible)
                    return "";
                return this.UIBody;
            }
        }

        public string UIBodyUserThumb
        {
            get
            {
                if (this.UserThumbVisibility != Visibility.Visible)
                    return "";
                return this.UIBody;
            }
        }

        public string UIDate
        {
            get
            {
                return this._uiDate;
            }
            set
            {
                if (!(this._uiDate != value))
                    return;
                this._uiDate = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIDate));
            }
        }

        public bool IsRead
        {
            get
            {
                return this._isRead;
            }
            set
            {
                if (this._isRead == value)
                    return;
                this._isRead = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsRead));
                this.NotifyPropertyChanged<FontFamily>((System.Linq.Expressions.Expression<Func<FontFamily>>)(() => this.FontFamily));
                this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.TextForegroundBrush));
                this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.TextBackgroundBrush));
                this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.MainBackgroundBrush));
            }
        }

        public SolidColorBrush TextForegroundBrush
        {
            get
            {
                return !this.IsRead ? (SolidColorBrush)Application.Current.Resources["PhoneDialogsTextUnreadForegroundBrush"] : (SolidColorBrush)Application.Current.Resources["PhoneDialogsTextForegroundBrush"];
            }
        }

        public SolidColorBrush TextBackgroundBrush
        {
            get
            {
                return this._message.@out != 1 || this.IsRead ? (SolidColorBrush)Application.Current.Resources["PhoneDialogsBackgroundBrush"] : (SolidColorBrush)Application.Current.Resources["PhoneDialogsUnreadBackgroundBrush"];
            }
        }

        public SolidColorBrush MainBackgroundBrush
        {
            get
            {
                if (this._message.@out == 0 && !this.IsRead)
                    return (SolidColorBrush)Application.Current.Resources["PhoneDialogsUnreadBackgroundBrush"];
                return (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            }
        }

        public bool IsChat
        {
            get
            {
                return (uint)this._message.chat_id > 0U;
            }
        }

        public Visibility IsChatVisibility
        {
            get
            {
                return !this.IsChat || !string.IsNullOrEmpty(this._message.photo_200) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility IsNotChatVisibility
        {
            get
            {
                return this.IsChatVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public long UserOrChatId
        {
            get
            {
                return this.IsChat ? (long)this._message.chat_id : (long)this._message.uid;
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                if (!this.IsRead)
                    return new FontFamily("Segoe WP Semibold");
                return new FontFamily("Segoe WP");
            }
        }

        public Visibility NoUserThumbVisibility
        {
            get
            {
                return this.UserThumbVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility UserThumbVisibility
        {
            get
            {
                return !this.IsChat && this._message.@out != 1 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string UserThumb
        {
            get
            {
                if (this._message.@out == 1)
                    return AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max;
                if (this.IsChat)
                {
                    User user = this.User;
                    if (user != null)
                        return user.photo_max;
                }
                return "";
            }
        }

        public User User
        {
            get
            {
                return this._associatedUsers.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == (long)this._message.uid));
            }
        }

        public User User2
        {
            get
            {
                return this._associatedUsers.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == this._message.action_mid));
            }
        }

        public bool IsOnline
        {
            get
            {
                return this._isOnline;
            }
            set
            {
                if (this._isOnline == value)
                    return;
                this._isOnline = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsOnline));
                this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.IsOnlineOrOnlineMobileMargin));
                this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.TitleMargin));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsOnlineVisibility));
            }
        }

        public bool IsOnlineMobile
        {
            get
            {
                return this._isOnlineMobile;
            }
            set
            {
                if (this._isOnlineMobile == value)
                    return;
                this._isOnlineMobile = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsOnlineMobile));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsOnlineVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsOnlineMobileVisibility));
                this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.IsOnlineOrOnlineMobileMargin));
                this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.TitleMargin));
            }
        }

        public Visibility IsOnlineMobileVisibility
        {
            get
            {
                return !this.IsOnlineMobile ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Thickness IsOnlineOrOnlineMobileMargin
        {
            get
            {
                if (this.AreNotificationsDisabled)
                    return ConversationHeader._mutedMargin;
                if (this.IsOnlineMobile)
                    return ConversationHeader._onlineMobileMargin;
                if (this.IsOnline)
                    return ConversationHeader._onlineMargin;
                return ConversationHeader._offlineMargin;
            }
        }

        public Visibility IsOnlineVisibility
        {
            get
            {
                return !this.IsOnline || this.IsOnlineMobile ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Thickness DateTextMargin
        {
            get
            {
                if (this.UserThumbVisibility != Visibility.Visible)
                    return ConversationHeader._dateTextMarginNoUserThumb;
                return ConversationHeader._dateTextMarginUserThumb;
            }
        }

        public bool AreNotificationsDisabled
        {
            get
            {
                if (this._message != null && this.IsChat)
                    return this._message.push_settings.AreDisabledNow;
                return false;
            }
            set
            {
                int num = value ? -1 : 0;
                if (this._message.push_settings.disabled_until == num)
                    return;
                this._message.push_settings.disabled_until = num;
                this.RespondToSettingsChange();
            }
        }

        public Visibility NotificationsDisabledVisibility
        {
            get
            {
                return !this.AreNotificationsDisabled ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Thickness TitleMargin
        {
            get
            {
                if (this.AreNotificationsDisabled)
                    return new Thickness(0.0, 0.0, 22.0, 0.0);
                if (this.IsOnlineMobileVisibility == Visibility.Visible)
                    return new Thickness(0.0, 0.0, 17.0, 0.0);
                if (this.IsOnlineVisibility == Visibility.Visible)
                    return new Thickness(0.0, 0.0, 13.0, 0.0);
                return new Thickness();
            }
        }

        public ObservableCollection<MenuItemData> MenuItems
        {
            get
            {
                ObservableCollection<MenuItemData> observableCollection = new ObservableCollection<MenuItemData>();
                if (this.IsChat)
                {
                    string str = this.AreNotificationsDisabled ? CommonResources.TurnOnNotifications : CommonResources.TurnOffNotifications;
                    observableCollection.Add(new MenuItemData()
                    {
                        Tag = "disableEnable",
                        Title = str
                    });
                }
                observableCollection.Add(new MenuItemData()
                {
                    Tag = "delete",
                    Title = CommonResources.Conversation_Delete
                });
                return observableCollection;
            }
        }

        public bool HaveEmoji
        {
            get
            {
                return BrowserNavigationService.ContainsEmoji(this.UIBody);
            }
        }

        public ConversationHeader(Message message, List<User> associatedUsers, int unread)
            : this()
        {
            this.SetMessageAndUsers(message, associatedUsers);
            this._unread = unread;
        }

        public ConversationHeader()
        {
            EventAggregator.Current.Subscribe((object)this);
        }

        public void SetMessageAndUsers(Message message, List<User> associatedUsers)
        {
            this._message = message;
            this._associatedUsers = associatedUsers;
            if (this._message == null || this._associatedUsers == null)
                return;
            this.RefreshUIProperties(false);
        }

        public virtual void RefreshUIProperties(bool suppressBodyRefresh = false)
        {
            this.IsRead = this._message.read_state == 1;
            string defaultAvatar = "";
            List<long> chatParticipantsIds = new List<long>();
            if (this._message.chat_id != 0)
            {
                defaultAvatar = this._message.photo_200;
                chatParticipantsIds = this._message.chat_active_str.ParseCommaSeparated();
                this.IsOnline = false;
                this.IsOnlineMobile = false;
                this.UITitle = this._message.title;
                if (!suppressBodyRefresh)
                    this.UIBody = this.GetHeaderBody();
                this.UIDate = this.FormatUIDate(this._message.date);
            }
            else
            {
                User user = this._associatedUsers.Where<User>((Func<User, bool>)(u => u.uid == (long)this._message.uid)).FirstOrDefault<User>();
                if (user != null)
                {
                    this.UITitle = this.FormatTitleForUser(user);
                    defaultAvatar = user.photo_max;
                }
                else
                    this.UITitle = "user_id " + (object)this._message.uid;
                if (!suppressBodyRefresh)
                    this.UIBody = this.GetHeaderBody();
                this.UIDate = this.FormatUIDate(this._message.date);
            }
            this._conversationAvatarVM.Initialize(defaultAvatar, (uint)this._message.chat_id > 0U, chatParticipantsIds, this._associatedUsers);
        }

        public static string GetMessageHeaderText(Message message, User user, User user2)
        {
            if (!string.IsNullOrWhiteSpace(message.body))
                return message.body;
            if (!string.IsNullOrWhiteSpace(message.action))
                return SystemMessageTextHelper.GenerateText(message, user, user2, false);
            if (message.attachments != null && message.attachments.Count > 0)
            {
                Attachment firstAttachment = message.attachments.First<Attachment>();
                int num = message.attachments.Count<Attachment>((Func<Attachment, bool>)(a => a.type == firstAttachment.type));
                string lowerInvariant = firstAttachment.type.ToLowerInvariant();

                //uint stringHash = 0; PrivateImplementationDetails.ComputeStringHash(lowerInvariant);
                /*
              if (stringHash <= 2804296981U)
              {
                if (stringHash <= 2166822627U)
                {
                  if ((int) stringHash != 232457833)
                  {
                    if ((int) stringHash == -2128144669 && lowerInvariant == "photo")
                    {
                      if (num == 1)
                        return CommonResources.Conversations_OnePhoto;
                      if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourPhotosFrm, (object) num);
                      return string.Format(CommonResources.Conversations_FiveOrMorePhotosFrm, (object) num);
                    }
                  }
                  else if (lowerInvariant == "link")
                    return CommonResources.Link;
                }
                else if ((int) stringHash != -1972165393)
                {
                  if ((int) stringHash == -1490670315 && lowerInvariant == "wall")
                    return CommonResources.Conversation_WallPost;
                }
                else if (lowerInvariant == "gift")
                  return CommonResources.Gift;
              }
              else if (stringHash <= 3398065954U)
              {
                if ((int) stringHash != -951962596)
                {
                  if ((int) stringHash == -896901342 && lowerInvariant == "wall_reply")
                    return CommonResources.Comment;
                }
                else if (lowerInvariant == "sticker")
                  return CommonResources.Conversation_Sticker;
              }
              else if ((int) stringHash != -822539412)
              {
                if ((int) stringHash != -530499175)
                {
                  if ((int) stringHash == -362233003 && lowerInvariant == "doc")
                  {
                    if (num == 1)
                      return CommonResources.Conversations_OneDocument;
                    if (num < 5)
                      return string.Format(CommonResources.Conversations_TwoFourDocumentsFrm, (object) num);
                    return string.Format(CommonResources.Conversations_FiveMoreDocumentsFrm, (object) num);
                  }
                }
                else if (lowerInvariant == "audio")
                {
                  if (num == 1)
                    return CommonResources.Conversations_OneAudio;
                  if (num < 5)
                    return string.Format(CommonResources.Conversations_TwoFourAudioFrm, (object) num);
                  return string.Format(CommonResources.Conversations_FiveOrMoreAudioFrm, (object) num);
                }
              }
              else if (lowerInvariant == "video")
              {
                if (num == 1)
                  return CommonResources.Conversations_OneVideo;
                if (num < 5)
                  return string.Format(CommonResources.Conversations_TwoFourVideosFrm, (object) num);
                return string.Format(CommonResources.Conversations_FiveOrMoreVideosFrm, (object) num);
              }*/
                if (lowerInvariant == "photo")
                {
                    if (num == 1)
                        return CommonResources.Conversations_OnePhoto;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourPhotosFrm, (object)num);
                    return string.Format(CommonResources.Conversations_FiveOrMorePhotosFrm, (object)num);
                }
                else if (lowerInvariant == "link")
                    return CommonResources.Link;
                else if (lowerInvariant == "wall")
                    return CommonResources.Conversation_WallPost;
                else if (lowerInvariant == "gift")
                    return CommonResources.Gift;
                else if (lowerInvariant == "wall_reply")
                    return CommonResources.Comment;
                else if (lowerInvariant == "sticker")
                    return CommonResources.Conversation_Sticker;
                else if (lowerInvariant == "doc")
                {
                    Doc doc = firstAttachment.doc;//NEW: 4.8.0
                    if ((doc != null ? (doc.IsGraffiti ? 1 : 0) : 0) != 0)
                        return CommonResources.Graffiti;

                    if (num == 1)
                        return CommonResources.Conversations_OneDocument;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourDocumentsFrm, num);
                    return string.Format(CommonResources.Conversations_FiveMoreDocumentsFrm, num);
                }
                else if (lowerInvariant == "audio")
                {
                    if (num == 1)
                        return CommonResources.Conversations_OneAudio;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourAudioFrm, num);
                    return string.Format(CommonResources.Conversations_FiveOrMoreAudioFrm, num);
                }
                else if (lowerInvariant == "video")
                {
                    if (num == 1)
                        return CommonResources.Conversations_OneVideo;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourVideosFrm, num);
                    return string.Format(CommonResources.Conversations_FiveOrMoreVideosFrm, num);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ConversationHeader.GetMessageHeaderText " + lowerInvariant);
                }
            }
            if (message.geo != null)
                return CommonResources.Conversations_Location;
            if (message.fwd_messages == null || message.fwd_messages.Count <= 0)
                return string.Empty;
            int count = message.fwd_messages.Count;
            if (count == 1)
                return CommonResources.Conversations_OneForwardedMessage;
            if (count < 5)
                return string.Format(CommonResources.Conversations_TwoFourForwardedMessagesFrm, (object)count);
            return string.Format(CommonResources.Conversations_FiveMoreForwardedMessagesFrm, (object)count);
        }

        private string GetHeaderBody()
        {
            return ConversationHeader.GetMessageHeaderText(this._message, this.User, this.User2);
        }

        protected string FormatUIDate(int unixDateTime)
        {
            return UIStringFormatterHelper.FormatDateForUIShort(Extensions.UnixTimeStampToDateTime((double)unixDateTime, true));
        }

        protected virtual string FormatTitleForUser(User user)
        {
            string str = string.Format("{0} {1}", (object)user.first_name, (object)user.last_name);
            this.IsOnline = (uint)user.online > 0U;
            this.IsOnlineMobile = (uint)user.online_mobile > 0U;
            return str;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write<Message>(this._message, false);
            writer.WriteList<User>((IList<User>)this._associatedUsers, 10000);
            writer.Write(this._unread);
        }

        public void Read(BinaryReader reader)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            reader.ReadInt32();
            this._message = reader.ReadGeneric<Message>();
            long elapsedMilliseconds1 = stopwatch.ElapsedMilliseconds;
            this._associatedUsers = reader.ReadList<User>();
            this._unread = reader.ReadInt32();
            long elapsedMilliseconds2 = stopwatch.ElapsedMilliseconds;
            this.RefreshUIProperties(false);
            long elapsedMilliseconds3 = stopwatch.ElapsedMilliseconds;
        }

        internal bool Matches(List<string> query)
        {
            bool flag = false;
            foreach (string str in query)
            {
                string searchTerm = str;
                flag = ((IEnumerable<string>)this.UITitle.ToLowerInvariant().Split(' ')).Any<string>((Func<string, bool>)(s => s.StartsWith(searchTerm.ToLowerInvariant())));
                if (!flag)
                {
                    flag = ((IEnumerable<string>)this.UIBody.ToLowerInvariant().Split(' ')).Any<string>((Func<string, bool>)(s => s.StartsWith(searchTerm.ToLowerInvariant())));
                    if (flag)
                        break;
                }
                else
                    break;
            }
            return flag;
        }

        public void DisableEnableNotifications(Action<bool> resultCallback)
        {
            if (this._changingNotifications)
                return;
            this._changingNotifications = true;
            string notificationsUri = AppGlobalStateManager.Current.GlobalState.NotificationsUri;
            if (!string.IsNullOrEmpty(notificationsUri))
            {
                AccountService.Instance.SetSilenceMode(notificationsUri, this.AreNotificationsDisabled ? 0 : -1, (Action<BackendResult<object, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    this._changingNotifications = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                    resultCallback(res.ResultCode == ResultCode.Succeeded);
                }))), this.IsChat ? this.UserOrChatId : 0L, !this.IsChat ? this.UserOrChatId : 0L);
            }
            else
            {
                this._changingNotifications = false;
                this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                resultCallback(true);
            }
        }

        internal void RespondToSettingsChange()
        {
            this.NotifyPropertyChanged<ObservableCollection<MenuItemData>>((System.Linq.Expressions.Expression<Func<ObservableCollection<MenuItemData>>>)(() => this.MenuItems));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.NotificationsDisabledVisibility));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.HaveUnreadMessagesBackground));
            this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.IsOnlineOrOnlineMobileMargin));
            this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.TitleMargin));
            EventAggregator current = EventAggregator.Current;
            NotificationSettingsChangedEvent settingsChangedEvent = new NotificationSettingsChangedEvent();
            int num = this.AreNotificationsDisabled ? 1 : 0;
            settingsChangedEvent.AreNotificationsDisabled = num != 0;
            long userOrChatId = this.UserOrChatId;
            settingsChangedEvent.ChatId = userOrChatId;
            current.Publish((object)settingsChangedEvent);
        }

        public void Handle(NotificationSettingsChangedEvent message)
        {
            if (message.ChatId != this.UserOrChatId || !this.IsChat)
                return;
            this.AreNotificationsDisabled = message.AreNotificationsDisabled;
        }
    }
}
