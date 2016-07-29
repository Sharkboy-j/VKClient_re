using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;

namespace VKClient.Photos
{
    public partial class PhotoAlbumPage : PageBase
    {
        private bool _isInitialized;
        private bool _pickMode;
        private PhotoAlbumViewModel.PhotoAlbumViewModelInput _inputData;
        private bool _isInEditMode;
        private PhotoChooserTask _photoChooserTask = new PhotoChooserTask();
        private Stream _choosenPhotoPending;
        private ApplicationBar _mainAppBar;
        private ApplicationBar _editAppBar;
        private ApplicationBarIconButton _appBarIconButtonEdit;
        private ApplicationBarIconButton _appBarIconButtonAddPhoto;
        private ApplicationBarIconButton _appBarButtonDelete;
        private ApplicationBarIconButton _appBarButtonMoveToAlbum;
        private ApplicationBarIconButton _appBarButtonCancel;

        private PhotoAlbumViewModel PhotoAlbumVM
        {
            get
            {
                return this.DataContext as PhotoAlbumViewModel;
            }
        }

        private bool IsInEditMode
        {
            get
            {
                return this._isInEditMode;
            }
            set
            {
                if (this._isInEditMode == value)
                    return;
                this._isInEditMode = value;
                if (this._isInEditMode)
                    this.listBoxPhotos.UnselectAll();
                this.itemsControlPhotos.Visibility = this._isInEditMode ? Visibility.Collapsed : Visibility.Visible;
                this.listBoxPhotos.Visibility = !this._isInEditMode ? Visibility.Collapsed : Visibility.Visible;
                this.SuppressMenu = this._isInEditMode;
                this.Header.HideSandwitchButton = this._isInEditMode;
                this.UpdateAppBar();
                this.UpdateHeaderOpacity();
            }
        }

        public PhotoAlbumPage()
        {
            this._appBarIconButtonEdit = new ApplicationBarIconButton() { Text = PhotoResources.PhotoAlbumPage_AppBar_Edit, IconUri = new Uri("Resources/appbar.manage.rest.png", UriKind.Relative) };
            this._appBarIconButtonAddPhoto = new ApplicationBarIconButton() { Text = PhotoResources.PhotoAlbumPage_AddPhoto, IconUri = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative) };
            this._appBarButtonDelete = new ApplicationBarIconButton() { Text = PhotoResources.EditAlbumPage_AppBar_Delete, IconUri = new Uri("Resources/appbar.delete.rest.png", UriKind.Relative) };
            this._appBarButtonMoveToAlbum = new ApplicationBarIconButton() { Text = PhotoResources.EditAlbumPage_AppBar_MoveToAlbum, IconUri = new Uri("Resources/appbar.movetofolder.rest.png", UriKind.Relative) };
            this._appBarButtonCancel = new ApplicationBarIconButton() { Text = PhotoResources.EditAlbumPage_AppBar_Cancel, IconUri = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative) };
            
            this.InitializeComponent();
            this._photoChooserTask.ShowCamera = true;
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
            this.BuildAppBar();
            this.itemsControlPhotos.ScrollPositionChanged += new EventHandler(this.itemsControlPhotos_ScrollPositionChanged);
            this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
            this.Header.HeaderBackgroundBrush = (Brush)(Application.Current.Resources["PhonePhotoHeaderBackgroundBrush"] as SolidColorBrush);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.itemsControlPhotos);
            this.itemsControlPhotos.OnRefresh = new Action(this.HandleOnRefresh);
        }

        private void HandleOnRefresh()
        {
            if (this.PhotoAlbumVM == null)
                return;
            this.PhotoAlbumVM.RefreshPhotos();
        }

        private void OnHeaderTap()
        {
            if (!this.PhotoAlbumVM.PhotosGenCol.Collection.Any<AlbumPhotoHeaderFourInARow>())
                return;
            this.itemsControlPhotos.ScrollToTop();
        }

        private void itemsControlPhotos_ScrollPositionChanged(object sender, EventArgs e)
        {
            this.UpdateHeaderOpacity();
        }

        private void UpdateHeaderOpacity()
        {
            if (this.IsInEditMode)
            {
                this.Header.Opacity = 1.0;
            }
            else
            {
                if (this.itemsControlPhotos.LockedBounds)
                    return;
                this.Header.Opacity = this.CalculateOpacityFadeAwayBasedOnScroll(this.itemsControlPhotos.ScrollPosition + 88.0);
                if (this.PhotoAlbumVM == null)
                    return;
                this.PhotoAlbumVM.HeaderOpacity = 1.0 - this.CalculateOpacityFadeAwayBasedOnScroll(this.itemsControlPhotos.ScrollPosition + 88.0 + 44.0);
            }
        }

        private double CalculateOpacityFadeAwayBasedOnScroll(double sp)
        {
            return sp >= 232.0 ? (sp <= 320.0 ? 1.0 / 88.0 * sp - 29.0 / 11.0 : 1.0) : 0.0;
        }

        private void _photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;
            this._choosenPhotoPending = e.ChosenPhoto;
        }

        private void _appBarIconButtonAddPhoto_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToPhotoPickerPhotos(20, false, false);
        }

        private void _appBarIconButtonEdit_Click(object sender, EventArgs e)
        {
            this.IsInEditMode = true;
        }

        private void BuildAppBar()
        {
            this._mainAppBar = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor
            };
            this._mainAppBar.Opacity = 0.9;
            this._appBarIconButtonAddPhoto.Click += new EventHandler(this._appBarIconButtonAddPhoto_Click);
            this._appBarIconButtonEdit.Click += new EventHandler(this._appBarIconButtonEdit_Click);
            this._mainAppBar.Buttons.Add((object)this._appBarIconButtonEdit);
            this._mainAppBar.Buttons.Add((object)this._appBarIconButtonAddPhoto);
            this._editAppBar = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor
            };
            this._editAppBar.Opacity = 0.9;
            this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
            this._appBarButtonDelete.Click += new EventHandler(this._appBarButtonDelete_Click);
            this._appBarButtonMoveToAlbum.Click += new EventHandler(this._appBarButtonMoveToAlbum_Click);
            this._editAppBar.Buttons.Add((object)this._appBarButtonDelete);
            this._editAppBar.Buttons.Add((object)this._appBarButtonMoveToAlbum);
            this._editAppBar.Buttons.Add((object)this._appBarButtonCancel);
        }

        private void _appBarButtonMoveToAlbum_Click(object sender, EventArgs e)
        {
            Navigator.Current.PickAlbumToMovePhotos(this.PhotoAlbumVM.InputData.UserOrGroupId, this.PhotoAlbumVM.InputData.IsGroup, this.PhotoAlbumVM.AlbumId, this.GetSelected().Select<AlbumPhoto, long>((Func<AlbumPhoto, long>)(a => a.Photo.pid)).ToList<long>(), this.PhotoAlbumVM.CanEditAlbum ? 3 : 0);
        }

        private void _appBarButtonDelete_Click(object sender, EventArgs e)
        {
            if (!this.AskDeletePhotoConfirmation(this.GetSelected().Count))
                return;
            this.PhotoAlbumVM.DeletePhotos(this.GetSelected());
            if (this.PhotoAlbumVM.PhotosCount != 0)
                return;
            this.IsInEditMode = false;
        }

        private bool AskDeletePhotoConfirmation(int count)
        {
            return MessageBox.Show(PhotoResources.DeletePhotoConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, PhotoResources.DeleteOnePhoto, PhotoResources.DeletePhotosFrm, PhotoResources.DeletePhotosFrm, true, null, false), MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            this.IsInEditMode = false;
        }

        private void UpdateAppBar()
        {
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
                return;
            if (!this._isInEditMode)
            {
                if (this.PhotoAlbumVM.CanEditAlbum)
                {
                    this.ApplicationBar = (IApplicationBar)this._mainAppBar;
                }
                else
                {
                    this.ApplicationBar = (IApplicationBar)null;
                    ExtendedLongListSelector longListSelector = this.itemsControlPhotos;
                    Thickness margin = this.itemsControlPhotos.Margin;
                    double left = margin.Left;
                    margin = this.itemsControlPhotos.Margin;
                    double top = margin.Top;
                    margin = this.itemsControlPhotos.Margin;
                    double right = margin.Right;
                    double bottom = -72.0;
                    Thickness thickness = new Thickness(left, top, right, bottom);
                    longListSelector.Margin = thickness;
                }
            }
            else
                this.ApplicationBar = (IApplicationBar)this._editAppBar;
            this._appBarButtonDelete.IsEnabled = this._appBarButtonMoveToAlbum.IsEnabled = this.listBoxPhotos.SelectedItems.Count > 0;
            this._appBarIconButtonEdit.IsEnabled = this.PhotoAlbumVM.PhotosCount > 0;
            if (this.PhotoAlbumVM.AType == AlbumType.SavedPhotos)
            {
                if (!this._mainAppBar.Buttons.Contains((object)this._appBarIconButtonAddPhoto))
                    return;
                this._mainAppBar.Buttons.Remove((object)this._appBarIconButtonAddPhoto);
            }
            else
            {
                if (this._mainAppBar.Buttons.Contains((object)this._appBarIconButtonAddPhoto))
                    return;
                this._mainAppBar.Buttons.Add((object)this._appBarIconButtonAddPhoto);
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (!this.IsInEditMode)
                return;
            e.Cancel = true;
            this.IsInEditMode = false;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            PhotosToMoveInfo photosToMove = ParametersRepository.GetParameterForIdAndReset("PhotosToMove") as PhotosToMoveInfo;
            bool needRefreshAfterMove = false;
            if (!this._isInitialized)
            {
                PhotoAlbumViewModel.PhotoAlbumViewModelInput inputData = new PhotoAlbumViewModel.PhotoAlbumViewModelInput();
                inputData.AlbumId = this.NavigationContext.QueryString["albumId"];
                inputData.UserOrGroupId = (long)int.Parse(this.NavigationContext.QueryString["userOrGroupId"]);
                inputData.IsGroup = bool.Parse(this.NavigationContext.QueryString["isGroup"]);
                if (this.NavigationContext.QueryString.ContainsKey("albumName"))
                {
                    inputData.AlbumName = this.NavigationContext.QueryString["albumName"];
                    inputData.AlbumType = (AlbumType)Enum.Parse(typeof(AlbumType), this.NavigationContext.QueryString["albumType"], true);
                    inputData.PageTitle = this.NavigationContext.QueryString["pageTitle"];
                    inputData.AlbumDescription = this.NavigationContext.QueryString["albumDesc"];
                    inputData.PhotosCount = int.Parse(this.NavigationContext.QueryString["photosCount"]);
                    this._pickMode = bool.Parse(this.NavigationContext.QueryString["PickMode"]);
                    inputData.AdminLevel = int.Parse(this.NavigationContext.QueryString["AdminLevel"]);
                }
                PhotoAlbumViewModel photoAlbumViewModel = new PhotoAlbumViewModel(inputData);
                this.UpdateHeaderOpacity();
                this.DataContext = (object)photoAlbumViewModel;
                if (photosToMove == null)
                    photoAlbumViewModel.RefreshPhotos();
                else
                    needRefreshAfterMove = true;
                this.UpdateAppBar();
                this._inputData = inputData;
                this._isInitialized = true;
            }
            if (photosToMove != null)
                this.PhotoAlbumVM.MovePhotos(photosToMove.albumId, photosToMove.photos, (Action<bool>)(result => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (needRefreshAfterMove)
                        this.PhotoAlbumVM.RefreshPhotos();
                    if (this.PhotoAlbumVM.PhotosCount == 0)
                        this.IsInEditMode = false;
                    if (!result)
                        ExtendedMessageBox.ShowSafe(CommonResources.GenericErrorText);
                    else if (MessageBox.Show(UIStringFormatterHelper.FormatNumberOfSomething(photosToMove.photos.Count, PhotoResources.PhotoAlbumPageOnePhotoMovedFrm, PhotoResources.PhotoAlbumPageTwoFourPhotosMovedFrm, PhotoResources.PhotoAlbumPageFivePhotosMovedFrm, true, photosToMove.albumName, false), PhotoResources.PhotoAlbumPage_PhotoMove, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        Navigator.Current.NavigateToPhotoAlbum(photosToMove.TargetAlbumInputData.UserOrGroupId, photosToMove.TargetAlbumInputData.IsGroup, photosToMove.TargetAlbumInputData.AlbumType.ToString(), photosToMove.TargetAlbumInputData.AlbumId, photosToMove.TargetAlbumInputData.AlbumName, photosToMove.TargetAlbumInputData.PhotosCount + photosToMove.photos.Count, photosToMove.TargetAlbumInputData.PageTitle, photosToMove.TargetAlbumInputData.AlbumDescription, false, 0);
                    this.PhotoAlbumVM.UpdateThumbAfterPhotosMoving();
                }))));
            if (this._choosenPhotoPending != null)
            {
                this.PhotoAlbumVM.UploadPhoto(this._choosenPhotoPending, (Action<BackendResult<Photo, ResultCode>>)(res => { }));
                this._choosenPhotoPending = null;
            }
            this.HandleInputParameters();
        }

        private void HandleInputParameters()
        {
            List<Stream> choosenPhotos = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            if (choosenPhotos == null || choosenPhotos.Count <= 0)
                return;
            this.UploadPhotos(choosenPhotos, 0);
        }

        private void UploadPhotos(List<Stream> choosenPhotos, int ind)
        {
            if (ind < choosenPhotos.Count)
                Execute.ExecuteOnUIThread((Action)(() => this.PhotoAlbumVM.UploadPhoto(choosenPhotos[ind], (Action<BackendResult<Photo, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    ++ind;
                    this.UploadPhotos(choosenPhotos, ind);
                }))));
            else
                Execute.ExecuteOnUIThread(new Action(this.UpdateAppBar));
        }

        private List<AlbumPhoto> GetSelected()
        {
            return this.listBoxPhotos.GetSelected<AlbumPhoto>();
        }

        private void MakeCover_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            AlbumPhoto albumPhoto = (AlbumPhoto)null;
            AlbumPhotoHeaderFourInARow headerFourInArow = menuItem.DataContext as AlbumPhotoHeaderFourInARow;
            if (headerFourInArow != null)
            {
                string @string = (menuItem.Parent as ContextMenu).Tag.ToString();
                if (!(@string == "1"))
                {
                    if (!(@string == "2"))
                    {
                        if (!(@string == "3"))
                        {
                            if (@string == "4")
                                albumPhoto = headerFourInArow.Photo4;
                        }
                        else
                            albumPhoto = headerFourInArow.Photo3;
                    }
                    else
                        albumPhoto = headerFourInArow.Photo2;
                }
                else
                    albumPhoto = headerFourInArow.Photo1;
            }
            else
                albumPhoto = menuItem.DataContext as AlbumPhoto;
            if (albumPhoto == null)
                return;
            this.PhotoAlbumVM.MakeCover(albumPhoto.Photo);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            AlbumPhoto albumPhoto = (AlbumPhoto)null;
            AlbumPhotoHeaderFourInARow headerFourInArow = menuItem.DataContext as AlbumPhotoHeaderFourInARow;
            if (headerFourInArow != null)
            {
                string @string = (menuItem.Parent as ContextMenu).Tag.ToString();
                if (!(@string == "1"))
                {
                    if (!(@string == "2"))
                    {
                        if (!(@string == "3"))
                        {
                            if (@string == "4")
                                albumPhoto = headerFourInArow.Photo4;
                        }
                        else
                            albumPhoto = headerFourInArow.Photo3;
                    }
                    else
                        albumPhoto = headerFourInArow.Photo2;
                }
                else
                    albumPhoto = headerFourInArow.Photo1;
            }
            else
                albumPhoto = menuItem.DataContext as AlbumPhoto;
            if (albumPhoto == null || !this.AskDeletePhotoConfirmation(1))
                return;
            this.PhotoAlbumVM.DeletePhoto(albumPhoto.Photo);
        }

        private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            AlbumPhoto albumPhoto = (AlbumPhoto)null;
            AlbumPhotoHeaderFourInARow headerFourInArow = frameworkElement.DataContext as AlbumPhotoHeaderFourInARow;
            if (headerFourInArow != null)
            {
                string @string = frameworkElement.Tag.ToString();
                if (!(@string == "1"))
                {
                    if (!(@string == "2"))
                    {
                        if (!(@string == "3"))
                        {
                            if (@string == "4")
                                albumPhoto = headerFourInArow.Photo4;
                        }
                        else
                            albumPhoto = headerFourInArow.Photo3;
                    }
                    else
                        albumPhoto = headerFourInArow.Photo2;
                }
                else
                    albumPhoto = headerFourInArow.Photo1;
            }
            else
                albumPhoto = frameworkElement.DataContext as AlbumPhoto;
            if (albumPhoto == null)
                return;
            if (this._pickMode)
            {
                ParametersRepository.SetParameterForId("PickedPhoto", (object)albumPhoto.Photo);
                this.NavigationService.RemoveBackEntrySafe();
                this.NavigationService.GoBackSafe();
            }
            else
            {
                List<Photo> list = this.PhotoAlbumVM.AlbumPhotos.Select<AlbumPhoto, Photo>((Func<AlbumPhoto, Photo>)(ap => ap.Photo)).ToList<Photo>();
                Navigator.Current.NavigateToImageViewer(this.PhotoAlbumVM.AlbumId, (int)this.PhotoAlbumVM.AType, this._inputData.UserOrGroupId, this._inputData.IsGroup, this.PhotoAlbumVM.PhotosCount, list.IndexOf(albumPhoto.Photo), list, new Func<int, Image>(this.GetPhotoById));
            }
        }

        private Image GetPhotoById(int arg)
        {
            int num1 = arg / 4;
            int num2 = arg % 4;
            return null;
        }

        public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj is T)
                return obj as T;
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return default(T);
            for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, childIndex);
                if (child is T)
                    return child as T;
            }
            for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
            {
                DependencyObject dependencyObject = (DependencyObject)this.FindDescendant<T>(VisualTreeHelper.GetChild(obj, childIndex));
                if (dependencyObject != null && dependencyObject is T)
                    return dependencyObject as T;
            }
            return default(T);
        }

        private void itemsControlPhotos_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            int count = this.PhotoAlbumVM.PhotosGenCol.Collection.Count;
            AlbumPhotoHeaderFourInARow headerFourInArow = e.ContentPresenter.Content as AlbumPhotoHeaderFourInARow;
            if (count < 10 || headerFourInArow == null || this.PhotoAlbumVM.PhotosGenCol.Collection[count - 10] != headerFourInArow)
                return;
            this.PhotoAlbumVM.LoadMorePhotos();
        }

        private void listBoxPhotos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            this.listBoxPhotos.SelectedItem = null;
        }

        private void listBoxPhotos_Link_1(object sender, MyLinkUnlinkEventArgs e)
        {
            int count = this.PhotoAlbumVM.AlbumPhotos.Count;
            AlbumPhoto albumPhoto = e.ContentPresenter.Content as AlbumPhoto;
            if (count < 20 || albumPhoto == null || this.PhotoAlbumVM.AlbumPhotos[count - 20] != albumPhoto)
                return;
            this.PhotoAlbumVM.LoadMorePhotos();
        }

        private void listBoxPhotos_MultiSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void Header_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        }
    }
}
