using Microsoft.Phone.Controls;
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
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Common;
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
using VKClient.Video.Library;
using VKClient.Video.Localization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Video
{
    public partial class VideoCommentsPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, ISupportShare
    {
        private readonly List<PhotoVideoTag> _tags = new List<PhotoVideoTag>();
        private readonly PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
        {
            ShowCamera = true
        };
        private readonly ApplicationBar _appBar = new ApplicationBar()
        {
            BackgroundColor = VKConstants.AppBarBGColor,
            ForegroundColor = VKConstants.AppBarFGColor
        };
        private readonly ApplicationBarIconButton _appBarButtonComment = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative),
            Text = CommonResources.PostCommentsPage_AppBar_Send
        };
        private readonly ApplicationBarIconButton _appBarButtonEmojiToggle = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.smile.png", UriKind.Relative),
            Text = "emoji"
        };
        private readonly ApplicationBarIconButton _appBarButtonAttachments = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/attach.png", UriKind.Relative),
            Text = CommonResources.NewPost_AppBar_AddAttachment
        };
        private readonly ApplicationBarIconButton _appBarButtonLikeUnlike = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.heart2.rest.png", UriKind.Relative),
            Text = CommonResources.PostCommentsPage_AppBar_Like
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemAddDelete = new ApplicationBarMenuItem()
        {
            Text = VideoResources.VideoComments_AppBar_AddToVideos
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemReport = new ApplicationBarMenuItem()
        {
            Text = CommonResources.Report
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemShare = new ApplicationBarMenuItem()
        {
            Text = CommonResources.PostCommentsPage_AppBar_Share
        };
        private readonly ApplicationBarMenuItem _appBarMenuItemEdit = new ApplicationBarMenuItem()
        {
            Text = CommonResources.Edit
        };
        private DialogService _ds = new DialogService();
        private bool _isInitialized;
        private bool _textsGenerated;
        private WallPostViewModel _commentVM;
        private const string LikeHeartImagePath = "Resources/appbar.heart2.rest.png";
        private const string UnlikeHeartImagePath = "Resources/appbar.heart2.broken.rest.png";
        //private bool _isAddedOrDeleted;
        private SharePostUC _sharePostUC;
        private ViewportScrollableAreaAdapter _adapter;
        private long _ownerId;
        private long _videoId;

        private VideoCommentsViewModel VM
        {
            get
            {
                return this.DataContext as VideoCommentsViewModel;
            }
        }

        private bool IsSelfOwner
        {
            get
            {
                return this.VM.OwnerId == AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public bool ReadyToSend
        {
            get
            {
                string text = this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
                ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
                if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
                    return true;
                if (outboundAttachments.Count > 0)
                    return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
                return false;
            }
        }

        public VideoCommentsPage()
        {
            this.InitializeComponent();
            this.ucMoreActions.SetBlue();
            this.ucMoreActions.TapCallback = new Action(this.ShowContextMenu);
            this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
            this.ucCommentGeneric.InitializeWithScrollViewer((IScrollableArea)this._adapter);
            this.ucCommentGeneric.UCNewComment = this.ucNewMessage;
            this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
            this.ucNewMessage.OnAddAttachTap = (Action)(() => this.AddAttachTap());
            this.ucNewMessage.OnSendTap = (Action)(() => this._appBarButtonSend_Click((object)null, (EventArgs)null));
            this.ucNewMessage.UCNewPost.OnImageDeleteTap = (Action<object>)(sender =>
            {
                FrameworkElement frameworkElement = sender as FrameworkElement;
                if (frameworkElement != null)
                    this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
                this.UpdateAppBar();
            });
            this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
            Binding binding = new Binding("OutboundAttachments");
            this.ucNewMessage.UCNewPost.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            this.scroll.BindViewportBoundsTo((FrameworkElement)this.stackPanel);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.ucCommentGeneric.Panel);
            this.CreateAppBar();
            this.ucCommentGeneric.UCNewComment.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void CreateAppBar()
        {
            this._appBarButtonComment.Click += new EventHandler(this._appBarButtonSend_Click);
            this._appBarButtonAttachments.Click += new EventHandler(this._appBarButtonAttachments_Click);
            this._appBarButtonLikeUnlike.Click += new EventHandler(this._appBarButtonLikeUnlike_Click);
            this._appBarMenuItemReport.Click += new EventHandler(this._appBarMenuItemReport_Click);
            this._appBarMenuItemShare.Click += new EventHandler(this._appBarButtonShare_Click);
            this._appBarButtonEmojiToggle.Click += new EventHandler(this._appBarButtonEmojiToggle_Click);
            this._appBarMenuItemEdit.Click += new EventHandler(this._appBarMenuItemEdit_Click);
            this._appBar.Buttons.Add((object)this._appBarButtonComment);
            this._appBar.Buttons.Add((object)this._appBarButtonEmojiToggle);
            this._appBar.Buttons.Add((object)this._appBarButtonAttachments);
            this._appBar.Buttons.Add((object)this._appBarButtonLikeUnlike);
            this._appBar.MenuItems.Add((object)this._appBarMenuItemShare);
            this._appBar.MenuItems.Add((object)this._appBarMenuItemAddDelete);
            this._appBar.Opacity = 0.9;
        }

        private void _appBarMenuItemEdit_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToEditVideo(this.VM.OwnerId, this.VM.VideoId, this.VM.Video);
        }

        private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
        {
        }

        private void _appBarMenuItemReport_Click(object sender, EventArgs e)
        {
            ReportContentHelper.ReportVideo(this.VM.OwnerId, this.VM.VideoId);
        }

        private void _appBarButtonAttachments_Click(object sender, EventArgs e)
        {
            if (this._commentVM.OutboundAttachments.Count == 0)
            {
                PickerUC.PickAttachmentTypeAndNavigate(AttachmentTypes.AttachmentTypesWithPhotoFromGallery, null, (Action)(() => Navigator.Current.NavigateToPhotoPickerPhotos(2, false, false)));
            }
            else
            {
                ParametersRepository.SetParameterForId("NewCommentVM", (object)this._commentVM);
                Navigator.Current.NavigateToNewWallPost(0L, false, 0, false, false, false);
            }
        }

        private void _appBarButtonShare_Click(object sender, EventArgs e)
        {
            this._ds = new DialogService();
            this._ds.SetStatusBarBackground = false;
            this._ds.HideOnNavigation = false;
            this._sharePostUC = new SharePostUC();
            this._sharePostUC.SendTap += this.ButtonSend_Click;
            this._sharePostUC.ShareTap += this.ButtonShare_Click;
            this._ds.Child = (FrameworkElement)this._sharePostUC;
            this._ds.AnimationType = DialogService.AnimationTypes.None;
            this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
            this._ds.Show(null);
        }

        private void ButtonShare_Click(object sender, EventArgs eventArgs)
        {
            this.Share(0L, "");
        }

        private void Share(long gid = 0, string groupName = "")
        {
            if (this.VM.Video == null)
                return;
            this._ds.Hide();
            this.VM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
        }

        private void ButtonSend_Click(object sender, EventArgs eventArgs)
        {
            if (this.VM.Video == null)
                return;
            this._ds.Hide();
            ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
            contentDataProvider.Message = this._sharePostUC.Text;
            contentDataProvider.Video = this.VM.Video;
            contentDataProvider.StoreDataToRepository();
            ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
            Navigator.Current.NavigateToPickConversation();
        }

        private void _appBarButtonLikeUnlike_Click(object sender, EventArgs e)
        {
            this.VM.LikeUnlike();
            this.ucCommentGeneric.UpdateLikesItem(this.VM.UserLiked);
            this.UpdateAppBar();
        }

        private void _appBarButtonSend_Click(object sender, EventArgs e)
        {
            this.ucCommentGeneric.AddComment(this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!res)
                    return;
                this.InitializeCommentVM();
                this.UpdateAppBar();
            }))), (StickerItemData)null, "");
        }

        private void UpdateAppBar()
        {
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
                return;
            if (this.VM.UserLiked)
            {
                this._appBarButtonLikeUnlike.IconUri = new Uri("Resources/appbar.heart2.broken.rest.png", UriKind.Relative);
                this._appBarButtonLikeUnlike.Text = CommonResources.PostCommentsPage_AppBar_Unlike;
            }
            else
            {
                this._appBarButtonLikeUnlike.IconUri = new Uri("Resources/appbar.heart2.rest.png", UriKind.Relative);
                this._appBarButtonLikeUnlike.Text = CommonResources.PostCommentsPage_AppBar_Like;
            }
            //if (!this._isAddedOrDeleted)//TODO: undone by author
            //{
                this._appBarMenuItemAddDelete.Text = !this.IsSelfOwner ? CommonResources.AppBar_Add : CommonResources.Delete;
                if (!this._appBar.MenuItems.Contains((object)this._appBarMenuItemAddDelete))
                    this._appBar.MenuItems.Add((object)this._appBarMenuItemAddDelete);
            //}
            //else if (this._appBar.MenuItems.Contains((object)this._appBarMenuItemAddDelete))
            //    this._appBar.MenuItems.Remove((object)this._appBarMenuItemAddDelete);
            if (this.VM.CanReport && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemReport))
                this._appBar.MenuItems.Add((object)this._appBarMenuItemReport);
            if (this.VM.CanEdit && !this._appBar.MenuItems.Contains((object)this._appBarMenuItemEdit))
            {
                if (this._appBar.MenuItems.Count >= 1)
                    this._appBar.MenuItems.Insert(1, (object)this._appBarMenuItemEdit);
                else
                    this._appBar.MenuItems.Add((object)this._appBarMenuItemEdit);
            }
            this._appBarButtonComment.IsEnabled = this.VM.CanComment && this.ReadyToSend;
            this.ucNewMessage.UpdateSendButton(this._appBarButtonComment.IsEnabled);
            this._appBarButtonAttachments.IsEnabled = this.VM.CanComment;
            int count = this._commentVM.OutboundAttachments.Count;
            if (count > 0)
                this._appBarButtonAttachments.IconUri = new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", (object)Math.Min(count, 10)), UriKind.Relative);
            else
                this._appBarButtonAttachments.IconUri = new Uri("Resources/attach.png", UriKind.Relative);
        }

        private void GridContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = e.NewSize.Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            this.canvasBackground.Height = height;
            this.canvasBackground.Children.Clear();
            Rectangle rect = new Rectangle();
            double num = height;
            rect.Height = num;
            Thickness thickness = new Thickness(0.0);
            rect.Margin = thickness;
            double width = e.NewSize.Width;
            rect.Width = width;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            rect.Fill = (Brush)solidColorBrush;
            foreach (UIElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
                this.canvasBackground.Children.Add(coverByRectangle);
        }

        private void StackPanelProductInfo_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = e.NewSize.Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            this.ucCommentGeneric.Panel.DeltaOffset = -height;
        }

        private void ShowContextMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            if (this.VM.CanEdit)
            {
                MenuItem menuItem1 = new MenuItem();
                string lowerInvariant = CommonResources.Edit.ToLowerInvariant();
                menuItem1.Header = (object)lowerInvariant;
                MenuItem menuItem2 = menuItem1;
                menuItem2.Click += new RoutedEventHandler(this.mItemEdit_Click);
                menuItems.Add(menuItem2);
            }
            if (this.VM.CanAddToMyVideos)
            {
                MenuItem menuItem1 = new MenuItem();
                string newAddToMyVideos = CommonResources.VideoNew_AddToMyVideos;
                menuItem1.Header = (object)newAddToMyVideos;
                MenuItem menuItem2 = menuItem1;
                menuItem2.Click += new RoutedEventHandler(this.mItemAddToMyVideos_Click);
                menuItems.Add(menuItem2);
            }
            MenuItem menuItem3 = new MenuItem();
            string lowerInvariant1 = CommonResources.VideoNew_AddToAlbum.ToLowerInvariant();
            menuItem3.Header = (object)lowerInvariant1;
            MenuItem menuItem4 = menuItem3;
            menuItem4.Click += new RoutedEventHandler(this.mItemAddToAlbum_Click);
            menuItems.Add(menuItem4);
            MenuItem menuItem5 = new MenuItem();
            string lowerInvariant2 = CommonResources.OpenInBrowser.ToLowerInvariant();
            menuItem5.Header = (object)lowerInvariant2;
            MenuItem menuItem6 = menuItem5;
            menuItem6.Click += new RoutedEventHandler(this.mItemOpenInBrowser_Click);
            menuItems.Add(menuItem6);
            MenuItem menuItem7 = new MenuItem();
            string lowerInvariant3 = CommonResources.CopyLink.ToLowerInvariant();
            menuItem7.Header = (object)lowerInvariant3;
            MenuItem menuItem8 = menuItem7;
            menuItem8.Click += new RoutedEventHandler(this.mItemCopyLink_Click);
            menuItems.Add(menuItem8);
            MenuItem menuItem9 = new MenuItem();
            string lowerInvariant4 = CommonResources.Report.ToLowerInvariant();
            menuItem9.Header = (object)lowerInvariant4;
            MenuItem menuItem10 = menuItem9;
            menuItem10.Click += new RoutedEventHandler(this.mItemReport_Click);
            menuItems.Add(menuItem10);
            if (this.VM.CanRemoveFromMyVideos)
            {
                MenuItem menuItem1 = new MenuItem();
                string removedFromMyVideos = CommonResources.VideoNew_RemovedFromMyVideos;
                menuItem1.Header = (object)removedFromMyVideos;
                MenuItem menuItem2 = menuItem1;
                menuItem2.Click += new RoutedEventHandler(this.mItemRemoveFromMyVideos_Click);
                menuItems.Add(menuItem2);
            }
            if (this.VM.CanDelete)
            {
                MenuItem menuItem1 = new MenuItem();
                string lowerInvariant5 = CommonResources.Delete.ToLowerInvariant();
                menuItem1.Header = (object)lowerInvariant5;
                MenuItem menuItem2 = menuItem1;
                menuItem2.Click += new RoutedEventHandler(this.mItemDelete_Click);
                menuItems.Add(menuItem2);
            }
            this.ucMoreActions.SetMenu(menuItems);
            this.ucMoreActions.ShowMenu();
        }

        private void mItemAddToAlbum_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToAddVideoToAlbum(this._ownerId, this._videoId);
        }

        private void mItemReport_Click(object sender, RoutedEventArgs e)
        {
            this.ReportVideo();
        }

        private void ReportVideo()
        {
            ReportContentHelper.ReportVideo(this.VM.OwnerId, this.VM.VideoId);
        }

        private void mItemCopyLink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.VM.VideoUri);
        }

        private void mItemOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToWebUri(this.VM.VideoUri, true, false);
        }

        private void mItemRemoveFromMyVideos_Click(object sender, RoutedEventArgs e)
        {
            this.VM.AddRemoveToMyVideos();
        }

        private void mItemAddToMyVideos_Click(object sender, RoutedEventArgs e)
        {
            this.VM.AddRemoveToMyVideos();
        }

        private void mItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(CommonResources.DeleteConfirmation, VideoResources.DeleteVideo, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this.VM.Delete((Action<ResultCode>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (res == ResultCode.Succeeded)
                    Navigator.Current.GoBack();
                else
                    GenericInfoUC.ShowBasedOnResult((int)res, "", (VKRequestsDispatcher.Error)null);
            }))));
        }

        private void mItemEdit_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToEditVideo(this.VM.OwnerId, this.VM.VideoId, this.VM.Video);
        }

        private void PanelIsOpenedChanged(object sender, bool e)
        {
            if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
                this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
            else
                this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
        }

        private void AddAttachTap()
        {
            AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action)(() =>
            {
                PostCommentsPage.HandleInputParams(this._commentVM);
                this.UpdateAppBar();
            }), true, 0L, 0);
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

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            bool flag = true;
            if (!this._isInitialized)
            {
                this._ownerId = long.Parse(this.NavigationContext.QueryString["OwnerId"]);
                this._videoId = long.Parse(this.NavigationContext.QueryString["VideoId"]);
                string accessKey = this.NavigationContext.QueryString["AccessKey"];
                string videoContext = "";
                if (this.NavigationContext.QueryString.ContainsKey("VideoContext"))
                    videoContext = this.NavigationContext.QueryString["VideoContext"];
                VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("Video") as VKClient.Common.Backend.DataObjects.Video;
                StatisticsActionSource actionSource = (StatisticsActionSource)Enum.Parse(typeof(StatisticsActionSource), this.NavigationContext.QueryString["VideoSource"]);
                this.InitializeCommentVM();
                VideoCommentsViewModel commentsViewModel = new VideoCommentsViewModel(this._ownerId, this._videoId, accessKey, video, actionSource, videoContext);
                commentsViewModel.PageLoadInfoViewModel.LoadingStateChangedCallback = new Action(this.OnLoadingStateChanged);
                this.DataContext = (object)commentsViewModel;
                commentsViewModel.Reload();
                this.RestoreUnboundState();
                this._isInitialized = true;
                flag = false;
            }
            if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
                WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
            this.ProcessInputData();
            this.UpdateAppBar();
        }

        private void OnLoadingStateChanged()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.VM.PageLoadInfoViewModel.LoadingState != PageLoadingState.Loaded)
                    return;
                this.VM.LoadMoreComments(7, new Action<bool>(this.CommentsAreLoadedCallback));
            }));
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
                this.VM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
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
            this._commentVM = WallPostViewModel.CreateNewVideoCommentVM(this._ownerId, this._videoId);
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
            if (outboundAttachments.Any<IOutboundAttachment>(predicate))
                return;
            this.VM.SetInProgress(false, "");
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
            this.State["CommentText"] = (object)this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
        }

        private void RestoreUnboundState()
        {
            if (!this.State.ContainsKey("CommentText"))
                return;
            this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text = this.State["CommentText"].ToString();
        }

        private void CommentsAreLoadedCallback(bool success)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.VM.PageLoadInfoViewModel.LoadingState == PageLoadingState.LoadingFailed)
                    return;
                if (!this._textsGenerated)
                    this._textsGenerated = true;
                this.ucCommentGeneric.ProcessLoadedComments(true);
                this.UpdateAppBar();
                if (this._ownerId >= 0)
                    return;
                Group cachedGroup = GroupsService.Current.GetCachedGroup(-this._ownerId);
                if (cachedGroup == null)
                    return;
                this.ucNewMessage.SetAdminLevel(cachedGroup.admin_level);
            }));
        }

        private void Play_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.PlayVideo();
        }

        protected override void TextBoxPanelIsOpenedChanged(object sender, bool e)
        {
            this.UpdateAppBar();
        }

        public void Handle(SpriteElementTapEvent data)
        {
            if (!this._isCurrentPage)
                return;
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                TextBox textBoxNewComment = this.ucCommentGeneric.UCNewComment.TextBoxNewComment;
                int selectionStart = textBoxNewComment.SelectionStart;
                string str = textBoxNewComment.Text.Insert(selectionStart, data.Data.ElementCode);
                textBoxNewComment.Text = str;
                int start = selectionStart + data.Data.ElementCode.Length;
                int length = 0;
                textBoxNewComment.Select(start, length);
            }));
        }

        public void Handle(StickerItemTapEvent message)
        {
            if (!this._isCurrentPage)
                return;
            this.ucCommentGeneric.AddComment(new List<IOutboundAttachment>(), (Action<bool>)(res => { }), message.StickerItem, message.Referrer);
        }

        public void InitiateShare()
        {
            this._appBarButtonShare_Click((object)this, (EventArgs)null);
        }

        private void TextBlockMetaData_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.VM == null || string.IsNullOrEmpty(this.VM.MetaDataStr) || string.IsNullOrEmpty(this.textBlockMetaData.Text) || e.NewSize.Height <= this.textBlockMetaData.LineHeight)
                return;
            this.textBlockMetaData.Text = this.VM.MetaDataStr.Replace(" · ", "\n");
        }

        private void Description_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.ExpandDescription();
        }
    }
}
