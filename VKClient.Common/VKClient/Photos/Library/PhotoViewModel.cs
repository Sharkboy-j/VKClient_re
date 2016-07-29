using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Photos.Library
{
  public class PhotoViewModel : ViewModelBase, ISupportCommentsAndLikes, ILikeable
  {
    private int _knownCommentsCount = -1;
    public readonly int CountToLoad = 5;
    private string _accessKey = "";
    private Photo _photo;
    private PhotoWithFullInfo _photoWithFullInfo;
    private bool _isLoading;
    private long _ownerId;
    private long _pid;
    private bool _isGifAdded;
    private bool _isSavingInSavedPhotos;
    private Doc _doc;
    private bool _adding;

    public int KnownCommentsCount
    {
      get
      {
        return this._knownCommentsCount;
      }
    }

    public SolidColorBrush LikeBackgroundBrush
    {
      get
      {
        if (!this.UserLiked)
          return Application.Current.Resources["PhoneGreyIconBrush"] as SolidColorBrush;
        return Application.Current.Resources["PhoneActiveIconBrush"] as SolidColorBrush;
      }
    }

    public SolidColorBrush LikeTextForegroundBrush
    {
      get
      {
        if (!this.UserLiked)
          return new SolidColorBrush(Colors.White);
        return Application.Current.Resources["PhoneNewsActionLikedForegroundBrush"] as SolidColorBrush;
      }
    }

    public int RealOffset
    {
      get
      {
        return this._photo.real_offset;
      }
    }

    public double LikeOpacity
    {
      get
      {
        return !this.UserLiked ? 0.6 : 1.0;
      }
    }

    public string LikesCountStr
    {
      get
      {
        if (this._photo == null || this._photo.likes == null || this._photo.likes.count <= 0)
          return "";
        return this._photo.likes.count.ToString();
      }
    }

    public Visibility IsFullInfoLoadedVisibility
    {
      get
      {
        return !this.IsLoadedFullInfo ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public double IsFullInfoLoadedOpacity
    {
      get
      {
        return 1.0;
      }
    }

    public Visibility CommentVisibility
    {
      get
      {
        return Visibility.Visible;
      }
    }

    public string CommentsCountStr
    {
      get
      {
        if (this._photo == null || this._photo.comments == null || this._photo.comments.count <= 0)
          return "";
        return this._photo.comments.count.ToString();
      }
    }

    public Visibility UserVisibility
    {
      get
      {
        return this.PhotoTags.Count <= 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string UserCountStr
    {
      get
      {
        return this.PhotoTags.Count.ToString();
      }
    }

    public Photo Photo
    {
      get
      {
        return this._photo;
      }
      set
      {
        this._photo = value;
      }
    }

    public bool IsLoadedFullInfo
    {
      get
      {
        return this._knownCommentsCount != -1;
      }
    }

    public string ImageSrc
    {
      get
      {
        if (this._photo == null)
          return null;
        if (!string.IsNullOrEmpty(this._photo.src_xbig))
          return this._photo.src_xbig;
        return this._photo.src_big;
      }
    }

    public string Text
    {
      get
      {
        if (this._photo == null || this._photo.text == null)
          return "";
        return Extensions.ForUI(this._photo.text);
      }
    }

    public PhotoWithFullInfo PhotoWithInfo
    {
      get
      {
        return this._photoWithFullInfo;
      }
    }

    public List<PhotoVideoTag> PhotoTags
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<PhotoVideoTag>();
        return this._photoWithFullInfo.PhotoTags;
      }
    }

    public string OwnerImageUri
    {
      get
      {
        string str = "";
        if (this._photoWithFullInfo != null)
        {
          if (this.AuthorId < 0L)
          {
            Group group = this._photoWithFullInfo.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this.AuthorId));
            if (group != null)
              str = group.photo_200;
          }
          else
          {
            User user = this._photoWithFullInfo.Users.FirstOrDefault<User>((Func<User, bool>) (u => u.id == this.AuthorId));
            if (user != null)
              str = user.photo_max;
          }
        }
        return str;
      }
    }

    public int TotalCommentsCount
    {
      get
      {
        return this.KnownCommentsCount;
      }
    }

    public long OwnerId
    {
      get
      {
        if (this._photo != null)
          return this._photo.owner_id;
        return 0;
      }
    }

    public long UserOwnerId
    {
      get
      {
        if (this._photo != null)
          return this._photo.user_id;
        return 0;
      }
    }

    public long AuthorId
    {
      get
      {
        if (this.UserOwnerId > 0L && this.UserOwnerId != User.ADMIN_ID)
          return this.UserOwnerId;
        return this.OwnerId;
      }
    }

    public string OwnerName
    {
      get
      {
        string str = "";
        if (this._photoWithFullInfo != null)
        {
          if (this.AuthorId < 0L)
          {
            Group group = this._photoWithFullInfo.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this.AuthorId));
            if (group != null)
              str = group.name;
          }
          else
          {
            User user = this._photoWithFullInfo.Users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == this.AuthorId));
            if (user != null)
              str = user.Name;
          }
        }
        return str;
      }
    }

    public CommentType CommentType
    {
      get
      {
        return CommentType.Photo;
      }
    }

    public bool UserLiked
    {
      get
      {
        if (this._photo == null || this._photo.likes == null)
          return false;
        return this._photo.likes.user_likes == 1;
      }
    }

    public bool CanRepost
    {
      get
      {
        return true;
      }
    }

    public string AccessKey
    {
      get
      {
        return this._accessKey;
      }
    }

    public long Pid
    {
      get
      {
        return this._pid;
      }
    }

    public bool IsGif { get; private set; }

    public Visibility CanAddVisibility
    {
      get
      {
        return !this._isGifAdded ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility AddedVisibility
    {
      get
      {
        return !this._isGifAdded ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Doc Document
    {
      get
      {
        return this._doc;
      }
    }

    public List<Comment> Comments
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<Comment>();
        return this._photoWithFullInfo.Comments;
      }
    }

    public List<User> Users
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<User>();
        return this._photoWithFullInfo.Users;
      }
    }

    public List<Group> Groups
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<Group>();
        return this._photoWithFullInfo.Groups;
      }
    }

    public List<User> Users2
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<User>();
        return this._photoWithFullInfo.Users2;
      }
    }

    public List<long> LikesAllIds
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return new List<long>();
        return this._photoWithFullInfo.LikesAllIds;
      }
    }

    public int LikesCount
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return 0;
        return this._photoWithFullInfo.Photo.likes.count;
      }
    }

    public int RepostsCount
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return 0;
        return this._photoWithFullInfo.RepostsCount;
      }
    }

    public long ItemId
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return 0;
        return this._photoWithFullInfo.Photo.pid;
      }
    }

    public LikeObjectType LikeObjectType
    {
      get
      {
        return LikeObjectType.photo;
      }
    }

    public bool CanComment
    {
      get
      {
        if (this._photoWithFullInfo == null)
          return false;
        return this._photoWithFullInfo.Photo.can_comment == 1;
      }
    }

    public bool CanReport
    {
      get
      {
        return this._ownerId != AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public PhotoViewModel(Photo photo, PhotoWithFullInfo photoWithFullInfo = null)
    {
      this._photo = photo;
      this._ownerId = photo.owner_id;
      this._pid = photo.pid;
      this._accessKey = photo.access_key;
      this._photoWithFullInfo = photoWithFullInfo;
      PhotoWithFullInfo photoWithFullInfo1 = this._photoWithFullInfo;
      VKClient.Common.Backend.DataObjects.Comments comments;
      if (photoWithFullInfo1 == null)
      {
        comments = (VKClient.Common.Backend.DataObjects.Comments) null;
      }
      else
      {
        Photo photo1 = photoWithFullInfo1.Photo;
        comments = photo1 != null ? photo1.comments : (VKClient.Common.Backend.DataObjects.Comments) null;
      }
      if (comments == null)
        return;
      this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;
    }

    public PhotoViewModel(long ownerId, long pid, string accessKey)
    {
      this._accessKey = accessKey;
      this._ownerId = ownerId;
      this._pid = pid;
    }

    public PhotoViewModel(Doc doc)
    {
      this._doc = doc;
      this.IsGif = true;
      this.Photo = doc.ConvertToPhotoPreview();
    }

    public void AddDocument()
    {
      if (this._doc == null || this._isGifAdded)
        return;
      this.SetInProgressMain(true, "");
      DocumentsService.Current.Add(this._doc.owner_id, this._doc.id, this._doc.access_key, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (res =>
      {
        this.SetInProgressMain(false, "");
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this._isGifAdded = true;
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.CanAddVisibility));
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.AddedVisibility));
          GenericInfoUC.ShowBasedOnResult(0, CommonResources.FileIsSavedInDocuments, (VKRequestsDispatcher.Error) null);
        }
        else if (res.ResultCode == ResultCode.WrongParameter && res.Error.error_msg.Contains("already added"))
          GenericInfoUC.ShowBasedOnResult(0, CommonResources.FileIsAlreadySavedInDocuments, (VKRequestsDispatcher.Error) null);
        else
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
      }));
    }

    public void LoadInfoWithComments(Action<bool, int> callback)
    {
      if (this.IsGif)
        callback(true, 0);
      else if (this._photoWithFullInfo != null)
      {
        Group group = this._photoWithFullInfo.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._ownerId));
        callback(true, group == null ? 0 : group.admin_level);
      }
      else
      {
        if (this._isLoading)
          return;
        this._isLoading = true;
        this.SetInProgress(true, "");
        PhotosService.Current.GetPhotoWithFullInfo(this._ownerId, this._pid, this._accessKey, -1, 0, this.CountToLoad, (Action<BackendResult<PhotoWithFullInfo, ResultCode>>) (res =>
        {
          int num1 = 0;
          if (res.ResultCode == ResultCode.Succeeded)
          {
            int num2 = string.IsNullOrEmpty(this.ImageSrc) ? 1 : 0;
            this._photoWithFullInfo = res.ResultData;
            this._photo = res.ResultData.Photo;
            if (this._photo != null && string.IsNullOrEmpty(this._photo.access_key) && !string.IsNullOrEmpty(this._accessKey))
              this._photo.access_key = this._accessKey;
            this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;
            if (res.ResultData != null)
            {
              Group group = res.ResultData.Groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._ownerId));
              if (group != null)
                num1 = group.admin_level;
            }
            if (num2 != 0)
              this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ImageSrc));
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.CommentsCountStr));
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Text));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsLoadedFullInfo));
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UserCountStr));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.UserVisibility));
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.LikesCountStr));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.UserLiked));
            this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.LikeOpacity));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.LikeBackgroundBrush));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.LikeTextForegroundBrush));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsFullInfoLoadedVisibility));
            this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.IsFullInfoLoadedOpacity));
          }
          this.SetInProgress(false, "");
          this._isLoading = false;
          callback(res.ResultCode == ResultCode.Succeeded, num1);
        }));
      }
    }

    public void LikeUnlike()
    {
      if (this._photo == null || this._photo.likes == null)
        return;
      if (this._photo.likes.user_likes == 0)
      {
        LikesService.Current.AddRemoveLike(true, this._photo.owner_id, this._photo.pid, LikeObjectType.photo, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}), this._accessKey);
        ++this._photo.likes.count;
        this._photo.likes.user_likes = 1;
      }
      else
      {
        LikesService.Current.AddRemoveLike(false, this._photo.owner_id, this._photo.pid, LikeObjectType.photo, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}), this._accessKey);
        --this._photo.likes.count;
        this._photo.likes.user_likes = 0;
      }
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.LikesCountStr));
      this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.LikeOpacity));
      this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.LikeBackgroundBrush));
      this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.LikeTextForegroundBrush));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.UserLiked));
    }

    public void LoadMoreComments(int countToLoad, Action<bool> callback)
    {
      if (!this.IsLoadedFullInfo || this._isLoading)
        return;
      this._isLoading = true;
      PhotosService.Current.GetPhotoWithFullInfo(this._ownerId, this._pid, this._accessKey, this.KnownCommentsCount, this._photoWithFullInfo.Comments.Count, countToLoad, (Action<BackendResult<PhotoWithFullInfo, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          List<Comment> comments = this._photoWithFullInfo.Comments;
          this._photoWithFullInfo.Comments = res.ResultData.Comments;
          this._photoWithFullInfo.Comments.AddRange((IEnumerable<Comment>) comments);
          this._photoWithFullInfo.Users.AddRange((IEnumerable<User>) res.ResultData.Users);
          this._photoWithFullInfo.Groups.AddRange((IEnumerable<Group>) res.ResultData.Groups);
          this._photoWithFullInfo.Users2.AddRange((IEnumerable<User>) res.ResultData.Users2);
        }
        this._isLoading = false;
        callback(res.ResultCode == ResultCode.Succeeded);
      }));
    }

    public void AddComment(Comment comment, List<string> attachmentIds, bool fromGroup, Action<bool, Comment> callback, string stickerReferrer = "")
    {
      if (this._adding)
      {
        callback(false, (Comment) null);
      }
      else
      {
        this._adding = true;
        PhotosService.Current.CreateComment(this.OwnerId, this._photo.pid, comment.reply_to_cid, comment.text, fromGroup, attachmentIds, (Action<BackendResult<Comment, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
          {
            ++this.Photo.comments.count;
            Execute.ExecuteOnUIThread((Action) (() =>
            {
              if (this.PhotoWithInfo == null)
                return;
              this.PhotoWithInfo.Comments.Add(res.ResultData);
            }));
            callback(true, res.ResultData);
          }
          else
            callback(false, (Comment) null);
          this._adding = false;
        }), this._accessKey, comment.sticker_id, stickerReferrer);
      }
    }

    public void DeleteComment(long cid)
    {
      --this.Photo.comments.count;
      PhotosService.Current.DeleteComment(this.OwnerId, this._photo.pid, cid, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if ( (this.Comments) == null)
          return;
        Comment comment =  (this.Comments).FirstOrDefault<Comment>((Func<Comment, bool>) (c => c.cid == cid));
        if (comment == null)
          return;
         (this.Comments).Remove(comment);
      }))));
    }

    public void Share(string text, long gid = 0, string groupName = "")
    {
      if (!this.IsGif)
      {
        WallService.Current.Repost(this._ownerId, this._pid, text, RepostObject.photo, gid, (Action<BackendResult<RepostResult, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Photo, gid, groupName);
          else
            new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
        }))));
      }
      else
      {
        WallService current = WallService.Current;
        WallPostRequestData postData = new WallPostRequestData();
        postData.owner_id = gid > 0L ? -gid : AppGlobalStateManager.Current.LoggedInUserId;
        postData.message = text;
        postData.AttachmentIds = new List<string>()
        {
          this._doc.UniqueIdForAttachment
        };
        Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Doc, gid, groupName);
          else
            new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
        })));
        current.Post(postData, callback);
      }
    }

    public void SaveInSavedPhotosAlbum()
    {
      if (this._isSavingInSavedPhotos)
        return;
      this._isSavingInSavedPhotos = true;
      PhotosService.Current.CopyPhotos(this._ownerId, this._pid, this._accessKey, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res =>
      {
        this._isSavingInSavedPhotos = false;
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        Execute.ExecuteOnUIThread((Action) (() => GenericInfoUC.ShowPhotoIsSavedInSavedPhotos()));
      }));
    }

    public void LikeUnlike(bool like)
    {
      this.LikeUnlike();
    }
  }
}
