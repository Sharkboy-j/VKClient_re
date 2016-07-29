using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.Utils;
using VKClient.Common.VideoCatalog;

namespace VKClient.Common.UC
{
    public class CommentsGenericUC : UserControl
    {
        public static readonly int CountToReload = 20;
        private DelayedExecutor _de = new DelayedExecutor(600);
        //private DialogService _dialogService;
        private bool _commentsAreLoaded;
        private UCItem _commentsCountItem;
        private LikesItem _likesItem;
        private UCItem _loadMoreCommentsItem;
        private int _runningCommentsCount;
        private IScrollableArea _scrollViewer;
        private NewMessageUC ucNewComment;
        private bool _addingComment;
        private long _replyToCid;
        private long _replyToUid;
        private string _replyAutoForm;
        private TextSeparatorUC _commentsCountSeparatorUC;
        private UCItem _moreVideosUCItem;
        private const int OTHER_VIDEOS_MAX_COUNT = 3;
        internal Grid LayoutRoot;
        internal MyVirtualizingPanel2 virtPanel;
        internal TextBlock textBlockError;
        private bool _contentLoaded;

        private ISupportCommentsAndLikes VM
        {
            get
            {
                return this.DataContext as ISupportCommentsAndLikes;
            }
        }

        public ISupportOtherVideos OtherVideosVM
        {
            get
            {
                return this.DataContext as ISupportOtherVideos;
            }
        }

        public MyVirtualizingPanel2 Panel
        {
            get
            {
                return this.virtPanel;
            }
        }

        public IScrollableArea Scroll
        {
            get
            {
                return this._scrollViewer;
            }
        }

        public NewMessageUC UCNewComment
        {
            get
            {
                return this.ucNewComment;
            }
            set
            {
                this.ucNewComment = value;
                if (this.ucNewComment == null)
                    return;
                this.ucNewComment.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
                this.ucNewComment.ReplyUserUC.Tap += new EventHandler<GestureEventArgs>(this.textBlockReplyToName_Tap_1);
            }
        }

        public ReplyUserUC ReplyUserUC
        {
            get
            {
                return this.ucNewComment.ReplyUserUC;
            }
        }

        public int CommentsCountForReload
        {
            get
            {
                int val2 = this.VM.TotalCommentsCount - this.VM.Comments.Count;
                return Math.Min(CommentsGenericUC.CountToReload, val2);
            }
        }

        private string ReplyAutoForm
        {
            get
            {
                return this._replyAutoForm;
            }
            set
            {
                this._replyAutoForm = value;
                this.ucNewComment.SetReplyAutoForm(value);
            }
        }

        public CommentsGenericUC()
        {
            this.InitializeComponent();
        }

        private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public void InitializeWithScrollViewer(IScrollableArea scrollViewer)
        {
            this._scrollViewer = scrollViewer;
            this.virtPanel.InitializeWithScrollViewer(this._scrollViewer, false);
            this.virtPanel.DeltaOffset = -550.0;
        }

        public void ProcessLoadedComments(bool result)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this._runningCommentsCount = this.VM.TotalCommentsCount;
                this.virtPanel.AddItems((IEnumerable<IVirtualizable>)this.GenereateVirtualizableItemsToAdd());
                this._commentsAreLoaded = true;
            }));
        }

        public void AddComment(List<IOutboundAttachment> attachments, Action<bool> resultCallback, StickerItemData stickerItemData = null, string stickerReferrer = "")
        {
            string str1 = this.ucNewComment.TextBoxNewComment.Text;
            if (this.ReplyAutoForm != null && str1.StartsWith(this.ReplyAutoForm))
            {
                string str2 = this.ReplyAutoForm.Remove(this.ReplyAutoForm.IndexOf(", "));
                string str3 = this._replyToUid > 0L ? "id" : "club";
                long num = this._replyToUid > 0L ? this._replyToUid : -this.VM.OwnerId;
                str1 = str1.Remove(0, this.ReplyAutoForm.Length).Insert(0, string.Format("[{0}{1}|{2}], ", (object)str3, (object)num, (object)str2));
            }
            string str4 = str1.Replace("\r\n", "\r").Replace("\r", "\r\n");
            if ((string.IsNullOrWhiteSpace(str4) || str4.Length < 2) && stickerItemData == null)
            {
                if (attachments.Count != 0)
                {
                    List<IOutboundAttachment> source = attachments;
                    Func<IOutboundAttachment, bool> predicate = (Func<IOutboundAttachment, bool>)(a => a.UploadState != OutboundAttachmentUploadState.Completed);
                    if (!source.Any<IOutboundAttachment>(predicate))
                        goto label_6;
                }
                resultCallback(false);
                return;
            }
        label_6:
            if (str4.Length < 2 && attachments.Count <= 0 && stickerItemData == null)
                return;
            if (this._addingComment)
            {
                resultCallback(false);
            }
            else
            {
                this._addingComment = true;
                if (stickerItemData == null)
                    this.ucNewComment.TextBoxNewComment.Text = string.Empty;
                Comment comment1 = new Comment();
                comment1.cid = 0L;
                comment1.date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
                comment1.text = stickerItemData == null ? str4 : "";
                comment1.reply_to_cid = this._replyToCid;
                comment1.reply_to_uid = this._replyToUid;
                comment1.likes = new Likes() { can_like = 0 };
                List<Attachment> attachmentList;
                if (stickerItemData != null)
                    attachmentList = new List<Attachment>()
          {
            stickerItemData.CreateAttachment()
          };
                else
                    attachmentList = attachments.Select<IOutboundAttachment, Attachment>((Func<IOutboundAttachment, Attachment>)(a => a.GetAttachment())).ToList<Attachment>();
                comment1.Attachments = attachmentList;
                int num1 = stickerItemData == null ? 0 : stickerItemData.StickerId;
                comment1.sticker_id = num1;
                Comment comment2 = comment1;
                bool fromGroupChecked = this.ucNewComment.FromGroupChecked;
                comment2.from_id = !fromGroupChecked ? AppGlobalStateManager.Current.LoggedInUserId : this.VM.OwnerId;
                this.VM.AddComment(comment2, attachments.Select<IOutboundAttachment, string>((Func<IOutboundAttachment, string>)(a => a.AttachmentId)).ToList<string>(), fromGroupChecked, (Action<bool, Comment>)((res, createdComment) =>
                {
                    if (res)
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            CommentItem commentItem = this.CreateCommentItem(createdComment);
                            this._addingComment = false;
                            MyVirtualizingPanel2 virtualizingPanel2 = this.virtPanel;
                            int count = this.virtPanel.VirtualizableItems.Count;
                            List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
                            itemsToInsert.Add((IVirtualizable)commentItem);
                            int num = 0;
                            virtualizingPanel2.InsertRemoveItems(count, itemsToInsert, num != 0, null);
                            this._runningCommentsCount = this._runningCommentsCount + 1;
                            this.KeepCommentsCountItemUpToDate();
                            this.ResetReplyFields();
                            resultCallback(true);
                            this.Panel.ScrollToBottom(true);
                        }));
                    else
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            GenericInfoUC.ShowBasedOnResult(1, "", (VKRequestsDispatcher.Error)null);
                            this._addingComment = false;
                            resultCallback(false);
                        }));
                }), stickerReferrer);
            }
        }

        private void DeleteComment(CommentItem obj)
        {
            this.VM.DeleteComment(obj.Comment.cid);
            this.virtPanel.RemoveItem((IVirtualizable)obj);
            this._runningCommentsCount = this._runningCommentsCount - 1;
            this.KeepCommentsCountItemUpToDate();
        }

        private void _loadMoreCommentsItem_Tap(object sender, EventArgs e)
        {
            this.VM.LoadMoreComments(this.CommentsCountForReload, new Action<bool>(this.MoreCommentsAreLoaded));
        }

        private void MoreCommentsAreLoaded(bool result)
        {
            if (!result)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                UCItem ucItem = this._loadMoreCommentsItem;
                IVirtualizable virtualizable = this.virtPanel.VirtualizableItems.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is CommentItem));
                if (virtualizable == null)
                    return;
                this.virtPanel.InsertRemoveItems(this.virtPanel.VirtualizableItems.IndexOf(virtualizable), this.GenereateVirtualizableItemsToAdd(), true, (IVirtualizable)ucItem);
            }));
        }

        private void KeepCommentsCountItemUpToDate()
        {
            this._commentsCountSeparatorUC.Text = CommentsItemsGeneratorHelper.GetTextForCommentsCount(this._runningCommentsCount);
        }

        public void UpdateLikesItem(bool liked)
        {
            if (this._likesItem == null)
                return;
            this._likesItem.Like(liked);
        }

        private List<IVirtualizable> GenereateVirtualizableItemsToAdd()
        {
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            if (!this._commentsAreLoaded)
            {
                LikesInfo likesInfo1 = new LikesInfo();
                likesInfo1.count = this.VM.LikesCount;
                likesInfo1.repostsCount = this.VM.RepostsCount;
                List<long> likesAllIds = this.VM.LikesAllIds;
                List<UserLike> userLikeList = (likesAllIds != null ? likesAllIds.Select<long, UserLike>((Func<long, UserLike>)(uid => new UserLike() { uid = uid })).ToList<UserLike>() : (List<UserLike>)null) ?? new List<UserLike>();
                likesInfo1.users = userLikeList;
                LikesInfo likesInfo2 = likesInfo1;
                double width1 = 480.0;
                Thickness margin1 = new Thickness();
                LikedObjectData objectData = new LikedObjectData();
                objectData.OwnerId = this.VM.OwnerId;
                objectData.ItemId = this.VM.ItemId;
                objectData.Type = (int)this.VM.LikeObjectType;
                LikesInfo likesInfo3 = likesInfo2;
                int num1 = this.VM.CanRepost ? 1 : 0;
                int num2 = this.VM.UserLiked ? 1 : 0;
                User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
                List<User> users = this.VM.Users;
                this._likesItem = new LikesItem(width1, margin1, objectData, likesInfo3, num1 != 0, num2 != 0, loggedInUser, users);
                virtualizableList.Add((IVirtualizable)this._likesItem);
                ISupportOtherVideos otherVideosVm = this.OtherVideosVM;
                List<VKClient.Common.Backend.DataObjects.Video> videoList;
                if (otherVideosVm == null)
                {
                    videoList = null;
                }
                else
                {
                    VKList<VKClient.Common.Backend.DataObjects.Video> otherVideos = otherVideosVm.OtherVideos;
                    videoList = otherVideos != null ? otherVideos.items : null;
                }
                if (videoList != null && otherVideosVm.OtherVideos.items.Count > 0)
                {
                    VKList<VKClient.Common.Backend.DataObjects.Video> otherVideos = otherVideosVm.OtherVideos;
                    List<Group> groupList = new List<Group>();
                    List<User> userList = new List<User>();
                    if (otherVideos.profiles != null)
                        userList.AddRange(otherVideos.profiles.Select<User, User>((Func<User, User>)(profile => new User()
                        {
                            id = profile.id,
                            first_name = profile.first_name,
                            last_name = profile.last_name
                        })));
                    if (otherVideos.groups != null)
                        groupList.AddRange(otherVideos.groups.Select<Group, Group>((Func<Group, Group>)(profile => new Group()
                        {
                            id = profile.id,
                            name = profile.name
                        })));
                    double width2 = 480.0;
                    Thickness margin2 = new Thickness(0.0, 0.0, 0.0, 8.0);
                    Func<UserControlVirtualizable> getUserControlFunc1 = (Func<UserControlVirtualizable>)(() => { return new TextSeparatorUC() { Text = CommonResources.OtherVideos }; });
                    
                    double landscapeWidth1 = 0.0;
                    int num3 = 0;
                    UCItem ucItem1 = new UCItem(width2, margin2, getUserControlFunc1, (Func<double>)(() => 56.0), null, landscapeWidth1, num3 != 0);
                    virtualizableList.Add((IVirtualizable)ucItem1);
                    IVideoCatalogItemUCFactory catalogItemFactory = ServiceLocator.Resolve<IVideoCatalogItemUCFactory>();
                    foreach (VKClient.Common.Backend.DataObjects.Video video1 in otherVideos.items.Take<VKClient.Common.Backend.DataObjects.Video>(3))
                    {
                        VKClient.Common.Backend.DataObjects.Video video = video1;
                        List<User> knownUsers = userList;
                        List<Group> knownGroups = groupList;
                        UCItem ucItem2 = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() =>
                        {
                            UserControlVirtualizable controlVirtualizable = catalogItemFactory.Create(video, knownUsers, knownGroups, StatisticsActionSource.video_recommend, this.CreateVideoContext(otherVideos.context));
                            (controlVirtualizable as CatalogItemUC).GridLayoutRoot.Background = (Brush)(Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush);
                            return controlVirtualizable;
                        }), new Func<double>(() => catalogItemFactory.Height), null, 0.0, false);
                        virtualizableList.Add((IVirtualizable)ucItem2);
                    }
                    double width3 = 480.0;
                    Thickness margin3 = new Thickness();
                    Func<UserControlVirtualizable> getUserControlFunc2 = (Func<UserControlVirtualizable>)(() => new UserControlVirtualizable());
                    
                    double landscapeWidth2 = 0.0;
                    int num4 = 0;
                    UCItem ucItem3 = new UCItem(width3, margin3, getUserControlFunc2, (Func<double>)(() => 8.0), null, landscapeWidth2, num4 != 0);
                    virtualizableList.Add((IVirtualizable)ucItem3);
                    if (otherVideos.items.Count > 3)
                    {
                        this._moreVideosUCItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)new CategoryFooterShortUC()
                        {
                            TapAction = new Action(this.MoreVideos_OnTap)
                        }), (Func<double>)(() => 64.0), null, 0.0, false);
                        virtualizableList.Add((IVirtualizable)this._moreVideosUCItem);
                    }
                }
                int totalCommentsCount = this.VM.TotalCommentsCount;
                this._commentsCountSeparatorUC = new TextSeparatorUC()
                {
                    Text = CommentsItemsGeneratorHelper.GetTextForCommentsCount(totalCommentsCount)
                };
                this._commentsCountItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)this._commentsCountSeparatorUC), (Func<double>)(() => 56.0), null, 0.0, false);
                virtualizableList.Add((IVirtualizable)this._commentsCountItem);
            }
            if (this.CommentsCountForReload > 0 && !this.VM.Comments.IsNullOrEmpty())
            {
                ShowMoreCommentsUC showMoreCommentsUc = new ShowMoreCommentsUC();
                double num = 54.0;
                showMoreCommentsUc.Height = num;
                Action action = (Action)(() => this._loadMoreCommentsItem_Tap(null, null));
                showMoreCommentsUc.OnClickAction = action;
                string textFor = CommentsItemsGeneratorHelper.GetTextFor(this.CommentsCountForReload);
                showMoreCommentsUc.Text = textFor;
                ShowMoreCommentsUC showMoreCommentsUC = showMoreCommentsUc;
                this._loadMoreCommentsItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)showMoreCommentsUC), (Func<double>)(() => 54.0), null, 0.0, false);
                virtualizableList.Add((IVirtualizable)this._loadMoreCommentsItem);
            }
            long num5 = -1;
            CommentItem commentItem1 = this.virtPanel.VirtualizableItems.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is CommentItem)) as CommentItem;
            if (commentItem1 != null)
                num5 = commentItem1.Comment.cid;
            foreach (Comment comment in this.VM.Comments)
            {
                if (comment.cid != num5)
                {
                    CommentItem commentItem2 = this.CreateCommentItem(comment);
                    virtualizableList.Add((IVirtualizable)commentItem2);
                }
                else
                    break;
            }
            this.ucNewComment.Opacity = this.VM.CanComment ? 1.0 : 0.6;
            this.ucNewComment.IsHitTestVisible = this.VM.CanComment;
            return virtualizableList;
        }

        private string CreateVideoContext(string context)
        {
            string str = string.Format("{0}_{1}", (object)this.VM.OwnerId, (object)this.VM.ItemId);
            if (!string.IsNullOrEmpty(context))
                str += string.Format("|{0}", (object)context);
            return str;
        }

        private void MoreVideos_OnTap()
        {
            Execute.ExecuteOnUIThread((Action)(() => this.virtPanel.InsertRemoveItems(this.virtPanel.VirtualizableItems.IndexOf((IVirtualizable)this._moreVideosUCItem) - 1, this.GetMoreOtherVideoItems(), false, (IVirtualizable)this._moreVideosUCItem)));
        }

        private List<IVirtualizable> GetMoreOtherVideoItems()
        {
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            IVideoCatalogItemUCFactory catalogItemFactory = ServiceLocator.Resolve<IVideoCatalogItemUCFactory>();
            VKList<VKClient.Common.Backend.DataObjects.Video> otherVideos = this.OtherVideosVM.OtherVideos;
            List<Group> groupList = new List<Group>();
            List<User> userList = new List<User>();
            if (otherVideos.profiles != null)
                userList.AddRange(otherVideos.profiles.Select<User, User>((Func<User, User>)(profile => new User()
                {
                    id = profile.id,
                    first_name = profile.first_name,
                    last_name = profile.last_name
                })));
            if (otherVideos.groups != null)
                groupList.AddRange(otherVideos.groups.Select<Group, Group>((Func<Group, Group>)(profile => new Group()
                {
                    id = profile.id,
                    name = profile.name
                })));
            foreach (VKClient.Common.Backend.DataObjects.Video video1 in otherVideos.items.Skip<VKClient.Common.Backend.DataObjects.Video>(3))
            {
                VKClient.Common.Backend.DataObjects.Video video = video1;
                List<User> knownUsers = userList;
                List<Group> knownGroups = groupList;
                UCItem ucItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() =>
                {
                    UserControlVirtualizable controlVirtualizable = catalogItemFactory.Create(video, knownUsers, knownGroups, StatisticsActionSource.video_recommend, this.CreateVideoContext(otherVideos.context));
                    (controlVirtualizable as CatalogItemUC).GridLayoutRoot.Background = (Brush)(Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush);
                    return controlVirtualizable;
                }), new Func<double>(() => catalogItemFactory.Height), null, 0.0, false);
                virtualizableList.Add((IVirtualizable)ucItem);
            }
            return virtualizableList;
        }

        private CommentItem CreateCommentItem(Comment comment)
        {
            User user = this.VM.Users.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == comment.from_id));
            User user2 = this.VM.Users2.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == comment.reply_to_uid));
            Group group = this.VM.Groups.FirstOrDefault<Group>((Func<Group, bool>)(g => g.id == -comment.from_id));
            if (user == null && comment.from_id == AppGlobalStateManager.Current.LoggedInUserId)
                user = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
            if (user2 == null && comment.reply_to_uid == AppGlobalStateManager.Current.LoggedInUserId)
                user2 = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
            Action<CommentItem> replyCallback = new Action<CommentItem>(this.ReplyToComment);
            LikeObjectType likeObjType = LikeObjectType.comment;
            if (this.VM.LikeObjectType == LikeObjectType.photo)
                likeObjType = LikeObjectType.photo_comment;
            if (this.VM.LikeObjectType == LikeObjectType.video)
                likeObjType = LikeObjectType.video_comment;
            if (this.VM.LikeObjectType == LikeObjectType.market)
                likeObjType = LikeObjectType.market_comment;
            return CommentsItemsGeneratorHelper.CreateCommentItem(480.0, comment, likeObjType, this.VM.OwnerId, user, user2, group, new Action<CommentItem>(this.DeleteComment), replyCallback, new Action<CommentItem>(this.EditComment), (Action<CommentItem>)null);
        }

        private void EditComment(CommentItem commentItem)
        {
            if (this.VM.LikeObjectType == LikeObjectType.photo)
            {
                commentItem.Comment.owner_id = this.VM.OwnerId;
                ParametersRepository.SetParameterForId("EditPhotoComment", (object)commentItem.Comment);
            }
            else if (this.VM.LikeObjectType == LikeObjectType.video)
            {
                commentItem.Comment.owner_id = this.VM.OwnerId;
                ParametersRepository.SetParameterForId("EditVideoComment", (object)commentItem.Comment);
            }
            else if (this.VM.LikeObjectType == LikeObjectType.market)
            {
                commentItem.Comment.owner_id = this.VM.OwnerId;
                ParametersRepository.SetParameterForId("EditProductComment", (object)commentItem.Comment);
            }
            Navigator.Current.NavigateToNewWallPost(Math.Abs(this.VM.OwnerId), this.VM.OwnerId < 0L, 0, false, false, false);
        }

        private void ReplyToComment(CommentItem commentItem)
        {
            this._replyToCid = commentItem.Comment.cid;
            this._replyToUid = commentItem.Comment.from_id;
            string str1 = "";
            string str2 = "";
            if (this._replyToUid > 0L)
            {
                User user = this.VM.Users2.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == this._replyToUid));
                if (user == null && this._replyToUid == AppGlobalStateManager.Current.LoggedInUserId)
                    user = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
                if (user != null)
                {
                    str1 = user.first_name;
                    str2 = user.first_name_dat;
                }
            }
            else
            {
                Group group = this.VM.Groups.FirstOrDefault<Group>((Func<Group, bool>)(u => u.id == this.VM.OwnerId * -1L)) ?? GroupsService.Current.GetCachedGroup(-this.VM.OwnerId);
                if (group != null)
                    str2 = str1 = group.name;
            }
            this.ReplyUserUC.Title = str2;
            this.ReplyUserUC.Visibility = Visibility.Visible;
            if (this.ucNewComment.TextBoxNewComment.Text == "" || this.ucNewComment.TextBoxNewComment.Text == this.ReplyAutoForm)
            {
                this.ReplyAutoForm = str1 + ", ";
                this.ucNewComment.TextBoxNewComment.Text = this.ReplyAutoForm;
                this.ucNewComment.TextBoxNewComment.SelectionStart = this.ReplyAutoForm.Length;
            }
            else
                this.ReplyAutoForm = str1 + ", ";
            this.ucNewComment.TextBoxNewComment.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.ResetReplyFields();
        }

        private void ResetReplyFields()
        {
            if (this.ucNewComment.TextBoxNewComment.Text == this.ReplyAutoForm)
                this.ucNewComment.TextBoxNewComment.Text = "";
            this.ReplyAutoForm = null;
            this._replyToUid = this._replyToCid = 0L;
            this.ReplyUserUC.Title = "";
            this.ReplyUserUC.Visibility = Visibility.Collapsed;
            this.UCNewComment.ucNewPost.TextBoxPost.Focus();
        }

        private void ShowHideErrorText(bool result)
        {
            this.textBlockError.Text = CommonResources.GenericErrorText;
            this.textBlockError.Visibility = result ? Visibility.Collapsed : Visibility.Visible;
        }

        private void textBlockReplyToName_Tap_1(object sender, GestureEventArgs e)
        {
            this.ResetReplyFields();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/CommentsGenericUC.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.virtPanel = (MyVirtualizingPanel2)this.FindName("virtPanel");
            this.textBlockError = (TextBlock)this.FindName("textBlockError");
        }
    }
}
