using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Photos.Library
{
  public class AlbumPhotoHeaderFourInARow : IItemsCount
  {
    public AlbumPhoto Photo1 { get; set; }

    public AlbumPhoto Photo2 { get; set; }

    public AlbumPhoto Photo3 { get; set; }

    public AlbumPhoto Photo4 { get; set; }

    public bool AllowEdit { get; private set; }

    public Visibility AllowEditVisibility
    {
      get
      {
        return !this.AllowEdit ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool AllowRemove { get; private set; }

    public Visibility AllowRemoveVisibility
    {
      get
      {
        return !this.AllowRemove ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility AllowEditOrRemoveVisibility
    {
      get
      {
        return !this.AllowEdit && !this.AllowRemove ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public double SelectionOpacity { get; set; }

    public Visibility Photo1IsSet
    {
      get
      {
        return this.Photo1 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Photo2IsSet
    {
      get
      {
        return this.Photo2 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Photo3IsSet
    {
      get
      {
        return this.Photo3 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Photo4IsSet
    {
      get
      {
        return this.Photo4 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int PhotosCount
    {
      get
      {
        return this.IsSet(this.Photo1) + this.IsSet(this.Photo2) + this.IsSet(this.Photo3) + this.IsSet(this.Photo4);
      }
    }

    public AlbumPhotoHeaderFourInARow(bool allowEdit, bool allowRemove)
    {
      this.AllowEdit = allowEdit;
      this.AllowRemove = allowRemove;
    }

    public AlbumPhotoHeaderFourInARow(IEnumerable<Photo> photos)
    {
      IEnumerator<Photo> enumerator = photos.GetEnumerator();
      if (enumerator.MoveNext())
        this.Photo1 = new AlbumPhoto(enumerator.Current);
      if (enumerator.MoveNext())
        this.Photo2 = new AlbumPhoto(enumerator.Current);
      if (enumerator.MoveNext())
        this.Photo3 = new AlbumPhoto(enumerator.Current);
      if (!enumerator.MoveNext())
        return;
      this.Photo4 = new AlbumPhoto(enumerator.Current);
    }

    public AlbumPhotoHeaderFourInARow(IEnumerable<AlbumPhoto> photos)
    {
      IEnumerator<AlbumPhoto> enumerator = photos.GetEnumerator();
      if (enumerator.MoveNext())
        this.Photo1 = enumerator.Current;
      if (enumerator.MoveNext())
        this.Photo2 = enumerator.Current;
      if (enumerator.MoveNext())
        this.Photo3 = enumerator.Current;
      if (!enumerator.MoveNext())
        return;
      this.Photo4 = enumerator.Current;
    }

    public IEnumerable<Photo> GetAsPhotos()
    {
      if (this.Photo1 != null)
        yield return this.Photo1.Photo;
      if (this.Photo2 != null)
        yield return this.Photo2.Photo;
      if (this.Photo3 != null)
        yield return this.Photo3.Photo;
      if (this.Photo4 != null)
        yield return this.Photo4.Photo;
    }

    public IEnumerable<AlbumPhoto> GetAsAlbumPhotos()
    {
      if (this.Photo1 != null)
        yield return this.Photo1;
      if (this.Photo2 != null)
        yield return this.Photo2;
      if (this.Photo3 != null)
        yield return this.Photo3;
      if (this.Photo4 != null)
        yield return this.Photo4;
    }

    private int IsSet(AlbumPhoto obj)
    {
      return obj == null || obj.Photo != null && string.IsNullOrEmpty(obj.Src) ? 0 : 1;
    }

    public AlbumPhoto GetPhotoByTag(string tag)
    {
      AlbumPhoto albumPhoto = (AlbumPhoto) null;
      if (!(tag == "1"))
      {
        if (!(tag == "2"))
        {
          if (!(tag == "3"))
          {
            if (tag == "4")
              albumPhoto = this.Photo4;
          }
          else
            albumPhoto = this.Photo3;
        }
        else
          albumPhoto = this.Photo2;
      }
      else
        albumPhoto = this.Photo1;
      return albumPhoto;
    }

    public int GetItemsCount()
    {
      return this.PhotosCount;
    }
  }
}
