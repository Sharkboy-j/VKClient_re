using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Audio.ViewModels;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

using Microsoft.Phone.BackgroundAudio;
using VKClient.Audio.Base;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKMessenger.Views;
using System.Windows.Media;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Audio.Base.BLExtensions;

using System.Linq;

namespace VKClient.Audio
{
    public class AllUC : UserControl, IHandle<AudioPlayerStateChanged>//mod
    {
        internal ExtendedLongListSelector AllAudios;
        private bool _contentLoaded;

        private AllAudioViewModel VM
        {
            get
            {
                return base.DataContext as AllAudioViewModel;
            }
        }

        public bool IsInPickMode { get; set; }

        public ExtendedLongListSelector ListAudios
        {
            get
            {
                return this.AllAudios;
            }
        }

        public AllUC()
        {
            this.InitializeComponent();

            //EventAggregator.Current.Subscribe(this);//mod
        }

        private void EditTrackItem_Tap(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            AudioHeader dataContext = frameworkElement.DataContext as AudioHeader;
            if (dataContext == null)
                return;
            Navigator.Current.NavigateToEditAudio(dataContext.Track);
        }

        private void DeleteTrackItem_Tap(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            AudioHeader dataContext = frameworkElement.DataContext as AudioHeader;
            if (dataContext == null)
                return;
            this.DeleteAudios(new List<AudioHeader>()
      {
        dataContext
      });
        }

        public void DeleteAudios(List<AudioHeader> list)
        {
            if (!this.AskDeleteAudioConfirmation(list.Count))
                return;
            this.VM.DeleteAudios(list);
        }

        private bool AskDeleteAudioConfirmation(int count)
        {
            return MessageBox.Show(CommonResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.DeleteOneAudioFrm, AudioResources.DeleteTwoFourAudiosFrm, AudioResources.DeleteFiveAudiosFrm, true, null, false), (MessageBoxButton)1) == MessageBoxResult.OK;
        }

        private void AllAudios_Link_2(object sender, LinkUnlinkEventArgs e)
        {
            this.VM.AllTracks.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Audio;component/UserControls/AllUC.xaml", UriKind.Relative));
            this.AllAudios = (ExtendedLongListSelector)base.FindName("AllAudios");
        }
        //
        private void Temp_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AudioTrack track = null;
            Grid btn = sender as Grid;
            for (int i = 0; i < this.VM.AllTracks.Collection.Count; i++)
            {
                AudioHeader temp = this.VM.AllTracks.Collection[i];
                if (temp.Track.UniqueId == btn.Tag.ToString())
                {
                    track = AudioTrackHelper.CreateTrack(temp.Track);
                }
            }

            if (track == null)
                return;

            if (track.Tag == BGAudioPlayerWrapper.Instance.Track.Tag)
            {
                if (BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Playing)
                    BGAudioPlayerWrapper.Instance.Pause();
                else
                    BGAudioPlayerWrapper.Instance.Play();
                return;
            }

            BGAudioPlayerWrapper.Instance.Track = track;
            BGAudioPlayerWrapper.Instance.Volume = 1.0;
            BGAudioPlayerWrapper.Instance.Play();

            Grid grid = btn.Children[0] as Grid;
            Border borderPlay = grid.Children[1] as Border;
            borderPlay.Opacity = 0.1;

            Border borderPause = grid.Children[2] as Border;
            borderPause.Opacity = 1;

            EventAggregator.Current.Publish(new AudioPlayerStateChanged(BGAudioPlayerWrapper.Instance.PlayerState));
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            string tag = AudioTrackExtensions.GetTagId(BGAudioPlayerWrapper.Instance.Track);
            IEnumerable<Grid> logicalChildrenByType1 = this.GetLogicalChildrenByType<Grid>(false);
            for (int index = 0; index < logicalChildrenByType1.Count<Grid>(); index++)
            {
                Grid btn = logicalChildrenByType1.ElementAt(index);
                Grid grid = btn.Children[0] as Grid;
                if (grid == null || grid.Tag == null || grid.Children.Count < 2)
                    continue;
                Border borderPlay = grid.Children[1] as Border;
                Border borderPause = grid.Children[2] as Border;

                if (btn.Tag.ToString() == tag && BGAudioPlayerWrapper.Instance.PlayerState == PlayState.Playing)
                {
                    borderPlay.Opacity = 0.1;
                    borderPause.Opacity = 1;
                }
                else
                {
                    borderPlay.Opacity = 1;
                    borderPause.Opacity = 0.1;
                }

            }
        }
    }
}
