using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Photos.Library
{
  public class ImageViewerViewModel : ViewModelBase
  {
    private ObservableCollection<PhotoViewModel> _photosCollection = new ObservableCollection<PhotoViewModel>();
    private string _aid;
    private AlbumType _albumType;
    private ViewerMode _mode;
    //private bool _isLocked;
    private int _photosCount;
    private long _userOrGroupId;
    private bool _isGroup;
    private bool _isLoading;
    private int _date;
    private bool _canLoadMoreProfileListPhotos;
    private long _albumId;
    private List<string> _accessKeys;
    private List<long> _ownerIds;
    private int _initialOffset;
    private List<Doc> _gifDocs;

    public ObservableCollection<PhotoViewModel> PhotosCollection
    {
      get
      {
        return this._photosCollection;
      }
    }

    public int PhotosCount
    {
      get
      {
        return this._photosCount;
      }
    }

    public int InitialOffset
    {
      get
      {
        return this._initialOffset;
      }
    }

    public ImageViewerViewModel(string aid, AlbumType albumType, long userOrGroupId, bool isGroup, int photosCount, List<Photo> photos)
    {
      this._aid = aid;
      this._userOrGroupId = userOrGroupId;
      this._isGroup = isGroup;
      this._albumType = albumType;
      this._photosCount = photosCount <= 0 ? -1 : photosCount;
      this._mode = ViewerMode.AlbumPhotos;
      if (photos == null || photos.Count <= 0)
        return;
      this.ReadPhotos(photos);
    }

    public ImageViewerViewModel(long userOrGroupId, bool isGroup, List<Photo> photos, bool canLoadMorePhotos, long albumId = 0)
    {
      this._canLoadMoreProfileListPhotos = canLoadMorePhotos;
      this._mode = ViewerMode.ProfilePhotosList;
      this._userOrGroupId = userOrGroupId;
      this._photosCount = -1;
      this._isGroup = isGroup;
      this._albumId = albumId;
      if (photos == null || photos.Count <= 0)
        return;
      this.ReadPhotos(photos);
    }

    public ImageViewerViewModel(List<PhotoOrDocument> photosOrDocuments)
    {
      this._mode = ViewerMode.PhotosByIds;
      this._photosCount = photosOrDocuments.Count;
      this.ReadPhotosOrDocuments(photosOrDocuments);
    }

    public ImageViewerViewModel(int photosCount, int initialOffset, List<long> photoIds, List<long> ownerIds, List<string> accessKeys, List<Photo> photos, ViewerMode mode)
    {
      this._mode = mode;
      this._photosCount = photosCount;
      this._accessKeys = accessKeys;
      this._ownerIds = ownerIds;
      this._initialOffset = initialOffset;
      if (photos != null && photos.Count > 0)
        this.ReadPhotos(photos);
      else
        this.LoadPhotosByIds(photoIds);
    }

    public ImageViewerViewModel(long userOrGroupId, bool isGroup, string aid, int photosCount, int date, List<Photo> photos, ViewerMode mode)
    {
      this._aid = aid;
      this._userOrGroupId = userOrGroupId;
      this._isGroup = isGroup;
      this._mode = mode;
      this._photosCount = photosCount;
      if (photos != null)
        this.ReadPhotos(photos);
      this._date = date;
    }

    public ImageViewerViewModel(List<Doc> gifDocs)
    {
      this._gifDocs = gifDocs;
    }

    public void LoadPhotosFromFeed(Action<bool> callback)
    {
      if (this._isLoading)
        return;
      this._isLoading = true;
      PhotosService.Current.GetPhotos(this._userOrGroupId, this._isGroup, this._aid, (List<long>) null, (long) this._date, this._mode == ViewerMode.Photos ? "photo" : "photo_tag", (Action<BackendResult<List<Photo>, ResultCode>>) (res =>
      {
        this._isLoading = false;
        if (res.ResultCode == ResultCode.Succeeded)
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            int num;
            for (int i = 0; i < res.ResultData.Count; i = num + 1)
            {
              PhotoViewModel photoViewModel = this.PhotosCollection.FirstOrDefault<PhotoViewModel>((Func<PhotoViewModel, bool>) (p =>
              {
                if (p.Photo != null)
                  return p.Photo.pid == res.ResultData[i].pid;
                return false;
              }));
              if (photoViewModel != null)
                this.PhotosCollection[this.PhotosCollection.IndexOf(photoViewModel)].Photo = res.ResultData[i];
              else
                this.PhotosCollection.Add(new PhotoViewModel(res.ResultData[i], (PhotoWithFullInfo) null));
              num = i;
            }
            if (this._photosCount < this.PhotosCollection.Count)
              this._photosCount = this.PhotosCollection.Count;
            callback(true);
          }));
        else
          callback(false);
      }));
    }

    private void LoadPhotosByIds(List<long> photoIds)
    {
      for (int index = 0; index < photoIds.Count; ++index)
        this.PhotosCollection.Add(new PhotoViewModel(this._ownerIds[0], photoIds[index], index < this._accessKeys.Count ? this._accessKeys[index] : ""));
    }

    public void LoadMorePhotos(Action<bool> callback = null)
    {
      if (this._isLoading)
        return;
      this._isLoading = true;
      if (this._photosCount == this.PhotosCollection.Count || this._mode == ViewerMode.ProfilePhotosList && !this._canLoadMoreProfileListPhotos)
        return;
      switch (this._mode)
      {
        case ViewerMode.AlbumPhotos:
          ImageViewerViewModel.GetAlbumPhotos(this._albumType, this._aid, this._userOrGroupId, this._isGroup, this.PhotosCollection.Count, 50, (Action<BackendResult<PhotosListWithCount, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (res.ResultCode == ResultCode.Succeeded)
              Execute.ExecuteOnUIThread((Action) (() =>
              {
                foreach (Photo photo in res.ResultData.response)
                  this._photosCollection.Add(new PhotoViewModel(photo, (PhotoWithFullInfo) null));
                this._photosCount = res.ResultData.photosCount;
              }));
            if (callback != null)
              callback(res.ResultCode == ResultCode.Succeeded);
            this._isLoading = false;
          }))));
          break;
        case ViewerMode.PhotosByIdsForFavorites:
          FavoritesService.Instance.GetFavePhotos(this._initialOffset + this.PhotosCollection.Count, 50, (Action<BackendResult<PhotosListWithCount, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (res.ResultCode == ResultCode.Succeeded)
            {
              foreach (Photo photo in res.ResultData.response)
                this._photosCollection.Add(new PhotoViewModel(photo, (PhotoWithFullInfo) null));
              this._photosCount = res.ResultData.photosCount;
            }
            if (callback != null)
              callback(res.ResultCode == ResultCode.Succeeded);
            this._isLoading = false;
          }))));
          break;
        case ViewerMode.ProfilePhotosList:
          if (this._isGroup)
          {
            if (this._albumId != 0L)
            {
              ProfilesService.Instance.GetPhotos(-this._userOrGroupId, this._photosCollection.Count, this._albumId, (Action<BackendResult<VKList<Photo>, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
              {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                  foreach (Photo photo in result.ResultData.items)
                    this._photosCollection.Add(new PhotoViewModel(photo, (PhotoWithFullInfo) null));
                  this._canLoadMoreProfileListPhotos = result.ResultData.more;
                }
                if (callback != null)
                  callback(result.ResultCode == ResultCode.Succeeded);
                this._isLoading = false;
              }))));
              break;
            }
            if (callback != null)
              callback(false);
            this._isLoading = false;
            break;
          }
          ProfilesService.Instance.GetAllPhotos(this._userOrGroupId, this._photosCollection.Last<PhotoViewModel>().RealOffset + 1, (Action<BackendResult<VKList<Photo>, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (result.ResultCode == ResultCode.Succeeded)
            {
              foreach (Photo photo in result.ResultData.items)
                this._photosCollection.Add(new PhotoViewModel(photo, (PhotoWithFullInfo) null));
              this._canLoadMoreProfileListPhotos = result.ResultData.more;
            }
            if (callback != null)
              callback(result.ResultCode == ResultCode.Succeeded);
            this._isLoading = false;
          }))));
          break;
      }
    }

    public static void GetAlbumPhotos(AlbumType albumType, string albumId, long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      PhotosService current = PhotosService.Current;
      switch (albumType)
      {
        case AlbumType.AllPhotos:
          current.GetAllPhotos(userOrGroupId, isGroup, offset, count, callback);
          break;
        case AlbumType.ProfilePhotos:
          current.GetProfilePhotos(userOrGroupId, offset, count, callback);
          break;
        case AlbumType.PhotosWithUser:
          current.GetUserPhotos(userOrGroupId, offset, count, callback);
          break;
        case AlbumType.WallPhotos:
          current.GetWallPhotos(userOrGroupId, isGroup, offset, count, callback);
          break;
        case AlbumType.SavedPhotos:
          current.GetSavedPhotos(userOrGroupId, isGroup, offset, count, callback);
          break;
        case AlbumType.NormalAlbum:
          current.GetAlbumPhotos(userOrGroupId, isGroup, albumId, offset, count, callback);
          break;
      }
    }

    private void ReadPhotos(List<Photo> photos)
    {
      this._photosCollection.Clear();
      foreach (Photo photo in photos)
      {
        PhotoWithFullInfo photoWithFullInfo1 = (PhotoWithFullInfo) null;
        if (photo.owner_id == AppGlobalStateManager.Current.LoggedInUserId && photo.album_id == -3L)
        {
          PhotoWithFullInfo photoWithFullInfo2 = new PhotoWithFullInfo();
          photoWithFullInfo2.Photo = photo;
          photoWithFullInfo2.Users = new List<User>()
          {
            AppGlobalStateManager.Current.GlobalState.LoggedInUser
          };
          List<Group> groupList = new List<Group>();
          photoWithFullInfo2.Groups = groupList;
          List<PhotoVideoTag> photoVideoTagList = new List<PhotoVideoTag>();
          photoWithFullInfo2.PhotoTags = photoVideoTagList;
          List<Comment> commentList = new List<Comment>();
          photoWithFullInfo2.Comments = commentList;
          List<long> longList = new List<long>();
          photoWithFullInfo2.LikesAllIds = longList;
          List<User> userList1 = new List<User>();
          photoWithFullInfo2.Users2 = userList1;
          List<User> userList2 = new List<User>();
          photoWithFullInfo2.Users3 = userList2;
          photoWithFullInfo1 = photoWithFullInfo2;
        }
        this._photosCollection.Add(new PhotoViewModel(photo, photoWithFullInfo1));
      }
    }

    private void ReadPhotos(List<Doc> gifDocs)
    {
      this._photosCollection.Clear();
      foreach (Doc gifDoc in gifDocs)
        this._photosCollection.Add(new PhotoViewModel(gifDoc));
    }

    private void ReadPhotosOrDocuments(List<PhotoOrDocument> photosOrDocuments)
    {
      this._photosCollection.Clear();
      foreach (PhotoOrDocument photosOrDocument in photosOrDocuments)
      {
        PhotoViewModel photoViewModel = (PhotoViewModel) null;
        if (photosOrDocument.photo != null)
          photoViewModel = new PhotoViewModel(photosOrDocument.photo, (PhotoWithFullInfo) null);
        else if (photosOrDocument.document != null)
          photoViewModel = new PhotoViewModel(photosOrDocument.document);
        if (photoViewModel != null)
          this._photosCollection.Add(photoViewModel);
      }
    }
  }
}
