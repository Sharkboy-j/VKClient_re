using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Utils;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.ImageEditor;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;
using VKClient.Photos.UC;
using Windows.Storage;

namespace VKClient.Photos
{
    public partial class PhotoPickerPhotos : PageBase
  {
    private static readonly double MAX_ALLOWED_VERT_SPEED = 3.0;
    private CameraCaptureTask _cameraTask = new CameraCaptureTask();
    private ApplicationBar _defaultAppBar = new ApplicationBar() { BackgroundColor = VKConstants.AppBarBGColor, ForegroundColor = VKConstants.AppBarFGColor, Opacity = 0.9 };
    private ApplicationBarIconButton _appBarIconButtonConfirm = new ApplicationBarIconButton() { IconUri = new Uri("Resources/send.photo.png", UriKind.Relative), Text = CommonResources.AppBarConfirmChoice };
    private ApplicationBarIconButton _appBarIconButtonAddPhoto = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative), Text = PhotoResources.PhotoAlbumPage_AddPhoto };
    private ApplicationBarIconButton _appBarIconButtonChooseAlbum = new ApplicationBarIconButton() { IconUri = new Uri("Resources/outline.squares.png", UriKind.Relative), Text = CommonResources.AppBarChooseAlbum };
    private bool _isInitialized;
    private bool _pickToStorageFile;
    private bool _isConfirmed;
    private FrameworkElement _hoveredOverElement;
    private Point _p;
    private Image _manipImage;
    private bool? _selectMode;
    private bool _readyToToggle;
    private bool _firstManipulationDeltaEvent;
    private Point _previousTranslatedPoint;

    public PhotoPickerPhotosViewModel VM
    {
      get
      {
        return this.DataContext as PhotoPickerPhotosViewModel;
      }
    }

    public PhotoPickerPhotos()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.SuppressMenu = true;
      this.ucHeader.OnHeaderTap = (Action) (() => this.itemsControlPhotos.ScrollToTop());
      this.ucHeader.InitializeMenu((FrameworkElement) this.ucPickAlbum, this.ContentPanel, (Action) (() => this.ucPickAlbum.SelectedAlbumCallback = (Action<string>) (albId =>
      {
        this.VM.AlbumId = albId;
        this.ucHeader.ShowHideMenu();
      })), new Action(this.ucPickAlbum.Initialize), new Action(this.ucPickAlbum.Cleanup));
      this._cameraTask.Completed += new EventHandler<PhotoResult>(this._cameraTask_Completed);
    }

    private void _cameraTask_Completed(object sender, PhotoResult e)
    {
      if (e.TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("CapturedPhoto", (object) e.ChosenPhoto);
    }

    private void BuildAppBar()
    {
      this._appBarIconButtonAddPhoto.Click += new EventHandler(this._appBarIconButtonAddPhoto_Click);
      this._appBarIconButtonConfirm.Click += new EventHandler(this._appBarIconButtonConfirm_Click);
      this._appBarIconButtonChooseAlbum.Click += new EventHandler(this._appBarIconButtonChooseAlbum_Click);
      this._defaultAppBar.Buttons.Add((object) this._appBarIconButtonConfirm);
      this._defaultAppBar.Buttons.Add((object) this._appBarIconButtonAddPhoto);
      this.ApplicationBar = (IApplicationBar) this._defaultAppBar;
    }

    private void UpdateAppBar()
    {
      this._appBarIconButtonConfirm.IsEnabled = this.VM.SelectedCount > 0;
      if (this.VM.CanTakePicture && !this._defaultAppBar.Buttons.Contains((object) this._appBarIconButtonAddPhoto))
      {
        this._defaultAppBar.Buttons.Insert(1, (object) this._appBarIconButtonAddPhoto);
      }
      else
      {
        if (this.VM.CanTakePicture || !this._defaultAppBar.Buttons.Contains((object) this._appBarIconButtonAddPhoto))
          return;
        this._defaultAppBar.Buttons.Remove((object) this._appBarIconButtonAddPhoto);
      }
    }

    private void _appBarIconButtonConfirm_Click(object sender, EventArgs e)
    {
      this.HandleConfirm();
    }

    public async void HandleConfirm()
    {
      if (this._isConfirmed)
        return;
      this._isConfirmed = true;
      List<Stream> choosedPhotos = new List<Stream>();
      List<Stream> streamList = new List<Stream>();
      List<Size> sizeList = new List<Size>();
      this.VM.ImageEditor.SuppressParseEXIF = true;
      List<Stream> photoStreams = new List<Stream>();
      foreach (AlbumPhoto selectedPhoto in this.VM.SelectedPhotos)
      {
        ImageEffectsInfo imageEffectsInfo = this.VM.ImageEditor.GetImageEffectsInfo(selectedPhoto.AlbumId, selectedPhoto.SeqNo);
        if (imageEffectsInfo.AppliedAny && AppGlobalStateManager.Current.GlobalState.SaveEditedPhotos)
        {
          photoStreams.Add((Stream) StreamUtils.ReadFully(selectedPhoto.ImageStream));
          selectedPhoto.ImageStream.Position = 0L;
        }
        Stream imageStream = selectedPhoto.ImageStream;
        if (imageStream != null)
        {
          choosedPhotos.Add(imageStream);
          streamList.Add(selectedPhoto.ThumbnailStream);
          Size size = new Size();
          if (imageEffectsInfo.CropRect == null)
          {
            size.Width = selectedPhoto.Width;
            size.Height = selectedPhoto.Height;
          }
          sizeList.Add(size);
        }
      }
      await this.SavePhotosAsync(photoStreams);
      if (!this._pickToStorageFile)
      {
        ParametersRepository.SetParameterForId("ChoosenPhotos", (object) choosedPhotos);
        ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", (object) streamList);
        ParametersRepository.SetParameterForId("ChoosenPhotosSizes", (object) sizeList);
      }
      else
      {
        StorageFile file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("VK_Photo_" + (object) DateTime.Now.Ticks + ".jpg", (CreationCollisionOption) 1);
        Stopwatch sw = Stopwatch.StartNew();
        Stream choosenFile = choosedPhotos.First<Stream>();
        Stream fileStream = await ((IStorageFile) file).OpenStreamForWriteAsync();
        try
        {
          await choosenFile.CopyToAsync(fileStream);
        }
        finally
        {
          if (fileStream != null)
            fileStream.Dispose();
        }
        fileStream = (Stream) null;
        long elapsedMilliseconds = sw.ElapsedMilliseconds;
        ParametersRepository.SetParameterForId("PickedPhotoDocument", (object) file);
        ParametersRepository.SetParameterForId("FilePickedType", (object) 10);
        file = (StorageFile) null;
        sw = (Stopwatch) null;
        choosenFile = (Stream) null;
      }
      this.imageEditor.Hide(true);
      Navigator.Current.GoBack();
    }

    public async Task SavePhotosAsync(List<Stream> photoStreams)
    {
      StorageFolder folder = ((IEnumerable<StorageFolder>) await KnownFolders.PicturesLibrary.GetFoldersAsync()).FirstOrDefault<StorageFolder>((Func<StorageFolder, bool>) (x => x.Name == "VK"));
      if (folder == null)
        folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("VK", (CreationCollisionOption) 1);
      int ind = 1;
      DateTime dt = DateTime.Now;
      foreach (Stream photoStream in photoStreams)
      {
        Stream ps = photoStream;
        Stream output = await ((IStorageFile) await folder.CreateFileAsync(dt.Ticks.ToString() + ind.ToString() + ".jpg")).OpenStreamForWriteAsync();
        StreamUtils.CopyStream(ps, output, (Action<double>) null, (Cancellation) null, 0L);
        output.Close();
        ++ind;
        ps = (Stream) null;
      }
      //List<Stream>.Enumerator enumerator = new List<Stream>.Enumerator();
    }

    private void _appBarIconButtonAddPhoto_Click(object sender, EventArgs e)
    {
      this._cameraTask.Show();
    }

    private void _appBarIconButtonChooseAlbum_Click(object sender, EventArgs e)
    {
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        PhotoPickerPhotosViewModel pickerPhotosViewModel = new PhotoPickerPhotosViewModel(int.Parse(this.NavigationContext.QueryString["MaxAllowedToSelect"]), bool.Parse(this.NavigationContext.QueryString["OwnPhotoPick"]));
        pickerPhotosViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.DataContext = (object) pickerPhotosViewModel;
        this._pickToStorageFile = bool.Parse(this.NavigationContext.QueryString["PickToStorageFile"]);
        this._isInitialized = true;
      }
      bool haveCapturedPhoto = ParametersRepository.GetParameterForIdAndReset("CapturedPhoto") != null;
      this.VM.LoadData(true, (Action) (() =>
      {
        if (!haveCapturedPhoto)
          return;
        this.SelectCapturedPhoto();
      }));
      this.UpdateAppBar();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      if (e.NavigationMode != NavigationMode.Back)
        return;
      this.VM.ImageEditor.CleanupSession();
    }

    private void SelectCapturedPhoto()
    {
      if (this.VM.PhotosCount <= 0 || this.VM.RecentlyAddedImageInd < 0)
        return;
      List<AlbumPhoto> list = this.VM.Photos[this.VM.RecentlyAddedImageInd / 4].GetAsAlbumPhotos().ToList<AlbumPhoto>();
      int index = this.VM.RecentlyAddedImageInd % 4;
      if (index >= list.Count)
        return;
      AlbumPhoto albumPhoto = list[index];
      albumPhoto.IsSelected = true;
      this.ShowPhotoEditor(albumPhoto.SeqNo);
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedCount") && !(e.PropertyName == "CanTakePicture"))
        return;
      this.UpdateAppBar();
    }

    private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      this.ShowPhotoEditor((frameworkElement.DataContext as AlbumPhotoHeaderFourInARow).GetPhotoByTag(frameworkElement.Tag.ToString()).SeqNo);
    }

    private void ShowPhotoEditor(int photoSeqNo)
    {
      this.imageEditor.Show(this.VM.TotalCount, this.VM.AlbumId, this.VM.TotalCount - photoSeqNo - 1, (Func<int, Image>) (ind => this.GetPhotoById(ind).FirstOrDefault<FrameworkElement>() as Image), (Action<int, bool>) ((ind, show) =>
      {
        List<FrameworkElement> photoById = this.GetPhotoById(ind);
        FrameworkElement frameworkElement = photoById.FirstOrDefault<FrameworkElement>();
        if (frameworkElement != null)
          frameworkElement.Opacity = show ? 1.0 : 0.0;
        if (photoById.Count <= 1)
          return;
        foreach (FrameworkElement target in photoById.Skip<FrameworkElement>(1))
          target.Animate(target.Opacity, show ? 1.0 : 0.0, (object) UIElement.OpacityProperty, 150, new int?(0), null, null);
      }), this);
    }

    private List<FrameworkElement> GetPhotoById(int arg)
    {
      List<FrameworkElement> frameworkElementList = new List<FrameworkElement>();
      int num1 = arg / 4;
      int num2 = arg % 4;
      return frameworkElementList;
    }

    private void SelectUnselectTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.VM.OwnPhotoPick)
        return;
      FrameworkElement element = sender as FrameworkElement;
      this.ToggleSelection(element, (element.DataContext as AlbumPhotoHeaderFourInARow).GetPhotoByTag(element.Tag.ToString()));
    }

    private void ToggleSelection(FrameworkElement element, AlbumPhoto choosenPhoto)
    {
      if (choosenPhoto == null)
        return;
      double animateToScale = choosenPhoto.IsSelected ? 0.8 : 1.2;
      int dur = 100;
      Ellipse ellipse = (element.Parent as Panel).Children[2] as Ellipse;
      Image image = (element.Parent as Panel).Children[3] as Image;
      if (!(ellipse.RenderTransform is ScaleTransform))
        ellipse.RenderTransform = (Transform) new ScaleTransform();
      if (!choosenPhoto.IsSelected && this.VM.SelectedCount == this.VM.MaxAllowedToSelect)
        return;
      choosenPhoto.IsSelected = !choosenPhoto.IsSelected;
      PhotoPickerPhotos.AnimateTransform(animateToScale, dur, image.RenderTransform, 20);
      PhotoPickerPhotos.AnimateTransform(animateToScale, dur, ellipse.RenderTransform, 20);
    }

    public static void AnimateTransform(double animateToScale, int dur, Transform transform, int center = 20)
    {
      ScaleTransform scaleTransform = transform as ScaleTransform;
      double num1 = (double) center;
      scaleTransform.CenterX = num1;
      double num2 = (double) center;
      scaleTransform.CenterY = num2;
      transform.Animate(1.0, animateToScale, (object) ScaleTransform.ScaleXProperty, dur, new int?(0), (IEasingFunction) new CubicEase(), null, true);
      transform.Animate(1.0, animateToScale, (object) ScaleTransform.ScaleYProperty, dur, new int?(0), (IEasingFunction) new CubicEase(), null, true);
    }

    private void Image_ManipulationStarted_1(object sender, ManipulationStartedEventArgs e)
    {
      this._p = e.ManipulationOrigin;
      this._manipImage = e.OriginalSource as Image;
      this._selectMode = new bool?();
      this._readyToToggle = false;
      this._firstManipulationDeltaEvent = true;
      this._previousTranslatedPoint = e.ManipulationOrigin;
    }

    private void Image_ManipulationDelta_1(object sender, ManipulationDeltaEventArgs e)
    {
      Point point1 = new Point(this._p.X + e.CumulativeManipulation.Translation.X, this._p.Y + e.CumulativeManipulation.Translation.Y);
      List<Point> pointList = new List<Point>();
      pointList.Add(this._manipImage.TransformToVisual(Application.Current.RootVisual).Transform(point1));
      Point point2 = this._previousTranslatedPoint;
      int num = 5;
      Point point3;
      for (int index = 1; index < num; ++index)
      {
        point3 = new Point();
        point3.X = ((double) index * point1.X + (double) (num - index) * this._previousTranslatedPoint.X) / (double) num;
        point3.Y = ((double) index * point1.Y + (double) (num - index) * this._previousTranslatedPoint.Y) / (double) num;
        Point point4 = this._manipImage.TransformToVisual(Application.Current.RootVisual).Transform(point3);
        pointList.Add(point4);
      }
      this._previousTranslatedPoint = point1;
      point3 = e.DeltaManipulation.Translation;
      double x = point3.X;
      point3 = e.DeltaManipulation.Translation;
      double y = point3.Y;
      if (!this._readyToToggle)
        this._readyToToggle = Math.Abs(y) < PhotoPickerPhotos.MAX_ALLOWED_VERT_SPEED;
      if (!this._readyToToggle)
        return;
      if (this._firstManipulationDeltaEvent)
      {
        this.HandleHoverOverUpdate((FrameworkElement) this._manipImage);
        this._firstManipulationDeltaEvent = false;
      }
      foreach (Point intersectingPoint in pointList)
        this.HandleHoverOverUpdate(VisualTreeHelper.FindElementsInHostCoordinates(intersectingPoint, Application.Current.RootVisual).FirstOrDefault<UIElement>() as FrameworkElement);
    }

    private void HandleHoverOverUpdate(FrameworkElement control)
    {
      if (control == this._hoveredOverElement || !(control is Image))
        return;
      Image image1 = control as Image;
      if (image1.Tag == null || !(image1.DataContext is AlbumPhotoHeaderFourInARow))
        return;
      AlbumPhoto photoByTag = (control.DataContext as AlbumPhotoHeaderFourInARow).GetPhotoByTag(control.Tag.ToString());
      if (photoByTag == null)
        return;
      Image image2 = (image1.Parent as Panel).Children[3] as Image;
      if (!this._selectMode.HasValue)
        this._selectMode = new bool?(!photoByTag.IsSelected);
      if (photoByTag.IsSelected != this._selectMode.Value)
        this.ToggleSelection((FrameworkElement) image2, photoByTag);
      this._hoveredOverElement = control;
    }

    private void Image_ManipulationCompleted_1(object sender, ManipulationCompletedEventArgs e)
    {
      this._hoveredOverElement = null;
    }

    private void photosLink(object sender, LinkUnlinkEventArgs e)
    {
      int count = this.VM.Photos.Count;
      object content = e.ContentPresenter.Content;
      int num = 10;
      if ((!(content is AlbumHeaderTwoInARow) || count >= num) && (count < num || this.VM.Photos[count - num] != content))
        return;
      this.VM.LoadData(false, null);
    }
  }
}
