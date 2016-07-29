using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Backend;
using VKMessenger.Utils;

namespace VKMessenger.Library
{
    public class MessageViewModel : ViewModelBase, IBinarySerializable
    {
        private static readonly Thickness _ownMessageMargin = new Thickness(96.0, 10.0, 0.0, 9.0);
        private static readonly Thickness _replyMessageMargin = new Thickness(0.0, 10.0, 96.0, 9.0);
        private static readonly Thickness _replyChatMessageMargin = new Thickness(0.0, 10.0, 36.0, 9.0);
        private static readonly Thickness _ownMessageStickerMargin = new Thickness(136.0, 10.0, 0.0, 9.0);
        private static readonly Thickness _replyMessageStickerMargin = new Thickness(0.0, 10.0, 136.0, 9.0);
        private static readonly Thickness _replyChatMessageStickerMargin = new Thickness(0.0, 10.0, 76.0, 9.0);
        private ObservableCollection<MessageViewModel> _forwardedMessages = new ObservableCollection<MessageViewModel>();
        private ObservableCollection<AttachmentViewModel> _attachments = new ObservableCollection<AttachmentViewModel>();
        private string _uiMessageText = string.Empty;
        private string _uiMessageDate = string.Empty;
        private string _uiImageUrl = string.Empty;
        private string _uiUserName = string.Empty;
        private Message _message = new Message();
        private User _associatedUser = new User();
        private User _associatedUser2 = new User();
        private double _uiOpacity = 1.0;
        private Thickness _ownMessageSelectionMarkMargin = new Thickness(60.0, 20.0, 0.0, 0.0);
        private Thickness _replySelectionMarkMargin = new Thickness(0.0, 20.0, 60.0, 0.0);
        private Thickness _chatReplySelectionMarkMargin = new Thickness(0.0, 20.0, 0.0, 0.0);
        private bool _isInSelectionMode;
        private bool _isSelected;
        private bool _isForwarded;
        private OutboundMessageViewModel _outboundMessage;
        private long _typingUserId;
        private User _typingUser;
        private bool _typingInChat;
        private bool _isUploading;

        public bool HaveForwardedMessages
        {
            get
            {
                return this.ForwardedMessages.Count > 0;
            }
        }

        public Visibility HaveForwardedMessagesVisivility
        {
            get
            {
                return this.HaveForwardedMessages ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public User AssociatedUser
        {
            get
            {
                return this._associatedUser;
            }
        }

        public User AssociatedUser2
        {
            get
            {
                return this._associatedUser2;
            }
        }

        public MessageDirectionType MessageDirectionType
        {
            get
            {
                if (!this.IsResponse)
                    return !this.IsChat ? MessageDirectionType.OutToUser : MessageDirectionType.OutToChat;
                return !this.IsChat ? MessageDirectionType.InFromUser : MessageDirectionType.InFromUserInChat;
            }
        }

        public string StatusImage
        {
            get
            {
                string str = "";
                if (this.UIStatusDelivered == Visibility.Visible)
                    str = this._message.read_state != 0 ? "message.state.read.png" : "message.state.sent.png";
                else if (this.SendStatus == OutboundMessageStatus.SendingNow)
                    str = "message.state.sending.png";
                if (str != "")
                    return MultiResolutionHelper.Instance.AppendResolutionSuffix("/VKClient.Common;component/Resources/" + str, false, "");
                return str;
            }
        }

        public string ForwardedMessagesHeaderText
        {
            get
            {
                if (!this.HaveForwardedMessages)
                    return string.Empty;
                if (this.ForwardedMessages.Count == 1)
                    return CommonResources.Conversation_ForwardedMessage;
                return CommonResources.Conversation_ForwardedMessages;
            }
        }

        public ObservableCollection<AttachmentViewModel> Attachments
        {
            get
            {
                return this._attachments;
            }
        }

        public string UIUserName
        {
            get
            {
                return this._uiUserName;
            }
            set
            {
                this._uiUserName = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIUserName));
            }
        }

        public string UIMessageText
        {
            get
            {
                return this._uiMessageText;
            }
            set
            {
                this._uiMessageText = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIMessageText));
            }
        }

        public Message Message
        {
            get
            {
                return this._message;
            }
        }

        public ObservableCollection<MessageViewModel> ForwardedMessages
        {
            get
            {
                return this._forwardedMessages;
            }
        }

        public SolidColorBrush TextBrush
        {
            get
            {
                if (this.StickerAttachment == null && this.GraffitiAttachment == null)
                    return new SolidColorBrush(Colors.White);
                return VKConstants.GrayColorHex.GetColor();
            }
        }

        public double TextOpacity
        {
            get
            {
                return this.StickerAttachment == null ? 0.6 : 1.0;
            }
        }

        public string UIDate
        {
            get
            {
                return this._uiMessageDate;
            }
            set
            {
                this._uiMessageDate = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIDate));
            }
        }

        public SolidColorBrush BGBrush
        {
            get
            {
                IEnumerable<AttachmentViewModel> arg_25_0 = this.Attachments;
                Func<AttachmentViewModel, bool> arg_25_1 = new Func<AttachmentViewModel, bool>(a =>
                {
                    if (a.AttachmentType == AttachmentType.Sticker)
                        return true;
                    if (a.AttachmentType != AttachmentType.Document)
                        return false;
                    Attachment attachment = a.Attachment;
                    bool? nullable;
                    if (attachment == null)
                    {
                        nullable = new bool?();
                    }
                    else
                    {
                        Doc doc = attachment.doc;
                        nullable = doc != null ? new bool?(doc.IsGraffiti) : new bool?();
                    }
                    return nullable ?? false;
                });

                if (Enumerable.Any<AttachmentViewModel>(arg_25_0, arg_25_1))
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
                if (this._message.@out == 1)
                {
                    return (SolidColorBrush)Application.Current.Resources["PhoneDialogOutMessageBackgroundBrush"];
                }
                return (SolidColorBrush)Application.Current.Resources["PhoneDialogInMessageBackgroundBrush"];
            }
        }

        public double UIOpacity
        {
            get
            {
                return this._uiOpacity;
            }
            private set
            {
                this._uiOpacity = value;
                this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.UIOpacity));
                this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.BGBrush));
            }
        }

        public bool IsResponse
        {
            get
            {
                return this._message.@out == 0;
            }
        }

        public Visibility IsNotResponseVisibility
        {
            get
            {
                return this.IsResponse ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Thickness ForwardedMargin
        {
            get
            {
                if (string.IsNullOrEmpty(this.UIMessageText))
                    return new Thickness(11.0, -24.0, 11.0, 0.0);
                return new Thickness(11.0, 0.0, 11.0, 0.0);
            }
        }

        public Visibility UIHaveMessageText
        {
            get
            {
                return string.IsNullOrEmpty(this.UIMessageText) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility HaveAttachments
        {
            get
            {
                return this.Attachments.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public int MessageInOrOutType
        {
            get
            {
                if (this.IsTetATetResponse)
                    return 1;
                return this.IsChatResponse ? 2 : 0;
            }
        }

        public Thickness MessageInOrOutTypeMargin
        {
            get
            {
                if (this._message.attachments != null)
                {
                    IEnumerable<Attachment> attachments = this._message.attachments;
                    Func<Attachment, bool> arg_37_1 = new Func<Attachment, bool>(a =>
                      {
                          if (a.type == "sticker")
                              return true;
                          if (!(a.type == "doc"))
                              return false;
                          Doc doc = a.doc;
                          if (doc == null)
                              return false;
                          return doc.IsGraffiti;
                      });



                    if (attachments.Any<Attachment>(arg_37_1))
                    {
                        switch (this.MessageInOrOutType)
                        {
                            case 1:
                                return MessageViewModel._replyMessageStickerMargin;
                            case 2:
                                return MessageViewModel._replyChatMessageStickerMargin;
                            default:
                                return MessageViewModel._ownMessageStickerMargin;
                        }
                    }
                }
                switch (this.MessageInOrOutType)
                {
                    case 1:
                        return MessageViewModel._replyMessageMargin;
                    case 2:
                        return MessageViewModel._replyChatMessageMargin;
                    default:
                        return MessageViewModel._ownMessageMargin;
                }
            }
        }

        public bool IsSticker
        {
            get
            {
                return this.StickerAttachment != null;
            }
        }

        public AttachmentViewModel StickerAttachment
        {
            get
            {
                return this.Attachments.FirstOrDefault<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Sticker));
            }
        }

        // NEW: 4.8.0
        public bool IsGraffiti
        {
            get
            {
                return this.GraffitiAttachment != null;
            }
        }

        public AttachmentViewModel GraffitiAttachment
        {
            get
            {
                return this.Attachments.FirstOrDefault<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Document));
            }
        }

        public bool IsChat
        {
            get
            {
                return (uint)this._message.chat_id > 0U;
            }
        }

        public bool IsTetATetResponse
        {
            get
            {
                if (this._message.@out == 0)
                    return !this.IsChat;
                return false;
            }
        }

        public Visibility IsTetATetResponseVisibility
        {
            get
            {
                return this.IsTetATetResponse ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsChatResponse
        {
            get
            {
                if (this._message.@out == 0)
                    return this.IsChat;
                return false;
            }
        }

        public Visibility IsChatResponseVisibility
        {
            get
            {
                return this.IsChatResponse ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string UIImageUrl
        {
            get
            {
                return this._uiImageUrl;
            }
            set
            {
                this._uiImageUrl = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UIImageUrl));
            }
        }

        public OutboundMessageStatus SendStatus
        {
            get
            {
                if (this._outboundMessage == null)
                    return OutboundMessageStatus.NotSent;
                return this._outboundMessage.OutboundMessageStatus;
            }
        }

        public int UserOrChatId
        {
            get
            {
                if (!this.IsChat)
                    return this.Message.uid;
                return this.Message.chat_id;
            }
        }

        public Visibility UIStatusFailed
        {
            get
            {
                return this._outboundMessage != null && this._message.@out == 1 && this._outboundMessage.OutboundMessageStatus == OutboundMessageStatus.Failed ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility UIStatusDelivered
        {
            get
            {
                return this._message.@out == 1 && (this._outboundMessage == null || this._outboundMessage.OutboundMessageStatus == OutboundMessageStatus.Delivered) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility UIStatusMessageNotRead
        {
            get
            {
                return this._message.read_state == 0 && !this.IsChat && this.UIStatusDelivered == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public HorizontalAlignment SelectionMarkAlignment
        {
            get
            {
                return !this.IsResponse ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
        }

        public bool IsInSelectionMode
        {
            get
            {
                return this._isInSelectionMode;
            }
            set
            {
                if (this._isInSelectionMode == value)
                    return;
                this._isInSelectionMode = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsInSelectionMode));
            }
        }

        public Thickness SimpleSelectionMarkMargin
        {
            get
            {
                if (!this.IsResponse)
                    return new Thickness(-35.0, 10.0, 0.0, 0.0);
                return new Thickness(0.0, 10.0, -35.0, 0.0);
            }
        }

        public Thickness StickerSelectionMarkMargin
        {
            get
            {
                AttachmentViewModel stickerAttachment = this.StickerAttachment;
                if (stickerAttachment == null)
                    return this.SelectionMarkMargin;
                if (this.StickerSelectionMarkAlignment == HorizontalAlignment.Left)
                    return new Thickness(stickerAttachment.StickerDimension + 20.0, 20.0, 0.0, 0.0);
                return new Thickness(0.0, 20.0, stickerAttachment.StickerDimension + 20.0, 0.0);
            }
        }

        public HorizontalAlignment StickerSelectionMarkAlignment
        {
            get
            {
                return !this.IsResponse ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
        }

        public Thickness SelectionMarkMargin
        {
            get
            {
                if (!this.IsResponse)
                    return this._ownMessageSelectionMarkMargin;
                if (this.IsChatResponse)
                    return this._chatReplySelectionMarkMargin;
                return this._replySelectionMarkMargin;
            }
        }

        public Visibility SelectionMarkVisibility
        {
            get
            {
                return !this.IsSelected ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (this._isSelected == value)
                    return;
                this._isSelected = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsSelected));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.SelectionMarkVisibility));
                this.UpdateUIOpacity();
            }
        }

        public OutboundMessageViewModel OutboundMessageVM
        {
            get
            {
                return this._outboundMessage;
            }
        }

        public bool IsForwarded
        {
            get
            {
                return this._isForwarded;
            }
        }

        public long TypingUserId
        {
            get
            {
                return this._typingUserId;
            }
        }

        public bool TypingInChat
        {
            get
            {
                return this._typingInChat;
            }
        }

        public User TypingUser
        {
            get
            {
                return this._typingUser;
            }
        }

        public DateTime DateTimeTyping { get; set; }

        public bool IsUploading
        {
            get
            {
                return this._isUploading;
            }
            set
            {
                this._isUploading = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsUploading));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.DateTimeVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsUploadingVisibility));
            }
        }

        public Visibility IsUploadingVisibility
        {
            get
            {
                return this.IsUploading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility DateTimeVisibility
        {
            get
            {
                return this.IsUploading || this.UIStatusFailed != Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public MessageViewModel(Message message)
        {
            this.InitializeWithMessage(message);
        }

        public MessageViewModel(long typingUserId, bool typingInChat, User typingUser = null)
        {
            this._typingUserId = typingUserId;
            this._typingUser = typingUser;
            this._typingInChat = typingInChat;
            this._message = new Message();
            this.DateTimeTyping = DateTime.Now;
        }

        public MessageViewModel(OutboundMessageViewModel outboundMessage)
        {
            this._outboundMessage = outboundMessage;
            Message message = new Message();
            message.@out = 1;
            if (outboundMessage.IsChat)
                message.chat_id = (int)outboundMessage.UserOrChatId;
            else
                message.uid = (int)outboundMessage.UserOrChatId;
            message.date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
            message.sticker_id = outboundMessage.StickerItem == null ? 0 : outboundMessage.StickerItem.StickerId;
            message.body = message.sticker_id != 0 || outboundMessage.GraffitiAttachmentItem != null ? "" : outboundMessage.MessageText;//UPDATE: 4.8.0
            message.attachments = new List<Attachment>();
            if (message.sticker_id == 0)
            {
                foreach (IOutboundAttachment outboundAttachment in outboundMessage.Attachments.Where<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => !a.IsGeo)))
                {
                    Attachment attachment = outboundAttachment.GetAttachment();
                    if (attachment != null)
                        message.attachments.Add(attachment);
                }
            }
            if (outboundMessage.StickerItem != null)
                message.attachments.Add(outboundMessage.StickerItem.CreateAttachment());
            if (outboundMessage.GraffitiAttachmentItem != null)
                message.attachments.Add(outboundMessage.GraffitiAttachmentItem.CreateAttachment());
            if (message.sticker_id == 0)
            {
                OutboundGeoAttachment outboundGeoAttachment = outboundMessage.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.IsGeo)) as OutboundGeoAttachment;
                if (outboundGeoAttachment != null)
                    message.geo = new Geo()
                    {
                        coordinates = string.Format("{0} {1}", (object)outboundGeoAttachment.Latitude.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)outboundGeoAttachment.Longitude.ToString((IFormatProvider)CultureInfo.InvariantCulture))
                    };
                OutboundForwardedMessages forwardedMessages = outboundMessage.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundForwardedMessages)) as OutboundForwardedMessages;
                message.fwd_messages = forwardedMessages != null ? new List<Message>((IEnumerable<Message>)forwardedMessages.Messages) : new List<Message>();
            }
            else
                message.fwd_messages = new List<Message>();
            this.InitializeWithMessage(message);
        }

        protected MessageViewModel(Message message, bool isForwarded)
        {
            this._isForwarded = true;
            this.InitializeWithMessage(message);
        }

        public MessageViewModel()
        {
        }

        private Color GetUnlblended(Color c)
        {
            double num = 0.8;
            if ((int)c.G == 200 && (int)c.R == 227 || (int)c.G == 196 && (int)c.R == 164)
                num = 1.0;
            return new Color()
            {
                A = byte.MaxValue,
                R = (byte)Math.Min((double)byte.MaxValue, (double)c.R / num),
                G = (byte)Math.Min((double)byte.MaxValue, (double)c.G / num),
                B = (byte)Math.Min((double)byte.MaxValue, (double)c.B / num)
            };
        }

        private void InitializeWithMessage(Message message)
        {
            this._message = message;
            long userId = 0;
            if (this._isForwarded && this._message.@out == 1)
                userId = AppGlobalStateManager.Current.LoggedInUserId;
            else if (this._message.uid != 0)
                userId = (long)this._message.uid;
            User user = new User();
            if (userId != 0L)
            {
                User cachedUser = UsersService.Instance.GetCachedUser(userId);
                if (cachedUser != null)
                {
                    user = cachedUser;
                }
                else
                {
                    UsersService instance = UsersService.Instance;
                    List<long> userIds = new List<long>();
                    userIds.Add(userId);
                    Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(res =>
                    {
                        if (res.ResultCode != ResultCode.Succeeded)
                            return;
                        this._associatedUser = res.ResultData.First<User>();
                        this.RefreshUIProperties();
                    });
                    instance.GetUsers(userIds, callback);
                }
            }
            if (message.action_mid > 0L)
            {
                User cachedUser = UsersService.Instance.GetCachedUser(message.action_mid);
                if (cachedUser != null)
                    this._associatedUser2 = cachedUser;
            }
            this._associatedUser = user;
            this.InitializeForwardedMessages();
            this.InitializeAttachments();
            this.RefreshUIProperties();
        }

        private void _outboundMessage_MessageSent(object sender, EventArgs e)
        {
            if (this._outboundMessage.OutboundMessageStatus == OutboundMessageStatus.Delivered)
            {
                this._message.date = Extensions.DateTimeToUnixTimestamp(this._outboundMessage.DeliveryDateTime, true);
                this._message.mid = (int)this._outboundMessage.DeliveredMessageId;
                int index = 0;
                foreach (IOutboundAttachment outboundAttachment1 in this._outboundMessage.Attachments.Where<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => !a.IsGeo)))
                {
                    if (outboundAttachment1.IsUploadAttachment && outboundAttachment1 is OutboundPhotoAttachment && index < this._message.attachments.Count)
                    {
                        Attachment attachment = this._message.attachments[index];
                        IOutboundAttachment outboundAttachment2 = outboundAttachment1;
                        if (attachment.photo != null && outboundAttachment2.GetAttachment().photo != null)
                        {
                            Photo photo = outboundAttachment2.GetAttachment().photo;
                            attachment.photo.aid = photo.aid;
                            attachment.photo.pid = photo.id;
                            attachment.photo.src = photo.photo_130;
                            attachment.photo.src_big = photo.photo_604;
                            attachment.photo.owner_id = photo.owner_id;
                        }
                    }
                    ++index;
                }

                if (this.GraffitiAttachment != null)//UPDTE: 4.8.0
                {
                    OutboundMessageViewModel messageViewModel = this._outboundMessage;
                    Doc doc1;
                    if (messageViewModel == null)
                    {
                        doc1 = (Doc)null;
                    }
                    else
                    {
                        GraffitiAttachmentItem graffitiAttachmentItem = messageViewModel.GraffitiAttachmentItem;
                        if (graffitiAttachmentItem == null)
                        {
                            doc1 = (Doc)null;
                        }
                        else
                        {
                            Attachment attachment = graffitiAttachmentItem.CreateAttachment();
                            doc1 = attachment != null ? attachment.doc : null;
                        }
                    }
                    Doc doc2 = doc1;
                    if (doc2 != null && this._message.attachments.Count > 0)
                    {
                        Attachment attachment = this._message.attachments[0];
                        if (attachment.doc != null)
                            attachment.doc = doc2;
                    }
                }

                this.EnsureCorrectOrderAfterDelivery();
            }
            this.RefreshUIProperties();
        }

        private void EnsureCorrectOrderAfterDelivery()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                ConversationViewModel vm = ConversationViewModelCache.Current.GetVM((long)this.UserOrChatId, this.IsChat, false);
                if (vm == null || vm.Messages.LastOrDefault<MessageViewModel>() == this)
                    return;
                vm.Messages.Remove(this);
                vm.Messages.AddOrdered<MessageViewModel>(this, ConversationViewModel._comparisonFunc, true);
            }));
        }

        private void InitializeAttachments()// UPDATE: 4.8.0
        {
            this.Attachments.Clear();
            if (this._message.attachments != null)
            {
                if (this._message.sticker_id != 0)
                {
                    Attachment attachment = this._message.attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>)(a =>
                    {
                        if (a.sticker != null)
                            return a.type == "sticker";
                        return false;
                    }));
                    if (attachment != null)
                        this.Attachments.Add(new AttachmentViewModel(attachment, this._message));
                }
                else
                {
                    Attachment attachment1 = this._message.attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>)(a =>
                    {
                        Doc doc = a.doc;
                        if (doc == null)
                            return false;
                        return doc.IsGraffiti;
                    }));
                    if (attachment1 != null)
                    {
                        this.Attachments.Add(new AttachmentViewModel(attachment1, this._message));
                    }
                    else
                    {
                        foreach (Attachment attachment2 in this._message.attachments)
                            this.Attachments.Add(new AttachmentViewModel(attachment2, this._message));
                    }
                }
            }
            if (this._message.geo == null || this._message.sticker_id != 0)
                return;
            this.Attachments.Add(new AttachmentViewModel(this._message.geo));
        }

        private void InitializeForwardedMessages()
        {
            this.ForwardedMessages.Clear();
            if (this._message.fwd_messages == null || this._message.fwd_messages.Count <= 0)
                return;
            foreach (Message fwdMessage in this._message.fwd_messages)
                this.ForwardedMessages.Add(new MessageViewModel(fwdMessage, true));
        }

        public void Send()
        {
            this.IsUploading = true;
            this._outboundMessage.UploadFinished += new EventHandler(this._outboundMessage_UploadFinished);
            this._outboundMessage.MessageSent += new EventHandler(this._outboundMessage_MessageSent);
            this._outboundMessage.Send();
            this.RefreshUIProperties();
        }

        private void _outboundMessage_UploadFinished(object sender, EventArgs e)
        {
            this.IsUploading = false;
            this.RefreshUIProperties();
            this.NotifyResourceUriChanged();
        }

        private void NotifyResourceUriChanged()
        {
            foreach (AttachmentViewModel attachmentViewModel in this.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Photo)))
                attachmentViewModel.NotifyResourceUriChanged();
        }

        public void RefreshUIProperties()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.UIMessageText = this._message.body;
                this.ProcessMessageAction();
                this.UIDate = this._outboundMessage == null || this._outboundMessage.OutboundMessageStatus != OutboundMessageStatus.Failed ? UIStringFormatterHelper.FormatDateForMessageUI(Extensions.UnixTimeStampToDateTime((double)this._message.date, true)) : string.Empty;
                this.UpdateUIOpacity();
                if (this.IsChat || this._isForwarded)
                    this.UIImageUrl = this._associatedUser.photo_max;
                this.UIUserName = this._associatedUser.first_name;
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UIStatusDelivered));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UIStatusFailed));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.DateTimeVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UIStatusMessageNotRead));
                foreach (MessageViewModel forwardedMessage in (Collection<MessageViewModel>)this.ForwardedMessages)
                    forwardedMessage.RefreshUIProperties();
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ForwardedMessagesHeaderText));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.HaveForwardedMessages));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UIHaveMessageText));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.HaveAttachments));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.HaveForwardedMessagesVisivility));
                this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>)(() => this.ForwardedMargin));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsInSelectionMode));
            }));
        }

        private void ProcessMessageAction()
        {
            if ((this._associatedUser == null ? 0 : (this._associatedUser.sex == 1 ? 1 : 0)) != 0)
            {
                string action = this._message.action;
                if (!(action == "chat_photo_update"))
                {
                    if (!(action == "chat_photo_remove"))
                        return;
                    this.UIMessageText = CommonResources.ChatPhotoRemoveFemale;
                }
                else
                    this.UIMessageText = CommonResources.ChatPhotoUpdateFemale;
            }
            else
            {
                string action = this._message.action;
                if (!(action == "chat_photo_update"))
                {
                    if (!(action == "chat_photo_remove"))
                        return;
                    this.UIMessageText = CommonResources.ChatPhotoRemove;
                }
                else
                    this.UIMessageText = CommonResources.ChatPhotoUpdate;
            }
        }

        private void UpdateUIOpacity()
        {
            if (!this.IsSelected)
                this.UIOpacity = this.IsResponse ? 1.0 : 0.8;
            else
                this.UIOpacity = 0.55;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write<Message>(this._message, false);
            writer.Write<User>(this._associatedUser, false);
            writer.WriteList<MessageViewModel>((IList<MessageViewModel>)this.ForwardedMessages, 10000);
            writer.Write(this._isForwarded);
            writer.Write<OutboundMessageViewModel>(this._outboundMessage, false);
            writer.WriteList<AttachmentViewModel>((IList<AttachmentViewModel>)this.Attachments, 10000);
            writer.Write(this.IsInSelectionMode);
            writer.Write<User>(this._associatedUser2, false);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this._message = reader.ReadGeneric<Message>();
            this._associatedUser = reader.ReadGeneric<User>();
            this._forwardedMessages = new ObservableCollection<MessageViewModel>(reader.ReadList<MessageViewModel>());
            this._isForwarded = reader.ReadBoolean();
            this._outboundMessage = reader.ReadGeneric<OutboundMessageViewModel>();
            this._attachments = new ObservableCollection<AttachmentViewModel>(reader.ReadList<AttachmentViewModel>());
            this.IsInSelectionMode = reader.ReadBoolean();
            int num2 = 2;
            if (num1 >= num2)
                this._associatedUser2 = reader.ReadGeneric<User>();
            this.RefreshUIProperties();
        }

        internal void CancelUpload()
        {
            if (this._outboundMessage == null)
                return;
            this._outboundMessage.CancelUpload();
            this.IsUploading = false;
            this.RefreshUIProperties();
        }
    }
}
