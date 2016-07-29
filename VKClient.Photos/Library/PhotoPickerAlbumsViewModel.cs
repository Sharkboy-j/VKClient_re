using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Photos.Library
{
  public class PhotoPickerAlbumsViewModel : ViewModelBase
  {
    private ObservableCollection<AlbumHeaderTwoInARow> _albums = new ObservableCollection<AlbumHeaderTwoInARow>();
    private bool _isLoaded;

    public ObservableCollection<AlbumHeaderTwoInARow> Albums
    {
      get
      {
        return this._albums;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.CHOOSEALBUM;
      }
    }

    public string FooterText
    {
      get
      {
        return PhotosMainViewModel.GetAlbumsTextForCount(this._albums.Sum<AlbumHeaderTwoInARow>((Func<AlbumHeaderTwoInARow, int>) (a => a.GetItemsCount())));
      }
    }

    public Visibility FooterTextVisibility
    {
      get
      {
        return string.IsNullOrEmpty(this.FooterText) || !this._isLoaded ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string StatusText
    {
      get
      {
        return "";
      }
    }

    public Visibility StatusTextVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public ICommand TryAgainCmd
    {
      get
      {
        return (ICommand) null;
      }
    }

    public Visibility TryAgainVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public PhotoPickerAlbumsViewModel()
    {
      this.Initialize();
    }

    private void Initialize()
    {
      ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
      {
        try
        {
          Stopwatch stopwatch = Stopwatch.StartNew();
          List<AlbumHeader> albumHeaders = new List<AlbumHeader>();
          using (MediaLibrary mediaLibrary = new MediaLibrary())
          {
            using (PictureAlbum rootPictureAlbum = mediaLibrary.RootPictureAlbum)
            {
              foreach (PictureAlbum album in rootPictureAlbum.Albums)
              {
                AlbumHeader albumHeader = new AlbumHeader();
                albumHeader.AlbumId = album.Name;
                albumHeader.AlbumName = album.Name;
                albumHeader.PhotosCount = album.Pictures.Count;
                string albumName = albumHeader.AlbumName;
                if (!(albumName == "Camera Roll"))
                {
                  if (!(albumName == "Saved Pictures"))
                  {
                    if (!(albumName == "Sample Pictures"))
                    {
                      if (albumName == "Screenshots")
                      {
                        albumHeader.AlbumName = CommonResources.AlbumScreenshots;
                        albumHeader.OrderNo = 3;
                      }
                      else
                        albumHeader.OrderNo = int.MaxValue;
                    }
                    else
                    {
                      albumHeader.AlbumName = CommonResources.AlbumSamplePictures;
                      albumHeader.OrderNo = 2;
                    }
                  }
                  else
                  {
                    albumHeader.AlbumName = CommonResources.AlbumSavedPictures;
                    albumHeader.OrderNo = 1;
                  }
                }
                else
                {
                  albumHeader.AlbumName = CommonResources.AlbumCameraRoll;
                  albumHeader.OrderNo = 0;
                }
                Picture picture = album.Pictures.FirstOrDefault<Picture>();
                if (picture != (Picture) null)
                {
                  albumHeader.ImageStream = picture.GetThumbnail();
                  picture.Dispose();
                }
                albumHeaders.Add(albumHeader);
              }
            }
          }
          stopwatch.Stop();
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            this._albums.Clear();
            foreach (IEnumerable<AlbumHeader> source in albumHeaders.OrderBy<AlbumHeader, int>((Func<AlbumHeader, int>) (ah => ah.OrderNo)).Partition<AlbumHeader>(2))
            {
              List<AlbumHeader> list = source.ToList<AlbumHeader>();
              AlbumHeaderTwoInARow albumHeaderTwoInArow = new AlbumHeaderTwoInARow();
              albumHeaderTwoInArow.AlbumHeader1 = list[0];
              if (list.Count > 1)
                albumHeaderTwoInArow.AlbumHeader2 = list[1];
              this._albums.Add(albumHeaderTwoInArow);
            }
            this._isLoaded = true;
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FooterText));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FooterTextVisibility));
          }));
        }
        catch (Exception ex)
        {
          Logger.Instance.ErrorAndSaveToIso("Failed to read gallery albums", ex);
        }
      }));
    }
  }
}
