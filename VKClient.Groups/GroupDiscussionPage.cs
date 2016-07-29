using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Groups
{
    public partial class GroupDiscussionPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>
    {
        private PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
        {
            ShowCamera = true
        };
        private ApplicationBarIconButton _appBarButtonAddComment = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative),
            Text = CommonResources.PostCommentsPage_AppBar_Send
        };
        private ApplicationBarIconButton _appBarButtonEmojiToggle = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.smile.png", UriKind.Relative),
            Text = "emoji"
        };
        private ApplicationBarIconButton _appBarButtonAttachments = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/attach.png", UriKind.Relative),
            Text = CommonResources.NewPost_AppBar_AddAttachment
        };
        private ApplicationBarIconButton _appBarButtonRefresh = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.refresh.rest.png", UriKind.Relative),
            Text = CommonResources.AppBar_Refresh
        };
        private ApplicationBar _defaultAppBar = new ApplicationBar()
        {
            BackgroundColor = VKConstants.AppBarBGColor,
            ForegroundColor = VKConstants.AppBarFGColor
        };
        private bool _isInitialized;
        private WallPostViewModel _commentVM;
        private ViewportScrollableAreaAdapter _adapter;
        private bool _isAddingComment;

        private GroupDiscussionViewModel GroupDiscussionVM
        {
            get
            {
                return this.DataContext as GroupDiscussionViewModel;
            }
        }

        public bool ReadyToSend
        {
            get
            {
                string text = this.newCommentUC.TextBoxNewComment.Text;
                ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
                if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
                    return true;
                if (outboundAttachments.Count > 0)
                    return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
                return false;
            }
        }

        public GroupDiscussionPage()
        {
            this.InitializeComponent();
            this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
            this.panel.InitializeWithScrollViewer((IScrollableArea)this._adapter, false);
            this.newCommentUC.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
            this.scroll.BindViewportBoundsTo((FrameworkElement)this.scrollablePanel);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.panel);
            this.BuildAppBar();
            this.newCommentUC.TextBoxNewComment.GotFocus += new RoutedEventHandler(this.textBoxGotFocus);
            this.newCommentUC.TextBoxNewComment.LostFocus += new RoutedEventHandler(this.textBoxLostFocus);
            this.newCommentUC.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
            this.panel.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.panel_ScrollPositionChanged);
            this.panel.ManuallyLoadMore = true;
            this.Loaded += new RoutedEventHandler(this.GroupDiscussionPage_Loaded);
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
            EventAggregator.Current.Subscribe((object)this);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.panel);
            this.panel.OnRefresh = (Action)(() => this.GroupDiscussionVM.LoadData(true, null));
            this.newCommentUC.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
            this.newCommentUC.OnAddAttachTap = (Action)(() => this.AddAttachTap());
            this.newCommentUC.OnSendTap = (Action)(() => this._appBarButtonAddComment_Click(null, null));
            this.newCommentUC.UCNewPost.OnImageDeleteTap = (Action<object>)(sender =>
            {
                FrameworkElement frameworkElement = sender as FrameworkElement;
                if (frameworkElement != null)
                    this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
                this.UpdateAppBar();
            });
            Binding binding = new Binding("OutboundAttachments");
            this.newCommentUC.UCNewPost.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
        }

        private void PanelIsOpenedChanged(object sender, bool e)
        {
            if (this.newCommentUC.PanelControl.IsOpen || this.newCommentUC.PanelControl.IsTextBoxTargetFocused)
                this.panel.ScrollTo(this._adapter.VerticalOffset + this.newCommentUC.PanelControl.PortraitOrientationHeight);
            else
                this.panel.ScrollTo(this._adapter.VerticalOffset - this.newCommentUC.PanelControl.PortraitOrientationHeight);
        }

        private void AddAttachTap()
        {
            AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action)(() =>
            {
                PostCommentsPage.HandleInputParams(this._commentVM);
                this.UpdateAppBar();
            }), true, 0, 0, null);
        }

        private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void _photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;
            ParametersRepository.SetParameterForId("ChoosenPhoto", (object)e.ChosenPhoto);
        }

        private void GroupDiscussionPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void panel_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
        {
            if (this.GroupDiscussionVM.LoadFromEnd)
            {
                if (e.ScrollHeight == 0.0 || e.CurrentPosition >= 100.0)
                    return;
                this.GroupDiscussionVM.LoadData(false, (Action<bool>)null);
            }
            else
            {
                if (e.ScrollHeight == 0.0 || e.ScrollHeight - e.CurrentPosition >= VKConstants.LoadMoreNewsThreshold)
                    return;
                this.GroupDiscussionVM.LoadData(false, (Action<bool>)null);
            }
        }

        private void BuildAppBar()
        {
            this._appBarButtonEmojiToggle.Click += new EventHandler(this._appBarButtonEmojiToggle_Click);
            this._appBarButtonAddComment.Click += new EventHandler(this._appBarButtonAddComment_Click);
            this._appBarButtonAttachments.Click += new EventHandler(this._appBarButtonAttachments_Click);
            this._appBarButtonRefresh.Click += new EventHandler(this._appBarButtonRefresh_Click);
            this._defaultAppBar.Buttons.Add((object)this._appBarButtonRefresh);
            this._defaultAppBar.Opacity = 0.9;
            this._defaultAppBar.StateChanged += new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged);
        }

        private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
        {
        }

        private void _defaultAppBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
        }

        private void _appBarButtonAttachments_Click(object sender, EventArgs e)
        {
        }

        private void _appBarButtonRefresh_Click(object sender, EventArgs e)
        {
            this.GroupDiscussionVM.LoadData(true, (Action<bool>)null);
        }

        private void _appBarButtonAddComment_Click(object sender, EventArgs e)
        {
            if (this._isAddingComment)
                return;
            string text = this.newCommentUC.TextBoxNewComment.Text;
            if (text.Length < 2 && this._commentVM.OutboundAttachments.Count <= 0)
                return;
            this._isAddingComment = true;
            this.GroupDiscussionVM.AddComment(text.Replace("\r\n", "\r").Replace("\r", "\r\n"), this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>)(res =>
            {
                this._isAddingComment = false;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (res)
                    {
                        this.newCommentUC.TextBoxNewComment.Text = string.Empty;
                        this.InitializeCommentVM();
                        this.UpdateAppBar();
                    }
                    else
                        ExtendedMessageBox.ShowSafe(CommonResources.Error);
                }));
            }), (StickerItemData)null, this.newCommentUC.FromGroupChecked, "");
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            bool flag1 = true;
            if (!this._isInitialized)
            {
                long gid = long.Parse(this.NavigationContext.QueryString["GroupId"]);
                long num1 = long.Parse(this.NavigationContext.QueryString["TopicId"]);
                string str = this.NavigationContext.QueryString["TopicName"];
                int num2 = int.Parse(this.NavigationContext.QueryString["KnownCommentsCount"]);
                bool flag2 = this.NavigationContext.QueryString["LoadFromTheEnd"] == bool.TrueString;
                bool flag3 = this.NavigationContext.QueryString["CanComment"] == bool.TrueString;
                long tid = num1;
                string topicName = str;
                int knownCommentsCount = num2;
                int num3 = flag3 ? 1 : 0;
                MyVirtualizingPanel2 virtPanel = this.panel;
                int num4 = flag2 ? 1 : 0;
                Action<CommentItem> replyCallback = new Action<CommentItem>(this.replyCallback);
                GroupDiscussionViewModel discussionViewModel = new GroupDiscussionViewModel(gid, tid, topicName, knownCommentsCount, num3 != 0, virtPanel, num4 != 0, replyCallback);
                this.InitializeCommentVM();
                this.DataContext = (object)discussionViewModel;
                discussionViewModel.LoadData(false, new Action<bool>(this.LoadedCallback));
                this.UpdateAppBar();
                this.RestoreUnboundState();
                this._isInitialized = true;
                flag1 = false;
            }
            if (!flag1 && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
                WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
            this.ProcessInputData();
            this.UpdateAppBar();
        }

        private void LoadedCallback(bool success)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                Group cachedGroup = GroupsService.Current.GetCachedGroup(this.GroupDiscussionVM.GroupId);
                if (cachedGroup == null)
                    return;
                this.newCommentUC.SetAdminLevel(cachedGroup.admin_level);
            }));
        }

        private void replyCallback(CommentItem obj)
        {
            this.newCommentUC.TextBoxNewComment.Text += string.Format("[post{0}|{1}], ", (object)obj.Comment.cid, (object)obj.NameWithoutLastName);
            this.newCommentUC.TextBoxNewComment.Focus();
            this.newCommentUC.TextBoxNewComment.Select(this.newCommentUC.TextBoxNewComment.Text.Length, 0);
        }

        private void ProcessInputData()
        {
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
                this.GroupDiscussionVM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
                this._commentVM.UploadAttachments();
            }
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if ((continuationEventArgs == null || !((IEnumerable<StorageFile>)continuationEventArgs.Files).Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocument"))
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

        private void InitializeCommentVM()
        {
            this._commentVM = WallPostViewModel.CreateNewDiscussionCommentVM();
            this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
            this.newCommentUC.DataContext = (object)this._commentVM;
        }

        private void _commentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender != this._commentVM || !(e.PropertyName == "CanPublish"))
                return;
            this.UpdateAppBar();
            ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
            Func<IOutboundAttachment, bool> predicate = (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Uploading);
            //Func<IOutboundAttachment, bool> predicate=null;
            if (outboundAttachments.Any<IOutboundAttachment>(predicate))
                return;
            this.GroupDiscussionVM.SetInProgress(false, "");
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            if (e.NavigationMode != NavigationMode.Back)
                WallPostVMCacheManager.RegisterForDelayedSerialization(this._commentVM);
            if (e.NavigationMode == NavigationMode.Back)
                WallPostVMCacheManager.ResetInstance();
            this.SaveUnboundState();
        }

        private void SaveUnboundState()
        {
            this.State["CommentText"] = (object)this.newCommentUC.TextBoxNewComment.Text;
        }

        private void RestoreUnboundState()
        {
            if (!this.State.ContainsKey("CommentText"))
                return;
            this.newCommentUC.TextBoxNewComment.Text = this.State["CommentText"].ToString();
        }

        private void UpdateAppBar()
        {
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
                return;
            if (this.GroupDiscussionVM.CanComment && !this._defaultAppBar.Buttons.Contains((object)this._appBarButtonAddComment))
            {
                this._defaultAppBar.Buttons.Insert(0, (object)this._appBarButtonAddComment);
                if (!this._defaultAppBar.Buttons.Contains((object)this._appBarButtonEmojiToggle))
                    this._defaultAppBar.Buttons.Insert(1, (object)this._appBarButtonEmojiToggle);
            }
            if (this.GroupDiscussionVM.CanComment && !this._defaultAppBar.Buttons.Contains((object)this._appBarButtonAttachments))
                this._defaultAppBar.Buttons.Insert(2, (object)this._appBarButtonAttachments);
            this._appBarButtonAddComment.IsEnabled = this.ReadyToSend;
            this.newCommentUC.UpdateSendButton(this.ReadyToSend && this.GroupDiscussionVM.CanComment);
            int count = this._commentVM.OutboundAttachments.Count;
            if (count > 0)
                this._appBarButtonAttachments.IconUri = new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", (object)Math.Min(count, 10)), UriKind.Relative);
            else
                this._appBarButtonAttachments.IconUri = new Uri("Resources/attach.png", UriKind.Relative);
        }

        private void textBoxGotFocus(object sender, RoutedEventArgs e)
        {
            this.GroupDiscussionVM.EnsureLoadFromEnd();
        }

        private void textBoxLostFocus(object sender, RoutedEventArgs e)
        {
        }

        public void Handle(StickerItemTapEvent message)
        {
            if (!this._isCurrentPage || this._isAddingComment)
                return;
            this._isAddingComment = true;
            this.GroupDiscussionVM.AddComment("", this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>)(res =>
            {
                this._isAddingComment = false;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (res)
                        return;
                    ExtendedMessageBox.ShowSafe(CommonResources.Error);
                }));
            }), message.StickerItem, this.newCommentUC.FromGroupChecked, message.Referrer);
        }

        public void Handle(SpriteElementTapEvent data)
        {
            if (!this._isCurrentPage)
                return;
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                TextBox textBoxNewComment = this.newCommentUC.TextBoxNewComment;
                int selectionStart = textBoxNewComment.SelectionStart;
                string str = textBoxNewComment.Text.Insert(selectionStart, data.Data.ElementCode);
                textBoxNewComment.Text = str;
                int start = selectionStart + data.Data.ElementCode.Length;
                int length = 0;
                textBoxNewComment.Select(start, length);
            }));
        }
    }
}
