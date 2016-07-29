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
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VKClient.Common;
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
using VKClient.Photos.Library;
using VKClient.Photos.Localization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Photos
{
    public partial class PhotoCommentsPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, ISupportShare
  {
    private static readonly string LikeHeartImagePath = "Resources/appbar.heart2.rest.png";
    private static readonly string UnlikeHeartImagePath = "Resources/appbar.heart2.broken.rest.png";
    private DelayedExecutor _de = new DelayedExecutor(250);
    private List<Hyperlink> _tagHyperlinks = new List<Hyperlink>();
    private List<PhotoVideoTag> _photoTags = new List<PhotoVideoTag>();
    private PhotoChooserTask _photoChooserTask = new PhotoChooserTask() { ShowCamera = true };
    private ApplicationBar _appBar = new ApplicationBar() { BackgroundColor = VKConstants.AppBarBGColor, ForegroundColor = VKConstants.AppBarFGColor };
    private ApplicationBarIconButton _appBarButtonAttachments = new ApplicationBarIconButton() { IconUri = new Uri("Resources/attach.png", UriKind.Relative), Text = CommonResources.NewPost_AppBar_AddAttachment };
    private ApplicationBarIconButton _appBarButtonComment = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative), Text = CommonResources.PostCommentsPage_AppBar_Send };
    private ApplicationBarIconButton _appBarButtonEmojiToggle = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.smile.png", UriKind.Relative), Text = "emoji" };
    private ApplicationBarIconButton _appBarButtonLikeUnlike = new ApplicationBarIconButton() { IconUri = new Uri(PhotoCommentsPage.LikeHeartImagePath, UriKind.Relative), Text = CommonResources.PostCommentsPage_AppBar_Like };
    private ApplicationBarMenuItem _appBarMenuItemSave = new ApplicationBarMenuItem() { Text = PhotoResources.ImageViewer_AppBar_Save };
    private ApplicationBarMenuItem _appBarMenuItemReport = new ApplicationBarMenuItem() { Text = CommonResources.Report };
    private ApplicationBarMenuItem _appBarMenuItemShare = new ApplicationBarMenuItem() { Text = CommonResources.PostCommentsPage_AppBar_Share };
    private DialogService _ds = new DialogService();
    private int _selectedTagInd = -1;
    private bool _isInitialized;
    private WallPostViewModel _commentVM;
    private ViewportScrollableAreaAdapter _adapter;
    private long _ownerId;
    private long _pid;
    private bool _friendsOnly;
    private bool _fromDialog;
    private SharePostUC _sharePostUC;

    private PhotoViewModel PhotoVM
    {
      get
      {
        return this.DataContext as PhotoViewModel;
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
          return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Completed));
        return false;
      }
    }

    public PhotoCommentsPage()
    {
      this.InitializeComponent();
      this.Header.TextBlockTitle.Text = PhotoResources.PhotoCommentsPage_PHOTO;
      this.Header.OnHeaderTap = new Action(this.HandleOnHeaderTap);
      this.scroll.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.CreateAppBar();
      this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
      this.ucCommentGeneric.InitializeWithScrollViewer((IScrollableArea) this._adapter);
      this.ucCommentGeneric.UCNewComment = this.ucNewMessage;
      this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
      this.ucMoreActions.SetBlue();
      this.ucMoreActions.TapCallback = new Action(this.ShowContextMenu);
      this.ucNewMessage.OnAddAttachTap = (Action) (() => this.AddAttachTap());
      this.ucNewMessage.OnSendTap = (Action) (() => this._appBarButtonSend_Click(null, (EventArgs) null));
      this.ucNewMessage.UCNewPost.OnImageDeleteTap = (Action<object>) (sender =>
      {
        FrameworkElement frameworkElement = sender as FrameworkElement;
        if (frameworkElement != null)
          this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
        this.UpdateAppBar();
      });
      this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
      Binding binding = new Binding("OutboundAttachments");
      this.ucNewMessage.UCNewPost.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.ucCommentGeneric.Panel);
      this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.GotFocus += new RoutedEventHandler(this.textBoxGotFocus);
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.LostFocus += new RoutedEventHandler(this.textBoxLostFocus);
      EventAggregator.Current.Subscribe((object) this);
    }

    private void PanelIsOpenedChanged(object sender, bool e)
    {
      if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
        this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
      else
        this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
    }

    private void ShowContextMenu()
    {
      List<MenuItem> menuItems = new List<MenuItem>();
      MenuItem menuItem1 = new MenuItem();
      string viewerAppBarSave = PhotoResources.ImageViewer_AppBar_Save;
      menuItem1.Header = (object) viewerAppBarSave;
      MenuItem menuItem2 = menuItem1;
      menuItem2.Click += (RoutedEventHandler) ((s, e) => this._appBarButtonSave_Click((object) this, (EventArgs) null));
      menuItems.Add(menuItem2);
      MenuItem menuItem3 = new MenuItem();
      string barMenuSaveInAlbum = CommonResources.AppBarMenu_SaveInAlbum;
      menuItem3.Header = (object) barMenuSaveInAlbum;
      MenuItem menuItem4 = menuItem3;
      menuItem4.Click += (RoutedEventHandler) ((s, e) => this.SavePhotoToAlbum());
      menuItems.Add(menuItem4);
      if (this.PhotoVM.PhotoWithInfo != null && this.PhotoVM.PhotoWithInfo.Photo != null && (this.PhotoVM.PhotoWithInfo.Photo.album_id != -8L && this.PhotoVM.PhotoWithInfo.Photo.album_id != -12L) && this.PhotoVM.PhotoWithInfo.Photo.album_id != -3L)
      {
        MenuItem menuItem5 = new MenuItem();
        string photosGoToAlbum = CommonResources.Photos_GoToAlbum;
        menuItem5.Header = (object) photosGoToAlbum;
        MenuItem menuItem6 = menuItem5;
        menuItem6.Click += (RoutedEventHandler) ((s, e) => this.GoToAlbum());
        menuItems.Add(menuItem6);
      }
      this.ucMoreActions.SetMenu(menuItems);
      this.ucMoreActions.ShowMenu();
    }

    private void GoToAlbum()
    {
      if (this.PhotoVM.PhotoWithInfo == null || this.PhotoVM.PhotoWithInfo.Photo == null)
        return;
      Photo photo = this.PhotoVM.PhotoWithInfo.Photo;
      AlbumType albumType = AlbumTypeHelper.GetAlbumType(photo.aid);
      Navigator.Current.NavigateToPhotoAlbum(photo.owner_id > 0L ? photo.owner_id : -photo.owner_id, photo.owner_id < 0L, albumType.ToString(), photo.aid.ToString(), "", 0, "", "", false, 0);
    }

    private void SavePhotoToAlbum()
    {
      this.PhotoVM.SaveInSavedPhotosAlbum();
    }

    private void AddAttachTap()
    {
      AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action) (() =>
      {
        PostCommentsPage.HandleInputParams(this._commentVM);
        this.UpdateAppBar();
      }), true, 0L, 0);
    }

    private void HandleOnHeaderTap()
    {
      this.ucCommentGeneric.Panel.ScrollToBottom(false);
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
      if (e.TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", (object) e.ChosenPhoto);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      bool flag = true;
      if (!this._isInitialized)
      {
        this._ownerId = long.Parse(this.NavigationContext.QueryString["ownerId"]);
        this._pid = long.Parse(this.NavigationContext.QueryString["pid"]);
        string accessKey = this.NavigationContext.QueryString["accessKey"];
        Photo photo = ParametersRepository.GetParameterForIdAndReset("Photo") as Photo;
        PhotoWithFullInfo photoWithFullInfo = ParametersRepository.GetParameterForIdAndReset("PhotoWithFullInfo") as PhotoWithFullInfo;
        this._friendsOnly = this.NavigationContext.QueryString["FriendsOnly"] == bool.TrueString;
        this._fromDialog = this.NavigationContext.QueryString["FromDialog"] == bool.TrueString;
        PhotoViewModel photoViewModel;
        if (photo == null)
        {
          photoViewModel = new PhotoViewModel(this._ownerId, this._pid, accessKey);
        }
        else
        {
          if (string.IsNullOrEmpty(photo.access_key))
            photo.access_key = accessKey;
          photoViewModel = new PhotoViewModel(photo, photoWithFullInfo);
        }
        this.InitializeCommentVM();
        this.DataContext = (object) photoViewModel;
        photoViewModel.LoadInfoWithComments(new Action<bool, int>(this.OnPhotoInfoLoaded));
        this.RestoreUnboundState();
        this._isInitialized = true;
        flag = false;
      }
      if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
        WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
      this.ProcessInputData();
      this.UpdateAppBar();
    }

    private void InitializeCommentVM()
    {
      this._commentVM = WallPostViewModel.CreateNewPhotoCommentVM(this._ownerId, this._pid);
      this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
      this.ucNewMessage.DataContext = (object) this._commentVM;
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
      this.PhotoVM.SetInProgress(false, "");
    }

    private void ProcessInputData()
    {
      Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
      if (group != null)
        this.Share(group.id, group.name);
      Photo photo = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
      if (photo != null)
        this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForChoosingExistingPhoto(photo, 0L, false, PostType.WallPost));
      VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
      if (video != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundVideoAttachment(video));
      AudioObj audio = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
      if (audio != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundAudioAttachment(audio));
      Doc pickedDocument = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
      if (pickedDocument != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundDocumentAttachment(pickedDocument));
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
          this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType) num2));
        }
        this.PhotoVM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
        this._commentVM.UploadAttachments();
      }
      FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if ((continuationEventArgs == null || !((IEnumerable<StorageFile>) continuationEventArgs.Files).Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocument"))
        return;
      object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
      StorageFile file = continuationEventArgs != null ? ((IEnumerable<StorageFile>) continuationEventArgs.Files).First<StorageFile>() : (StorageFile) ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument");
      AttachmentType result;
      if (parameterForIdAndReset == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset.ToString(), out result))
        return;
      if (result != AttachmentType.VideoFromPhone)
      {
        if (result != AttachmentType.DocumentFromPhone && result != AttachmentType.DocumentPhoto)
          return;
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadDocumentAttachment(file));
        this._commentVM.UploadAttachments();
      }
      else
      {
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadVideoAttachment(file, true, 0L));
        this._commentVM.UploadAttachments();
      }
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
      this.State["CommentText"] = (object) this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
    }

    private void RestoreUnboundState()
    {
      if (!this.State.ContainsKey("CommentText"))
        return;
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text = this.State["CommentText"].ToString();
    }

    public void CreateAppBar()
    {
      this._appBarButtonComment.Click += new EventHandler(this._appBarButtonSend_Click);
      this._appBarButtonEmojiToggle.Click += new EventHandler(this._appBarButtonEmojiToggle_Click);
      this._appBarButtonAttachments.Click += new EventHandler(this._appBarButtonAttachments_Click);
      this._appBarButtonLikeUnlike.Click += new EventHandler(this._appBarButtonLikeUnlike_Click);
      this._appBarMenuItemSave.Click += new EventHandler(this._appBarButtonSave_Click);
      this._appBarMenuItemReport.Click += new EventHandler(this._appBarMenuItemReport_Click);
      this._appBarMenuItemShare.Click += new EventHandler(this._appBarButtonShare_Click);
      this._appBar.Buttons.Add((object) this._appBarButtonComment);
      this._appBar.Buttons.Add((object) this._appBarButtonEmojiToggle);
      this._appBar.Buttons.Add((object) this._appBarButtonAttachments);
      this._appBar.Buttons.Add((object) this._appBarButtonLikeUnlike);
      this._appBar.MenuItems.Add((object) this._appBarMenuItemShare);
      this._appBar.MenuItems.Add((object) this._appBarMenuItemSave);
      this._appBar.MenuItems.Add((object) this._appBarMenuItemReport);
      this._appBar.Opacity = 0.9;
      this._appBar.StateChanged += new EventHandler<ApplicationBarStateChangedEventArgs>(this._appBar_StateChanged);
    }

    private void _appBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
    {
    }

    private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
    {
    }

    private void textBoxGotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void textBoxLostFocus(object sender, RoutedEventArgs e)
    {
    }

    private void _appBarMenuItemReport_Click(object sender, EventArgs e)
    {
      ReportContentHelper.ReportPhoto(this.PhotoVM.OwnerId, this.PhotoVM.Pid);
    }

    private void _appBarButtonAttachments_Click(object sender, EventArgs e)
    {
    }

    private void _appBarButtonShare_Click(object sender, EventArgs e)
    {
      this._ds = new DialogService()
      {
        SetStatusBarBackground = true,
        HideOnNavigation = false
      };
      this._sharePostUC = new SharePostUC();
      this._sharePostUC.SendTap += new EventHandler(this.ButtonSend_Click);
      this._sharePostUC.ShareTap += new EventHandler(this.ButtonShare_Click);
      if (this._fromDialog || this._friendsOnly)
      {
        this._sharePostUC.SetShareCommunityEnabled(false);
        this._sharePostUC.SetShareCommunityEnabled(false);
      }
      this._ds.Child = (FrameworkElement) this._sharePostUC;
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
      this._ds.Hide();
      this.PhotoVM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
    }

    private void ButtonSend_Click(object sender, EventArgs eventArgs)
    {
      if (this.PhotoVM.Photo == null)
        return;
      this._ds.Hide();
      ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
      contentDataProvider.Message = this._sharePostUC.Text;
      contentDataProvider.Photo = this.PhotoVM.Photo;
      contentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
      Navigator.Current.NavigateToPickConversation();
    }

    private void _appBarButtonSave_Click(object sender, EventArgs e)
    {
      ImageHelper.SaveImage(this.image.Source as BitmapImage);
    }

    private void UpdateAppBar()
    {
      if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
        return;
      if (this.PhotoVM.UserLiked)
      {
        this._appBarButtonLikeUnlike.IconUri = new Uri(PhotoCommentsPage.UnlikeHeartImagePath, UriKind.Relative);
        this._appBarButtonLikeUnlike.Text = CommonResources.PostCommentsPage_AppBar_Unlike;
      }
      else
      {
        this._appBarButtonLikeUnlike.IconUri = new Uri(PhotoCommentsPage.LikeHeartImagePath, UriKind.Relative);
        this._appBarButtonLikeUnlike.Text = CommonResources.PostCommentsPage_AppBar_Like;
      }
      this._appBarButtonComment.IsEnabled = this.PhotoVM.CanComment && this.ReadyToSend;
      this.ucNewMessage.UpdateSendButton(this._appBarButtonComment.IsEnabled);
      this._appBarButtonAttachments.IsEnabled = this.PhotoVM.CanComment;
      int count = this._commentVM.OutboundAttachments.Count;
      this._appBarButtonAttachments.IconUri = count <= 0 ? new Uri("Resources/attach.png", UriKind.Relative) : new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", (object) Math.Min(count, 10)), UriKind.Relative);
      if (this._appBar.MenuItems.Contains((object) this._appBarMenuItemReport) || !this.PhotoVM.CanReport)
        return;
      this._appBar.MenuItems.Add((object) this._appBarMenuItemReport);
    }

    private void _appBarButtonLikeUnlike_Click(object sender, EventArgs e)
    {
      this.PhotoVM.LikeUnlike();
      this.ucCommentGeneric.UpdateLikesItem(this.PhotoVM.UserLiked);
      this.UpdateAppBar();
    }

    private void _appBarButtonSend_Click(object sender, EventArgs e)
    {
      this.ucCommentGeneric.AddComment(this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!res)
          return;
        this.InitializeCommentVM();
        this.UpdateAppBar();
      }))), (StickerItemData) null, "");
    }

    private void OnPhotoInfoLoaded(bool result, int adminLevel)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.GenerateAuthorText();
        this.GeneratePhotoText();
        this.GenerateTextForTags();
        this.ucCommentGeneric.ProcessLoadedComments(result);
        this.ucNewMessage.SetAdminLevel(adminLevel);
        this.stackPanelInfo.Visibility = result ? Visibility.Visible : Visibility.Collapsed;
        this.UpdateAppBar();
      }));
    }

    private void GeneratePhotoText()
    {
      if (!string.IsNullOrEmpty(this.PhotoVM.Text))
      {
        BrowserNavigationService.SetText((DependencyObject) this.textPhotoText, this.PhotoVM.Text);
        this.textPhotoText.Visibility = Visibility.Visible;
      }
      else
      {
        BrowserNavigationService.SetText((DependencyObject) this.textPhotoText, "");
        this.textPhotoText.Visibility = Visibility.Collapsed;
      }
    }

    private void GenerateAuthorText()
    {
      string date = "";
      if (this.PhotoVM.Photo != null)
        date = UIStringFormatterHelper.FormatDateTimeForUI(this.PhotoVM.Photo.created);
      this.UserHeader.Initilize(this.PhotoVM.OwnerImageUri, this.PhotoVM.OwnerName ?? "", date, this.PhotoVM.AuthorId);
    }

    private void GenerateTextForTags()
    {
      this.textTags.Blocks.Clear();
      this._tagHyperlinks.Clear();
      this._photoTags.Clear();
      this._photoTags.AddRange((IEnumerable<PhotoVideoTag>) this.PhotoVM.PhotoTags);
      this.textTags.Visibility = this._photoTags.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
      if (this._photoTags.Count <= 0)
        return;
      Paragraph paragraph = new Paragraph();
      Run run1 = new Run();
      run1.Text = PhotoResources.PhotoUC_OnThisPhoto + " ";
      SolidColorBrush solidColorBrush = Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush;
      run1.Foreground = (Brush) solidColorBrush;
      Run run2 = run1;
      paragraph.Inlines.Add((Inline) run2);
      for (int index = 0; index < this._photoTags.Count; ++index)
      {
        Hyperlink hyperlink = HyperlinkHelper.GenerateHyperlink(this._photoTags[index].tagged_name ?? "", index.ToString(), (Action<Hyperlink, string>) ((h, t) =>
        {
          int num = (int) HyperlinkHelper.GetState(h);
          int ind = int.Parse(t);
          PhotoVideoTag photoVideoTag = this._photoTags[int.Parse(t)];
          this.SelectTaggedUser(ind);
        }), (Brush) null);
        HyperlinkHelper.SetState(hyperlink, HyperlinkState.Accent, (Brush) null);
        this._tagHyperlinks.Add(hyperlink);
        paragraph.Inlines.Add((Inline) hyperlink);
        if (index < this.PhotoVM.PhotoTags.Count - 1)
        {
          Run run3 = new Run() { Text = ", " };
          paragraph.Inlines.Add((Inline) run3);
        }
      }
      this.textTags.Blocks.Add((Block) paragraph);
    }

    private void SelectTaggedUser(int ind)
    {
      PhotoVideoTag photoVideoTag = this._photoTags[ind];
      if (this._selectedTagInd == ind)
      {
        if (photoVideoTag.uid == 0L)
          return;
        Navigator.Current.NavigateToUserProfile(photoVideoTag.uid, photoVideoTag.tagged_name, "", false);
      }
      else
      {
        for (int index = 0; index < this._tagHyperlinks.Count; ++index)
        {
          Hyperlink h = this._tagHyperlinks[index];
          if (index == ind)
          {
            if (photoVideoTag.uid != 0L)
              HyperlinkHelper.SetState(h, HyperlinkState.Normal, (Brush) null);
          }
          else
            HyperlinkHelper.SetState(h, HyperlinkState.Accent, (Brush) null);
        }
        WriteableBitmap opacityMask = this.GenerateOpacityMask(this.image.ActualWidth, this.image.ActualHeight, photoVideoTag.x, photoVideoTag.x2, photoVideoTag.y, photoVideoTag.y2);
        Image image = this.image;
        ImageBrush imageBrush = new ImageBrush();
        imageBrush.ImageSource = (ImageSource) opacityMask;
        int num = 1;
        imageBrush.Stretch = (Stretch) num;
        image.OpacityMask = (Brush) imageBrush;
        this._selectedTagInd = ind;
      }
    }

    private void ResetTaggedUsersSelection()
    {
      foreach (Hyperlink tagHyperlink in this._tagHyperlinks)
        HyperlinkHelper.SetState(tagHyperlink, HyperlinkState.Accent, (Brush) null);
      this.image.OpacityMask = (Brush) null;
      this._selectedTagInd = -1;
    }

    private WriteableBitmap GenerateOpacityMask(double totalWidth, double totalHeight, double x1, double x2, double y1, double y2)
    {
      int pixelHeight = (int) (100.0 * (totalHeight / totalWidth));
      int num1 = (int) (100.0 * x1 / 100.0);
      int num2 = (int) (100.0 * x2 / 100.0);
      int num3 = (int) ((double) pixelHeight * y1 / 100.0);
      int num4 = (int) ((double) pixelHeight * y2 / 100.0);
      WriteableBitmap writeableBitmap = new WriteableBitmap(100, pixelHeight);
      for (int index = 0; index < writeableBitmap.Pixels.Length; ++index)
      {
        int num5 = index % writeableBitmap.PixelWidth;
        int num6 = index / writeableBitmap.PixelWidth;
        writeableBitmap.Pixels[index] = num5 < num1 || num5 > num2 || (num6 < num3 || num6 > num4) ? int.MinValue : -16777216;
      }
      return writeableBitmap;
    }

    private void image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Point position = e.GetPosition((UIElement) this.image);
      if (this.image.ActualHeight == 0.0 || this.image.ActualWidth == 0.0)
        return;
      int relativePosition = this.GetTagIndForRelativePosition((int) (position.X * 100.0 / this.image.ActualWidth), (int) (position.Y * 100.0 / this.image.ActualHeight));
      if (relativePosition >= 0)
        this.SelectTaggedUser(relativePosition);
      else
        this.ResetTaggedUsersSelection();
    }

    private int GetTagIndForRelativePosition(int x, int y)
    {
      if (this._photoTags != null)
      {
        for (int index = 0; index < this._photoTags.Count; ++index)
        {
          PhotoVideoTag photoVideoTag = this._photoTags[index];
          if ((double) x >= photoVideoTag.x && (double) x <= photoVideoTag.x2 && ((double) y >= photoVideoTag.y && (double) y <= photoVideoTag.y2))
            return index;
        }
      }
      return -1;
    }

    public void Handle(SpriteElementTapEvent data)
    {
      if (!this._isCurrentPage)
        return;
      this.Dispatcher.BeginInvoke((Action) (() =>
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
      this.ucCommentGeneric.AddComment(new List<IOutboundAttachment>(), (Action<bool>) (res => {}), message.StickerItem, message.Referrer);
    }

    public void InitiateShare()
    {
      this._appBarButtonShare_Click((object) this, (EventArgs) null);
    }
  }
}
