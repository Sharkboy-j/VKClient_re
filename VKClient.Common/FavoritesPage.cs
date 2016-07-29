using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.UC;
using VKClient.Photos.Library;
using VKClient.Video.Library;

namespace VKClient.Common
{
  public class FavoritesPage : PageBase
  {
    private bool _isInitialized;
    private bool _photosLoaded;
    private bool _videosLoaded;
    private bool _postsLoaded;
    private bool _usersLoaded;
    private bool _linksLoaded;
    private bool _productsLoaded;
    private int _initialOffset;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Pivot pivot;
    internal PivotItem pivotItemPhotos;
    internal ExtendedLongListSelector photosListBox;
    internal PivotItem pivotItemVideos;
    internal ExtendedLongListSelector videosListBox;
    internal PivotItem pivotItemPosts;
    internal ViewportControl scrollPosts;
    internal StackPanel stackPanelPosts;
    internal MyVirtualizingPanel2 panelPosts;
    internal PivotItem pivotItemPersons;
    internal ExtendedLongListSelector usersListBox;
    internal PivotItem pivotItemLinks;
    internal ExtendedLongListSelector linksListBox;
    internal PivotItem pivotItemProducts;
    internal ExtendedLongListSelector productsListBox;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private FavoritesViewModel FavVM
    {
      get
      {
        return this.DataContext as FavoritesViewModel;
      }
    }

    public FavoritesPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = new Action(this.HandleHeaderTap);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.photosListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.videosListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelPosts);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.linksListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.usersListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.productsListBox);
      this.photosListBox.OnRefresh = (Action) (() => this.FavVM.PhotosVM.LoadData(true, false, (Action<BackendResult<PhotosListWithCount, ResultCode>>) null, false));
      this.videosListBox.OnRefresh = (Action) (() => this.FavVM.VideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false));
      this.panelPosts.OnRefresh = (Action) (() => this.FavVM.PostsVM.LoadData(true, false, (Action<BackendResult<WallData, ResultCode>>) null, false));
      this.linksListBox.OnRefresh = (Action) (() => this.FavVM.LinksVM.LoadData(true, false, (Action<BackendResult<List<Link>, ResultCode>>) null, false));
      this.usersListBox.OnRefresh = (Action) (() => this.FavVM.UsersVM.LoadData(true, false, (Action<BackendResult<UsersListWithCount, ResultCode>>) null, false));
      this.productsListBox.OnRefresh = (Action) (() => this.FavVM.ReloadProducts(false));
      this.scrollPosts.BindViewportBoundsTo((FrameworkElement) this.stackPanelPosts);
      this.panelPosts.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollPosts), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelPosts);
      this.panelPosts.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.panelPosts_ScrollPositionChanged);
    }

    private void HandleHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemPhotos)
        this.photosListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemVideos)
        this.videosListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemPosts)
        this.panelPosts.ScrollToBottom(false);
      else if (this.pivot.SelectedItem == this.pivotItemLinks)
        this.linksListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemProducts)
      {
        this.productsListBox.ScrollToTop();
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemPersons)
          return;
        this.usersListBox.ScrollToTop();
      }
    }

    private void panelPosts_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this.DataContext = (object) new FavoritesViewModel();
        this._isInitialized = true;
      }
      CurrentMarketItemSource.Source = MarketItemSource.fave;
      if (e.NavigationMode == NavigationMode.New)
        this.pivot.SelectedIndex = AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection;
      this.ProcessInputParameters();
    }

    private void pivot_LoadedPivotItem_1(object sender, PivotItemEventArgs e)
    {
      if (e.Item == this.pivotItemPhotos && !this._photosLoaded)
      {
        this.FavVM.PhotosVM.LoadData(false, false, (Action<BackendResult<PhotosListWithCount, ResultCode>>) null, false);
        this._photosLoaded = true;
        AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 0;
      }
      else if (e.Item == this.pivotItemVideos && !this._videosLoaded)
      {
        this.FavVM.VideosVM.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
        this._videosLoaded = true;
        AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 1;
      }
      else if (e.Item == this.pivotItemPosts && !this._postsLoaded)
      {
        this.FavVM.PostsVM.LoadData(false, false, (Action<BackendResult<WallData, ResultCode>>) null, false);
        this._postsLoaded = true;
        AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 2;
      }
      else if (e.Item == this.pivotItemPersons && !this._usersLoaded)
      {
        this.FavVM.UsersVM.LoadData(false, false, (Action<BackendResult<UsersListWithCount, ResultCode>>) null, false);
        this._usersLoaded = true;
        AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 3;
      }
      else if (e.Item == this.pivotItemLinks && !this._linksLoaded)
      {
        this.FavVM.LinksVM.LoadData(false, false, (Action<BackendResult<List<Link>, ResultCode>>) null, false);
        this._linksLoaded = true;
        AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 4;
      }
      else
      {
        if (e.Item != this.pivotItemProducts || this._productsLoaded)
          return;
        this.FavVM.ProductsVM.LoadData(false, false, (Action<BackendResult<VKList<Product>, ResultCode>>) null, false);
        this._productsLoaded = true;
        AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 5;
      }
    }

    private void photos_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.FavVM.PhotosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      AlbumPhotoHeaderFourInARow headerFourInArow1 = frameworkElement.DataContext as AlbumPhotoHeaderFourInARow;
      Photo photo = (Photo) null;
      if (headerFourInArow1 != null)
      {
        string @string = frameworkElement.Tag.ToString();
        if (!(@string == "1"))
        {
          if (!(@string == "2"))
          {
            if (!(@string == "3"))
            {
              if (@string == "4")
                photo = headerFourInArow1.Photo4.Photo;
            }
            else
              photo = headerFourInArow1.Photo3.Photo;
          }
          else
            photo = headerFourInArow1.Photo2.Photo;
        }
        else
          photo = headerFourInArow1.Photo1.Photo;
      }
      if (photo == null)
        return;
      List<Photo> photoList1 = new List<Photo>();
      foreach (AlbumPhotoHeaderFourInARow headerFourInArow2 in (Collection<AlbumPhotoHeaderFourInARow>) this.FavVM.PhotosVM.Collection)
        photoList1.AddRange(headerFourInArow2.GetAsPhotos());
      int num = photoList1.IndexOf(photo);
      List<Photo> photoList2 = new List<Photo>();
      int initialOffset = Math.Max(0, num - 20);
      for (int index = initialOffset; index < Math.Min(photoList1.Count, num + 30); ++index)
        photoList2.Add(photoList1[index]);
      this._initialOffset = initialOffset;
      Navigator.Current.NavigateToImageViewer(this.FavVM.PhotosVM.TotalCount, initialOffset, photoList2.IndexOf(photo), photoList2.Select<Photo, long>((Func<Photo, long>) (p => p.pid)).ToList<long>(), photoList2.Select<Photo, long>((Func<Photo, long>) (p => p.owner_id)).ToList<long>(), photoList2.Select<Photo, string>((Func<Photo, string>) (p => p.access_key)).ToList<string>(), photoList2, "PhotosByIdsForFavorites", false, false, new Func<int, Image>(this.GetPhotoById), (PageBase) null, false);
    }

    private Image GetPhotoById(int arg)
    {
      arg += this._initialOffset;
      int num1 = arg / 4;
      int num2 = arg % 4;
      return (Image) null;
    }

    private FrameworkElement SearchVisualTree(DependencyObject targetElement, DependencyObject comp)
    {
      FrameworkElement frameworkElement = null;
      int childrenCount = VisualTreeHelper.GetChildrenCount(targetElement);
      if (childrenCount == 0)
        return frameworkElement;
      for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
      {
        DependencyObject child = VisualTreeHelper.GetChild(targetElement, childIndex);
        if ((child as FrameworkElement).DataContext == (comp as FrameworkElement).DataContext)
          return child as FrameworkElement;
        frameworkElement = this.SearchVisualTree(child, comp);
        if (frameworkElement != null)
          return frameworkElement;
      }
      return frameworkElement;
    }

    private void Videos_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.FavVM.VideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Video_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      VideoHeader videoHeader = this.videosListBox.SelectedItem as VideoHeader;
      if (videoHeader == null)
        return;
      this.videosListBox.SelectedItem = null;
      Navigator.Current.NavigateToVideoWithComments(videoHeader.VKVideo, videoHeader.VKVideo.owner_id, videoHeader.VKVideo.vid, videoHeader.VKVideo.access_key ?? "");
    }

    private void Users_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.FavVM.UsersVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader friendHeader = this.usersListBox.SelectedItem as FriendHeader;
      if (friendHeader == null)
        return;
      this.usersListBox.SelectedItem = null;
      Navigator.Current.NavigateToUserProfile(friendHeader.UserId, friendHeader.FullName, "", false);
    }

    private void Links_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      Link link = this.linksListBox.SelectedItem as Link;
      if (link == null)
        return;
      this.linksListBox.SelectedItem = null;
      if (string.IsNullOrWhiteSpace(link.url))
        return;
      Navigator.Current.NavigateToWebUri(link.url.Trim('/'), false, false);
    }

    private void Links_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.FavVM.LinksVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Products_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.FavVM.ProductsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ProcessInputParameters()
    {
      Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
      if (group == null)
        return;
      FavoritesViewModel favVm = this.FavVM;
      ObservableCollection<IVirtualizable> observableCollection;
      if (favVm == null)
      {
        observableCollection = (ObservableCollection<IVirtualizable>) null;
      }
      else
      {
        GenericCollectionViewModel<WallData, IVirtualizable> postsVm = favVm.PostsVM;
        observableCollection = postsVm != null ? postsVm.Collection : (ObservableCollection<IVirtualizable>) null;
      }
      if (observableCollection == null)
        return;
      foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>) this.FavVM.PostsVM.Collection)
      {
        WallPostItem wallPostItem = virtualizable as WallPostItem;
        if (wallPostItem == null && virtualizable is NewsFeedAdsItem)
          wallPostItem = (virtualizable as NewsFeedAdsItem).WallPostItem;
        if ((wallPostItem != null ? wallPostItem.LikesAndCommentsItem : (LikesAndCommentsItem) null) != null && wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(group.id, group.name))
          break;
        VideosNewsItem videosNewsItem = virtualizable as VideosNewsItem;
        if (videosNewsItem != null)
          videosNewsItem.LikesAndCommentsItem.ShareInGroupIfApplicable(group.id, group.name);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/FavoritesPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemPhotos = (PivotItem) this.FindName("pivotItemPhotos");
      this.photosListBox = (ExtendedLongListSelector) this.FindName("photosListBox");
      this.pivotItemVideos = (PivotItem) this.FindName("pivotItemVideos");
      this.videosListBox = (ExtendedLongListSelector) this.FindName("videosListBox");
      this.pivotItemPosts = (PivotItem) this.FindName("pivotItemPosts");
      this.scrollPosts = (ViewportControl) this.FindName("scrollPosts");
      this.stackPanelPosts = (StackPanel) this.FindName("stackPanelPosts");
      this.panelPosts = (MyVirtualizingPanel2) this.FindName("panelPosts");
      this.pivotItemPersons = (PivotItem) this.FindName("pivotItemPersons");
      this.usersListBox = (ExtendedLongListSelector) this.FindName("usersListBox");
      this.pivotItemLinks = (PivotItem) this.FindName("pivotItemLinks");
      this.linksListBox = (ExtendedLongListSelector) this.FindName("linksListBox");
      this.pivotItemProducts = (PivotItem) this.FindName("pivotItemProducts");
      this.productsListBox = (ExtendedLongListSelector) this.FindName("productsListBox");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
    }
  }
}
