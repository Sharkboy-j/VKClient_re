using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Photos.Library;

namespace VKClient.Photos.UC
{
  public partial class PickAlbumUC : UserControl
  {
    private static PhotoPickerAlbumsViewModel _vmInstance;

    public static PhotoPickerAlbumsViewModel VM
    {
      get
      {
        if (PickAlbumUC._vmInstance == null)
          PickAlbumUC._vmInstance = new PhotoPickerAlbumsViewModel();
        return PickAlbumUC._vmInstance;
      }
    }

    public Action<string> SelectedAlbumCallback { get; set; }

    public PickAlbumUC()
    {
      this.InitializeComponent();
    }

    public void Initialize()
    {
      this.DataContext = (object) PickAlbumUC.VM;
    }

    public void Cleanup()
    {
      this.DataContext = null;
    }

    private void Image_Tap(object sender, GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumHeaderTwoInARow albumHeaderTwoInArow = frameworkElement.DataContext as AlbumHeaderTwoInARow;
      if (albumHeaderTwoInArow == null)
        return;
      AlbumHeader albumHeader = frameworkElement.Tag.ToString() == "1" ? albumHeaderTwoInArow.AlbumHeader1 : albumHeaderTwoInArow.AlbumHeader2;
      if (albumHeader == null)
        return;
      this.SelectedAlbumCallback(albumHeader.AlbumId);
    }

  }
}
