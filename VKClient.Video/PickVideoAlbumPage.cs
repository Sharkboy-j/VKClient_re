using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Video.Library;

namespace VKClient.Video
{
    public partial class PickVideoAlbumPage : PageBase
  {
    private bool _isInitialized;
    private long _videoId;
    private long _videoOwnerId;
    private VideosOfOwnerViewModel VM
    {
      get
      {
        return this.DataContext as VideosOfOwnerViewModel;
      }
    }

    public PickVideoAlbumPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._videoId = long.Parse(this.NavigationContext.QueryString["VideoId"]);
      this._videoOwnerId = long.Parse(this.NavigationContext.QueryString["VideoOwnerId"]);
      VideosOfOwnerViewModel ofOwnerViewModel = new VideosOfOwnerViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, 0L, false);
      ofOwnerViewModel.ShowAddedAlbum = true;
      this.DataContext = (object) ofOwnerViewModel;
      ofOwnerViewModel.AlbumsVM.LoadData(false, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void Albums_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.AlbumsVM.LoadMoreIfNeeded((object) e.ContentPresenter);
    }

    private void Albums_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      AlbumHeader albumHeader = this.listBoxAlbums.SelectedItem as AlbumHeader;
      if (albumHeader == null)
        return;
      this.listBoxAlbums.SelectedItem = (object) null;
      this.AddVideoToAlbum(albumHeader.VideoAlbum.id);
    }

    private void AddVideoToAlbum(long album_id)
    {
    }
  }
}
