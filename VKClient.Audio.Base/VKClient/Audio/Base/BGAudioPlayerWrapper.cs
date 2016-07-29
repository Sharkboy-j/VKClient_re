using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Audio.Base
{
    public class BGAudioPlayerWrapper
    {
        private static BGAudioPlayerWrapper _instance;
        public EventHandler PlayStateChanged;
        private AudioTrack _audioTrack;
        //private bool _fetchingTrackHeader;
        //private bool _inSync;

        public bool BackgroundAppMode { get; set; }

        public static BGAudioPlayerWrapper Instance
        {
            get
            {
                if (BGAudioPlayerWrapper._instance == null)
                    BGAudioPlayerWrapper._instance = new BGAudioPlayerWrapper();
                return BGAudioPlayerWrapper._instance;
            }
        }

        public PlayState PlayerState
        {
            get
            {
                return BackgroundAudioPlayer.Instance.PlayerState;
            }
        }

        public double Volume
        {
            get
            {
                return BackgroundAudioPlayer.Instance.Volume;
            }
            set
            {
                BackgroundAudioPlayer.Instance.Volume = value;
            }
        }

        public TimeSpan Position
        {
            get
            {
                //if (this._fetchingTrackHeader)
                //    return new TimeSpan();
                return BackgroundAudioPlayer.Instance.Position;
            }
            set
            {
                //if (this._fetchingTrackHeader)
                //    return;
                BackgroundAudioPlayer.Instance.Position = value;
            }
        }

        public AudioTrack Track
        {
            get
            {
                return BackgroundAudioPlayer.Instance.Track;
            }
            set
            {
                /*
                 * {System.SystemException: HRESULT = 0x8004020E}
Cannot modify or delete an object that was added using the COM+ Administrative SDK.
                 * */

                BackgroundAudioPlayer.Instance.Track = value;
                this.FirePlayChanged();
            }
        }

        public static void InitializeInstance()
        {
            BackgroundAudioPlayer.Instance.PlayStateChanged += BGAudioPlayerWrapper.Instance_PlayStateChanged;
        }

        private static void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            BGAudioPlayerWrapper.Instance._audioTrack = BackgroundAudioPlayer.Instance.Track;
            BGAudioPlayerWrapper.Instance.FirePlayChanged();
        }

        private void FirePlayChanged()
        {
            if (this.PlayStateChanged == null)
                return;
            this.PlayStateChanged(this, EventArgs.Empty);
        }

        public void Play()
        {
            BackgroundAudioPlayer.Instance.Play();
        }

        public void Pause()
        {
            BackgroundAudioPlayer.Instance.Pause();
        }

        public void Stop()
        {
            BackgroundAudioPlayer.Instance.Stop();
        }

        private void RefetchTrack(AudioTrack track, Action<AudioTrack> callback)
        {
            if (track != null && track.Source != null)
            {
                Uri currentSource = track.Source;
                AudioService instance = AudioService.Instance;
                List<string> ids = new List<string>();
                ids.Add(track.GetTagId());
                Action<BackendResult<List<AudioObj>, ResultCode>> callback1 = (Action<BackendResult<List<AudioObj>, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded && res.ResultData != null && (res.ResultData.Count > 0 && currentSource.OriginalString != res.ResultData[0].url))
                        Execute.ExecuteOnUIThread((Action)(() => callback(AudioTrackHelper.CreateTrack(res.ResultData[0]))));
                    else
                    {
                        //Action closure_0 = null;
                        Execute.ExecuteOnUIThread(/*closure_0 ??*/ (/*closure_0 =*/ (Action)(() => callback(null))));
                    }
                });
                instance.GetAudio(ids, callback1);
            }
            else
                callback(null);
        }

        public void RespondToAppDeactivation()
        {
            //this._inSync = false;
        }
    }
}
