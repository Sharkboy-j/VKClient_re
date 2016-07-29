using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Video.Library;
using VKClient.Video.Localization;

namespace VKClient.Video
{
  public class VideosOfOwnerViewModel : ViewModelBase, ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>, ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>, IHandle<VideoAddedDeleted>, IHandle, IHandle<VideoAlbumAddedDeletedEvent>, IHandle<VideoAlbumEditedEvent>
  {
    private GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> _allVideosVM;
    private GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> _uploadedVideosVM;
    private GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> _albumsVM;
    private long _albumId;
    private bool _haveUploadedVideos;
    private bool _haveAlbums;
    private bool _pickMode;
    private bool _loadAddedVideos;

    public long UserOrGroupId { get; private set; }

    public bool IsGroup { get; private set; }

    public long OwnerId
    {
      get
      {
        if (!this.IsGroup)
          return this.UserOrGroupId;
        return -this.UserOrGroupId;
      }
    }

    public GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> AllVideosVM
    {
      get
      {
        return this._allVideosVM;
      }
    }

    public GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> UploadedVideosVM
    {
      get
      {
        return this._uploadedVideosVM;
      }
    }

    public GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> AlbumsVM
    {
      get
      {
        return this._albumsVM;
      }
    }

    public Action GotUploadedAndAlbumsInfoCallback { get; set; }

    private long ActualAlbumId
    {
      get
      {
        if (this._albumId != 0L)
          return this._albumId;
        if (!this._loadAddedVideos)
          return VideoAlbum.UPLOADED_ALBUM_ID;
        return VideoAlbum.ADDED_ALBUM_ID;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Videos.ToUpperInvariant();
      }
    }

    public bool HaveUploadedVideos
    {
      get
      {
        return this._haveUploadedVideos;
      }
      set
      {
        this._haveUploadedVideos = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveUploadedVideos));
      }
    }

    public Visibility HaveUploadedVideosVisibility
    {
      get
      {
        return !this._haveUploadedVideos ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool HaveAlbums
    {
      get
      {
        return this._haveAlbums;
      }
      set
      {
        this._haveAlbums = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveAlbums));
      }
    }

    public Visibility HaveAlbumsVisibility
    {
      get
      {
        return !this._haveAlbums ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool ShowAddedAlbum { get; set; }

    public Action<bool, GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>> ReportBusyCallback
    {
      get
      {
        return (Action<bool, GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>>) ((b, c) => {});
      }
    }

    public Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>>) (input => new ListWithCount<VideoHeader>()
        {
          TotalCount = input.count,
          List = new List<VideoHeader>(input.items.Select<VKClient.Common.Backend.DataObjects.Video, VideoHeader>((Func<VKClient.Common.Backend.DataObjects.Video, VideoHeader>) (v =>
          {
            VKClient.Common.Backend.DataObjects.Video video = v;
            List<User> profiles = input.profiles;
            List<Group> groups = input.groups;
            int num1 = this.IsGroup ? 4 : 3;
            long num2 = this.UserOrGroupId;
            string string1 = num2.ToString();
            string str = "_";
            num2 = this.ActualAlbumId;
            string string2 = num2.ToString();
            string context = string1 + str + string2;
            int num3 = this._pickMode ? 1 : 0;
            long albumId = this._albumId;
            return new VideoHeader(video, null, profiles, groups, (StatisticsActionSource) num1, context, num3 != 0, albumId);
          })))
        });
      }
    }

    Func<VKList<VideoAlbum>, ListWithCount<AlbumHeader>> ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<VideoAlbum>, ListWithCount<AlbumHeader>>) (list =>
        {
          ListWithCount<AlbumHeader> listWithCount = new ListWithCount<AlbumHeader>();
          listWithCount.TotalCount = list.count;
          foreach (VideoAlbum va in list.items)
          {
            if (va.album_id != VideoAlbum.ADDED_ALBUM_ID)
              listWithCount.List.Add(new AlbumHeader(va, this._pickMode, false));
          }
          return listWithCount;
        });
      }
    }

    public VideosOfOwnerViewModel(long userOrGroupId, bool isGroup, long albumId, bool pickMode = false)
    {
      this.UserOrGroupId = userOrGroupId;
      this.IsGroup = isGroup;
      this._albumId = albumId;
      this._pickMode = pickMode;
      this._allVideosVM = new GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>((ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>) this)
      {
        NoContentText = CommonResources.NoContent_Videos,
        NoContentImage = "../Resources/NoContentImages/Videos.png"
      };
      this._uploadedVideosVM = new GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>((ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>) this)
      {
        NoContentText = CommonResources.NoContent_Videos,
        NoContentImage = "../Resources/NoContentImages/Videos.png"
      };
      this._albumsVM = new GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader>((ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>) this);
      EventAggregator.Current.Subscribe((object) this);
    }

    public void Handle(VideoAddedDeleted message)
    {
      if (this.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && !this.IsGroup)
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (message.IsAdded && message.AlbumId == VideoAlbum.ADDED_ALBUM_ID && (message.TargetId == 0L || message.TargetId == AppGlobalStateManager.Current.LoggedInUserId) && this._albumId == 0L)
            this.AllVideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
          else if (message.IsAdded && message.AlbumId == this._albumId && this._albumId != 0L && (message.TargetId == 0L || message.TargetId == AppGlobalStateManager.Current.LoggedInUserId))
          {
            this.AllVideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
          }
          else
          {
            if ((message.IsAdded || message.IsDeletedPermanently || message.AlbumId != this._albumId) && (message.AlbumId != VideoAlbum.ADDED_ALBUM_ID || this._albumId != 0L))
              return;
            VideoHeader videoHeader = this.AllVideosVM.Collection.FirstOrDefault<VideoHeader>((Func<VideoHeader, bool>) (v =>
            {
              if (v.VKVideo.vid == message.VideoId)
                return v.VKVideo.owner_id == message.OwnerId;
              return false;
            }));
            if (videoHeader == null)
              return;
            this.AllVideosVM.Delete(videoHeader);
          }
        }));
      if (!message.IsDeletedPermanently)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        VideoHeader videoHeader1 = this.AllVideosVM.Collection.FirstOrDefault<VideoHeader>((Func<VideoHeader, bool>) (v =>
        {
          if (v.VKVideo.vid == message.VideoId)
            return v.VKVideo.owner_id == message.OwnerId;
          return false;
        }));
        if (videoHeader1 != null)
          this.AllVideosVM.Delete(videoHeader1);
        VideoHeader videoHeader2 = this.UploadedVideosVM.Collection.FirstOrDefault<VideoHeader>((Func<VideoHeader, bool>) (v =>
        {
          if (v.VKVideo.id == message.VideoId)
            return v.VKVideo.owner_id == message.OwnerId;
          return false;
        }));
        if (videoHeader2 == null)
          return;
        this.UploadedVideosVM.Delete(videoHeader2);
      }));
    }

    public void GetData(GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> caller, int offset, int count, Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>> callback)
    {
      if (this._albumId == 0L)
      {
        this._loadAddedVideos = caller == this._allVideosVM;
        if (this._loadAddedVideos && offset == 0)
          VideoService.Instance.GetVideoData(this.OwnerId, (Action<BackendResult<VideosData, ResultCode>>) (res =>
          {
            if (res.ResultCode == ResultCode.Succeeded)
            {
              this.HaveUploadedVideos = res.ResultData.UploadedVideosCount > 0;
              this.HaveAlbums = res.ResultData.VideoAlbumsCount > 0;
              if (this.GotUploadedAndAlbumsInfoCallback != null)
                this.GotUploadedAndAlbumsInfoCallback();
            }
            VKList<VKClient.Common.Backend.DataObjects.Video> resultData = res.ResultData == null || res.ResultData.AddedVideos == null ? (VKList<VKClient.Common.Backend.DataObjects.Video>) null : res.ResultData.AddedVideos;
            callback(new BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>(res.ResultCode, resultData));
          }));
        VideoService.Instance.GetVideos(this.UserOrGroupId, this.IsGroup, offset, count, callback, this._loadAddedVideos ? 0L : -1L, true);
      }
      else
        VideoService.Instance.GetVideos(this.UserOrGroupId, this.IsGroup, offset, count, callback, this._albumId, true);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> caller, int count)
    {
      if (count == 0)
        return VideoResources.NoVideos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, VideoResources.OneVideoFrm, VideoResources.TwoFourVideosFrm, VideoResources.FiveVideosFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> caller, int offset, int count, Action<BackendResult<VKList<VideoAlbum>, ResultCode>> callback)
    {
      VideoService.Instance.GetAlbums(this.UserOrGroupId, this.IsGroup, this.ShowAddedAlbum, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> caller, int count)
    {
      if (count <= 0)
        return VideoResources.NoAlbums;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, VideoResources.OneAlbumFrm, VideoResources.TwoFourAlbumsFrm, VideoResources.FiveAlbumsFrm, true, null, false);
    }

    public void Handle(VideoAlbumEditedEvent message)
    {
      if (message.OwnerId != this.OwnerId)
        return;
      AlbumHeader albumHeader = this._albumsVM.Collection.FirstOrDefault<AlbumHeader>((Func<AlbumHeader, bool>) (a =>
      {
        if (a.VideoAlbum.album_id == message.AlbumId)
          return a.VideoAlbum.owner_id == message.OwnerId;
        return false;
      }));
      if (albumHeader == null)
        return;
      albumHeader.VideoAlbum.privacy = message.Privacy.ToStringList();
      albumHeader.VideoAlbum.title = message.Name;
    }

    public void Handle(VideoAlbumAddedDeletedEvent message)
    {
      if (message.OwnerId != this.OwnerId)
        return;
      this._albumsVM.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>) null, false);
    }
  }
}
