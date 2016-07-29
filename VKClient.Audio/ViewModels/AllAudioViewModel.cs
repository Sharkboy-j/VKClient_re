using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Audio.ViewModels
{
  public class AllAudioViewModel : ViewModelBase, ICollectionDataProvider<List<AudioObj>, AudioHeader>, IHandle<AudioTrackAddedRemoved>, IHandle, ISupportReorder<AudioHeader>
  {
    protected long _userOrGroupId;
    protected bool _isGroup;
    private bool _isPickMode;
    private long _albumId;
    private GenericCollectionViewModel<List<AudioObj>, AudioHeader> _allTracks;
    private AllAlbumsViewModel _allAlbumsVM;

    public long UserOrGroupId
    {
      get
      {
        return this._userOrGroupId;
      }
    }

    public bool IsGroup
    {
      get
      {
        return this._isGroup;
      }
    }

    public GenericCollectionViewModel<List<AudioObj>, AudioHeader> AllTracks
    {
      get
      {
        return this._allTracks;
      }
      set
      {
        this._allTracks = value;
        this.NotifyPropertyChanged<GenericCollectionViewModel<List<AudioObj>, AudioHeader>>((Expression<Func<GenericCollectionViewModel<List<AudioObj>, AudioHeader>>>) (() => this.AllTracks));
      }
    }

    public AllAlbumsViewModel AllAlbumsVM
    {
      get
      {
        return this._allAlbumsVM;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Audio.ToUpperInvariant();
      }
    }

    public Func<List<AudioObj>, ListWithCount<AudioHeader>> ConverterFunc
    {
      get
      {
        return (Func<List<AudioObj>, ListWithCount<AudioHeader>>) (input => new ListWithCount<AudioHeader>()
        {
          List = new List<AudioHeader>(input.Select<AudioObj, AudioHeader>((Func<AudioObj, AudioHeader>) (i => new AudioHeader(i))))
        });
      }
    }

    public Action<bool, GenericCollectionViewModel<List<AudioObj>, AudioHeader>> ReportBusyCallback
    {
      get
      {
        return (Action<bool, GenericCollectionViewModel<List<AudioObj>, AudioHeader>>) ((b, c) => {});
      }
    }

    public AllAudioViewModel(long userOrGroupId, bool isGroup, bool isPickMode, long albumId = 0, long exludeAlbumId = 0)
    {
      this._isGroup = isGroup;
      this._userOrGroupId = userOrGroupId;
      this._isPickMode = isPickMode;
      this._albumId = albumId;
      this.AllTracks = new GenericCollectionViewModel<List<AudioObj>, AudioHeader>((ICollectionDataProvider<List<AudioObj>, AudioHeader>) this)
      {
        NoContentText = CommonResources.NoContent_Audios,
        NoContentImage = "../Resources/NoContentImages/Audios.png"
      };
      this.AllTracks.LoadCount = 80;
      this._allAlbumsVM = new AllAlbumsViewModel(userOrGroupId, isGroup, isPickMode, exludeAlbumId);
      EventAggregator.Current.Subscribe((object) this);
    }

    public void LoadMore(object linkedItem)
    {
      this.AllTracks.LoadMoreIfNeeded(linkedItem);
    }

    public void GetData(GenericCollectionViewModel<List<AudioObj>, AudioHeader> caller, int offset, int count, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
    {
      if (this._albumId == 0L)
        AudioService.Instance.GetAllAudio((Action<BackendResult<List<AudioObj>, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded && AudioCacheManager.Instance.CachedListForCurrentUser.Count > 0 && (!this._isGroup && this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId) || this._userOrGroupId == 0L)
            callback(new BackendResult<List<AudioObj>, ResultCode>(ResultCode.Succeeded, AudioCacheManager.Instance.CachedListForCurrentUser));
          else
            callback(res);
        }), new long?(this.UserOrGroupId), this.IsGroup, new long?(), offset, count);
      else if (this._albumId == AllAlbumsViewModel.RECOMMENDED_ALBUM_ID)
        AudioService.Instance.GetRecommended(this.UserOrGroupId, (long) offset, (long) count, callback);
      else if (this._albumId == AllAlbumsViewModel.POPULAR_ALBUM_ID)
        AudioService.Instance.GetPopular(offset, count, callback);
      else if (this._albumId == AllAlbumsViewModel.SAVED_ALBUM_ID)
      {
        callback(new BackendResult<List<AudioObj>, ResultCode>(ResultCode.Succeeded, AudioCacheManager.Instance.CachedListForCurrentUser));
      }
      else
      {
        AudioService instance = AudioService.Instance;
        Action<BackendResult<List<AudioObj>, ResultCode>> callback1 = callback;
        long? userOrGroupId = new long?(this.UserOrGroupId);
        int num1 = this.IsGroup ? 1 : 0;
        int num2 = offset;
        int num3 = count;
        long? albumId = new long?(this._albumId);
        int offset1 = num2;
        int count1 = num3;
        instance.GetAllAudio(callback1, userOrGroupId, num1 != 0, albumId, offset1, count1);
      }
    }

    public string GetFooterTextForCount(int count)
    {
      if (count <= 0)
        return AudioResources.NoTracks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneTrackFrm, AudioResources.TwoFourTracksFrm, AudioResources.FiveTracksFrm, true, null, false);
    }

    public void Handle(AudioTrackAddedRemoved message)
    {
      if (this.IsGroup || this.UserOrGroupId != AppGlobalStateManager.Current.LoggedInUserId)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (message.Added && this._albumId == 0L)
        {
          this.AllTracks.Insert(new AudioHeader(message.Audio), 0);
        }
        else
        {
          AudioHeader audioHeader = this.AllTracks.Collection.FirstOrDefault<AudioHeader>((Func<AudioHeader, bool>) (ah => ah.Track.aid == message.Audio.aid));
          if (audioHeader == null)
            return;
          this.AllTracks.Delete(audioHeader);
        }
      }));
    }

    internal void DeleteAudios(List<AudioHeader> list)
    {
      AudioService.Instance.DeleteAudios(list.Select<AudioHeader, long>((Func<AudioHeader, long>) (a => a.Track.aid)).ToList<long>());
      foreach (AudioHeader audioHeader in list)
      {
        EventAggregator current = EventAggregator.Current;
        AudioTrackAddedRemoved trackAddedRemoved = new AudioTrackAddedRemoved();
        trackAddedRemoved.Added = false;
        AudioObj track = audioHeader.Track;
        trackAddedRemoved.Audio = track;
        current.Publish((object) trackAddedRemoved);
      }
    }

    internal void AddTracksToAlbum(List<AudioHeader> headersToMove)
    {
      throw new NotImplementedException();
    }

    internal void MoveTracksToAlbum(List<AudioHeader> headersToMove, AudioAlbum pickedAlbum, Action<bool> callback)
    {
      AudioService.Instance.MoveToAlbum(headersToMove.Select<AudioHeader, long>((Func<AudioHeader, long>) (h => h.Track.aid)).ToList<long>(), pickedAlbum.album_id, (Action<BackendResult<object, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          callback(true);
        else
          callback(false);
      }));
    }

    public void Reordered(AudioHeader item, AudioHeader before, AudioHeader after)
    {
      if (item == null)
        return;
      AudioService.Instance.ReorderAudio(item.Track.aid, AppGlobalStateManager.Current.LoggedInUserId, this._albumId, after == null ? 0L : after.Track.aid, before == null ? 0L : before.Track.aid, (Action<BackendResult<long, ResultCode>>) (res => {}));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<AudioObj>, AudioHeader> caller, int count)
    {
      return this.GetFooterTextForCount(count);
    }
  }
}
