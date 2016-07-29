using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Library;
using VKClient.Audio.UserControls;
using VKClient.Audio.ViewModels;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Localization;

namespace VKClient.Audio.Views
{
    public partial class AudioPlayer : PageBase
    {
        private ApplicationBarIconButton _addNewAppBarButton = new ApplicationBarIconButton()
        {
            IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
            Text = CommonResources.Audio_AppBar_AddToMyAudios
        };
        private bool _applyPositionFromVM = true;
        private ApplicationBar _appBar = new ApplicationBar()
        {
            BackgroundColor = VKConstants.AppBarBGColor,
            ForegroundColor = VKConstants.AppBarFGColor
        };
        private bool _isInitialized;
        //private bool _subscribed;

        private AudioPlayerViewModel VM
        {
            get
            {
                return this.DataContext as AudioPlayerViewModel;
            }
        }

        public AudioPlayer()
        {
            this.InitializeComponent();
            this.SetupAppBar();
            this.textBlockNowPlayingLabel.Text = CommonResources.AudioPlayer_NowPlaying.ToUpperInvariant();
            this.textBlockNextLabel.Text = CommonResources.AudioPlayer_Next.ToUpperInvariant();
            this._progressBar.Foreground = (Brush)(Application.Current.Resources["PhoneAudioPlayerForeground2Brush"] as SolidColorBrush);
            this.Loaded += new RoutedEventHandler(this.AudioPlayer_Loaded);
        }

        private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!this._subscribed)
                this.VM.PropertyChanged += this.VM_PropertyChanged;
            this.slider.Value = this.VM.PositionSeconds;
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanAddTrack")
                this.UpdateAppBar();
            if (!(e.PropertyName == "PositionSeconds") || !this._applyPositionFromVM)
                return;
            this.slider.Value = this.VM.PositionSeconds;
        }

        private void Slider_ManipulationStarted_1(object sender, ManipulationStartedEventArgs e)
        {
            this._applyPositionFromVM = false;
        }

        private void Slider_ManipulationCompleted_1(object sender, ManipulationCompletedEventArgs e)
        {
            this.VM.PositionSeconds = this.slider.Value;
            this._applyPositionFromVM = true;
        }

        private void UpdateAppBar()
        {
        }

        private void SetupAppBar()
        {
            this._addNewAppBarButton.Click += this.addNew_Click;
            this._appBar.Buttons.Add((object)this._addNewAppBarButton);
        }

        private void addNew_Click(object sender, EventArgs e)
        {
            this.VM.AddTrack();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                AudioPlayerViewModel audioPlayerViewModel = new AudioPlayerViewModel();
                audioPlayerViewModel.PreventPositionUpdates = true;
                this.DataContext = (object)audioPlayerViewModel;
                audioPlayerViewModel.PreventPositionUpdates = false;
                //int num = this.NavigationContext.QueryString["startPlaying"] == bool.TrueString ? 1 : 0;
                this._isInitialized = true;
            }
            this.VM.Activate(true);
            this.UpdateAppBar();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.VM.Activate(false);
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            base.OnRemovedFromJournal(e);
            this.VM.Cleanup();
        }

        private void playImage_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Play();
        }

        private void pauseImage_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Pause();
        }

        private void RevButton_Tap(object sender, GestureEventArgs e)
        {
            this.VM.PreviousTrack();
        }

        private void ForwardButton_Tap(object sender, GestureEventArgs e)
        {
            this.VM.NextTrack();
        }

        private void Shuffle_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Shuffle = !this.VM.Shuffle;
        }

        private void Repeat_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Repeat = !this.VM.Repeat;
        }

        private void Broadcast_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Broadcast = !this.VM.Broadcast;
        }

        private void SongText_Tap(object sender, GestureEventArgs e)
        {
            long lyricsId = this.VM.LyricsId;
            if (lyricsId == 0L)
                return;
            DialogService dialogService = new DialogService();
            dialogService.SetStatusBarBackground = true;
            dialogService.HideOnNavigation = false;
            LyricsUC ucLyrics = new LyricsUC();
            ucLyrics.textBlockNowPlayingTitle.Text = this.VM.CurrentTrackStr.ToUpperInvariant();
            LyricsUC lyricsUc = ucLyrics;
            dialogService.Child = (FrameworkElement)lyricsUc;
            dialogService.Show(null);
            AudioService.Instance.GetLyrics(lyricsId, (Action<BackendResult<Lyrics, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => ucLyrics.textBlockLyrics.Text = res.ResultData.text));
            }));
        }

        private void Add_Tap(object sender, GestureEventArgs e)
        {
            this.VM.AddTrack();
        }

        private void Next_Tap(object sender, GestureEventArgs e)
        {
            DialogService dialogService = new DialogService();
            dialogService.SetStatusBarBackground = true;
            dialogService.HideOnNavigation = false;
            PlaylistUC ucPlaylist = new PlaylistUC();
            PlaylistUC playlistUc = ucPlaylist;
            dialogService.Child = (FrameworkElement)playlistUc;
            EventHandler eventHandler = (EventHandler)((s, ev) =>
            {
                PlaylistViewModel vm = new PlaylistViewModel();
                vm.Shuffle = this.VM.Shuffle;
                ucPlaylist.DataContext = (object)vm;
                vm.Audios.LoadData(false, false, (Action<BackendResult<List<AudioObj>, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    AudioHeader audioHeader = vm.Audios.Collection.FirstOrDefault<AudioHeader>((Func<AudioHeader, bool>)(i => i.IsCurrentTrack));
                    if (audioHeader == null)
                        return;
                    int num = vm.Audios.Collection.IndexOf(audioHeader);
                    if (num > 0)
                        audioHeader = vm.Audios.Collection[num - 1];
                    ucPlaylist.AllAudios.ScrollTo((object)audioHeader);
                }))), false);
            });
            dialogService.Opened += eventHandler;
            dialogService.Show(null);
        }
    }
}
