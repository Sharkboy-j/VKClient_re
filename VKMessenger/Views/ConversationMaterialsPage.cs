using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.UC;
using VKClient.Common.UC.InplaceGifViewer;
using VKClient.Photos.Library;
using VKClient.Video.Library;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ConversationMaterialsPage : PageBase
  {
    private bool _isInitialized;
    private bool _photosLoaded;
    private bool _videosLoaded;
    private bool _audiosLoaded;
    private bool _documentsLoaded;
    private bool _linksLoaded;
    internal GenericHeaderUC header;
    internal Pivot pivot;
    internal PivotItem pivotItemPhotos;
    internal ExtendedLongListSelector photosList;
    internal PivotItem pivotItemVideos;
    internal ExtendedLongListSelector videosList;
    internal PivotItem pivotItemAudios;
    internal ExtendedLongListSelector audiosList;
    internal PivotItem pivotItemDocuments;
    internal ExtendedLongListSelector documentsList;
    internal PivotItem pivotItemLinks;
    internal ExtendedLongListSelector linksList;
    internal PullToRefreshUC pullToRefresh;
    private bool _contentLoaded;

    private ConversationMaterialsViewModel ViewModel
    {
      get
      {
        return this.DataContext as ConversationMaterialsViewModel;
      }
    }

    public ConversationMaterialsPage()
    {
      this.InitializeComponent();
      this.header.OnHeaderTap = new Action(this.HandleHeaderTap);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.photosList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.videosList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.audiosList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.documentsList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.linksList);
      this.photosList.OnRefresh = (Action) (() => this.ViewModel.PhotosVM.LoadData(true, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false));
      this.videosList.OnRefresh = (Action) (() => this.ViewModel.VideosVM.LoadData(true, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false));
      this.audiosList.OnRefresh = (Action) (() => this.ViewModel.AudiosVM.LoadData(true, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false));
      this.documentsList.OnRefresh = (Action) (() => this.ViewModel.DocumentsVM.LoadData(true, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false));
      this.linksList.OnRefresh = (Action) (() => this.ViewModel.LinksVM.LoadData(true, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false));
    }

    private void HandleHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemPhotos)
        this.photosList.ScrollToTop();
      if (this.pivot.SelectedItem == this.pivotItemVideos)
        this.videosList.ScrollToTop();
      if (this.pivot.SelectedItem == this.pivotItemAudios)
        this.audiosList.ScrollToTop();
      if (this.pivot.SelectedItem == this.pivotItemDocuments)
        this.documentsList.ScrollToTop();
      if (this.pivot.SelectedItem != this.pivotItemLinks)
        return;
      this.linksList.ScrollToTop();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this.DataContext = (object) new ConversationMaterialsViewModel(long.Parse(this.NavigationContext.QueryString["PeerId"]));
      this._isInitialized = true;
    }

    private void pivot_OnItemLoaded(object sender, PivotItemEventArgs e)
    {
      if (e.Item == this.pivotItemPhotos && !this._photosLoaded)
      {
        this.ViewModel.PhotosVM.LoadData(false, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false);
        this._photosLoaded = true;
      }
      if (e.Item == this.pivotItemVideos && !this._videosLoaded)
      {
        this.ViewModel.VideosVM.LoadData(false, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false);
        this._videosLoaded = true;
      }
      if (e.Item == this.pivotItemAudios && !this._audiosLoaded)
      {
        this.ViewModel.AudiosVM.LoadData(false, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false);
        this._audiosLoaded = true;
      }
      if (e.Item == this.pivotItemDocuments && !this._documentsLoaded)
      {
        this.ViewModel.DocumentsVM.LoadData(false, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false);
        this._documentsLoaded = true;
      }
      if (e.Item != this.pivotItemLinks || this._linksLoaded)
        return;
      this.ViewModel.LinksVM.LoadData(false, false, (Action<BackendResult<VKList<Attachment>, ResultCode>>) null, false);
      this._linksLoaded = true;
    }

    private void photosList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.PhotosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void videosList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.VideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void audiosList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.AudiosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void documentsList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.DocumentsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void linksList_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.LinksVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void photo_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      Photo photo = (Photo) null;
      AlbumPhotoHeaderFourInARow headerFourInArow1 = frameworkElement.DataContext as AlbumPhotoHeaderFourInARow;
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
      foreach (AlbumPhotoHeaderFourInARow headerFourInArow2 in (Collection<AlbumPhotoHeaderFourInARow>) this.ViewModel.PhotosVM.Collection)
        photoList1.AddRange(headerFourInArow2.GetAsPhotos());
      int num = photoList1.IndexOf(photo);
      int initialOffset = Math.Max(0, num - 20);
      List<Photo> photoList2 = new List<Photo>();
      for (int index = initialOffset; index < Math.Min(photoList1.Count, num + 30); ++index)
        photoList2.Add(photoList1[index]);
      Navigator.Current.NavigateToImageViewer(this.ViewModel.PhotosVM.TotalCount, initialOffset, photoList2.IndexOf(photo), photoList2.Select<Photo, long>((Func<Photo, long>) (p => p.pid)).ToList<long>(), photoList2.Select<Photo, long>((Func<Photo, long>) (p => p.owner_id)).ToList<long>(), photoList2.Select<Photo, string>((Func<Photo, string>) (p => p.access_key)).ToList<string>(), photoList2, "PhotosByIds", true, false, (Func<int, Image>) (i => (Image) null), (PageBase) null, false);
    }

    private void videosList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      VideoHeader videoHeader = this.videosList.SelectedItem as VideoHeader;
      this.videosList.SelectedItem = null;
      if (videoHeader == null)
        return;
      Navigator.Current.NavigateToVideoWithComments(videoHeader.VKVideo, videoHeader.VKVideo.owner_id, videoHeader.VKVideo.vid, videoHeader.VKVideo.access_key ?? "");
    }

    private void audiosList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      AudioHeader audioHeader = this.audiosList.SelectedItem as AudioHeader;
      this.audiosList.SelectedItem = null;
      if (audioHeader == null)
        return;
      PlaylistManager.SetAudioAgentPlaylist(this.audiosList.ItemsSource.OfType<AudioHeader>().Select<AudioHeader, AudioObj>((Func<AudioHeader, AudioObj>) (item => item.Track)).ToList<AudioObj>(), CurrentMediaSource.AudioSource);
      audioHeader.AssignTrack();
      Navigator.Current.NavigateToAudioPlayer(false);
    }

    private void documentsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      DocumentHeader documentHeader1 = this.documentsList.SelectedItem as DocumentHeader;
      this.documentsList.SelectedItem = null;
      if (documentHeader1 == null)
        return;
      if (!documentHeader1.IsGif)
      {
        Navigator.Current.NavigateToWebUri(documentHeader1.Document.url, true, false);
      }
      else
      {
        List<DocumentHeader> list1 = this.ViewModel.DocumentsVM.Collection.Where<DocumentHeader>((Func<DocumentHeader, bool>) (doc => doc.IsGif)).ToList<DocumentHeader>();
        int selectedIndex = -1;
        List<PhotoOrDocument> list = new List<PhotoOrDocument>();
        for (int index = 0; index < list1.Count; ++index)
        {
          DocumentHeader documentHeader2 = list1[index];
          if (documentHeader2 == documentHeader1)
            selectedIndex = index;
          list.Add(new PhotoOrDocument()
          {
            document = documentHeader2.Document
          });
        }
        if (selectedIndex < 0)
          return;
        InplaceGifViewerUC gifViewer = new InplaceGifViewerUC();
        Navigator.Current.NavigateToImageViewerPhotosOrGifs(selectedIndex, list, false, false, (Func<int, Image>) null, (PageBase) null, false, (FrameworkElement) gifViewer, (Action<int>) (ind =>
        {
          Doc document = list[ind].document;
          if (document != null)
          {
            InplaceGifViewerViewModel gifViewerViewModel = new InplaceGifViewerViewModel(document, true, false, false);
            gifViewerViewModel.Play(GifPlayStartType.manual);
            gifViewer.VM = gifViewerViewModel;
            gifViewer.Visibility = Visibility.Visible;
          }
          else
          {
            InplaceGifViewerViewModel vm = gifViewer.VM;
            if (vm != null)
              vm.Stop();
            gifViewer.Visibility = Visibility.Collapsed;
          }
        }), (Action<int, bool>) ((i, b) => {}), false);
      }
    }

    private void linksList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      LinkHeader linkHeader = this.linksList.SelectedItem as LinkHeader;
      this.linksList.SelectedItem = null;
      if (linkHeader == null)
        return;
      Navigator.Current.NavigateToWebUri(linkHeader.Url, false, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ConversationMaterialsPage.xaml", UriKind.Relative));
      this.header = (GenericHeaderUC) this.FindName("header");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemPhotos = (PivotItem) this.FindName("pivotItemPhotos");
      this.photosList = (ExtendedLongListSelector) this.FindName("photosList");
      this.pivotItemVideos = (PivotItem) this.FindName("pivotItemVideos");
      this.videosList = (ExtendedLongListSelector) this.FindName("videosList");
      this.pivotItemAudios = (PivotItem) this.FindName("pivotItemAudios");
      this.audiosList = (ExtendedLongListSelector) this.FindName("audiosList");
      this.pivotItemDocuments = (PivotItem) this.FindName("pivotItemDocuments");
      this.documentsList = (ExtendedLongListSelector) this.FindName("documentsList");
      this.pivotItemLinks = (PivotItem) this.FindName("pivotItemLinks");
      this.linksList = (ExtendedLongListSelector) this.FindName("linksList");
      this.pullToRefresh = (PullToRefreshUC) this.FindName("pullToRefresh");
    }
  }
}
