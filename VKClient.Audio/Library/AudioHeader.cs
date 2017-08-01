using Microsoft.Phone.BackgroundAudio;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Audio.Library
{
  public class AudioHeader : ViewModelBase, IHaveUniqueKey, ISearchableItemHeader<AudioObj>, IHandle<AudioPlayerStateChanged>, IHandle, IHandle<AudioTrackEdited>
  {
    private DateTime _lastTimeAssignedTrack = DateTime.MinValue;

    public AudioObj Track { get; private set; }

    public long MessageId { get; private set; }

    public bool IsMenuEnabled
    {
      get
      {
        return AppGlobalStateManager.Current.LoggedInUserId == this.Track.owner_id;
      }
    }

    public string UIDuration
    {
      get
      {
        int result = 0;
        if (int.TryParse(this.Track.duration, out result))
          return UIStringFormatterHelper.FormatDuration(result);
        return "";
      }
    }

    public Visibility IsCachedVisibility//mod
    {
        get
        {
            return VKClient.Audio.Base.AudioCache.AudioCacheManager.Instance.GetLocalFileForUniqueId(this.Track.UniqueId) != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public SolidColorBrush TitleBrush
    {
      get
      {
        if (this.IsCurrentTrack)
          return (SolidColorBrush) Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"];
        return (SolidColorBrush) Application.Current.Resources["PhoneContrastTitleBrush"];
      }
    }

    public SolidColorBrush SubtitleBrush
    {
      get
      {
        if (this.IsCurrentTrack)
          return (SolidColorBrush) Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"];
        return (SolidColorBrush) Application.Current.Resources["PhoneVKSubtleBrush"];
      }
    }

    public bool IsCurrentTrack
    {
      get
      {
        try
        {
          AudioTrack track = BGAudioPlayerWrapper.Instance.Track;
          return track != null && track.GetTagId() == this.Track.UniqueId;
        }
        catch (Exception )
        {
          return false;
        }
      }
    }

    public Visibility IsCurrentTrackVisibility
    {
      get
      {
        return this.IsCurrentTrack.ToVisiblity();
      }
    }

    public bool IsContentRestricted
    {
      get
      {
        return this.ContentRestricted > 0;
      }
    }

    private int ContentRestricted
    {
      get
      {
        return this.Track.content_restricted;
      }
    }

    public double TrackOpacity
    {
      get
      {
        return this.IsContentRestricted ? 0.4 : 1.0;
      }
    }

    public bool IsLocalItem
    {
      get
      {
        return this.Track.owner_id == AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public string Artist
    {
      get
      {
        return Extensions.ForUI(this.Track.artist);
      }
    }

    public string Title
    {
      get
      {
        return Extensions.ForUI(this.Track.title);
      }
    }

    public AudioHeader(AudioObj track, long messageId = 0)
    {
      this.Track = track;
      this.MessageId = messageId;
      EventAggregator.Current.Subscribe(this);
    }

    public void NotifyChanged()
    {
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.IsCurrentTrack);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.IsCurrentTrackVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<double>(() => this.TrackOpacity);
      // ISSUE: method reference
      this.NotifyPropertyChanged<SolidColorBrush>(() => this.TitleBrush);
      // ISSUE: method reference
      this.NotifyPropertyChanged<SolidColorBrush>(() => this.SubtitleBrush);
    }

    public void ShowContentRestrictedMessage()
    {
      AudioHelper.ShowContentRestrictedMessage(this.ContentRestricted);
    }

    public bool TryAssignTrack()
    {
      if (this.ContentRestricted > 0)
        return false;
      if ((DateTime.Now- this._lastTimeAssignedTrack).TotalMilliseconds > 5000.0)
      {
        BGAudioPlayerWrapper.Instance.Track = AudioTrackHelper.CreateTrack(this.Track);
        this._lastTimeAssignedTrack = DateTime.Now;
      }
      return true;
    }

    public string GetKey()
    {
      AudioObj track = this.Track;
      if (track == null)
        return "";
      return track.owner_id.ToString() + "_" + track.id;
    }

    public bool Matches(string searchString)
    {
      return this.Track.title.ToLowerInvariant().Contains(searchString.ToLowerInvariant());
    }

    public void Handle(AudioPlayerStateChanged message)
    {
      this.NotifyChanged();
    }

    public void Handle(AudioTrackEdited message)
    {
      if (message.OwnerId != this.Track.owner_id || message.Id != this.Track.id)
        return;
      this.Track.artist = message.Artist;
      this.Track.title = message.Title;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Artist);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Title);
    }
  }
}
