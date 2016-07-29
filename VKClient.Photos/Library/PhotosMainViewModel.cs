using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Localization;

namespace VKClient.Photos.Library
{
    public class PhotosMainViewModel : ViewModelBase, IHandle<PhotoDeletedFromAlbum>, IHandle, IHandle<PhotoUploadedToAlbum>, IHandle<PhotosMovedToAlbum>, IHandle<PhotoSetAsAlbumCover>, IHandle<PhotoAlbumCreated>, IHandle<PhotoAlbumDeleted>, ICollectionDataProvider<AlbumsData, Group<AlbumHeader>>, ICollectionDataProvider<AlbumsData, AlbumHeader>, ISupportReorder<AlbumHeader>
    {
        private readonly ThemeHelper _themeHelper = new ThemeHelper();
        private readonly bool _selectForMove;
        private readonly string _excludeAlbumId;
        private AlbumsData _albumsData;
        private bool _inAlbumCreatedHandler;

        public Visibility PhotoFeedMoveNotificationVisibility { get; set; }

        public long UserOrGroupId { get; set; }

        public bool IsGroup { get; set; }

        public GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> AlbumsVM { get; set; }

        public GenericCollectionViewModel<AlbumsData, AlbumHeader> EditAlbumsVM { get; set; }

        public string Title
        {
            get
            {
                if (!this._selectForMove)
                    return this.PhotoPageTitle2;
                return PhotoResources.PhotosMainPage_ChooseAlbum;
            }
        }

        public string PhotoPageTitle2
        {
            get
            {
                if (this.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && !this.IsGroup || (this._albumsData == null || this.IsGroup))
                    return PhotoResources.PhotosMainPage_MyPhotosTitle;
                return string.Format(PhotoResources.PhotosMainPage_TitleFrm, (object)this._albumsData.userGen.first_name).ToUpperInvariant();
            }
        }

        public string Title2
        {
            get
            {
                if (this.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && !this.IsGroup)
                    return CommonResources.PhotoPage_My;
                return PhotoResources.PhotosMainPage_Albums;
            }
        }

        private long OwnerId
        {
            get
            {
                if (this.UserOrGroupId == 0L)
                    return AppGlobalStateManager.Current.LoggedInUserId;
                if (!this.IsGroup)
                    return this.UserOrGroupId;
                return -this.UserOrGroupId;
            }
        }

        public string PhotoFeedMoveNotificationIconSource
        {
            get
            {
                return this._themeHelper.PhoneLightThemeVisibility == Visibility.Visible ? "/Resources/PhotosMovedNotification/PhotosMovedNotificationLight.png" : "/Resources/PhotosMovedNotification/PhotosMovedNotificationDark.png";
            }
        }

        Func<AlbumsData, ListWithCount<Group<AlbumHeader>>> ICollectionDataProvider<AlbumsData, Group<AlbumHeader>>.ConverterFunc
        {
            get
            {
                return (Func<AlbumsData, ListWithCount<Group<AlbumHeader>>>)(data =>
                {
                    List<AlbumHeader> nonEditAlbums;
                    List<AlbumHeader> editAlbums;
                    this.ConvertToAlbumHeaders(data, out nonEditAlbums, out editAlbums);
                    if (this._selectForMove)
                        nonEditAlbums.Clear();
                    ListWithCount<Group<AlbumHeader>> listWithCount = new ListWithCount<Group<AlbumHeader>>() { TotalCount = nonEditAlbums.Count + data.Albums.count };
                    Group<AlbumHeader> source1 = new Group<AlbumHeader>("", false);
                    foreach (AlbumHeader albumHeader in nonEditAlbums)
                        source1.Add(albumHeader);
                    Group<AlbumHeader> source2 = new Group<AlbumHeader>(PhotosMainViewModel.FormatAlbumsCount(data.Albums.count), !source1.Any<AlbumHeader>());
                    foreach (AlbumHeader albumHeader in editAlbums)
                    {
                        if (!this._selectForMove || albumHeader.AlbumId != "-6")
                            source2.Add(albumHeader);
                    }
                    listWithCount.List.Add(source1);
                    if (source2.Any<AlbumHeader>())
                        listWithCount.List.Add(source2);
                    return listWithCount;
                });
            }
        }

        Func<AlbumsData, ListWithCount<AlbumHeader>> ICollectionDataProvider<AlbumsData, AlbumHeader>.ConverterFunc
        {
            get
            {
                return (Func<AlbumsData, ListWithCount<AlbumHeader>>)(data =>
                {
                    List<AlbumHeader> nonEditAlbums;
                    List<AlbumHeader> editAlbums;
                    this.ConvertToAlbumHeaders(data, out nonEditAlbums, out editAlbums);
                    return new ListWithCount<AlbumHeader>() { TotalCount = editAlbums.Count, List = editAlbums };
                });
            }
        }

        public PhotosMainViewModel(long userOrGroupId, bool isGroup, bool selectForMove = false, string excludeAlbumId = "")
        {
            this.PhotoFeedMoveNotificationVisibility = Visibility.Visible;

            this.UserOrGroupId = userOrGroupId;
            this.IsGroup = isGroup;
            this._selectForMove = selectForMove;
            this._excludeAlbumId = excludeAlbumId;
            EventAggregator.Current.Subscribe((object)this);
            this.AlbumsVM = new GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>>((ICollectionDataProvider<AlbumsData, Group<AlbumHeader>>)this)
            {
                NoContentText = CommonResources.NoContent_Photos_Albums,
                NoContentImage = "/Resources/NoContentImages/Photos.png"
            };
            this.EditAlbumsVM = new GenericCollectionViewModel<AlbumsData, AlbumHeader>((ICollectionDataProvider<AlbumsData, AlbumHeader>)this)
            {
                CanShowProgress = false
            };
        }

        public void HidePhotoFeedMoveNotification()
        {
            AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown = true;
            this.PhotoFeedMoveNotificationVisibility = Visibility.Collapsed;
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.PhotoFeedMoveNotificationVisibility));
        }

        public void Reordered(AlbumHeader item, AlbumHeader before, AlbumHeader after)
        {
            PhotosService.Current.ReorderAlbums(item.AlbumId, before != null ? before.AlbumId : "", after != null ? after.AlbumId : "", this.OwnerId, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }));
            this.UpdateAlbums();
        }

        private void UpdateAlbums()
        {
            if (!this.EditAlbumsVM.Collection.Any<AlbumHeader>())
            {
                this.AlbumsVM.Collection.RemoveAt(1);
                this.AlbumsVM.NotifyChanged();
            }
            else
            {
                this.AlbumsVM.Collection[1].Clear();
                foreach (AlbumHeader albumHeader in (Collection<AlbumHeader>)this.EditAlbumsVM.Collection)
                    this.AlbumsVM.Collection[1].Add(albumHeader);
                this.UpdateAlbumsCount();
                this.AlbumsVM.NotifyChanged();
            }
        }

        public void LoadAlbums()
        {
            this.AlbumsVM.LoadData(false, false, (Action<BackendResult<AlbumsData, ResultCode>>)null, false);
            this.EditAlbumsVM.LoadData(false, false, (Action<BackendResult<AlbumsData, ResultCode>>)null, false);
        }

        internal void AddOrUpdateAlbum(Album createdOrUpdatedAlbum)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                AlbumHeader byAlbumId = this.FindByAlbumId(createdOrUpdatedAlbum.aid);
                if (byAlbumId != null)
                {
                    byAlbumId.Album = createdOrUpdatedAlbum;
                    byAlbumId.ReadDataFromAlbumField();
                    AlbumHeader albumHeader = this.EditAlbumsVM.Collection.FirstOrDefault<AlbumHeader>((Func<AlbumHeader, bool>)(ah => ah.AlbumId == createdOrUpdatedAlbum.aid));
                    if (albumHeader == null)
                        return;
                    albumHeader.Album = createdOrUpdatedAlbum;
                    albumHeader.ReadDataFromAlbumField();
                }
                else
                {
                    AlbumHeader albumHeader1 = new AlbumHeader();
                    albumHeader1.AlbumType = AlbumType.NormalAlbum;
                    Album album = createdOrUpdatedAlbum;
                    albumHeader1.Album = album;
                    AlbumHeader albumHeader2 = albumHeader1;
                    albumHeader2.ImageUri = albumHeader2.ImageUriSmall = "http://vk.com/images/m_noalbum.png";
                    albumHeader2.ReadDataFromAlbumField();
                    this.EditAlbumsVM.Insert(albumHeader2, 0);
                    if (this.AlbumsVM.Collection.Count > 1)
                    {
                        this.AlbumsVM.Collection[1].Insert(0, albumHeader2);
                    }
                    else
                    {
                        GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> albumsVm = this.AlbumsVM;
                        string name = PhotosMainViewModel.FormatAlbumsCount(1);
                        List<AlbumHeader> albumHeaderList = new List<AlbumHeader>();
                        albumHeaderList.Add(albumHeader2);
                        int num = 0;
                        Group<AlbumHeader> group = new Group<AlbumHeader>(name, (IEnumerable<AlbumHeader>)albumHeaderList, num != 0);
                        int count = this.AlbumsVM.Collection.Count;
                        albumsVm.Insert(group, count);
                    }
                    this.UpdateAlbums();
                    this.UpdateAlbumsCount();
                    if (this._inAlbumCreatedHandler)
                        return;
                    EventAggregator.Current.Publish((object)new PhotoAlbumCreated()
                    {
                        Album = createdOrUpdatedAlbum,
                        EventSource = this.GetHashCode()
                    });
                }
            }));
        }

        private void UpdateAlbumsCount()
        {
            if (this.AlbumsVM.Collection.Count < 2)
                return;
            this.AlbumsVM.Collection[1].Title = PhotosMainViewModel.FormatAlbumsCount(this.AlbumsVM.Collection[1].Count);
        }

        internal void DeleteAlbums(List<AlbumHeader> list)
        {
            PhotosService.Current.DeleteAlbums(list.Select<AlbumHeader, string>((Func<AlbumHeader, string>)(ah => ah.AlbumId)).ToList<string>(), this.IsGroup ? this.UserOrGroupId : 0L);
            foreach (AlbumHeader albumHeader in list)
            {
                this.EditAlbumsVM.Delete(albumHeader);
                EventAggregator.Current.Publish((object)new PhotoAlbumDeleted()
                {
                    aid = albumHeader.AlbumId,
                    EventSource = this.GetHashCode()
                });
            }
            this.UpdateAlbums();
        }

        private AlbumHeader FindByAlbumId(string aid)
        {
            foreach (Collection<AlbumHeader> collection in (Collection<Group<AlbumHeader>>)this.AlbumsVM.Collection)
            {
                foreach (AlbumHeader albumHeader in collection)
                {
                    if (albumHeader.AlbumId == aid)
                        return albumHeader;
                }
            }
            return (AlbumHeader)null;
        }

        private void ApplyAlbumAction(string aid, Action<AlbumHeader> action)
        {
            AlbumHeader albumHeader = this.EditAlbumsVM.Collection.FirstOrDefault<AlbumHeader>((Func<AlbumHeader, bool>)(ah => ah.AlbumId == aid));
            if (albumHeader != null)
                action(albumHeader);
            AlbumHeader byAlbumId = this.FindByAlbumId(aid);
            if (byAlbumId != null)
            {
                if (albumHeader == null || albumHeader != byAlbumId)
                    action(byAlbumId);
                byAlbumId.ReadDataFromAlbumField();
            }
            else
                this.AlbumsVM.LoadData(true, false, (Action<BackendResult<AlbumsData, ResultCode>>)null, false);
        }

        public void Handle(PhotoSetAsAlbumCover message)
        {
            this.ApplyAlbumAction(message.aid, (Action<AlbumHeader>)(a =>
            {
                AlbumHeader albumHeader1 = a;
                Photo photo1 = message.Photo;
                string str1 = photo1 != null ? photo1.src_big : null;
                albumHeader1.ImageUri = str1;
                AlbumHeader albumHeader2 = a;
                Photo photo2 = message.Photo;
                string str2 = photo2 != null ? photo2.src : null;
                albumHeader2.ImageUriSmall = str2;
            }));
        }

        public void Handle(PhotoUploadedToAlbum message)
        {
            this.ApplyAlbumAction(message.aid, (Action<AlbumHeader>)(a =>
            {
                ++a.Album.size;
                a.ReadDataFromAlbumField();
            }));
        }

        public void Handle(PhotoDeletedFromAlbum message)
        {
            this.ApplyAlbumAction(message.aid, (Action<AlbumHeader>)(a =>
            {
                --a.Album.size;
                a.ReadDataFromAlbumField();
            }));
        }

        public void Handle(PhotosMovedToAlbum message)
        {
            int count = message.photos.Count;
            this.ApplyAlbumAction(message.fromAlbumId, (Action<AlbumHeader>)(a =>
            {
                a.Album.size -= count;
                a.ReadDataFromAlbumField();
            }));
            this.ApplyAlbumAction(message.toAlbumId, (Action<AlbumHeader>)(a =>
            {
                a.Album.size += count;
                a.ReadDataFromAlbumField();
            }));
        }

        public void Handle(PhotoAlbumCreated message)
        {
            if (message.EventSource == this.GetHashCode())
                return;
            this._inAlbumCreatedHandler = true;
            this.AddOrUpdateAlbum(message.Album);
            this._inAlbumCreatedHandler = false;
        }

        public void Handle(PhotoAlbumDeleted message)
        {
            if (message.EventSource == this.GetHashCode())
                return;
            AlbumHeader albumHeader = this.EditAlbumsVM.Collection.FirstOrDefault<AlbumHeader>((Func<AlbumHeader, bool>)(a => a.AlbumId == message.aid));
            if (albumHeader == null)
                return;
            this.EditAlbumsVM.Delete(albumHeader);
            this.UpdateAlbums();
        }

        public void GetData(GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> caller, int offset, int count, Action<BackendResult<AlbumsData, ResultCode>> callback)
        {
            if (this.AlbumsVM.Collection.Count > 0)
            {
                Group<AlbumHeader> group = this.AlbumsVM.Collection.First<Group<AlbumHeader>>();
                if (offset != 0)
                    offset -= group.Count;
            }
            PhotosService.Current.GetUsersAlbums(this.UserOrGroupId, this.IsGroup, offset, count, (Action<BackendResult<AlbumsData, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    this._albumsData = res.ResultData;
                    this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Title));
                    this.PhotoFeedMoveNotificationVisibility = (!AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown).ToVisiblity();
                    this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.PhotoFeedMoveNotificationVisibility));
                }
                callback(res);
            }), true);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> caller, int count)
        {
            return PhotosMainViewModel.GetAlbumsTextForCount(count);
        }

        public void GetData(GenericCollectionViewModel<AlbumsData, AlbumHeader> caller, int offset, int count, Action<BackendResult<AlbumsData, ResultCode>> callback)
        {
            PhotosService.Current.GetUsersAlbums(this.UserOrGroupId, this.IsGroup, offset, count, (Action<BackendResult<AlbumsData, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    this._albumsData = res.ResultData;
                    this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Title));
                }
                callback(res);
            }), false);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<AlbumsData, AlbumHeader> caller, int count)
        {
            return PhotosMainViewModel.GetAlbumsTextForCount(count);
        }

        public static string GetAlbumsTextForCount(int count)
        {
            if (count > 0)
                return "";
            return PhotoResources.NoAlbums;
        }

        private static string FormatAlbumsCount(int count)
        {
            return UIStringFormatterHelper.FormatNumberOfSomething(count, PhotoResources.OneAlbumFrm, PhotoResources.TwoFourAlbumsFrm, PhotoResources.FiveAlbumsFrm, true, null, false);
        }

        private void ConvertToAlbumHeaders(AlbumsData data, out List<AlbumHeader> nonEditAlbums, out List<AlbumHeader> editAlbums)
        {
            nonEditAlbums = new List<AlbumHeader>();
            editAlbums = new List<AlbumHeader>();
            if (data.allPhotos.NotNullAndHasAtLeastOneNonNullElement())
            {
                List<AlbumHeader> albumHeaderList = nonEditAlbums;
                AlbumHeader albumHeader = new AlbumHeader();
                albumHeader.AlbumName = PhotoResources.PhotosMainPage_AllPhotos;
                string srcBig = data.allPhotos[0].src_big;
                albumHeader.ImageUri = srcBig;
                string src = data.allPhotos[0].src;
                albumHeader.ImageUriSmall = src;
                string str1 = data.allPhotos.Count >= 2 ? data.allPhotos[1].src_big : "";
                albumHeader.ImageUri2 = str1;
                string str2 = data.allPhotos.Count >= 3 ? data.allPhotos[2].src_big : "";
                albumHeader.ImageUri3 = str2;
                int allPhotosCount = data.allPhotosCount;
                albumHeader.PhotosCount = allPhotosCount;
                int num = 0;
                albumHeader.AlbumType = (AlbumType)num;
                albumHeaderList.Add(albumHeader);
            }
            if (data.profilePhotos.NotNullAndHasAtLeastOneNonNullElement())
            {
                List<AlbumHeader> albumHeaderList = nonEditAlbums;
                AlbumHeader albumHeader = new AlbumHeader();
                albumHeader.AlbumName = PhotoResources.PhotosMainPage_ProfilePhotos;
                string srcBig = data.profilePhotos[0].src_big;
                albumHeader.ImageUri = srcBig;
                string src = data.profilePhotos[0].src;
                albumHeader.ImageUriSmall = src;
                string str1 = data.profilePhotos.Count >= 2 ? data.profilePhotos[1].src_big : "";
                albumHeader.ImageUri2 = str1;
                string str2 = data.profilePhotos.Count >= 3 ? data.profilePhotos[2].src_big : "";
                albumHeader.ImageUri3 = str2;
                int profilePhotosCount = data.profilePhotosCount;
                albumHeader.PhotosCount = profilePhotosCount;
                int num = 1;
                albumHeader.AlbumType = (AlbumType)num;
                albumHeaderList.Add(albumHeader);
            }
            if (data.userPhotos.NotNullAndHasAtLeastOneNonNullElement())
                nonEditAlbums.Add(new AlbumHeader()
                {
                    AlbumName = string.Format(PhotoResources.PhotosMainPage_PhotosWithFormat, (object)data.userIns.first_name),
                    ImageUri = data.userPhotos[0].src_big,
                    ImageUriSmall = data.userPhotos[0].src,
                    ImageUri2 = data.userPhotos.Count >= 2 ? data.userPhotos[1].src_big : "",
                    ImageUri3 = data.userPhotos.Count >= 3 ? data.userPhotos[2].src_big : "",
                    PhotosCount = data.userPhotosCount,
                    AlbumType = AlbumType.PhotosWithUser
                });
            if (data.wallPhotos.NotNullAndHasAtLeastOneNonNullElement())
            {
                List<AlbumHeader> albumHeaderList = nonEditAlbums;
                AlbumHeader albumHeader = new AlbumHeader();
                albumHeader.AlbumName = PhotoResources.PhotosMainPage_WallPhotos;
                string srcBig = data.wallPhotos[0].src_big;
                albumHeader.ImageUri = srcBig;
                string src = data.wallPhotos[0].src;
                albumHeader.ImageUriSmall = src;
                string str1 = data.wallPhotos.Count >= 2 ? data.wallPhotos[1].src_big : "";
                albumHeader.ImageUri2 = str1;
                string str2 = data.wallPhotos.Count >= 3 ? data.wallPhotos[2].src_big : "";
                albumHeader.ImageUri3 = str2;
                int wallPhotosCount = data.wallPhotosCount;
                albumHeader.PhotosCount = wallPhotosCount;
                int num = 3;
                albumHeader.AlbumType = (AlbumType)num;
                albumHeaderList.Add(albumHeader);
            }
            if (data.savedPhotos.NotNullAndHasAtLeastOneNonNullElement())
            {
                List<AlbumHeader> albumHeaderList = nonEditAlbums;
                AlbumHeader albumHeader = new AlbumHeader();
                albumHeader.AlbumName = PhotoResources.PhotosMainPage_SavedPhotos;
                string srcBig = data.savedPhotos[0].src_big;
                albumHeader.ImageUri = srcBig;
                string src = data.savedPhotos[0].src;
                albumHeader.ImageUriSmall = src;
                string str1 = data.savedPhotos.Count >= 2 ? data.savedPhotos[1].src_big : "";
                albumHeader.ImageUri2 = str1;
                string str2 = data.savedPhotos.Count >= 3 ? data.savedPhotos[2].src_big : "";
                albumHeader.ImageUri3 = str2;
                int savedPhotosCount = data.savedPhotosCount;
                albumHeader.PhotosCount = savedPhotosCount;
                int num = 4;
                albumHeader.AlbumType = (AlbumType)num;
                albumHeaderList.Add(albumHeader);
            }
            foreach (Album album in data.albums)
            {
                if (album.aid != this._excludeAlbumId)
                    editAlbums.Add(new AlbumHeader()
                    {
                        AlbumName = album.title,
                        PhotosCount = album.size,
                        ImageUri = album.thumb_src,
                        ImageUriSmall = album.thumb_src_small,
                        AlbumId = album.aid,
                        AlbumType = AlbumType.NormalAlbum,
                        Album = album
                    });
            }
        }
    }
}
