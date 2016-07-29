using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace VKClient.Video.Library
{
  public class AddEditVideoViewModel : ViewModelBase
  {
    private bool _isInEditMode;
    private long _videoId;
    private long _ownerId;
    private string _filePath;
    private string _name;
    private string _description;
    private bool _autoReplay;
    private bool _isSaving;
    private double _progress;
    private StorageFile _sf;
    private Cancellation _c;
    //private AccessType _accessType;
    //private AccessType _accessTypeComments;
    private string _localThumbPath;
    public static StorageFile PickedExternalFile;
    private EditPrivacyViewModel _viewVideoPricacyVM;
    private EditPrivacyViewModel _commentVideoPrivacyVM;

    public Cancellation C
    {
      get
      {
        return this._c;
      }
    }

    public string LocalThumbPath
    {
      get
      {
        return this._localThumbPath;
      }
    }

    public EditPrivacyViewModel ViewVideoPrivacyVM
    {
      get
      {
        return this._viewVideoPricacyVM;
      }
      set
      {
        this._viewVideoPricacyVM = value;
        this.NotifyPropertyChanged<EditPrivacyViewModel>((System.Linq.Expressions.Expression<Func<EditPrivacyViewModel>>) (() => this.ViewVideoPrivacyVM));
      }
    }

    public EditPrivacyViewModel CommentVideoPrivacyVM
    {
      get
      {
        return this._commentVideoPrivacyVM;
      }
      set
      {
        this._commentVideoPrivacyVM = value;
        this.NotifyPropertyChanged<EditPrivacyViewModel>((System.Linq.Expressions.Expression<Func<EditPrivacyViewModel>>) (() => this.CommentVideoPrivacyVM));
      }
    }

    public bool IsSaving
    {
      get
      {
        return this._isSaving;
      }
      private set
      {
        this._isSaving = value;
        this.SetInProgress(value, "");
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSaving));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanEdit));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsUploadingVisibility));
      }
    }

    public bool CanEdit
    {
      get
      {
        return !this.IsSaving;
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return !this._isSaving || this._isInEditMode ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public double Progress
    {
      get
      {
        return this._progress;
      }
      private set
      {
        this._progress = value;
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.Progress));
      }
    }

    public Visibility IsUserVideo
    {
      get
      {
        return this._ownerId <= 0L ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string Title
    {
      get
      {
        if (!this._isInEditMode)
          return CommonResources.AddEditVideo_Add;
        return CommonResources.AddEditVideo_Edit;
      }
    }

    public string Name
    {
      get
      {
        return this._name;
      }
      set
      {
        this._name = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Name));
      }
    }

    public string Description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Description));
      }
    }

    public bool AutoReplay
    {
      get
      {
        return this._autoReplay;
      }
      set
      {
        this._autoReplay = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.AutoReplay));
      }
    }

    private AddEditVideoViewModel()
    {
    }

    public static AddEditVideoViewModel CreateForNewVideo(string filePath, long ownerId)
    {
      AddEditVideoViewModel editVideoViewModel = new AddEditVideoViewModel();
      editVideoViewModel._ownerId = ownerId;
      editVideoViewModel._filePath = filePath;
      EditPrivacyViewModel privacyViewModel1 = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanView, new PrivacyInfo(), "", (List<string>) null);
      editVideoViewModel.ViewVideoPrivacyVM = privacyViewModel1;
      EditPrivacyViewModel privacyViewModel2 = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanComment, new PrivacyInfo(), "", (List<string>) null);
      editVideoViewModel.CommentVideoPrivacyVM = privacyViewModel2;
      editVideoViewModel.PrepareVideo();
      return editVideoViewModel;
    }

    public static AddEditVideoViewModel CreateForEditVideo(long ownerId, long videoId, VKClient.Common.Backend.DataObjects.Video video = null)
    {
      AddEditVideoViewModel vm = new AddEditVideoViewModel();
      vm._ownerId = ownerId;
      vm._videoId = videoId;
      vm._isInEditMode = true;
      if (video != null)
        vm.InitializeWithVideo(video);
      else
        VideoService.Instance.GetVideoById(ownerId, videoId, "", (Action<BackendResult<List<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          VKClient.Common.Backend.DataObjects.Video vid = res.ResultData.First<VKClient.Common.Backend.DataObjects.Video>();
          Execute.ExecuteOnUIThread((Action) (() => vm.InitializeWithVideo(vid)));
        }));
      return vm;
    }

    private void InitializeWithVideo(VKClient.Common.Backend.DataObjects.Video video)
    {
      this.Name = video.title;
      this.Description = video.description;
      this.ViewVideoPrivacyVM = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanView, video.PrivacyViewInfo, "", (List<string>) null);
      this.CommentVideoPrivacyVM = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanComment, video.PrivacyCommentInfo, "", (List<string>) null);
      this._localThumbPath = video.photo_320;
    }

    private async void PrepareVideo()
    {
      try
      {
        if (this._filePath != "")
        {
          AddEditVideoViewModel editVideoViewModel = this;
          StorageFile storageFile = editVideoViewModel._sf;
          StorageFile fileFromPathAsync = await StorageFile.GetFileFromPathAsync(this._filePath);
          editVideoViewModel._sf = fileFromPathAsync;
          editVideoViewModel = (AddEditVideoViewModel) null;
        }
        else
          this._sf = AddEditVideoViewModel.PickedExternalFile;
        VideoProperties videoPropertiesAsync = await this._sf.Properties.GetVideoPropertiesAsync();
        StorageItemThumbnail thumbnailAsync = await this._sf.GetThumbnailAsync((ThumbnailMode) 1);
        this._localThumbPath = "/" + Guid.NewGuid().ToString();
        ImageCache.Current.TrySetImageForUri(this._localThumbPath, ((IRandomAccessStream) thumbnailAsync).AsStream());
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.LocalThumbPath));
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Failed to prepare video data", ex);
      }
    }

    public async void Save(Action<bool> resultCallback)
    {
      if (this._isSaving)
        return;
      this.IsSaving = true;
      if (!this._isInEditMode)
      {
        try
        {
          if (this._sf == null)
          {
            if (this._filePath != "")
            {
              AddEditVideoViewModel editVideoViewModel = this;
              StorageFile storageFile = editVideoViewModel._sf;
              StorageFile fileFromPathAsync = await StorageFile.GetFileFromPathAsync(this._filePath);
              editVideoViewModel._sf = fileFromPathAsync;
              editVideoViewModel = (AddEditVideoViewModel) null;
            }
            else
            {
              this._sf = AddEditVideoViewModel.PickedExternalFile;
              AddEditVideoViewModel.PickedExternalFile = (StorageFile) null;
            }
          }
          Stream stream = ((IInputStream) await this._sf.OpenAsync((FileAccessMode) 0)).AsStreamForRead();
          this._c = new Cancellation();
          VideoService.Instance.UploadVideo(stream, false, 0L, this._ownerId < 0L ? -this._ownerId : 0L, this.Name, this.Description, (Action<BackendResult<SaveVideoResponse, ResultCode>>) (res =>
          {
            this.IsSaving = false;
            if (res.ResultCode == ResultCode.Succeeded)
            {
              EventAggregator current = EventAggregator.Current;
              VideoAddedDeleted videoAddedDeleted = new VideoAddedDeleted();
              videoAddedDeleted.IsAdded = true;
              long videoId = res.ResultData.video_id;
              videoAddedDeleted.VideoId = videoId;
              long ownerId = res.ResultData.owner_id;
              videoAddedDeleted.OwnerId = ownerId;
              current.Publish((object) videoAddedDeleted);
              resultCallback(true);
            }
            else
            {
              this.Progress = 0.0;
              resultCallback(false);
            }
          }), (Action<double>) (progress => this.Progress = progress), this._c, this.ViewVideoPrivacyVM.GetAsPrivacyInfo(), this.CommentVideoPrivacyVM.GetAsPrivacyInfo());
        }
        catch
        {
          this.IsSaving = false;
          resultCallback(false);
        }
      }
      else
        VideoService.Instance.EditVideo(this._videoId, this._ownerId < 0L ? -this._ownerId : 0L, this.Name, this.Description, this.ViewVideoPrivacyVM.GetAsPrivacyInfo(), this.CommentVideoPrivacyVM.GetAsPrivacyInfo(), (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
        {
          this.IsSaving = false;
          if (res.ResultCode == ResultCode.Succeeded)
          {
            this.FireEditedEvent();
            resultCallback(true);
          }
          else
            resultCallback(false);
        }));
    }

    private void FireEditedEvent()
    {
      VKClient.Common.Backend.DataObjects.Video basedOnCurrentState = this.CreateVideoBasedOnCurrentState();
      EventAggregator.Current.Publish((object) new VideoEdited()
      {
        Video = basedOnCurrentState
      });
    }

    private VKClient.Common.Backend.DataObjects.Video CreateVideoBasedOnCurrentState()
    {
      return new VKClient.Common.Backend.DataObjects.Video()
      {
        id = this._videoId,
        owner_id = this._ownerId,
        title = this.Name,
        description = this.Description,
        privacy_view = this.ViewVideoPrivacyVM.GetAsPrivacyInfo().ToStringList(),
        privacy_comment = this.CommentVideoPrivacyVM.GetAsPrivacyInfo().ToStringList()
      };
    }

    public void Cancel()
    {
      if (this._c == null)
        return;
      this._c.Set();
    }
  }
}
