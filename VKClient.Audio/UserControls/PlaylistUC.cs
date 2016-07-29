using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Library;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Audio.UserControls
{
  public partial class PlaylistUC : UserControl
  {

    public PlaylistUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = CommonResources.Audio_Playlist.ToUpperInvariant();
    }

    private void AllAudios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      if (longListSelector == null)
        return;
      AudioHeader audioHeader = longListSelector.SelectedItem as AudioHeader;
      if (audioHeader == null)
        return;
      audioHeader.AssignTrack();
      longListSelector.SelectedItem = null;
    }
  }
}
