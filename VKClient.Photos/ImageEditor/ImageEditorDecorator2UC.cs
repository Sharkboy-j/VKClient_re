using ExifLib;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.UC;

namespace VKClient.Photos.ImageEditor
{
    public partial class ImageEditorDecorator2UC : UserControl, INotifyPropertyChanged
  {
    private IEasingFunction _easing;
    private bool _inCropMode;
    private bool _filtersPanelShown;
    private IApplicationBar _savedPageAppBar;
    private PhotoPickerPhotosViewModel _pppVM;
    private PhotoPickerPhotos _pickerPage;
    private int _totalCount;
    private bool _isShown;
    private int _indToShow;
    private List<Size> _imageSizes;
    private string _albumId;
    private bool _isInSetResetCrop;
    private ImageEditorViewModel _imageEditorVM;
    private DialogService _de;
    //private BitmapImage _tempBI;
    private bool _showingImageViewer;
    private ScrollViewerOffsetMediator _scrollMediator;
    private bool _inSelectOwnPhotoArea;

    public ImageEditorViewModel ImageEditor
    {
      get
      {
        return this._imageEditorVM;
      }
    }

    public List<FilterViewModel> Filters
    {
      get
      {
        return AvailableFilters.Filters;
      }
    }

    public bool IsShown
    {
      get
      {
        return this._isShown;
      }
      private set
      {
        this._isShown = value;
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._pppVM != null && this._pppVM.SelectedPhotos.Any<AlbumPhoto>((Func<AlbumPhoto, bool>) (p =>
        {
          if (p.AlbumId == this._pppVM.AlbumId)
            return p.SeqNo == this.CurrentPhotoSeqNo;
          return false;
        }));
      }
    }

    private AlbumPhoto CurrentPhoto
    {
      get
      {
        if (this._pppVM != null)
        {
          foreach (AlbumPhotoHeaderFourInARow photo in (Collection<AlbumPhotoHeaderFourInARow>) this._pppVM.Photos)
          {
            foreach (AlbumPhoto asAlbumPhoto in photo.GetAsAlbumPhotos())
            {
              if (asAlbumPhoto.AlbumId == this._pppVM.AlbumId && asAlbumPhoto.SeqNo == this.CurrentPhotoSeqNo)
                return asAlbumPhoto;
            }
          }
        }
        return (AlbumPhoto) null;
      }
    }

    private int CurrentPhotoSeqNo
    {
      get
      {
        return this._totalCount - this.imageViewer.CurrentInd - 1;
      }
    }

    public string SelectUnselectImageUri
    {
      get
      {
        return !this.IsSelected ? "/VKClient.Common;component/Resources/PhotoChooser-Check-WXGA.png" : "/VKClient.Common;component/Resources/PhotoChooser-Checked-WXGA.png";
      }
    }

    public Visibility IsSelectedVisibility
    {
      get
      {
        return !this.IsSelected ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        return (Application.Current.RootVisual as PhoneApplicationFrame).Content as PhoneApplicationPage;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ImageEditorDecorator2UC()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      cubicEase.EasingMode = (EasingMode) num;
      this._easing = (IEasingFunction) cubicEase;
      this._imageSizes = new List<Size>();
      this._scrollMediator = new ScrollViewerOffsetMediator();
      
      this.InitializeComponent();
      this.DataContext = (object) this;
      this.Visibility = Visibility.Collapsed;
      this.imageViewer.HideCallback = (Action) (() => this.Hide(false));
      this.imageViewer.ChangeIndexBeforeAnimation = true;
      this.imageViewer.MaxScale = 8.0;
      this.imageViewer.CurrentIndexChanged = new Action(this.RespondToCurrentIndexChanged);
      this.imageViewer.IsInVerticalSwipeChanged = new Action(this.RespondToVertSwipeChange);
      this.imageViewer.SupportOrientationChange = false;
      this._scrollMediator.ScrollViewer = this.scrollFilters;
      if (ScaleFactor.GetScaleFactor() != 150)
        return;
      this.LayoutRoot.Height = 854.0;
      this.imageViewer.Height = 782.0;
    }

    public void Show(int totalCount, string albumId, int ind, Func<int, Image> getImageFunc, Action<int, bool> showHideOriginalImageCallback, PhotoPickerPhotos pickerPage)
    {
      if (this.IsShown)
        return;
      this.Visibility = Visibility.Visible;
      this._indToShow = ind;
      if (this._pppVM != null)
        this._pppVM.PropertyChanged -= new PropertyChangedEventHandler(this.PickerVM_PropertyChanged);
      this._pppVM = pickerPage.VM;
      this._pppVM.PropertyChanged += new PropertyChangedEventHandler(this.PickerVM_PropertyChanged);
      this._pickerPage = pickerPage;
      this._totalCount = totalCount;
      this._savedPageAppBar = this.Page.ApplicationBar;
      this._albumId = albumId;
      this._imageEditorVM = this._pickerPage.VM.ImageEditor;
      this.elliplseSelect.Opacity = 0.0;
      this.imageSelect.Opacity = 0.0;
      this.OnPropertyChanged("ImageEditor");
      this.InitializeImageSizes();
      PhoneApplicationPage page = this.Page;
      object local = null;
      page.ApplicationBar = (IApplicationBar) local;
      EventHandler<CancelEventArgs> eventHandler = new EventHandler<CancelEventArgs>(this.Page_BackKeyPress);
      page.BackKeyPress += eventHandler;
      this.UpdateConfirmButtonState();
      this.imageViewer.Initialize(totalCount, (Func<int, ImageInfo>) (i =>
      {
        double num1 = 0.0;
        double num2 = 0.0;
        if (this._imageSizes.Count > i)
        {
          num1 = this._imageSizes[i].Width;
          num2 = this._imageSizes[i].Height;
        }
        ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this._totalCount - i - 1);
        if (imageEffectsInfo != null && imageEffectsInfo.ParsedExif != null && (imageEffectsInfo.ParsedExif.Width != 0 && imageEffectsInfo.ParsedExif.Height != 0))
        {
          num1 = (double) imageEffectsInfo.ParsedExif.Width;
          num2 = (double) imageEffectsInfo.ParsedExif.Height;
          if (imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.TopRight || imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.BottomLeft)
          {
            double num3 = num1;
            num1 = num2;
            num2 = num3;
          }
        }
        if (imageEffectsInfo != null && imageEffectsInfo.CropRect != null && !this._inCropMode)
        {
          num1 = (double) imageEffectsInfo.CropRect.Width;
          num2 = (double) imageEffectsInfo.CropRect.Height;
        }
        return new ImageInfo() { GetSourceFunc = (Func<bool, BitmapSource>) (allowBackgroundCreation => this._imageEditorVM.GetBitmapSource(this._albumId, this._totalCount - i - 1, allowBackgroundCreation)), Width = num1, Height = num2 };
      }), getImageFunc, showHideOriginalImageCallback, null, (Action<int>) null);
      this.IsShown = true;
      this.ShowViewer();
    }

    private void ShowViewer()
    {
      if (this._showingImageViewer)
        return;
      this._showingImageViewer = true;
      this.imageViewer.Show(this._indToShow, (Action) (() =>
      {
        this._showingImageViewer = false;
        this.Update();
      }), true, (BitmapImage) null);
    }

    private void PickerVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedCount"))
        return;
      this.UpdateConfirmButtonState();
    }

    private void UpdateConfirmButtonState()
    {
      this.sendPhotosButton.Opacity = 1.0;
      this.sendPhotosButton.IsHitTestVisible = true;
    }

    public void Hide(bool leavingPageImmediately = false)
    {
      if (!this.IsShown)
        return;
      this.Page.BackKeyPress -= new EventHandler<CancelEventArgs>(this.Page_BackKeyPress);
      this.Page.ApplicationBar = this._savedPageAppBar;
      this.IsShown = false;
      this.Update();
      this.ShowHideStackPanelEffects(false);
      this.imageViewer.Hide((Action) (() => this.Visibility = Visibility.Collapsed), leavingPageImmediately);
    }

    private void RespondToVertSwipeChange()
    {
      this.Update();
    }

    private void Update()
    {
      if (this._showingImageViewer)
        return;
      Visibility visibility = this.imageViewer.IsInVerticalSwipe ? Visibility.Collapsed : Visibility.Visible;
      if (!this.IsShown)
        visibility = Visibility.Collapsed;
      if (!this._pickerPage.VM.OwnPhotoPick)
        this.UpdateImageAndEllipseSelectOpacity(visibility == Visibility.Visible ? 1 : 0);
      this.ShowHideStackPanelEffects(this.IsShown && !this.imageViewer.IsInVerticalSwipe);
      if (this._filtersPanelShown && (!this.IsShown || this.imageViewer.IsInVerticalSwipe))
        this.ShowHideGridFilters(false);
      this.OnPropertyChanged("SelectUnselectImageUri");
      this.OnPropertyChanged("IsSelectedVisibility");
    }

    private void RespondToCurrentIndexChanged()
    {
      this.ImageEditor.SetCurrentPhoto(this._albumId, this.CurrentPhotoSeqNo);
      this.UpdateFiltersState("", null);
      this.Update();
    }

    private void InitializeImageSizes()
    {
      Stopwatch.StartNew();
      this._imageSizes.Clear();
      using (MediaLibrary mediaLibrary = new MediaLibrary())
      {
        PictureAlbum pictureAlbum = mediaLibrary.RootPictureAlbum.Albums.FirstOrDefault<PictureAlbum>((Func<PictureAlbum, bool>) (a => a.Name == this._albumId));
        if (pictureAlbum != (PictureAlbum) null)
        {
          foreach (Picture picture in pictureAlbum.Pictures)
          {
            this._imageSizes.Add(new Size()
            {
              Width = (double) picture.Width,
              Height = (double) picture.Height
            });
            picture.Dispose();
          }
          pictureAlbum.Dispose();
        }
        this._imageSizes.Reverse();
      }
    }

    private void Page_BackKeyPress(object sender, CancelEventArgs e)
    {
      e.Cancel = true;
      if (this._de != null && this._de.IsOpen)
        this._de.Hide();
      else if (this._inCropMode)
        this.ToggleCropMode();
      else
        this.Hide(false);
    }

    private void TextEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this._de = new DialogService();
      EditPhotoTextUC uc = new EditPhotoTextUC();
      ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
      uc.TextBoxText.Text = imageEffectsInfo.Text ?? "";
      uc.ButtonSave.Click += (RoutedEventHandler) ((s, ev) =>
      {
        string text = uc.TextBoxText.Text;
        Deployment.Current.Dispatcher.BeginInvoke((Action) (() => this.ImageEditor.SetResetText(text, (Action<BitmapSource>) (b => this.HandleEffectApplied(b)))));
        this._de.Hide();
      });
      this._de.Child = (FrameworkElement) uc;
      this._de.HideOnNavigation = false;
      this._de.Show((UIElement) this.gridDecorator);
    }

    private void HandleEffectApplied(BitmapSource b)
    {
      this._pickerPage.VM.HandleEffectUpdate(this._albumId, this.CurrentPhotoSeqNo);
      this.imageViewer.CurrentImage.Source = (ImageSource) b;
    }

    private void CropEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Deployment.Current.Dispatcher.BeginInvoke((Action) (() => this.ToggleCropMode()));
    }

    private void FixEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Deployment.Current.Dispatcher.BeginInvoke((Action) (() => this.ImageEditor.SetResetContrast(!this.ImageEditor.ContrastApplied, new Action<BitmapSource>(this.HandleEffectApplied))));
    }

    private void FilterEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ShowHideGridFilters(!this._filtersPanelShown);
    }

    private void SelectUnselectTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ToggleSelection((FrameworkElement) (sender as Image), this.CurrentPhoto);
      this.Update();
    }

    private void ToggleSelection(FrameworkElement iconImage, AlbumPhoto choosenPhoto)
    {
      if (choosenPhoto == null)
        return;
      double animateToScale = choosenPhoto.IsSelected ? 0.8 : 1.2;
      int dur = 100;
      Ellipse ellipse = (iconImage.Parent as Grid).Children.FirstOrDefault<UIElement>((Func<UIElement, bool>) (c => c is Ellipse)) as Ellipse;
      if (!(ellipse.RenderTransform is ScaleTransform))
        ellipse.RenderTransform = (Transform) new ScaleTransform();
      if (!(iconImage.RenderTransform is ScaleTransform))
        iconImage.RenderTransform = (Transform) new ScaleTransform();
      if (!choosenPhoto.IsSelected && this._pppVM.SelectedCount == this._pppVM.MaxAllowedToSelect)
        return;
      choosenPhoto.IsSelected = !choosenPhoto.IsSelected;
      PhotoPickerPhotos.AnimateTransform(animateToScale, dur, iconImage.RenderTransform, 25);
      PhotoPickerPhotos.AnimateTransform(animateToScale, dur, ellipse.RenderTransform, 25);
    }

    private void SendPhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._pickerPage.VM.OwnPhotoPick && this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo).CropRect == null)
      {
        this._inSelectOwnPhotoArea = true;
        this.gridChooseThumbnail.Visibility = Visibility.Visible;
        this.ToggleCropMode();
      }
      else
        this.EnsureSelectCurrentAndConfirm();
    }

    private void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged == null)
        return;
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    private void ShowHideStackPanelEffects(bool show)
    {
      TranslateTransform target1 = this.stackPanelEffects.RenderTransform as TranslateTransform;
      int num = show ? 0 : 221;
      target1.Animate(target1.Y, (double) num, (object) TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
      TranslateTransform target2 = this.rectChrome.RenderTransform as TranslateTransform;
      target2.Animate(target2.Y, (double) num, (object) TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
    }

    private void ShowHideGridFilters(bool show)
    {
      TranslateTransform target = this.gridFilters.RenderTransform as TranslateTransform;
      int num = show ? 0 : 221;
      this._filtersPanelShown = show;
      target.Animate(target.Y, (double) num, (object) TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
    }

    private void FilterTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FilterViewModel fvm = (sender as FrameworkElement).DataContext as FilterViewModel;
      this.UpdateFiltersState(fvm.FilterName, (Action) (() =>
      {
        if (fvm == null)
          return;
        Deployment.Current.Dispatcher.BeginInvoke((Action) (() => this.ImageEditor.SetResetFilter(fvm.FilterName, new Action<BitmapSource>(this.HandleEffectApplied))));
      }));
    }

    private void UpdateFiltersState(string forceFilterName = "", Action callback = null)
    {
      ImageEffectsInfo imageEffectsInfo = this.ImageEditor.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
      string str = string.IsNullOrEmpty(forceFilterName) ? imageEffectsInfo.Filter : forceFilterName;
      foreach (FilterViewModel filter in this.Filters)
      {
        bool flag = str == filter.FilterName;
        filter.IsSelectedVisibility = flag ? Visibility.Visible : Visibility.Collapsed;
        if (flag)
          this.ScrollToIndex(this.Filters.IndexOf(filter), callback);
      }
    }

    private void ScrollToIndex(int p, Action callback)
    {
      double horizontalOffset;
      double num1 = (horizontalOffset = this.scrollFilters.HorizontalOffset) + 472.0;
      double to = (double) (p * 110);
      double num2 = to + 110.0;
      if (to >= horizontalOffset && num2 <= num1)
      {
        if (callback == null)
          return;
        callback();
      }
      else if (to < horizontalOffset && num2 >= horizontalOffset)
        this.ScrollToOffset(to, callback);
      else if (num2 > num1 && to <= num1)
        this.ScrollToOffset(horizontalOffset + (num2 - num1), callback);
      else
        this.ScrollToOffset(to - 181.0, callback);
    }

    private void ScrollToOffset(double to, Action callback)
    {
      ScrollViewerOffsetMediator target = this._scrollMediator;
      double horizontalOffset = this.scrollFilters.HorizontalOffset;
      double to1 = to;
      DependencyProperty dependencyProperty = ScrollViewerOffsetMediator.HorizontalOffsetProperty;
      int duration = 350;
      int? startTime = new int?(0);
      CubicEase cubicEase = new CubicEase();
      int num1 = 2;
      cubicEase.EasingMode = (EasingMode) num1;
      Action completed = callback;
      int num2 = 0;
      target.Animate(horizontalOffset, to1, (object) dependencyProperty, duration, startTime, (IEasingFunction) cubicEase, completed, num2 != 0);
    }

    private void SetCrop(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._inSelectOwnPhotoArea)
      {
        ParametersRepository.SetParameterForId("UserPicSquare", (object) this.imageViewer.RectangleFillRelative);
        this._inSelectOwnPhotoArea = false;
        this.gridChooseThumbnail.Visibility = Visibility.Collapsed;
        this.EnsureSelectCurrentAndConfirm();
      }
      else
      {
        if (this._isInSetResetCrop)
          return;
        this._isInSetResetCrop = true;
        ParametersRepository.SetParameterForId("UserPicSquare", (object) new Rect(0.0, 0.0, 1.0, 1.0));
        Rect rectToCrop = this.imageViewer.RectangleFillInCurrentImageCoordinates;
        Deployment.Current.Dispatcher.BeginInvoke((Action) (() =>
        {
          ImageEditorViewModel imageEditorViewModel = this._imageEditorVM;
          double rotate = 0.0;
          CropRegion rect = new CropRegion();
          rect.X = (int) rectToCrop.X;
          rect.Y = (int) rectToCrop.Y;
          rect.Width = (int) rectToCrop.Width;
          rect.Height = (int) rectToCrop.Height;
          WriteableBitmap imSource = this.imageViewer.CurrentImage.Source as WriteableBitmap;
          Action<BitmapSource> callback = (Action<BitmapSource>) (result =>
          {
            this._isInSetResetCrop = false;
            this.HandleEffectApplied(result);
            this.ToggleCropMode();
            this.imageViewer.CurrentImage.RenderTransform = (Transform) RectangleUtils.TransformRect(this.imageViewer.CurrentImageFitRectOriginal, this.imageViewer.RectangleFill, false);
            this.imageViewer.AnimateImage(1.0, 1.0, 0.0, 0.0, null);
          });
          imageEditorViewModel.SetCrop(rotate, rect, imSource, callback);
        }));
      }
    }

    private void EnsureSelectCurrentAndConfirm()
    {
      if (this._pickerPage.VM.SelectedCount == 0)
        this.CurrentPhoto.IsSelected = true;
      this._pickerPage.HandleConfirm();
    }

    private void ResetCrop(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._isInSetResetCrop)
        return;
      this._isInSetResetCrop = true;
      Deployment.Current.Dispatcher.BeginInvoke((Action) (() => this._imageEditorVM.ResetCrop((Action<BitmapSource>) (result =>
      {
        this._isInSetResetCrop = false;
        this.HandleEffectApplied(result);
        this.imageViewer.AnimateImage(1.0, 1.0, 0.0, 0.0, null);
        this.ToggleCropMode();
      }))));
    }

    private void ToggleCropMode()
    {
      if (this._isInSetResetCrop)
        return;
      if (this._inCropMode)
      {
        this._inCropMode = false;
        this._inSelectOwnPhotoArea = false;
        this.gridChooseThumbnail.Visibility = Visibility.Collapsed;
        this.gridCropLines.Visibility = Visibility.Collapsed;
        this.gridCrop.Visibility = Visibility.Collapsed;
        this.imageViewer.Mode = ImageViewerMode.Normal;
        this.stackPanelEffects.Visibility = Visibility.Visible;
        this.stackPanelCrop.Visibility = Visibility.Collapsed;
        this.UpdateImageAndEllipseSelectOpacity(1);
      }
      else
      {
        this._inCropMode = true;
        Picture galleryImage = this._imageEditorVM.GetGalleryImage(this._albumId, this.CurrentPhotoSeqNo);
        bool rotated90 = false;
        Size correctImageSize = this._imageEditorVM.GetCorrectImageSize(galleryImage, this._albumId, this.CurrentPhotoSeqNo, out rotated90);
        BitmapImage bitmapImage = new BitmapImage();
        Point location = new Point();
        Size viewportSize = this._imageEditorVM.ViewportSize;
        double width = viewportSize.Width * 2.0;
        viewportSize = this._imageEditorVM.ViewportSize;
        double height = viewportSize.Height * 2.0;
        Size size = new Size(width, height);
        Rect fit1 = RectangleUtils.ResizeToFit(new Rect(location, size), correctImageSize);
        if (fit1.Height < (double) galleryImage.Height)
          bitmapImage.DecodePixelHeight = rotated90 ? (int) fit1.Height : (int) fit1.Width;
        bitmapImage.SetSource(galleryImage.GetImage());
        this.imageViewer.CurrentImage.Source = (ImageSource) this._imageEditorVM.RotateIfNeeded(this._albumId, this.CurrentPhotoSeqNo, new WriteableBitmap((BitmapSource) bitmapImage));
        this.imageViewer.RectangleFill = ScaleFactor.GetScaleFactor() != 150 ? new Rect(12.0, 136.0, 456.0, 456.0) : new Rect(12.0, 163.0, 456.0, 456.0);
        this.imageViewer.Mode = ImageViewerMode.RectangleFill;
        ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
        if (imageEffectsInfo.CropRect != null)
        {
          Rect rect = new Rect() { X = (double) imageEffectsInfo.CropRect.X, Y = (double) imageEffectsInfo.CropRect.Y, Width = (double) imageEffectsInfo.CropRect.Width, Height = (double) imageEffectsInfo.CropRect.Height };
          Rect fit2 = RectangleUtils.ResizeToFit(new Rect(new Point(), new Size(this.imageViewer.Width, this.imageViewer.Height)), correctImageSize);
          this.imageViewer.CurrentImage.RenderTransform = (Transform) RectangleUtils.TransformRect(RectangleUtils.TransformRect(new Rect(new Point(), correctImageSize), fit2, false).TransformBounds(rect), this.imageViewer.RectangleFill, false);
        }
        else
          this.imageViewer.AnimateToRectangleFill();
        galleryImage.Dispose();
        this.ShowHideGridFilters(false);
        this.gridCropLines.Visibility = Visibility.Visible;
        this.gridCrop.Visibility = Visibility.Visible;
        this.stackPanelEffects.Visibility = Visibility.Collapsed;
        this.stackPanelCrop.Visibility = Visibility.Visible;
        this.UpdateImageAndEllipseSelectOpacity(0);
      }
    }

    private void UpdateImageAndEllipseSelectOpacity(int op)
    {
      if (this._pickerPage.VM.OwnPhotoPick)
        return;
      this.elliplseSelect.Animate(this.elliplseSelect.Opacity, (double) op, (object) UIElement.OpacityProperty, 150, new int?(0), null, null);
      this.imageSelect.Animate(this.imageSelect.Opacity, (double) op, (object) UIElement.OpacityProperty, 150, new int?(0), null, null);
    }
  }
}
