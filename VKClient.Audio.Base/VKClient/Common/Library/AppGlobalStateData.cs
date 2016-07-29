using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class AppGlobalStateData : IBinarySerializable
    {
        private object _lockObj = new object();
        private bool _needReferchStickers = true;
        public DateTime _lastTimeShownBDNotification = DateTime.MinValue;
        private bool _showBirthdaysNotifications = true;
        private DateTime _lastDeactTime = DateTime.MinValue;
        private Dictionary<string, List<long>> _uidToListDisabledUidsParsed = new Dictionary<string, List<long>>();
        private Dictionary<string, List<long>> _uidToListDisabledChatIdsParsed = new Dictionary<string, List<long>>();
        private long _maxMessageId;
        //private string _accessToken;
        private bool _gifAutoplayAvailable;

        public GifAutoplayMode GifAutoplayType { get; set; }

        public List<PendingStatisticsEvent> PendingStatisticsEvents { get; set; }

        public List<StoreProduct> Stickers { get; set; }

        public List<StockItem> StickersStockItems { get; set; }

        public string SupportUri { get; set; }

        public string DeviceId
        {
            get
            {
                return Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId"));
            }
        }

        public bool NeedRefetchStickers
        {
            get
            {
                return this._needReferchStickers;
            }
            set
            {
                this._needReferchStickers = value;
            }
        }

        public bool GamesSectionEnabled { get; set; }

        public string AccessToken { get; set; }

        public string Secret { get; set; }

        public long LoggedInUserId { get; set; }

        public DateTime LastTimeShownBSNotification
        {
            get
            {
                return this._lastTimeShownBDNotification;
            }
            set
            {
                this._lastTimeShownBDNotification = value;
            }
        }

        public bool ShowBirthdaysNotifications
        {
            get
            {
                return this._showBirthdaysNotifications;
            }
            set
            {
                this._showBirthdaysNotifications = value;
            }
        }

        public DateTime PushNotificationsBlockedUntil { get; set; }

        public User LoggedInUser { get; set; }

        public bool SyncContacts { get; set; }

        public long MaxMessageId
        {
            get
            {
                return this._maxMessageId;
            }
            set
            {
                lock (this._lockObj)
                {
                    if (value <= this._maxMessageId)
                        return;
                    this._maxMessageId = value;
                }
            }
        }

        public int TipsShownCount { get; set; }

        public string NotificationsUri { get; set; }

        public long LastTS { get; set; }

        public bool VibrationsEnabled { get; set; }

        public bool SoundEnabled { get; set; }

        public bool NotificationsEnabled { get; set; }

        public int ServerMinusLocalTimeDelta { get; set; }

        public bool MessageNotificationsEnabled { get; set; }

        public bool FriendsNotificationsEnabled { get; set; }

        public bool MentionsNotificationsEnabled { get; set; }

        public bool ReplyNotificationsEnabled { get; set; }

        public bool MessageTextInNotification { get; set; }

        public bool PushNotificationsEnabled { get; set; }

        public PushSettings PushSettings { get; set; }

        public int FavoritesDefaultSection { get; set; }

        public string DefaultVideoResolution { get; set; }

        public string RegisteredDeviceId { get; set; }

        public bool AllowToastNotificationsQuestionAsked { get; set; }

        public bool AllowUseLocationQuestionAsked { get; set; }

        public bool AllowUseLocation { get; set; }

        public bool AllowSendContacts { get; set; }

        public PickableItem SelectedNewsSource { get; set; }

        public int FriendListOrder { get; set; }

        public bool CompressPhotosOnUpload { get; set; }

        public bool SaveLocationDataOnUpload { get; set; }

        public bool SaveEditedPhotos { get; set; }

        public bool LoadBigPhotosOverMobile { get; set; }

        public bool IsMusicCachingEnabled { get; set; }

        public GamesVisitSource GamesVisitSource { get; set; }

        public List<long> MyGamesIds { get; set; }

        public string BaseDomain { get; set; }

        public string BaseLoginDomain { get; set; }

        public bool ForceStatsSend { get; set; }

        public bool NewsfeedTopEnabled { get; set; }

        private bool CanUseInApps { get; set; }

        public bool StickersAutoSuggestEnabled { get; set; }

        public AccountPaymentType PaymentType { get; set; }

        public int NewStoreItemsCount { get; set; }

        public bool HasStickersUpdates { get; set; }

        public StoreStickers RecentStickers { get; set; }

        public DateTime LastDeactivatedTime
        {
            get
            {
                return this._lastDeactTime;
            }
            set
            {
                this._lastDeactTime = value;
            }
        }

        public bool GifAutoplayFeatureAvailable
        {
            get
            {
                if (this.GifAutoplayManualSetting.HasValue)
                    return this.GifAutoplayManualSetting.Value;
                return this._gifAutoplayAvailable;
            }
            set
            {
                this._gifAutoplayAvailable = value;
            }
        }

        public bool? GifAutoplayManualSetting { get; set; }

        public bool PhotoFeedMoveHintShown { get; set; }

        public AppGlobalStateData()
        {
            this.GifAutoplayType = GifAutoplayMode.Always;

            this.LoggedInUser = new User();
            this.SoundEnabled = true;
            this.VibrationsEnabled = true;
            this.NotificationsEnabled = true;
            this.SyncContacts = true;
            this.PendingStatisticsEvents = new List<PendingStatisticsEvent>();
            this.CompressPhotosOnUpload = true;
            this.SaveEditedPhotos = false;
            this.LoadBigPhotosOverMobile = true;
            this.IsMusicCachingEnabled = true;
            this.SaveLocationDataOnUpload = true;
            this.PushSettings = new PushSettings();
            this.FavoritesDefaultSection = 0;
            this.DefaultVideoResolution = "360";
            this.StickersAutoSuggestEnabled = true;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(29);
            writer.WriteString(this.AccessToken);
            writer.Write(this.LoggedInUserId);
            writer.Write(this.MaxMessageId);
            writer.Write(this.LastTS);
            writer.WriteString(this.NotificationsUri);
            writer.Write(this.PushNotificationsBlockedUntil);
            writer.Write(this.VibrationsEnabled);
            writer.Write(this.SoundEnabled);
            writer.Write(this.NotificationsEnabled);
            writer.WriteString(this.Secret);
            writer.Write(this.MessageNotificationsEnabled);
            writer.Write(this.AllowUseLocationQuestionAsked);
            writer.Write(this.AllowUseLocation);
            writer.Write<User>(this.LoggedInUser, false);
            writer.Write(this.AllowToastNotificationsQuestionAsked);
            writer.Write(this.FriendsNotificationsEnabled);
            writer.Write(this.MentionsNotificationsEnabled);
            writer.Write(this.ReplyNotificationsEnabled);
            writer.Write(this.MessageTextInNotification);
            writer.Write(this.ServerMinusLocalTimeDelta);
            writer.Write(this.LastDeactivatedTime);
            writer.WriteDictionary(this.ConvertToDictStringString(this._uidToListDisabledUidsParsed));
            writer.WriteDictionary(this.ConvertToDictStringString(this._uidToListDisabledChatIdsParsed));
            writer.Write<PickableItem>(this.SelectedNewsSource, false);
            writer.Write(this.SyncContacts);
            writer.WriteList<PendingStatisticsEvent>((IList<PendingStatisticsEvent>)this.PendingStatisticsEvents, 10000);
            writer.WriteList<StoreProduct>((IList<StoreProduct>)this.Stickers, 10000);
            writer.Write(this.LastTimeShownBSNotification);
            writer.Write(this.ShowBirthdaysNotifications);
            writer.WriteString(this.SupportUri);
            writer.Write(this.TipsShownCount);
            writer.Write(this.FriendListOrder);
            writer.Write(this.CompressPhotosOnUpload);
            writer.Write(this.SaveEditedPhotos);
            writer.Write(this.LoadBigPhotosOverMobile);
            writer.Write(this.IsMusicCachingEnabled);
            writer.Write(this.SaveLocationDataOnUpload);
            writer.Write<PushSettings>(this.PushSettings, false);
            writer.WriteString(this.RegisteredDeviceId);
            writer.Write(this.PushNotificationsEnabled);
            writer.Write((int)this.GamesVisitSource);
            writer.WriteList(this.MyGamesIds);
            writer.Write(this.FavoritesDefaultSection);
            writer.Write(this.AllowSendContacts);
            writer.Write(this.GamesSectionEnabled);
            writer.Write(this.DefaultVideoResolution);
            writer.WriteString(this.BaseDomain);
            writer.WriteString(this.BaseLoginDomain);
            writer.Write(this.ForceStatsSend);
            writer.Write(this.NewsfeedTopEnabled);
            writer.Write((int)this.GifAutoplayType);
            writer.Write(this._gifAutoplayAvailable);
            writer.WriteBoolNullable(this.GifAutoplayManualSetting);
            writer.Write(this.CanUseInApps);
            writer.Write(this.StickersAutoSuggestEnabled);
            writer.WriteList<StockItem>((IList<StockItem>)this.StickersStockItems, 10000);
            writer.Write((int)this.PaymentType);
            writer.Write(this.NewStoreItemsCount);
            writer.Write(this.HasStickersUpdates);
            writer.Write(this.PhotoFeedMoveHintShown);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.AccessToken = reader.ReadString();
            this.LoggedInUserId = reader.ReadInt64();
            this.MaxMessageId = reader.ReadInt64();
            this.LastTS = reader.ReadInt64();
            this.NotificationsUri = reader.ReadString();
            this.PushNotificationsBlockedUntil = reader.ReadDateTime();
            this.VibrationsEnabled = reader.ReadBoolean();
            this.SoundEnabled = reader.ReadBoolean();
            this.NotificationsEnabled = reader.ReadBoolean();
            this.Secret = reader.ReadString();
            this.MessageNotificationsEnabled = reader.ReadBoolean();
            this.AllowUseLocationQuestionAsked = reader.ReadBoolean();
            this.AllowUseLocation = reader.ReadBoolean();
            this.LoggedInUser = reader.ReadGeneric<User>();
            this.AllowToastNotificationsQuestionAsked = reader.ReadBoolean();
            this.FriendsNotificationsEnabled = reader.ReadBoolean();
            this.MentionsNotificationsEnabled = reader.ReadBoolean();
            this.ReplyNotificationsEnabled = reader.ReadBoolean();
            this.MessageTextInNotification = reader.ReadBoolean();
            this.ServerMinusLocalTimeDelta = reader.ReadInt32();
            this.LastDeactivatedTime = reader.ReadDateTime();
            int num2 = 2;
            if (num1 >= num2)
            {
                this._uidToListDisabledUidsParsed = this.ConvertToDictStringListLong(reader.ReadDictionary());
                this._uidToListDisabledChatIdsParsed = this.ConvertToDictStringListLong(reader.ReadDictionary());
            }
            int num3 = 3;
            if (num1 >= num3)
                this.SelectedNewsSource = reader.ReadGeneric<PickableItem>();
            int num4 = 4;
            if (num1 >= num4)
                this.SyncContacts = reader.ReadBoolean();
            int num5 = 5;
            if (num1 >= num5)
                this.PendingStatisticsEvents = reader.ReadList<PendingStatisticsEvent>();
            int num6 = 6;
            if (num1 >= num6)
                this.Stickers = reader.ReadList<StoreProduct>();
            int num7 = 7;
            if (num1 >= num7)
            {
                this.LastTimeShownBSNotification = reader.ReadDateTime();
                this.ShowBirthdaysNotifications = reader.ReadBoolean();
            }
            int num8 = 8;
            if (num1 >= num8)
                this.SupportUri = reader.ReadString();
            int num9 = 9;
            if (num1 >= num9)
                this.TipsShownCount = reader.ReadInt32();
            int num10 = 10;
            if (num1 >= num10)
            {
                this.FriendListOrder = reader.ReadInt32();
                this.CompressPhotosOnUpload = reader.ReadBoolean();
                this.SaveEditedPhotos = reader.ReadBoolean();
                this.LoadBigPhotosOverMobile = reader.ReadBoolean();
                this.IsMusicCachingEnabled = reader.ReadBoolean();
            }
            int num11 = 11;
            if (num1 >= num11)
                this.SaveLocationDataOnUpload = reader.ReadBoolean();
            bool flag = false;
            int num12 = 12;
            if (num1 >= num12)
            {
                flag = true;
                this.PushSettings = reader.ReadGeneric<PushSettings>();
                this.RegisteredDeviceId = reader.ReadString();
                this.PushNotificationsEnabled = reader.ReadBoolean();
            }
            if (!flag)
                this.MigratePushSettingsFromOldSettings();
            int num13 = 13;
            if (num1 >= num13)
            {
                this.GamesVisitSource = (GamesVisitSource)reader.ReadInt32();
                this.MyGamesIds = reader.ReadListLong();
            }
            int num14 = 14;
            if (num1 >= num14)
                this.FavoritesDefaultSection = reader.ReadInt32();
            int num15 = 15;
            if (num1 >= num15)
                this.AllowSendContacts = reader.ReadBoolean();
            int num16 = 16;
            if (num1 >= num16)
                this.GamesSectionEnabled = reader.ReadBoolean();
            int num17 = 17;
            if (num1 >= num17)
                this.DefaultVideoResolution = reader.ReadString();
            int num18 = 18;
            if (num1 >= num18)
            {
                this.BaseDomain = reader.ReadString();
                this.BaseLoginDomain = reader.ReadString();
            }
            int num19 = 19;
            if (num1 >= num19)
                this.ForceStatsSend = reader.ReadBoolean();
            int num20 = 20;
            if (num1 >= num20)
                this.NewsfeedTopEnabled = reader.ReadBoolean();
            int num21 = 21;
            if (num1 >= num21)
                this.GifAutoplayType = (GifAutoplayMode)reader.ReadInt32();
            int num22 = 22;
            if (num1 >= num22)
                this._gifAutoplayAvailable = reader.ReadBoolean();
            int num23 = 23;
            if (num1 >= num23)
                this.GifAutoplayManualSetting = reader.ReadBoolNullable();
            int num24 = 24;
            if (num1 >= num24)
                this.CanUseInApps = reader.ReadBoolean();
            int num25 = 25;
            if (num1 >= num25)
                this.StickersAutoSuggestEnabled = reader.ReadBoolean();
            int num26 = 26;
            if (num1 >= num26)
            {
                this.StickersStockItems = reader.ReadList<StockItem>();
                this.PaymentType = (AccountPaymentType)reader.ReadInt32();
            }
            int num27 = 27;
            if (num1 >= num27)
            {
                this.NewStoreItemsCount = reader.ReadInt32();
                this.HasStickersUpdates = reader.ReadBoolean();
            }
            int num28 = 29;
            if (num1 < num28)
                return;
            this.PhotoFeedMoveHintShown = reader.ReadBoolean();
        }

        private void MigratePushSettingsFromOldSettings()
        {
            PushSettings pushSettings = new PushSettings();
            pushSettings.msg = pushSettings.chat = this.MessageNotificationsEnabled;
            pushSettings.msg_no_text = pushSettings.chat_no_text = !this.MessageTextInNotification;
            pushSettings.friend = this.FriendsNotificationsEnabled;
            pushSettings.mention = this.MentionsNotificationsEnabled;
            pushSettings.reply = this.ReplyNotificationsEnabled;
            this.PushSettings = pushSettings;
            this.PushNotificationsEnabled = pushSettings.msg || pushSettings.chat || (pushSettings.friend || pushSettings.mention) || pushSettings.reply;
            if (!this.PushNotificationsEnabled)
                return;
            pushSettings.comment = true;
            pushSettings.event_soon = true;
            pushSettings.event_soon = true;
            pushSettings.friend_accepted = true;
            pushSettings.friend_found = true;
            pushSettings.group_accepted = true;
            pushSettings.group_invite = true;
            pushSettings.like = true;
            pushSettings.new_post = true;
            pushSettings.reply = true;
            pushSettings.repost = true;
            pushSettings.wall_post = true;
            pushSettings.wall_publish = true;
        }

        private Dictionary<string, string> ConvertToDictStringString(Dictionary<string, List<long>> dict)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string key in dict.Keys)
                dictionary[key] = dict[key].GetCommaSeparated();
            return dictionary;
        }

        private Dictionary<string, List<long>> ConvertToDictStringListLong(Dictionary<string, string> dict)
        {
            Dictionary<string, List<long>> dictionary = new Dictionary<string, List<long>>();
            foreach (string key in dict.Keys)
                dictionary[key] = dict[key].ParseCommaSeparated();
            return dictionary;
        }

        internal void ResetForNewUser()
        {
            this.AccessToken = "";
            this.LoggedInUserId = 0L;
            this.MaxMessageId = 0L;
            this.LastTS = 0L;
            this.NotificationsUri = "";
            this.PushNotificationsBlockedUntil = DateTime.MinValue;
            this.Secret = "";
            this.LoggedInUser = new User();
            this.SelectedNewsSource = (PickableItem)null;
            this.LastTimeShownBSNotification = DateTime.MinValue;
            this.PendingStatisticsEvents.Clear();
            this.SupportUri = "";
        }
    }
}
