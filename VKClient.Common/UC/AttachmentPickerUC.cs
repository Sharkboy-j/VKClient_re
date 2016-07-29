using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using Windows.Storage.Pickers;

namespace VKClient.Common.UC
{
    public class AttachmentPickerUC : UserControl
    {
        private readonly CameraCaptureTask _cameraCaptureTask = new CameraCaptureTask();
        private string _savedText = "";
        private DialogService _ds;
        private Action _quickPhotoPickCallback;
        private int _adminLevel;
        private ConversationInfo _conversationInfo;// NEW: 4.8.0
        private bool _albumPhotosSelected;
        private AttachmentSubPickerUC _subPickerUC;
        internal ScrollViewer scrollViewer;// NEW: 4.8.0
        internal ExtendedLongListSelector listBoxPhotos;
        private bool _contentLoaded;

        private AttachmentPickerViewModel VM
        {
            get
            {
                return this.DataContext as AttachmentPickerViewModel;
            }
        }

        public bool IsShown { get; private set; }

        private long OwnerId { get; set; }

        public AttachmentPickerUC()
        {
            this.InitializeComponent();
            this._cameraCaptureTask.Completed += new EventHandler<PhotoResult>(AttachmentPickerUC.CameraCaptureTask_OnCompleted);
        }

        private static void CameraCaptureTask_OnCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;
            MemoryStream memoryStream = StreamUtils.ReadFully(e.ChosenPhoto);
            e.ChosenPhoto.Position = 0L;
            memoryStream.Position = 0L;
            ParametersRepository.SetParameterForId("ChoosenPhotos", (object)new List<Stream>()
      {
        e.ChosenPhoto
      });
            ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", (object)new List<Stream>()
      {
        (Stream) memoryStream
      });
            ParametersRepository.SetParameterForId("ChoosenPhotosSizes", (object)new List<Size>()
      {
        new Size()
      });
        }

        public static AttachmentPickerUC Show(List<NamedAttachmentType> attachmentTypes, int maxCount, Action quickPhotoPickCallback, bool excludeLocation, long ownerId = 0, int adminLevel = 0, ConversationInfo conversationInfo = null)
        {
            AttachmentPickerUC attachmentPickerUc = new AttachmentPickerUC()
            {
                OwnerId = ownerId,
                _adminLevel = adminLevel,
                _conversationInfo = conversationInfo
            };
            if (maxCount > 0)
                attachmentPickerUc.DoShow((IEnumerable<NamedAttachmentType>)attachmentTypes, maxCount, quickPhotoPickCallback, excludeLocation);
            return attachmentPickerUc;
        }

        private void DoShow(IEnumerable<NamedAttachmentType> attachmentTypes, int maxCount, Action quickPhotoPickCallback, bool excludeLocation)
        {
            IEnumerable<NamedAttachmentType> source = attachmentTypes;
            Func<NamedAttachmentType, bool> predicate = (Func<NamedAttachmentType, bool>)(t => t.AttachmentType == AttachmentType.Timer);
            if (source.All<NamedAttachmentType>(predicate))
                --maxCount;
            this.DataContext = (object)new AttachmentPickerViewModel(AttachmentPickerUC.Convert(attachmentTypes, excludeLocation), maxCount);
            this._quickPhotoPickCallback = quickPhotoPickCallback;
            this._ds = new DialogService()
            {
                AnimationType = DialogService.AnimationTypes.None,
                AnimationTypeChild = DialogService.AnimationTypes.Swivel,
                Child = (FrameworkElement)this,
                HideOnNavigation = true
            };
            this.SizeChanged += (SizeChangedEventHandler)((sender, args) => Execute.ExecuteOnUIThread(new Action(this.UpdateListBoxPhotosSize)));// UPDATE: 4.8.0
            PageBase currentPage = FramePageUtils.CurrentPage;
            if (currentPage != null)
            {
                currentPage.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(this.Page_OnOrientationChanged);// UPDATE: 4.8.0
                this.UpdateScrollViewer(currentPage.Orientation);
            }
            if (FramePageUtils.IsHorizontal)
                this.listBoxPhotos.Visibility = Visibility.Collapsed;
            this.VM.PPPVM.LoadData(true, (Action)(() => Execute.ExecuteOnUIThread(new Action(this.UpdateListBoxPhotosSize))));
            this._ds.Closed += (EventHandler)((s, e) =>
            {
                this.VM.PPPVM.CleanupSession();
                this.IsShown = false;
            });
            this._ds.Show(null);
            this.IsShown = true;
        }

        private void Page_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.UpdateScrollViewer(e.Orientation);// UPDATE: 4.8.0
            this.UpdateListBoxPhotosSize();
        }

        private void UpdateScrollViewer(PageOrientation orientation)
        {
            this.scrollViewer.VerticalScrollBarVisibility = orientation == PageOrientation.Landscape || orientation == PageOrientation.LandscapeLeft || orientation == PageOrientation.LandscapeRight ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        }

        private void UpdateListBoxPhotosSize()
        {
            double buttonsCurrentSize = FramePageUtils.SoftNavButtonsCurrentSize;
            Content content = Application.Current.Host.Content;
            this.listBoxPhotos.Height = !FramePageUtils.IsHorizontal ? content.ActualWidth : content.ActualHeight - buttonsCurrentSize;
            double left = (this.listBoxPhotos.Height - this.listBoxPhotos.Width) / 2.0;
            this.listBoxPhotos.Margin = new Thickness(left, -left, 0.0, -left);
        }

        private static List<AttachmentPickerItem> Convert(IEnumerable<NamedAttachmentType> attachmentTypes, bool excludeLocation)
        {
            List<AttachmentPickerItem> attachmentPickerItemList = new List<AttachmentPickerItem>();
            foreach (NamedAttachmentType attachmentType in attachmentTypes)
            {
                if (!excludeLocation || attachmentType.AttachmentType != AttachmentType.Location)
                    attachmentPickerItemList.Add(new AttachmentPickerItem()
                    {
                        Text = attachmentType.Name,
                        AttachmentType = attachmentType
                    });
            }
            return attachmentPickerItemList;
        }

        private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null || !(element.DataContext is AlbumPhoto))
                return;
            AlbumPhoto choosenPhoto = element.DataContext as AlbumPhoto;
            this.ToggleSelection(element, choosenPhoto);
            this.UpdateAttachPhotoItem();
        }

        private void ToggleSelection(FrameworkElement element, AlbumPhoto choosenPhoto)
        {
            if (choosenPhoto == null)
                return;
            double animateToScale = choosenPhoto.IsSelected ? 0.8 : 1.2;
            Ellipse ellipse = (element.Parent as Grid).Children.FirstOrDefault<UIElement>((Func<UIElement, bool>)(c => c is Ellipse)) as Ellipse;
            Image image = (element.Parent as Grid).Children.LastOrDefault<UIElement>((Func<UIElement, bool>)(c => c is Image)) as Image;
            if (!(ellipse.RenderTransform is ScaleTransform))
                ellipse.RenderTransform = (Transform)new ScaleTransform();
            if (!(image.RenderTransform is ScaleTransform))
                image.RenderTransform = (Transform)new ScaleTransform();
            if (!choosenPhoto.IsSelected && this.VM.PPPVM.SelectedCount == this.VM.MaxCount)
                return;
            choosenPhoto.IsSelected = !choosenPhoto.IsSelected;
            AttachmentPickerUC.AnimateTransform(animateToScale, 100, image.RenderTransform, 20);
            AttachmentPickerUC.AnimateTransform(animateToScale, 100, ellipse.RenderTransform, 20);
        }

        public static void AnimateTransform(double animateToScale, int dur, Transform transform, int center = 20)
        {
            ScaleTransform scaleTransform = transform as ScaleTransform;
            double num1 = (double)center;
            scaleTransform.CenterX = num1;
            double num2 = (double)center;
            scaleTransform.CenterY = num2;
            transform.Animate(1.0, animateToScale, (object)ScaleTransform.ScaleXProperty, dur, new int?(0), (IEasingFunction)new CubicEase(), null, true);
            transform.Animate(1.0, animateToScale, (object)ScaleTransform.ScaleYProperty, dur, new int?(0), (IEasingFunction)new CubicEase(), null, true);
        }

        private void UpdateAttachPhotoItem()
        {
            int number = this.VM.PPPVM.AlbumPhotos.Count<AlbumPhoto>((Func<AlbumPhoto, bool>)(p => p.IsSelected));
            this._albumPhotosSelected = number > 0;
            AttachmentPickerItem attachmentPickerItem = this.VM.AttachmentTypes.FirstOrDefault<AttachmentPickerItem>((Func<AttachmentPickerItem, bool>)(at => at.AttachmentType.AttachmentType == AttachmentType.Photo));
            if (attachmentPickerItem == null)
                return;
            if (this._albumPhotosSelected)
            {
                if (this._savedText == "")
                    this._savedText = attachmentPickerItem.Text;
                attachmentPickerItem.Text = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.AttachOnePhotoFrm, CommonResources.AttachTwoFourPhotosFrm, CommonResources.AttachFivePhotosFrm, true, null, false);
                attachmentPickerItem.IsHighlighted = true;
            }
            else
            {
                attachmentPickerItem.Text = this._savedText;
                attachmentPickerItem.IsHighlighted = false;
            }
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AttachmentPickerItem attachmentPickerItem = (sender as FrameworkElement).DataContext as AttachmentPickerItem;
            if (attachmentPickerItem == null)
                return;
            this.HandleAttachmentSelection(attachmentPickerItem);
        }

        private void HandleAttachmentSelection(AttachmentPickerItem item)
        {
            if (item == null)
                return;
            if (this._subPickerUC != null)
                this._subPickerUC.ItemSelected -= new AttachmentSubItemSelectedEventHandler(this.HandleAttachmentSelection);
            AttachmentType attachmentType = item.AttachmentType.AttachmentType;
            switch (attachmentType)
            {
                case AttachmentType.Photo:
                    if (!item.IsHighlighted)
                    {
                        List<NamedAttachmentType> attachmentTypes = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>)AttachmentTypes.AttachmentSubTypesPhotos);
                        if (this.OwnerId < 0L && this._adminLevel > 1)
                            attachmentTypes.Add(AttachmentTypes.PhotoCommunityType);
                        this.ShowAttachmentSubPickerFor(attachmentTypes);
                        break;
                    }
                    List<Stream> streamList1 = new List<Stream>();
                    List<Stream> streamList2 = new List<Stream>();
                    List<Size> sizeList = new List<Size>();
                    this.VM.PPPVM.SuppressEXIFFetch = true;
                    foreach (AlbumPhoto albumPhoto in this.VM.PPPVM.AlbumPhotos.Where<AlbumPhoto>((Func<AlbumPhoto, bool>)(ap => ap.IsSelected)))
                    {
                        Stream imageStream = albumPhoto.ImageStream;
                        if (imageStream != null)
                        {
                            streamList1.Add(imageStream);
                            streamList2.Add(albumPhoto.ThumbnailStream);
                            Size size = new Size();
                            sizeList.Add(size);
                        }
                    }
                    ParametersRepository.SetParameterForId("ChoosenPhotos", (object)streamList1);
                    ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", (object)streamList2);
                    ParametersRepository.SetParameterForId("ChoosenPhotosSizes", (object)sizeList);
                    this._ds.Hide();
                    this._quickPhotoPickCallback();
                    break;
                case AttachmentType.Video:
                    List<NamedAttachmentType> attachmentTypes1 = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>)AttachmentTypes.AttachmentSubTypesVideos);
                    if (this.OwnerId < 0L && this._adminLevel > 1)
                        attachmentTypes1.Add(AttachmentTypes.VideoCommunityType);
                    this.ShowAttachmentSubPickerFor(attachmentTypes1);
                    break;
                case AttachmentType.Audio:
                    Navigator.Current.NavigateToAudio(1, 0L, false, 0L, 0L, "");
                    break;
                case AttachmentType.Document:
                    Navigator.Current.NavigateToDocumentsPicker();
                    break;
                case AttachmentType.Location:
                    Navigator.Current.NavigateToMap(true, 0.0, 0.0);
                    break;
                case AttachmentType.PhotoFromPhone:
                    Navigator.Current.NavigateToPhotoPickerPhotos(this.VM.MaxCount, false, false);
                    break;
                case AttachmentType.VideoFromPhone:
                    this._ds.Hide();
                    FileOpenPicker fileOpenPicker1 = new FileOpenPicker();
                    ((IDictionary<string, object>)fileOpenPicker1.ContinuationData)["FilePickedType"] = (object)attachmentType;
                    foreach (string supportedVideoExtension in VKConstants.SupportedVideoExtensions)
                        fileOpenPicker1.FileTypeFilter.Add(supportedVideoExtension);
                    ((IDictionary<string, object>)fileOpenPicker1.ContinuationData)["Operation"] = (object)"VideoFromPhone";
                    fileOpenPicker1.PickSingleFileAndContinue();
                    break;
                case AttachmentType.DocumentFromPhone:
                    this._ds.Hide();
                    FileOpenPicker fileOpenPicker2 = new FileOpenPicker();
                    ((IDictionary<string, object>)fileOpenPicker2.ContinuationData)["FilePickedType"] = (object)attachmentType;
                    foreach (string supportedDocExtension in VKConstants.SupportedDocExtensions)
                        fileOpenPicker2.FileTypeFilter.Add(supportedDocExtension);
                    ((IDictionary<string, object>)fileOpenPicker2.ContinuationData)["Operation"] = (object)"DocumentFromPhone";
                    fileOpenPicker2.PickSingleFileAndContinue();
                    break;
                case AttachmentType.PhotoMy:
                    Navigator.Current.NavigateToPhotoAlbums(true, 0L, false, 0);
                    break;
                case AttachmentType.VideoMy:
                    Navigator.Current.NavigateToVideo(true, 0L, false, false);
                    break;
                case AttachmentType.DocumentMy:
                    Navigator.Current.NavigateToDocumentsPicker();
                    break;
                case AttachmentType.DocumentPhoto:
                    this._ds.Hide();
                    FileOpenPicker fileOpenPicker3 = new FileOpenPicker();
                    ((IDictionary<string, object>)fileOpenPicker3.ContinuationData)["FilePickedType"] = (object)attachmentType;
                    foreach (string libraryExtension in VKConstants.SupportedDocLibraryExtensions)
                        fileOpenPicker3.FileTypeFilter.Add(libraryExtension);
                    ((IDictionary<string, object>)fileOpenPicker3.ContinuationData)["Operation"] = (object)"DocumentLibraryFromPhone";
                    fileOpenPicker3.PickSingleFileAndContinue();
                    break;
                case AttachmentType.Poll:
                    Navigator.Current.NavigateToCreateEditPoll(this.OwnerId, 0L, (Poll)null);
                    break;
                case AttachmentType.Timer:
                    Navigator.Current.NavigateToPostSchedule(new DateTime?());
                    break;
                case AttachmentType.PhotoCommunity:
                    Navigator.Current.NavigateToPhotoAlbums(true, -this.OwnerId, true, this._adminLevel);
                    break;
                case AttachmentType.VideoCommunity:
                    Navigator.Current.NavigateToVideo(true, -this.OwnerId, true, false);
                    break;
                case AttachmentType.Graffiti:
                    if (this._conversationInfo == null)
                        break;
                    this._ds.Hide();
                    ParametersRepository.SetParameterForId("ConversationInfo", (object)this._conversationInfo);
                    Navigator.Current.NavigateToGraffitiDrawPage(this._conversationInfo.UserOrChatId, this._conversationInfo.IsChat, this._conversationInfo.Title);
                    break;
            }
        }

        private void ShowAttachmentSubPickerFor(List<NamedAttachmentType> attachmentTypes)
        {
            if (this._subPickerUC != null)
                this._subPickerUC.ItemSelected -= new AttachmentSubItemSelectedEventHandler(this.HandleAttachmentSelection);
            this._subPickerUC = new AttachmentSubPickerUC();
            this._subPickerUC.ItemSelected += new AttachmentSubItemSelectedEventHandler(this.HandleAttachmentSelection);
            this._subPickerUC.itemsControl.ItemsSource = (IEnumerable)AttachmentPickerUC.Convert((IEnumerable<NamedAttachmentType>)attachmentTypes, true);
            this._ds.ChangeChild((FrameworkElement)this._subPickerUC, null);
        }

        private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
        {
            int count = this.VM.PPPVM.AlbumPhotos.Count;
            object dataContext = e.ContentPresenter.DataContext;
            if (count >= 20 && (count < 20 || this.VM.PPPVM.AlbumPhotos[count - 20] != dataContext))
                return;
            this.VM.PPPVM.CountToLoad = 100;
            this.VM.PPPVM.LoadData(false, null);
        }

        private void Camera_tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._ds.Hide();
            this._cameraCaptureTask.Show();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/AttachmentPickerUC.xaml", UriKind.Relative));
            this.scrollViewer = (ScrollViewer)this.FindName("scrollViewer");
            this.listBoxPhotos = (ExtendedLongListSelector)this.FindName("listBoxPhotos");
        }
    }
}
