using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;
using VKClient.Photos.UC;

namespace VKClient.Photos
{
    public partial class PhotosMainPage : PageBase
  {
    private List<long> _selectedPhotos = new List<long>();
    private readonly ApplicationBar _albumsAppBar = new ApplicationBar() { BackgroundColor = VKConstants.AppBarBGColor, ForegroundColor = VKConstants.AppBarFGColor };
    private readonly ApplicationBar _editAppBar = new ApplicationBar() { BackgroundColor = VKConstants.AppBarBGColor, ForegroundColor = VKConstants.AppBarFGColor };
    private readonly ApplicationBarIconButton _appBarButtonEdit = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.manage.rest.png", UriKind.Relative), Text = PhotoResources.PhotoAlbumPage_AppBar_Edit };
    private readonly ApplicationBarIconButton _appBarButtonDelete = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.delete.rest.png", UriKind.Relative), Text = PhotoResources.EditAlbumPage_AppBar_Delete };
    private readonly ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative), Text = PhotoResources.EditAlbumPage_AppBar_Cancel };
    private readonly ApplicationBarIconButton _appBarMenuItemAddAlbum = new ApplicationBarIconButton() { IconUri = new Uri("Resources/appbar.add.rest.png", UriKind.Relative), Text = PhotoResources.PhotosMainPage_Albums_Add };
    private bool _isInitialized;
    private int _adminLevel;
    private bool _selectForMove;
    private bool _isInEditMode;
    private bool _showingDialog;

    public bool IsInEditMode
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
          this.listBoxAlbums.UnselectAll();
        this.itemsControlAlbums.Visibility = this._isInEditMode ? Visibility.Collapsed : Visibility.Visible;
        this.listBoxAlbums.Visibility = !this._isInEditMode ? Visibility.Collapsed : Visibility.Visible;
        this.UpdateAppBar();
      }
    }

    public PhotosMainViewModel PhotosMainVM
    {
      get
      {
        return this.DataContext as PhotosMainViewModel;
      }
    }

    private bool OwnPhotos
    {
      get
      {
        if (!this.CommonParameters.IsGroup)
          return this.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
        return false;
      }
    }

    private bool EditableGroupPhotos
    {
      get
      {
        if (this.CommonParameters.IsGroup)
          return this._adminLevel > 1;
        return false;
      }
    }

    public PhotosMainViewModel PhotoMainVM
    {
      get
      {
        return this.DataContext as PhotosMainViewModel;
      }
    }

    public PhotosMainPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.itemsControlAlbums);
      this.itemsControlAlbums.OnRefresh = (Action) (() => this.PhotoMainVM.AlbumsVM.LoadData(true, false, (Action<BackendResult<AlbumsData, ResultCode>>) null, false));
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    private void OnHeaderTap()
    {
      if (this._isInEditMode)
        return;
      this.itemsControlAlbums.ScrollToTop();
    }

    private void BuildAppBar()
    {
      this._albumsAppBar.Buttons.Add((object) this._appBarMenuItemAddAlbum);
      this._albumsAppBar.Opacity = 0.9;
      this._editAppBar.Buttons.Add((object) this._appBarMenuItemAddAlbum);
      this._editAppBar.Buttons.Add((object) this._appBarButtonDelete);
      this._editAppBar.Buttons.Add((object) this._appBarButtonCancel);
      this._editAppBar.Opacity = 0.9;
      this._appBarMenuItemAddAlbum.Click += new EventHandler(this._appBarMenuItemAddAlbum_Click);
      this._appBarButtonEdit.Click += new EventHandler(this._appBarButtonEdit_Click);
      this._appBarButtonDelete.Click += new EventHandler(this._appBarButtonDelete_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
    }

    private void _appBarMenuItemAddAlbum_Click(object sender, EventArgs e)
    {
      this.ShowEditAlbum(new Album());
    }

    private void _appBarButtonEdit_Click(object sender, EventArgs e)
    {
      this.IsInEditMode = true;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      this.IsInEditMode = false;
    }

    private void _appBarButtonDelete_Click(object sender, EventArgs e)
    {
      if (!PhotosMainPage.AskDeleteAlbum(this.listBoxAlbums.GetSelected<AlbumHeader>().Count))
        return;
      this.PhotoMainVM.DeleteAlbums(this.listBoxAlbums.GetSelected<AlbumHeader>());
      if (this.PhotoMainVM.EditAlbumsVM.Collection.Count != 0)
        return;
      this.IsInEditMode = false;
    }

    private void UpdateAppBar()
    {
      if (this.OwnPhotos || this.EditableGroupPhotos)
      {
        if (this.CommonParameters.PickMode)
          this.ApplicationBar = (IApplicationBar) null;
        else
          this.ApplicationBar = !this._isInEditMode ? (IApplicationBar) this._albumsAppBar : (IApplicationBar) this._editAppBar;
      }
      else
      {
        this.ApplicationBar = (IApplicationBar) null;
        ExtendedLongListSelector longListSelector = this.itemsControlAlbums;
        Thickness margin = this.itemsControlAlbums.Margin;
        double left = margin.Left;
        margin = this.itemsControlAlbums.Margin;
        double top = margin.Top;
        margin = this.itemsControlAlbums.Margin;
        double right = margin.Right;
        double bottom = -72.0;
        Thickness thickness = new Thickness(left, top, right, bottom);
        longListSelector.Margin = thickness;
      }
      this._appBarButtonDelete.IsEnabled = this.listBoxAlbums.SelectedItems.Count > 0;
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (this._showingDialog)
      {
        this._showingDialog = false;
      }
      else
      {
        if (!this.IsInEditMode)
          return;
        e.Cancel = true;
        this.IsInEditMode = false;
      }
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._selectForMove = this.NavigationContext.QueryString.ContainsKey("SelectForMove");
      string excludeAlbumId = this.NavigationContext.QueryString.ContainsKey("ExcludeId") ? this.NavigationContext.QueryString["ExcludeId"] : "";
      this._adminLevel = int.Parse(this.NavigationContext.QueryString["AdminLevel"]);
      if (this.NavigationContext.QueryString.ContainsKey("SelectedPhotos"))
        this._selectedPhotos = this.NavigationContext.QueryString["SelectedPhotos"].ParseCommaSeparated();
      PhotosMainViewModel photosMainVM = new PhotosMainViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, this._selectForMove, excludeAlbumId);
      photosMainVM.AlbumsVM.Collection.CollectionChanged += (NotifyCollectionChangedEventHandler) ((p, f) =>
      {
        ObservableCollection<Group<AlbumHeader>> collection = photosMainVM.AlbumsVM.Collection;
        Func<Group<AlbumHeader>, bool> predicate = (Func<Group<AlbumHeader>, bool>)(group => group.Any<AlbumHeader>((Func<AlbumHeader, bool>)(album => album.AlbumType == AlbumType.NormalAlbum)));
        if (collection.Any<Group<AlbumHeader>>(predicate))
        {
          if (this._albumsAppBar.Buttons.Contains((object) this._appBarButtonEdit))
            return;
          this._albumsAppBar.Buttons.Add((object) this._appBarButtonEdit);
        }
        else
        {
          if (!this._albumsAppBar.Buttons.Contains((object) this._appBarButtonEdit))
            return;
          this._albumsAppBar.Buttons.Remove((object) this._appBarButtonEdit);
        }
      });
      this.DataContext = (object) photosMainVM;
      photosMainVM.LoadAlbums();
      this.UpdateAppBar();
      this._isInitialized = true;
    }

    private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumHeader albumHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as AlbumHeader;
      if (albumHeader == null)
        return;
      if (!this._selectForMove)
      {
        Navigator.Current.NavigateToPhotoAlbum(this.PhotoMainVM.UserOrGroupId, this.PhotoMainVM.IsGroup, albumHeader.AlbumType.ToString(), albumHeader.AlbumId, albumHeader.AlbumName, albumHeader.PhotosCount, this.PhotoMainVM.Title, albumHeader.Album == null ? "" : albumHeader.Album.description ?? "", this.CommonParameters.PickMode, this._adminLevel);
      }
      else
      {
        PhotosToMoveInfo photosToMoveInfo = new PhotosToMoveInfo();
        photosToMoveInfo.albumId = albumHeader.AlbumId;
        photosToMoveInfo.albumName = albumHeader.AlbumName;
        photosToMoveInfo.photos = this._selectedPhotos;
        PhotoAlbumViewModel.PhotoAlbumViewModelInput albumViewModelInput = new PhotoAlbumViewModel.PhotoAlbumViewModelInput();
        albumViewModelInput.AlbumDescription = albumHeader.Album == null ? "" : albumHeader.Album.description ?? "";
        albumViewModelInput.AlbumId = albumHeader.AlbumId;
        albumViewModelInput.AlbumName = albumHeader.AlbumName;
        albumViewModelInput.AlbumType = albumHeader.AlbumType;
        int num = this.PhotoMainVM.IsGroup ? 1 : 0;
        albumViewModelInput.IsGroup = num != 0;
        string photoPageTitle2 = this.PhotoMainVM.PhotoPageTitle2;
        albumViewModelInput.PageTitle = photoPageTitle2;
        int photosCount = albumHeader.PhotosCount;
        albumViewModelInput.PhotosCount = photosCount;
        long userOrGroupId = this.PhotoMainVM.UserOrGroupId;
        albumViewModelInput.UserOrGroupId = userOrGroupId;
        photosToMoveInfo.TargetAlbumInputData = albumViewModelInput;
        ParametersRepository.SetParameterForId("PhotosToMove", (object) photosToMoveInfo);
        this.NavigationService.GoBackSafe();
      }
    }

    private void ShowEditAlbum(Album album)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        DialogService dc = new DialogService() { SetStatusBarBackground = true, HideOnNavigation = false };
        dc.Child = (FrameworkElement) new CreateAlbumUC()
        {
          DataContext = (object) new CreateEditAlbumViewModel(album, (Action<Album>) (a => Execute.ExecuteOnUIThread((Action) (() =>
          {
            this.PhotoMainVM.AddOrUpdateAlbum(a);
            dc.Hide();
            this._showingDialog = false;
          }))), this.PhotoMainVM.IsGroup ? this.PhotoMainVM.UserOrGroupId : 0L),
          Visibility = Visibility.Visible
        };
        this._showingDialog = true;
        dc.Show(null);
      }));
    }

    private static bool AskDeleteAlbum(int count)
    {
      return MessageBox.Show(PhotoResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, PhotoResources.DeleteOneAlbumFrm, PhotoResources.DeleteAlbumsFrm, PhotoResources.DeleteAlbumsFrm, true, null, false), MessageBoxButton.OKCancel) == MessageBoxResult.OK;
    }

    private void listBoxAlbums_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void listBoxAlbumsSelection(object sender, SelectionChangedEventArgs e)
    {
      this.listBoxAlbums.SelectedItem = null;
    }

    private void EditHeader_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumHeader albumHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as AlbumHeader;
      if (albumHeader == null)
        return;
      this.ShowEditAlbum(albumHeader.Album);
    }

    private void listBoxAlbums_Link(object sender, MyLinkUnlinkEventArgs e)
    {
      this.PhotosMainVM.EditAlbumsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void itemsControlAlbums_Link(object sender, LinkUnlinkEventArgs e)
    {
      AlbumHeader albumHeader = e.ContentPresenter.Content as AlbumHeader;
      if (albumHeader == null)
        return;
      foreach (Group<AlbumHeader> group in (Collection<Group<AlbumHeader>>) this.PhotosMainVM.AlbumsVM.Collection)
      {
        int count = group.Count;
        if (count > 20 && group[count - 20] == albumHeader)
          this.PhotosMainVM.AlbumsVM.LoadData(false, false, (Action<BackendResult<AlbumsData, ResultCode>>) null, false);
      }
    }

    private void ButtonPhotosMoveNotification_OnClick(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToNewsFeed(0, true);
    }

    private void Dismiss_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.PhotosMainVM.HidePhotoFeedMoveNotification();
    }

  }
}
