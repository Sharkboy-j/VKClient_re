using Microsoft.Phone.BackgroundAudio;
using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Audio.Library
{
    public class AudioHeader : ViewModelBase, IHaveUniqueKey, ISearchableItemHeader<AudioObj>, IHandle<AudioPlayerStateChanged>, IHandle
    {
        private DateTime _lastTimeAssignedTrack = DateTime.MinValue;
        private AudioObj _track;

        public AudioObj Track
        {
            get
            {
                return this._track;
            }
        }

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

        public Visibility IsCachedVisibility
        {
            get
            {
                return AudioCacheManager.Instance.GetLocalFileForUniqueId(this.Track.UniqueId) != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SolidColorBrush TitleBrush
        {
            get
            {
                if (this.IsCurrentTrack)
                    return (SolidColorBrush)Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"];
                return (SolidColorBrush)Application.Current.Resources["PhoneContrastTitleBrush"];
            }
        }

        public SolidColorBrush SubtitleBrush
        {
            get
            {
                if (this.IsCurrentTrack)
                    return (SolidColorBrush)Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"];
                return (SolidColorBrush)Application.Current.Resources["PhoneVKSubtleBrush"];
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
                catch
                {
                    return false;
                }
            }
        }

        public Visibility IsCurrentTrackVisibility
        {
            get
            {
                return !this.IsCurrentTrack ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsLocalItem
        {
            get
            {
                return this._track.owner_id == AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public AudioHeader(AudioObj track)
        {
            this._track = track;
            EventAggregator.Current.Subscribe((object)this);
        }

        public void NotifyChanged()
        {
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCurrentTrack));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsCurrentTrackVisibility));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.TitleBrush));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.SubtitleBrush));
        }

        public void AssignTrack()
        {
            if ((DateTime.Now - this._lastTimeAssignedTrack).TotalMilliseconds <= 5000.0)
                return;
            BGAudioPlayerWrapper.Instance.Track = AudioTrackHelper.CreateTrack(this.Track);
            this._lastTimeAssignedTrack = DateTime.Now;
        }

        public string GetKey()
        {
            if (this._track == null)
                return "";
            return this._track.owner_id.ToString() + "_" + (object)this._track.id;
        }

        public bool Matches(string searchString)
        {
            return this._track.title.ToLowerInvariant().Contains(searchString.ToLowerInvariant());
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            this.NotifyChanged();
        }
    }
}
