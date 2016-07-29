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

namespace VKClient.Audio
{
    public partial class AllUC : UserControl
  {
    private AllAudioViewModel VM
    {
      get
      {
        return this.DataContext as AllAudioViewModel;
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
    }

    private void NavigateToAudioPlayer(AudioHeader track, IEnumerable enumerable)
    {
      List<AudioObj> tracks = new List<AudioObj>();
      foreach (object obj in enumerable)
      {
        if (obj is AudioHeader)
          tracks.Add((obj as AudioHeader).Track);
      }
      PlaylistManager.SetAudioAgentPlaylist(tracks, CurrentMediaSource.AudioSource);
      Navigator.Current.NavigateToAudioPlayer(false);
    }

    private void DeleteTrackItem_Tap(object sender, RoutedEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      AudioHeader audioHeader = frameworkElement.DataContext as AudioHeader;
      if (audioHeader == null)
        return;
      this.DeleteAudios(new List<AudioHeader>()
      {
        audioHeader
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
      return MessageBox.Show(CommonResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.DeleteOneAudioFrm, AudioResources.DeleteTwoFourAudiosFrm, AudioResources.DeleteFiveAudiosFrm, true, null, false), MessageBoxButton.OKCancel) == MessageBoxResult.OK;
    }

    private void AllAudios_Link_1(object sender, MyLinkUnlinkEventArgs e)
    {
      this.VM.AllTracks.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void AllAudios_Link_2(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.AllTracks.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }
  }
}
