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
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Common
{
    public class NewPost : PageBase
    {
        private PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
        {
            ShowCamera = true
        };
        private ApplicationBarIconButton _appBarButtonSend = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative),
            Text = CommonResources.NewPost_Send
        };
        private ApplicationBarIconButton _appBarButtonAttachImage = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative),
            Text = CommonResources.NewPost_AppBar_AddPhoto
        };
        private ApplicationBarIconButton _appBarButtonAttachLocation = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.checkin.rest.png", UriKind.Relative),
            Text = CommonResources.NewPost_AppBar_AddLocation
        };
        private ApplicationBarIconButton _appBarButtonAddAttachment = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/attach.png", UriKind.Relative),
            Text = CommonResources.NewPost_AppBar_AddAttachment
        };
        private DelayedExecutor _de = new DelayedExecutor(100);
        private bool _isInitialized;
        private AttachmentPickerUC _pickerUC;
        private Point _textBoxTapPoint;
        private bool _excludeLocation;
        private bool _isPublishing;
        private bool _published;
        private bool _fromPhotoPicker;
        private bool _isForwardNav;
        private bool _isFromWallPostPage;
        private int _adminLevel;
        private IShareContentDataProvider _shareContentDataProvider;
        //private double savedHeight;
        internal Grid LayoutRoot;
        internal GenericHeaderUC ucHeader;
        internal Grid ContentPanel;
        internal ScrollViewer scroll;
        internal StackPanel stackPanel;
        internal TextBox textBoxTopicTitle;
        internal TextBlock textBlockWatermarkTitle;
        internal NewPostUC ucNewPost;
        internal Border wallRepostContainer;
        internal CheckBox checkBoxFriendsOnly;
        internal TextBoxPanelControl textBoxPanel;
        private bool _contentLoaded;

        protected WallPostViewModel WallPostVM
        {
            get
            {
                return this.DataContext as WallPostViewModel;
            }
        }

        private TextBox textBoxPost
        {
            get
            {
                return this.ucNewPost.TextBoxPost;
            }
        }

        private TextBlock textBlockWatermarkText
        {
            get
            {
                return this.ucNewPost.TextBlockWatermarkText;
            }
        }

        public NewPost()
        {
            this.InitializeComponent();
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
            this.BuildAppBar();
            this.ucNewPost.textBoxPost.TextChanged += new TextChangedEventHandler(this.textBoxPost_TextChanged_1);
            this.ucNewPost.textBoxPost.GotFocus += new RoutedEventHandler(this.TextBox_OnGotFocus);
            this.ucNewPost.textBoxPost.LostFocus += new RoutedEventHandler(this.TextBox_OnLostFocus);
            this.ucNewPost.textBoxPost.Tap += new EventHandler<GestureEventArgs>(this.TextBoxPost_OnTap);
            this.ucNewPost.OnImageDeleteTap = (Action<object>)(sender => this.Image_Delete_Tap(sender, null));
            this.ucNewPost.OnAddAttachmentTap = (Action)(() => this.AddAttachmentTap(null, null));
        }

        private void TextBoxPost_OnTap(object sender, GestureEventArgs e)
        {
            this._textBoxTapPoint = e.GetPosition((UIElement)this.stackPanel);
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.textBoxPanel.IsOpen = true;
            FrameworkElement element = (FrameworkElement)sender;
            if (element.Name == this.textBoxTopicTitle.Name)
            {
                this.scroll.ScrollToVerticalOffset(0.0);
                this.UpdateLayout();
                this.scroll.ScrollToOffsetWithAnimation(element.GetRelativePosition((UIElement)this.stackPanel).Y, 0.2, false);
            }
            else
            {
                this.scroll.ScrollToVerticalOffset(0.0);
                this.UpdateLayout();
                this.scroll.ScrollToOffsetWithAnimation(this._textBoxTapPoint.Y - 40.0, 0.2, false);
            }
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.textBoxPanel.IsOpen = false;
        }

        private void _photoChooserTask_Completed(object sender, PhotoResult e)
        {
            Logger.Instance.Info("Back from photo chooser");
            if (e.TaskResult != TaskResult.OK)
                return;
            ParametersRepository.SetParameterForId("ChoosenPhoto", (object)e.ChosenPhoto);
        }

        private void BuildAppBar()
        {
            ApplicationBar applicationBar = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor,
                Opacity = 0.9
            };
            applicationBar.Buttons.Add((object)this._appBarButtonSend);
            this._appBarButtonSend.Click += new EventHandler(this._appBarButtonSend_Click);
            this.ApplicationBar = (IApplicationBar)applicationBar;
        }

        private void _appBarButtonAddAttachment_Click(object sender, EventArgs e)
        {
            this.Focus();
            this._pickerUC = AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, 10, (Action)(() => this.HandleInputParams(null)), this._excludeLocation, this.CommonParameters.IsGroup ? -this.CommonParameters.UserOrGroupId : this.CommonParameters.UserOrGroupId, 0, null);
        }

        private void _appBarButtonAttachLocation_Click(object sender, EventArgs e)
        {
            this.WallPostVM.GoDirectlyToPhotoChooser = false;
            Navigator.Current.NavigateToMap(true, 0.0, 0.0);
        }

        private void _appBarButtonAttachImage_Click(object sender, EventArgs e)
        {
            this.ShowPhotoChooser(false);
        }

        private void ShowPhotoChooser(bool goDirectly = false)
        {
            this.WallPostVM.GoDirectlyToPhotoChooser = goDirectly;
            Navigator.Current.NavigateToPhotoPickerPhotos(this.WallPostVM.NumberOfAttAllowedToAdd, false, false);
        }

        private void _appBarButtonSend_Click(object sender, EventArgs e)
        {
            this._isPublishing = true;
            this.UpdateViewState();
            this.WallPostVM.Publish((Action<ResultCode>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this._isPublishing = false;
                if (res == ResultCode.Succeeded)
                {
                    this._published = true;
                    WallPostVMCacheManager.ResetVM(this.WallPostVM);
                    if (this._shareContentDataProvider is ShareExternalContentDataProvider)
                    {
                        ((ShareExternalContentDataProvider)this._shareContentDataProvider).ShareOperation.ReportCompleted();
                    }
                    else
                    {
                        if (this.WallPostVM.WMMode == WallPostViewModel.Mode.PublishWallPost && this._isFromWallPostPage && this.NavigationService.BackStack.Any<JournalEntry>())
                            this.NavigationService.RemoveBackEntrySafe();
                        Navigator.Current.GoBack();
                    }
                }
                else if (res == ResultCode.PostsLimitOrAlreadyScheduled)
                {
                    ObservableCollection<IOutboundAttachment> outboundAttachments = this.WallPostVM.OutboundAttachments;
                    Func<IOutboundAttachment, bool> func = (Func<IOutboundAttachment, bool>)(a => a.AttachmentId == "timestamp");

                    Func<IOutboundAttachment, bool> predicate = new Func<IOutboundAttachment, bool>(a => { return a.AttachmentId == "timestamp"; });

                    if (outboundAttachments.FirstOrDefault<IOutboundAttachment>(predicate) != null)
                    {
                        this.UpdateViewState();
                        new GenericInfoUC(2000).ShowAndHideLater(CommonResources.ScheduledForExistingTime, null);
                    }
                    else
                    {
                        this.UpdateViewState();
                        new GenericInfoUC(2000).ShowAndHideLater(CommonResources.PostsLimitReached, null);
                    }
                }
                else
                {
                    this.UpdateViewState();
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
                }
            }))));
        }

        private void UpdateAppBar()
        {
            this._appBarButtonSend.IsEnabled = this.WallPostVM.CanPublish && !this._isPublishing;
            this._appBarButtonAttachImage.IsEnabled = !this._isPublishing;
            int num;
            if (!this._isPublishing)
            {
                ObservableCollection<IOutboundAttachment> outboundAttachments = this.WallPostVM.OutboundAttachments;
                Func<IOutboundAttachment, bool> predicate = (Func<IOutboundAttachment, bool>)(a => a.IsGeo);
                if (!outboundAttachments.Any<IOutboundAttachment>(predicate))
                {
                    num = this.WallPostVM.EditWallRepost ? 1 : 0;
                    goto label_4;
                }
            }
            num = 1;
        label_4:
            this._excludeLocation = num != 0;
            this._appBarButtonAddAttachment.IsEnabled = !this._isPublishing;
            switch (this.WallPostVM.WMMode)
            {
                case WallPostViewModel.Mode.NewWallComment:
                case WallPostViewModel.Mode.NewPhotoComment:
                case WallPostViewModel.Mode.NewVideoComment:
                case WallPostViewModel.Mode.NewDiscussionComment:
                case WallPostViewModel.Mode.NewProductComment:
                    this._excludeLocation = true;
                    this.ApplicationBar.Buttons.Remove((object)this._appBarButtonSend);
                    break;
                case WallPostViewModel.Mode.EditWallComment:
                case WallPostViewModel.Mode.EditPhotoComment:
                case WallPostViewModel.Mode.EditVideoComment:
                case WallPostViewModel.Mode.EditDiscussionComment:
                case WallPostViewModel.Mode.NewTopic:
                case WallPostViewModel.Mode.EditProductComment:
                    this._excludeLocation = true;
                    break;
            }
            this._appBarButtonAttachImage.IsEnabled = this.WallPostVM.CanAddMoreAttachments;
            this._appBarButtonAddAttachment.IsEnabled = this.WallPostVM.CanAddMoreAttachments;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            this._fromPhotoPicker = false;
            this._isForwardNav = e.NavigationMode == NavigationMode.New;
            if (ParametersRepository.Contains("FromPhotoPicker"))
                this._fromPhotoPicker = (bool)ParametersRepository.GetParameterForIdAndReset("FromPhotoPicker");
            if (!this._isInitialized)
            {
                this._adminLevel = int.Parse(this.NavigationContext.QueryString["AdminLevel"]);
                bool isPublicPage = this.NavigationContext.QueryString["IsPublicPage"] == bool.TrueString;
                bool isNewTopicMode = this.NavigationContext.QueryString["IsNewTopicMode"] == bool.TrueString;
                this._isFromWallPostPage = this.NavigationContext.QueryString["FromWallPostPage"] == bool.TrueString;
                WallPostViewModel.Mode result;
                Enum.TryParse<WallPostViewModel.Mode>(this.NavigationContext.QueryString["Mode"], out result);
                WallPost wallPost1 = ParametersRepository.GetParameterForIdAndReset("PublishWallPost") as WallPost;
                WallPost wallPost2 = ParametersRepository.GetParameterForIdAndReset("EditWallPost") as WallPost;
                Comment comment1 = ParametersRepository.GetParameterForIdAndReset("EditWallComment") as Comment;
                Comment comment2 = ParametersRepository.GetParameterForIdAndReset("EditPhotoComment") as Comment;
                Comment comment3 = ParametersRepository.GetParameterForIdAndReset("EditVideoComment") as Comment;
                Comment comment4 = ParametersRepository.GetParameterForIdAndReset("EditProductComment") as Comment;
                Comment comment5 = ParametersRepository.GetParameterForIdAndReset("EditDiscussionComment") as Comment;
                Dictionary<long, long> cidToAuthorIdDict = ParametersRepository.GetParameterForIdAndReset("CidToAuthorIdDict") as Dictionary<long, long>;
                WallRepostInfo wallRepostInfo = ParametersRepository.GetParameterForIdAndReset("WallRepostInfo") as WallRepostInfo;
                WallPostViewModel wallPostViewModel = ParametersRepository.GetParameterForIdAndReset("NewCommentVM") as WallPostViewModel;
                this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
                if (this._shareContentDataProvider is ShareExternalContentDataProvider)
                {
                    this.NavigationService.ClearBackStack();
                    this.ucHeader.HideSandwitchButton = true;
                    this.SuppressMenu = true;
                }
                WallPostViewModel vm;
                if (wallPost1 != null)
                    vm = new WallPostViewModel(wallPost1, this._adminLevel, (WallRepostInfo)null)
                    {
                        WMMode = WallPostViewModel.Mode.PublishWallPost
                    };
                else if (wallPost2 != null)
                {
                    vm = new WallPostViewModel(wallPost2, this._adminLevel, wallRepostInfo)
                    {
                        WMMode = WallPostViewModel.Mode.EditWallPost
                    };
                    if (vm.WallRepostInfo != null)
                    {
                        RepostHeaderUC repostHeaderUc1 = new RepostHeaderUC();
                        Thickness thickness = new Thickness(0.0, 14.0, 0.0, 14.0);
                        repostHeaderUc1.Margin = thickness;
                        RepostHeaderUC repostHeaderUc2 = repostHeaderUc1;
                        repostHeaderUc2.Configure(vm.WallRepostInfo, null);
                        this.wallRepostContainer.Child = (UIElement)repostHeaderUc2;
                    }
                }
                else
                    vm = comment1 == null ? (comment2 == null ? (comment3 == null ? (comment4 == null ? (comment5 == null ? (wallPostViewModel == null ? new WallPostViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, this._adminLevel, isPublicPage, isNewTopicMode) : wallPostViewModel) : WallPostViewModel.CreateEditDiscussionCommentVM(comment5, cidToAuthorIdDict)) : WallPostViewModel.CreateEditProductCommentVM(comment4)) : WallPostViewModel.CreateEditVideoCommentVM(comment3)) : WallPostViewModel.CreateEditPhotoCommentVM(comment2)) : WallPostViewModel.CreateEditWallCommentVM(comment1);
                vm.IsOnPostPage = true;
                vm.WMMode = result;
                if (!this._fromPhotoPicker && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New || (result == WallPostViewModel.Mode.NewTopic || result == WallPostViewModel.Mode.NewWallPost)))
                    WallPostVMCacheManager.TryDeserializeVM(vm);
                vm.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
                this.DataContext = (object)vm;
                this._isInitialized = true;
            }
            if (this.HandleInputParams(e))
                return;
            this.UpdateViewState();
            this.WallPostVM.OutboundAttachments.ForEach<IOutboundAttachment>((Action<IOutboundAttachment>)(a => a.SetRetryFlag()));
            if (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New)
                return;
            if (this.WallPostVM.IsInNewWallPostMode || this.WallPostVM.EditWallRepost)
            {
                this.FocusTextBox();
            }
            else
            {
                if (!this.WallPostVM.IsInNewTopicMode)
                    return;
                this.FocusTitleTextBox();
            }
        }

        private void FocusTextBox()
        {
            this._de.AddToDelayedExecution((Action)(() => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.textBoxPost.Focus();
                this.textBoxPost.Select(this.textBoxPost.Text.Length, 0);
            }))));
        }

        private void FocusTitleTextBox()
        {
            this._de.AddToDelayedExecution((Action)(() => Execute.ExecuteOnUIThread((Action)(() => this.textBoxTopicTitle.Focus()))));
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            if (this._published)
                return;
            WallPostVMCacheManager.TrySerializeVM(this.WallPostVM);
        }

        private void UpdateViewState()
        {
            if (this.WallPostVM.IsNewCommentMode)
                this.textBoxPost.Visibility = Visibility.Collapsed;
            this.textBoxPost.Text = this.WallPostVM.Text ?? "";
            this.textBoxTopicTitle.Text = this.WallPostVM.TopicTitle ?? "";
            this.textBlockWatermarkText.Opacity = this.textBoxPost.Text == "" ? 1.0 : 0.0;
            this.textBlockWatermarkTitle.Opacity = this.textBoxTopicTitle.Text == "" ? 1.0 : 0.0;
            this.UpdateAppBar();
        }

        private bool HandleInputParams(NavigationEventArgs e = null)
        {
            if (ParametersRepository.GetParameterForIdAndReset("GoPickImage") != null)
            {
                this.ShowPhotoChooser(true);
                return true;
            }
            string str = ParametersRepository.GetParameterForIdAndReset("NewMessageContents") as string;
            if (!string.IsNullOrEmpty(str))
                this.WallPostVM.Text = str;
            GeoCoordinate geoCoordinate = ParametersRepository.GetParameterForIdAndReset("NewPositionToBeAttached") as GeoCoordinate;
            if (geoCoordinate != (GeoCoordinate)null)
                this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundGeoAttachment(geoCoordinate.Latitude, geoCoordinate.Longitude));
            Poll poll = ParametersRepository.GetParameterForIdAndReset("UpdatedPoll") as Poll;
            if (poll != null)
            {
                OutboundPollAttachment outboundPollAttachment = this.WallPostVM.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundPollAttachment)) as OutboundPollAttachment;
                if (outboundPollAttachment != null)
                    outboundPollAttachment.Poll = poll;
                else
                    this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundPollAttachment(poll));
            }
            Stream stream1 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhoto") as Stream;
            List<Stream> streamList1 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            List<Stream> streamList2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
            if (stream1 != null)
            {
                if (!this._fromPhotoPicker || this._isForwardNav)
                {
                    this.WallPostVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, this.WallPostVM.UserOrGroupId, this.WallPostVM.IsGroup, null, PostType.WallPost));
                    this.WallPostVM.UploadAttachments();
                }
            }
            else if (streamList1 != null)
            {
                if (!this._fromPhotoPicker || this._isForwardNav)
                {
                    for (int index = 0; index < streamList1.Count; ++index)
                    {
                        Stream stream2 = streamList1[index];
                        Stream stream3 = null;
                        if (streamList2 != null && streamList2.Count > index)
                            stream3 = streamList2[index];
                        long userOrGroupId = this.WallPostVM.UserOrGroupId;
                        int num1 = this.WallPostVM.IsGroup ? 1 : 0;
                        Stream previewStream = stream3;
                        int num2 = 0;
                        this.WallPostVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForUploadNewPhoto(stream2, userOrGroupId, num1 != 0, previewStream, (PostType)num2));
                    }
                    this.WallPostVM.UploadAttachments();
                }
            }
            else if (this.WallPostVM.GoDirectlyToPhotoChooser && e != null && e.IsNavigationInitiator)
                Navigator.Current.GoBack();
            Photo photo = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (photo != null)
                this.WallPostVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForChoosingExistingPhoto(photo, this.WallPostVM.UserOrGroupId, this.WallPostVM.IsGroup, PostType.WallPost));
            VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
            if (video != null)
                this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundVideoAttachment(video));
            AudioObj audio = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (audio != null)
                this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundAudioAttachment(audio));
            Doc pickedDocument = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (pickedDocument != null)
                this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundDocumentAttachment(pickedDocument));
            TimerAttachment timer = ParametersRepository.GetParameterForIdAndReset("PickedTimer") as TimerAttachment;
            if (timer != null)
            {
                OutboundTimerAttachment outboundTimerAttachment = new OutboundTimerAttachment(timer);
                IOutboundAttachment outboundAtt = this.WallPostVM.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.AttachmentId == "timestamp"));
                if (outboundAtt != null)
                {
                    int index = this.WallPostVM.Attachments.IndexOf(outboundAtt);
                    this.WallPostVM.RemoveAttachment(outboundAtt);
                    this.WallPostVM.InsertAttachment(index, (IOutboundAttachment)outboundTimerAttachment);
                }
                else
                    this.WallPostVM.AddAttachment((IOutboundAttachment)outboundTimerAttachment);
                this.WallPostVM.FromGroup = true;
            }
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if (continuationEventArgs != null && (continuationEventArgs.Files).Any<StorageFile>() || ParametersRepository.Contains("PickedPhotoDocument"))
            {
                object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
                StorageFile file = continuationEventArgs != null ? (continuationEventArgs.Files).First<StorageFile>() : (StorageFile)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument");
                AttachmentType result;
                if (parameterForIdAndReset != null && Enum.TryParse<AttachmentType>(parameterForIdAndReset.ToString(), out result))
                {
                    if (result != AttachmentType.VideoFromPhone)
                    {
                        if (result == AttachmentType.DocumentFromPhone || result == AttachmentType.DocumentPhoto)
                        {
                            this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundUploadDocumentAttachment(file));
                            this.WallPostVM.UploadAttachments();
                        }
                    }
                    else
                    {
                        long groupId = this.WallPostVM.FromGroup ? this.WallPostVM.UserOrGroupId : 0L;
                        this.WallPostVM.AddAttachment((IOutboundAttachment)new OutboundUploadVideoAttachment(file, true, groupId));
                        this.WallPostVM.UploadAttachments();
                    }
                }
            }
            List<StorageFile> storageFileList1 = ParametersRepository.GetParameterForIdAndReset("ChosenDocuments") as List<StorageFile>;
            if (storageFileList1 != null)
            {
                foreach (IOutboundAttachment attachment in ((IEnumerable<StorageFile>)storageFileList1).Select<StorageFile, OutboundUploadDocumentAttachment>((Func<StorageFile, OutboundUploadDocumentAttachment>)(chosenDocument => new OutboundUploadDocumentAttachment(chosenDocument))))
                    this.WallPostVM.AddAttachment(attachment);
                this.WallPostVM.UploadAttachments();
            }
            List<StorageFile> storageFileList2 = ParametersRepository.GetParameterForIdAndReset("ChosenVideos") as List<StorageFile>;
            if (storageFileList2 != null)
            {
                foreach (IOutboundAttachment attachment in ((IEnumerable<StorageFile>)storageFileList2).Select<StorageFile, OutboundUploadVideoAttachment>((Func<StorageFile, OutboundUploadVideoAttachment>)(chosenDocument => new OutboundUploadVideoAttachment(chosenDocument, true, 0L))))
                    this.WallPostVM.AddAttachment(attachment);
                this.WallPostVM.UploadAttachments();
            }
            return false;
        }

        private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "CanPublish") && !(e.PropertyName == "CanAddMoreAttachments"))
                return;
            this.UpdateAppBar();
        }

        private void Image_Delete_Tap(object sender, GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            this.WallPostVM.RemoveAttachment(frameworkElement.DataContext as IOutboundAttachment);
            if (!this.WallPostVM.IsNewCommentMode || this.WallPostVM.OutboundAttachments.Count != 0)
                return;
            Navigator.Current.GoBack();
        }

        private void textBoxPost_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            this.WallPostVM.Text = this.textBoxPost.Text;
        }

        private void textBoxTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.WallPostVM.TopicTitle = this.textBoxTopicTitle.Text;
            this.textBlockWatermarkTitle.Opacity = this.textBoxTopicTitle.Text == "" ? 1.0 : 0.0;
        }

        private void textBoxTitle_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || string.IsNullOrEmpty(this.textBoxTopicTitle.Text))
                return;
            this.textBoxPost.Focus();
        }

        private void Image_Tap(object sender, GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            OutboundPhotoAttachment outboundPhotoAttachment = frameworkElement.DataContext as OutboundPhotoAttachment;
            if (outboundPhotoAttachment == null || outboundPhotoAttachment.UploadState != OutboundAttachmentUploadState.NotStarted && outboundPhotoAttachment.UploadState != OutboundAttachmentUploadState.Failed)
                return;
            this.WallPostVM.UploadAttachment((IOutboundAttachment)outboundPhotoAttachment, null);
        }

        private void AddAttachmentTap(object sender, GestureEventArgs e)
        {
            this.Focus();
            List<NamedAttachmentType> attachmentTypes;
            int maxCount;
            if (this.WallPostVM.CanAddMoreAttachments)
            {
                attachmentTypes = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>)AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation);
                if (this.WallPostVM.CannAddTimerAttachment)
                    attachmentTypes.Add(AttachmentTypes.TimerAttachmentType);
                if (this.WallPostVM.CanAddPollAttachment)
                    attachmentTypes.Add(AttachmentTypes.PollAttachmentType);
                maxCount = this.WallPostVM.NumberOfAttAllowedToAdd;
            }
            else
            {
                if (!this.WallPostVM.CannAddTimerAttachment)
                    return;
                attachmentTypes = new List<NamedAttachmentType>()
        {
          AttachmentTypes.TimerAttachmentType
        };
                maxCount = 1;
            }
            this._pickerUC = AttachmentPickerUC.Show(attachmentTypes, maxCount, (Action)(() => this.HandleInputParams(null)), this._excludeLocation, this.CommonParameters.IsGroup ? -this.CommonParameters.UserOrGroupId : this.CommonParameters.UserOrGroupId, this._adminLevel, null);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/NewPost.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.ucHeader = (GenericHeaderUC)this.FindName("ucHeader");
            this.ContentPanel = (Grid)this.FindName("ContentPanel");
            this.scroll = (ScrollViewer)this.FindName("scroll");
            this.stackPanel = (StackPanel)this.FindName("stackPanel");
            this.textBoxTopicTitle = (TextBox)this.FindName("textBoxTopicTitle");
            this.textBlockWatermarkTitle = (TextBlock)this.FindName("textBlockWatermarkTitle");
            this.ucNewPost = (NewPostUC)this.FindName("ucNewPost");
            this.wallRepostContainer = (Border)this.FindName("wallRepostContainer");
            this.checkBoxFriendsOnly = (CheckBox)this.FindName("checkBoxFriendsOnly");
            this.textBoxPanel = (TextBoxPanelControl)this.FindName("textBoxPanel");
        }
    }
}
