using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

using VKClient.Common.BLExtensions;
using VKClient.Audio.Base.BLExtensions;

namespace VKClient.Common
{
    public class PostCommentsPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, IHandle<WallPostPinnedUnpinned>, ISupportShare
    {
        private readonly PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
        {
            ShowCamera = true
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemGoToOriginal = new ApplicationBarMenuItem()
        {
            Text = CommonResources.GoToOriginal
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemPin = new ApplicationBarMenuItem()
        {
            Text = CommonResources.PinPost
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemUnpin = new ApplicationBarMenuItem()
        {
            Text = CommonResources.UnpinPost
        };
        private readonly ApplicationBarIconButton _appBarButtonSend = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative),
            Text = CommonResources.PostCommentsPage_AppBar_Send
        };
        private readonly ApplicationBarIconButton _appBarButtonLike = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.heart2.rest.png", UriKind.Relative),
            Text = CommonResources.PostCommentsPage_AppBar_Like
        };
        private readonly ApplicationBarIconButton _appBarButtonUnlike = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.heart2.broken.rest.png", UriKind.Relative),
            Text = CommonResources.PostCommentsPage_AppBar_Unlike
        };
        private readonly ApplicationBarIconButton _appBarButtonEmojiToggle = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.smile.png", UriKind.Relative),
            Text = "emoji"
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemShare = new ApplicationBarMenuItem()
        {
            Text = CommonResources.PostCommentsPage_AppBar_Share
        };
        private readonly ApplicationBarIconButton _appBarButtonAttachments = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/attach.png", UriKind.Relative),
            Text = CommonResources.NewPost_AppBar_AddAttachment
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemRefresh = new ApplicationBarMenuItem()
        {
            Text = CommonResources.PostCommentsPage_AppBar_Refresh
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemEdit = new ApplicationBarMenuItem()
        {
            Text = CommonResources.Edit
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemDelete = new ApplicationBarMenuItem()
        {
            Text = CommonResources.Delete
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemReport = new ApplicationBarMenuItem()
        {
            Text = CommonResources.Report
        };
        private DialogService _dc = new DialogService();
        private DelayedExecutor _de = new DelayedExecutor(200);
        private DelayedExecutor _de2 = new DelayedExecutor(550);
        private bool _isInitialized;
        private SharePostUC _sharePostUC;
        private readonly ViewportScrollableAreaAdapter _adapter;
        private WallPostViewModel _commentVM;
        private ApplicationBar _appBar;
        private bool _addingComment;
        private bool _focusedComments;
        private long _replyToCid;
        private long _replyToUid;
        private string _replyAutoForm;
        internal Grid LayoutRoot;
        internal ViewportControl scroll;
        internal StackPanel scrollableGrid;
        internal MyVirtualizingPanel2 panel;
        internal NewMessageUC ucNewMessage;
        internal GenericHeaderUC Header;
        internal PullToRefreshUC ucPullToRefresh;
        internal MoreActionsUC ucMoreActions;
        private bool _contentLoaded;

        private PostCommentsViewModel PostCommentsVM
        {
            get
            {
                return this.DataContext as PostCommentsViewModel;
            }
        }

        private ReplyUserUC ReplyUserUC
        {
            get
            {
                return this.ucNewMessage.ReplyUserUC;
            }
        }

        public TextBox textBoxNewMessage
        {
            get
            {
                return this.ucNewMessage.ucNewPost.textBoxPost;
            }
        }

        private bool FocusComments
        {
            get
            {
                if (this.NavigationContext.QueryString.ContainsKey("FocusComments"))
                    return this.NavigationContext.QueryString["FocusComments"] == bool.TrueString;
                return false;
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
                this.ucNewMessage.SetReplyAutoForm(value);
            }
        }

        public bool ReadyToSend
        {
            get
            {
                string text = this.textBoxNewMessage.Text;
                ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
                if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
                    return true;
                if (outboundAttachments.Count > 0)
                    return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
                return false;
            }
        }

        public PostCommentsPage()
        {
            this.InitializeComponent();
            this.Header.OnHeaderTap = (Action)(() => this.panel.ScrollToBottom(false));
            this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
            this.panel.InitializeWithScrollViewer((IScrollableArea)this._adapter, false);
            this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.panel);
            this.panel.LoadedHeightDownwards = this.panel.LoadedHeightDownwardsNotScrolling = 1600.0;
            this.BuildAppBar();
            this.ucNewMessage.ReplyUserUC.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.Button_Click_1);
            this.ucMoreActions.SetBlue();
            this.ucMoreActions.TapCallback = (Action)(() =>
            {
                WallPostItem wallPostItem = this.PostCommentsVM.WallPostItem;
                if (wallPostItem == null)
                    return;
                ContextMenu menu = ContextMenuHelper.CreateMenu(wallPostItem.GenerateMenuItems());
                ContextMenuService.SetContextMenu((DependencyObject)this.ucMoreActions, menu);
                menu.IsOpen = true;
            });
            this.ucNewMessage.OnAddAttachTap = new Action(this.AddAttachTap);
            this.ucNewMessage.OnSendTap = (Action)(() => this._appBarButtonSend_Click(null, (EventArgs)null));
            this.ucNewMessage.UCNewPost.OnImageDeleteTap = (Action<object>)(sender =>
            {
                FrameworkElement frameworkElement = sender as FrameworkElement;
                if (frameworkElement != null)
                    this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
                this.UpdateAppBar();
            });
            this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
            this.ucNewMessage.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
            Binding binding = new Binding("OutboundAttachments");
            this.ucNewMessage.ucNewPost.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            this.scroll.BindViewportBoundsTo((FrameworkElement)this.scrollableGrid);
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
            EventAggregator.Current.Subscribe((object)this);
        }

        private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void PanelIsOpenedChanged(object sender, bool e)
        {
            if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
                this.panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
            else
                this.panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
        }

        private void AddAttachTap()
        {
            AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action)(() =>
            {
                PostCommentsPage.HandleInputParams(this._commentVM);
                this.UpdateAppBar();
            }), true, 0, 0, null);
        }

        private void _photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;
            ParametersRepository.SetParameterForId("ChoosenPhoto", (object)e.ChosenPhoto);
        }

        private void BuildAppBar()
        {
            this._appBarButtonSend.Click += new EventHandler(this._appBarButtonSend_Click);
            this._appBarButtonAttachments.Click += new EventHandler(this._appBarButtonAttachments_Click);
            this._appBarButtonEmojiToggle.Click += new EventHandler(this._appBarButtonEmojiToggle_Click);
            this._appBarButtonLike.Click += new EventHandler(this._appBarButtonLike_Click);
            this._appBarMenuItemShare.Click += new EventHandler(this._appBarButtonShare_Click);
            this._appBarButtonUnlike.Click += new EventHandler(this._appBarButtonUnlike_Click);
            this._appBarMenuItemRefresh.Click += new EventHandler(this._appBarMenuItemRefresh_Click);
            this._appBarMenuItemGoToOriginal.Click += new EventHandler(this._appBarMenuItemGoToOriginal_Click);
            this._appBarMenuItemEdit.Click += new EventHandler(this._appBarMenuItemEdit_Click);
            this._appBarMenuItemDelete.Click += new EventHandler(this._appBarMenuItemDelete_Click);
            this._appBarMenuItemReport.Click += new EventHandler(this._appBarMenuItemReport_Click);
            this._appBar = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor
            };
            this._appBar.StateChanged += new EventHandler<ApplicationBarStateChangedEventArgs>(this._appBar_StateChanged);
            this._appBar.Buttons.Add((object)this._appBarButtonSend);
            this._appBar.Buttons.Add((object)this._appBarButtonEmojiToggle);
            this._appBar.Buttons.Add((object)this._appBarButtonAttachments);
            this._appBar.Buttons.Add((object)this._appBarButtonLike);
            this._appBar.MenuItems.Add((object)this._appBarMenuItemShare);
            this._appBar.MenuItems.Add((object)this._appBarMenuItemRefresh);
            this._appBarMenuItemPin.Click += new EventHandler(this._appBarMenuItemPin_Click);
            this._appBarMenuItemUnpin.Click += new EventHandler(this._appBarMenuItemUnpin_Click);
            this._appBar.Opacity = 0.9;
        }

        private void _appBarMenuItemUnpin_Click(object sender, EventArgs e)
        {
            this.PinUnpin();
        }

        private void _appBarMenuItemPin_Click(object sender, EventArgs e)
        {
            this.PinUnpin();
        }

        private void PinUnpin()
        {
            this.PostCommentsVM.PinUnpin((Action<bool>)(res => { }));
        }

        private void _appBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
        }

        private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
        {
        }

        private void _appBarMenuItemReport_Click(object sender, EventArgs e)
        {
            if (this.PostCommentsVM.WallPost == null)
                return;
            ReportContentHelper.ReportWallPost(this.PostCommentsVM.WallPost, "");
        }

        private void _appBarMenuItemDelete_Click(object sender, EventArgs e)
        {
        }

        private void _appBarMenuItemEdit_Click(object sender, EventArgs e)
        {
            if (this.PostCommentsVM.WallPostData == null || this.PostCommentsVM.WallPostData.WallPost == null)
                return;
            this.PostCommentsVM.WallPostData.WallPost.NavigateToEditWallPost(this.PostCommentsVM.WallPostItem == null ? 3 : this.PostCommentsVM.WallPostItem.AdminLevel);
        }

        private void _appBarButtonAttachments_Click(object sender, EventArgs e)
        {
        }

        private void _appBarMenuItemGoToOriginal_Click(object sender, EventArgs e)
        {
            if (this.PostCommentsVM.WallPost == null || this.PostCommentsVM.WallPost.copy_history.IsNullOrEmpty())
                return;
            Navigator.Current.NavigateToWallPostComments(this.PostCommentsVM.WallPost.copy_history[0].WallPostOrReplyPostId, this.PostCommentsVM.WallPost.copy_history[0].owner_id, false, this.PostCommentsVM.PollId, this.PostCommentsVM.PollOwnerId, "");
        }

        private void _appBarMenuItemRefresh_Click(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            this.ucNewMessage.Opacity = 0.0;
            this.PostCommentsVM.Refresh();
        }

        private void _appBarButtonUnlike_Click(object sender, EventArgs e)
        {
            this.PostCommentsVM.Unlike();
            this.UpdateAppBar();
        }

        private void _appBarButtonShare_Click(object sender, EventArgs e)
        {
            this.Focus();
            this._dc = new DialogService();
            this._dc.SetStatusBarBackground = true;
            this._dc.HideOnNavigation = false;
            this._sharePostUC = new SharePostUC();
            this._sharePostUC.SetShareEnabled(this.PostCommentsVM.CanRepost);
            this._sharePostUC.SetShareCommunityEnabled(this.PostCommentsVM.CanRepostCommunity);
            this._sharePostUC.SendTap += new EventHandler(this.buttonSend_Click);
            this._sharePostUC.ShareTap += new EventHandler(this.buttonShare_Click);
            this._dc.Child = (FrameworkElement)this._sharePostUC;
            this._dc.AnimationType = DialogService.AnimationTypes.None;
            this._dc.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
            this._dc.Show((UIElement)this.scroll);
        }

        private void buttonShare_Click(object sender, EventArgs eventArgs)
        {
            this.Share(0L, "");
        }

        private void Share(long gid = 0, string groupName = "")
        {
            this.PostCommentsVM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
            this.UpdateAppBar();
            this._dc.Hide();
        }

        private void buttonSend_Click(object sender, EventArgs eventArgs)
        {
            ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
            contentDataProvider.Message = this._sharePostUC.Text;
            contentDataProvider.WallPost = this.PostCommentsVM.WallPost;
            contentDataProvider.StoreDataToRepository();
            ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
            this._dc.Hide();
            Navigator.Current.NavigateToPickConversation();
        }

        private void _appBarButtonLike_Click(object sender, EventArgs e)
        {
            this.PostCommentsVM.Like();
            this.UpdateAppBar();
        }

        private void _appBarButtonSend_Click(object sender, EventArgs e)
        {
            string str1 = this.textBoxNewMessage.Text;
            if (this.ReplyAutoForm != null && str1.StartsWith(this.ReplyAutoForm))
            {
                string str2 = this.ReplyAutoForm.Remove(this.ReplyAutoForm.IndexOf(", "));
                string str3 = this._replyToUid > 0L ? "id" : "club";
                long num = this._replyToUid > 0L ? this._replyToUid : -this.PostCommentsVM.WallPostData.WallPost.owner_id;
                str1 = str1.Remove(0, this.ReplyAutoForm.Length).Insert(0, string.Format("[{0}{1}|{2}], ", (object)str3, (object)num, (object)str2));
            }
            string text = str1.Replace("\r\n", "\r").Replace("\r", "\r\n");
            if (!this.PostCommentsVM.CanPostComment(text, this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (StickerItemData)null))
                return;
            bool fromGroupChecked = this.ucNewMessage.FromGroupChecked;
            if (this._addingComment)
                return;
            this._addingComment = true;
            this.PostCommentsVM.PostComment(text, this._replyToCid, this._replyToUid, fromGroupChecked, this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>)(result =>
            {
                this._addingComment = false;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (!result)
                    {
                        ExtendedMessageBox.ShowSafe(CommonResources.Error);
                    }
                    else
                    {
                        this.textBoxNewMessage.Text = "";
                        this.InitializeCommentVM();
                        this.UpdateAppBar();
                        this.ScrollToBottom();
                        this.textBoxNewMessage.Text = "";
                        this.ResetReplyFields();
                    }
                }));
            }), (StickerItemData)null, "");
        }

        private void ScrollToBottom()
        {
            Execute.ExecuteOnUIThread((Action)(() => this.panel.ScrollToBottom(true)));
        }

        private void UpdateAppBar()
        {
            this._appBarButtonSend.IsEnabled = this.PostCommentsVM.CanComment && this.ReadyToSend;
            this.ucNewMessage.UpdateSendButton(this._appBarButtonSend.IsEnabled);
            this._appBarButtonLike.IsEnabled = this._appBarButtonUnlike.IsEnabled = this.PostCommentsVM.CanLike;
            this._appBarButtonEmojiToggle.IsEnabled = this.PostCommentsVM.CanComment;
            this._appBarMenuItemShare.IsEnabled = true;
            this._appBarButtonAttachments.IsEnabled = this.PostCommentsVM.CanComment;
            int count = this._commentVM.OutboundAttachments.Count;
            this._appBarButtonAttachments.IconUri = count <= 0 ? new Uri("Resources/attach.png", UriKind.Relative) : new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", (object)Math.Min(count, 10)), UriKind.Relative);
            if (this.PostCommentsVM.Liked)
            {
                this._appBar.Buttons.Remove((object)this._appBarButtonLike);
                if (!this._appBar.Buttons.Contains((object)this._appBarButtonUnlike))
                    this._appBar.Buttons.Insert(3, (object)this._appBarButtonUnlike);
            }
            else
            {
                this._appBar.Buttons.Remove((object)this._appBarButtonUnlike);
                if (!this._appBar.Buttons.Contains((object)this._appBarButtonLike))
                    this._appBar.Buttons.Insert(3, (object)this._appBarButtonLike);
            }
            if (this.PostCommentsVM.WallPost != null && this.PostCommentsVM.WallPost.CanGoToOriginal() && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemGoToOriginal))
                this._appBar.MenuItems.Add((object)this._appBarMenuItemGoToOriginal);
            if (this.PostCommentsVM.WallPostData != null && this.PostCommentsVM.WallPostData.WallPost != null && (this.PostCommentsVM.WallPostData.WallPost.CanEdit(this.PostCommentsVM.WallPostData.Groups) && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemEdit)))
                this._appBar.MenuItems.Add((object)this._appBarMenuItemEdit);
            if (this.PostCommentsVM.WallPostData != null && this.PostCommentsVM.WallPostData.WallPost != null && (this.PostCommentsVM.WallPostData.WallPost.CanDelete(this.PostCommentsVM.WallPostData.Groups, false) && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemDelete)))
                this._appBar.MenuItems.Add((object)this._appBarMenuItemDelete);
            if (this.PostCommentsVM.WallPost != null && this.PostCommentsVM.WallPost.CanReport() && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemReport))
                this._appBar.MenuItems.Add((object)this._appBarMenuItemReport);
            if (this.PostCommentsVM.CanPin && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemPin))
                this._appBar.MenuItems.Insert(0, (object)this._appBarMenuItemPin);
            if (!this.PostCommentsVM.CanPin && this._appBar.MenuItems.Contains((object)this._appBarMenuItemPin))
                this._appBar.MenuItems.Remove((object)this._appBarMenuItemPin);
            if (this.PostCommentsVM.CanUnpin && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemUnpin))
                this._appBar.MenuItems.Insert(0, (object)this._appBarMenuItemUnpin);
            if (this.PostCommentsVM.CanUnpin || !this._appBar.MenuItems.Contains((object)this._appBarMenuItemUnpin))
                return;
            this._appBar.MenuItems.Remove((object)this._appBarMenuItemUnpin);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            bool flag = true;
            if (!this._isInitialized)
            {
                this.ucNewMessage.Opacity = 0.0;
                NewsItemDataWithUsersAndGroupsInfo wallPostData = ParametersRepository.GetParameterForIdAndReset("WallPost") as NewsItemDataWithUsersAndGroupsInfo;
                long num1 = long.Parse(this.NavigationContext.QueryString["PollId"]);
                long num2 = long.Parse(this.NavigationContext.QueryString["PollOwnerId"]);
                if (this.FocusComments)
                    this.panel.OnlyPartialLoad = true;
                long postId = this.CommonParameters.PostId;
                long ownerId = this.CommonParameters.OwnerId;
                MyVirtualizingPanel2 panel = this.panel;
                Action loadedCallback = new Action(this.ViewModelIsLoaded);
                Action<CommentItem> replyCommentAction = new Action<CommentItem>(this.ReplyToComment);
                long knownPollId = num1;
                long knownPollOwnerId = num2;
                PostCommentsViewModel commentsViewModel = new PostCommentsViewModel(wallPostData, postId, ownerId, panel, loadedCallback, replyCommentAction, knownPollId, knownPollOwnerId);
                this.InitializeCommentVM();
                this.DataContext = (object)commentsViewModel;
                commentsViewModel.LoadMoreCommentsInUI();
                this.UpdateAppBar();
                this.RestoreUnboundState();
                this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.panel);
                this.panel.OnRefresh = new Action(this.Refresh);
                this._isInitialized = true;
                flag = false;
            }
            if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
                WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
            this.ProcessInputData();
            PostCommentsPage.HandleInputParams(this._commentVM);
            this.UpdateAppBar();
        }

        public static void HandleInputParams(WallPostViewModel wallPostVM)
        {
            GeoCoordinate geoCoordinate = ParametersRepository.GetParameterForIdAndReset("NewPositionToBeAttached") as GeoCoordinate;
            if (geoCoordinate != (GeoCoordinate)null)
            {
                OutboundGeoAttachment outboundGeoAttachment = new OutboundGeoAttachment(geoCoordinate.Latitude, geoCoordinate.Longitude);
                wallPostVM.AddAttachment((IOutboundAttachment)outboundGeoAttachment);
            }
            List<Stream> streamList1 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            List<Stream> streamList2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
            if (streamList1 != null)
            {
                for (int index = 0; index < streamList1.Count; ++index)
                {
                    Stream stream1 = streamList1[index];
                    Stream stream2 = streamList2[index];
                    long userOrGroupId = wallPostVM.UserOrGroupId;
                    int num1 = wallPostVM.IsGroup ? 1 : 0;
                    Stream previewStream = stream2;
                    int num2 = 0;
                    OutboundPhotoAttachment forUploadNewPhoto = OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType)num2);
                    wallPostVM.AddAttachment((IOutboundAttachment)forUploadNewPhoto);
                }
                wallPostVM.UploadAttachments();
            }
            Photo photo = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (photo != null)
            {
                OutboundPhotoAttachment choosingExistingPhoto = OutboundPhotoAttachment.CreateForChoosingExistingPhoto(photo, wallPostVM.UserOrGroupId, wallPostVM.IsGroup, PostType.WallPost);
                wallPostVM.AddAttachment((IOutboundAttachment)choosingExistingPhoto);
            }
            VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
            if (video != null)
            {
                OutboundVideoAttachment outboundVideoAttachment = new OutboundVideoAttachment(video);
                wallPostVM.AddAttachment((IOutboundAttachment)outboundVideoAttachment);
            }
            AudioObj audio = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (audio != null)
            {
                OutboundAudioAttachment outboundAudioAttachment = new OutboundAudioAttachment(audio);
                wallPostVM.AddAttachment((IOutboundAttachment)outboundAudioAttachment);
            }
            Doc pickedDocument = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (pickedDocument != null)
            {
                OutboundDocumentAttachment documentAttachment = new OutboundDocumentAttachment(pickedDocument);
                wallPostVM.AddAttachment((IOutboundAttachment)documentAttachment);
            }
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if ((continuationEventArgs == null || !((IEnumerable<StorageFile>)continuationEventArgs.Files).Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocument"))
                return;
            object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
            StorageFile file = continuationEventArgs != null ? (continuationEventArgs.Files).First<StorageFile>() : (StorageFile)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument");
            AttachmentType result;
            if (parameterForIdAndReset == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset.ToString(), out result))
                return;
            if (result != AttachmentType.VideoFromPhone)
            {
                if (result != AttachmentType.DocumentFromPhone && result != AttachmentType.DocumentPhoto)
                    return;
                OutboundUploadDocumentAttachment documentAttachment = new OutboundUploadDocumentAttachment(file);
                wallPostVM.AddAttachment((IOutboundAttachment)documentAttachment);
                wallPostVM.UploadAttachments();
            }
            else
            {
                OutboundUploadVideoAttachment uploadVideoAttachment = new OutboundUploadVideoAttachment(file, true, 0L);
                wallPostVM.AddAttachment((IOutboundAttachment)uploadVideoAttachment);
                wallPostVM.UploadAttachments();
            }
        }

        private void InitializeCommentVM()
        {
            this._commentVM = WallPostViewModel.CreateNewWallCommentVM(this.CommonParameters.OwnerId, this.CommonParameters.PostId);
            this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
            this.ucNewMessage.DataContext = (object)this._commentVM;
        }

        private void _commentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender != this._commentVM || !(e.PropertyName == "CanPublish"))
                return;
            this.UpdateAppBar();
            ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
            Func<IOutboundAttachment, bool> predicate = (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Uploading);
            //Func<IOutboundAttachment, bool> predicate;
            if (outboundAttachments.Any<IOutboundAttachment>(predicate))
                return;
            this.PostCommentsVM.SetInProgress(false, "");
        }

        private void ProcessInputData()
        {
            Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (group != null)
                this.Share(group.id, group.name);
            Photo photo = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (photo != null)
                this._commentVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForChoosingExistingPhoto(photo, 0L, false, PostType.WallPost));
            VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
            if (video != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundVideoAttachment(video));
            AudioObj audio = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (audio != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundAudioAttachment(audio));
            Doc pickedDocument = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (pickedDocument != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundDocumentAttachment(pickedDocument));
            List<Stream> streamList1 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            List<Stream> streamList2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
            if (streamList1 != null)
            {
                for (int index = 0; index < streamList1.Count; ++index)
                {
                    Stream stream1 = streamList1[index];
                    Stream stream2 = streamList2[index];
                    long userOrGroupId = 0;
                    int num1 = 0;
                    Stream previewStream = stream2;
                    int num2 = 0;
                    this._commentVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType)num2));
                }
                this.PostCommentsVM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
                this._commentVM.UploadAttachments();
            }
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if ((continuationEventArgs == null || !(continuationEventArgs.Files).Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocument"))
                return;
            object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
            StorageFile file = continuationEventArgs != null ? ((IEnumerable<StorageFile>)continuationEventArgs.Files).First<StorageFile>() : (StorageFile)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument");
            AttachmentType result;
            if (parameterForIdAndReset == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset.ToString(), out result))
                return;
            if (result != AttachmentType.VideoFromPhone)
            {
                if (result != AttachmentType.DocumentFromPhone && result != AttachmentType.DocumentPhoto)
                    return;
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundUploadDocumentAttachment(file));
                this._commentVM.UploadAttachments();
            }
            else
            {
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundUploadVideoAttachment(file, true, 0L));
                this._commentVM.UploadAttachments();
            }
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.SaveUnboundState();
            if (e.NavigationMode != NavigationMode.Back)
                WallPostVMCacheManager.RegisterForDelayedSerialization(this._commentVM);
            if (e.NavigationMode != NavigationMode.Back)
                return;
            WallPostVMCacheManager.ResetInstance();
        }

        private void SaveUnboundState()
        {
            try
            {
                this.State["CommentText"] = (object)this.textBoxNewMessage.Text;
            }
            catch
            {
            }
        }

        private void RestoreUnboundState()
        {
            if (!this.State.ContainsKey("CommentText"))
                return;
            this.textBoxNewMessage.Text = this.State["CommentText"].ToString();
        }

        private void ViewModelIsLoaded()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.ucNewMessage.Opacity = this.PostCommentsVM.CanComment ? 1.0 : 0.6;
                this.ucNewMessage.IsHitTestVisible = this.PostCommentsVM.CanComment;
                if (this.PostCommentsVM.WallPostItem != null)
                {
                    this.ucNewMessage.SetAdminLevel(this.PostCommentsVM.WallPostItem.AdminLevel);
                    this.UpdateLayout();
                }
                if (this.FocusComments && !this._focusedComments)
                {
                    this.ScrollToBottom();
                    this._focusedComments = true;
                }
                this.UpdateAppBar();
            }));
        }

        private void ReplyToComment(CommentItem commentItem)
        {
            this._replyToCid = commentItem.Comment.cid;
            this._replyToUid = commentItem.Comment.from_id;
            string str1 = "";
            string str2 = "";
            if (this._replyToUid > 0L)
            {
                User user = this.PostCommentsVM.Users2.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == this._replyToUid));
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
                Group group = this.PostCommentsVM.Groups.FirstOrDefault<Group>((Func<Group, bool>)(u => u.id == this.PostCommentsVM.OwnerId * -1L)) ?? GroupsService.Current.GetCachedGroup(-this.PostCommentsVM.OwnerId);
                if (group != null)
                    str2 = str1 = group.name;
            }
            this.ReplyUserUC.Visibility = Visibility.Visible;
            this.ReplyUserUC.Title = str2;
            if (this.textBoxNewMessage.Text == "" || this.textBoxNewMessage.Text == this.ReplyAutoForm)
            {
                this.ReplyAutoForm = str1 + ", ";
                this.textBoxNewMessage.Text = this.ReplyAutoForm;
                this.textBoxNewMessage.SelectionStart = this.ReplyAutoForm.Length;
            }
            else
                this.ReplyAutoForm = str1 + ", ";
            this.textBoxNewMessage.Focus();
        }

        private void ResetReplyFields()
        {
            if (this.textBoxNewMessage.Text == this.ReplyAutoForm)
                this.textBoxNewMessage.Text = "";
            this.ReplyAutoForm = null;
            this._replyToUid = this._replyToCid = 0L;
            this.ReplyUserUC.Visibility = Visibility.Collapsed;
            this.ReplyUserUC.Title = "";
            this.textBoxNewMessage.Focus();
        }

        private void Button_Click_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ResetReplyFields();
        }

        public void Handle(StickerItemTapEvent message)
        {
            if (!this._isCurrentPage)
                return;
            bool fromGroupChecked = this.ucNewMessage.FromGroupChecked;
            if (this._addingComment)
                return;
            this._addingComment = true;
            this.PostCommentsVM.PostComment("", this._replyToCid, this._replyToUid, fromGroupChecked, new List<IOutboundAttachment>(), (Action<bool>)(result =>
            {
                this._addingComment = false;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (!result)
                    {
                        ExtendedMessageBox.ShowSafe(CommonResources.Error);
                    }
                    else
                    {
                        this.ScrollToBottom();
                        this.ResetReplyFields();
                    }
                }));
            }), message.StickerItem, message.Referrer);
        }

        public void Handle(SpriteElementTapEvent data)
        {
            if (!this._isCurrentPage)
                return;
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                int selectionStart = this.textBoxNewMessage.SelectionStart;
                this.textBoxNewMessage.Text = this.textBoxNewMessage.Text.Insert(selectionStart, data.Data.ElementCode);
                this.textBoxNewMessage.Select(selectionStart + data.Data.ElementCode.Length, 0);
            }));
        }

        public void Handle(WallPostPinnedUnpinned message)
        {
            if (message.OwnerId != this.CommonParameters.OwnerId || message.PostId != this.CommonParameters.PostId)
                return;
            this.Refresh();
        }

        public void InitiateShare()
        {
            this._appBarButtonShare_Click((object)this, (EventArgs)null);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/PostCommentsPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.scroll = (ViewportControl)this.FindName("scroll");
            this.scrollableGrid = (StackPanel)this.FindName("scrollableGrid");
            this.panel = (MyVirtualizingPanel2)this.FindName("panel");
            this.ucNewMessage = (NewMessageUC)this.FindName("ucNewMessage");
            this.Header = (GenericHeaderUC)this.FindName("Header");
            this.ucPullToRefresh = (PullToRefreshUC)this.FindName("ucPullToRefresh");
            this.ucMoreActions = (MoreActionsUC)this.FindName("ucMoreActions");
        }
    }
}
