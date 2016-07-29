using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Audio.UserControls
{
    public partial class EditAlbumUC : UserControl
  {

    public EditAlbumUC()
    {
      this.InitializeComponent();
    }

    private void textBoxText_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.buttonSave.IsEnabled = this.textBoxText.Text.Length >= 2;
    }
  }
}
