using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC.MapAttachments
{
  public class MapPlaceFullAttachmentUC : MapAttachmentUCBase
  {
    private Uri _mapUri;
    private Uri _groupPhotoUri;
    private const int PLACE_HEIGHT = 80;
    internal Canvas canvas;
    internal Rectangle rectanglePlaceholder;
    internal Image imageMap;
    internal Image imageMapIcon;
    internal Rectangle rectMapBorderBottom;
    internal Grid gridPlace;
    internal Image imageGroupPhoto;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitle;
    internal Rectangle rectBorder;
    private bool _contentLoaded;

    public MapPlaceFullAttachmentUC()
    {
      this.InitializeComponent();
    }

    public static double CalculateTotalHeight(double width)
    {
      return MapAttachmentUCBase.GetMapHeight(width) + 80.0;
    }

    public override void OnReady()
    {
      double mapHeight = MapAttachmentUCBase.GetMapHeight(this.Width);
      double totalHeight = MapPlaceFullAttachmentUC.CalculateTotalHeight(this.Width);
      this._mapUri = this.GetMapUri();
      this.canvas.Width = this.Width;
      this.canvas.Height = mapHeight + 80.0;
      this.rectBorder.Width = this.Width;
      this.rectBorder.Height = totalHeight;
      this.rectanglePlaceholder.Width = this.Width;
      this.rectanglePlaceholder.Height = mapHeight;
      this.imageMap.Width = this.Width;
      this.imageMap.Height = mapHeight;
      Canvas.SetLeft((UIElement) this.imageMapIcon, this.Width / 2.0 - this.imageMapIcon.Width / 2.0);
      Canvas.SetTop((UIElement) this.imageMapIcon, mapHeight / 2.0 - this.imageMapIcon.Height);
      this.rectMapBorderBottom.Width = this.Width - 2.0;
      Canvas.SetTop((UIElement) this.rectMapBorderBottom, mapHeight - 1.0);
      Canvas.SetTop((UIElement) this.gridPlace, mapHeight);
      this.UpdateTitleSubtitle();
      double val2 = this.Width - this.textBlockTitle.Margin.Left;
      this.textBlockTitle.CorrectText(Math.Max(0.0, val2));
      this.textBlockSubtitle.CorrectText(Math.Max(0.0, val2));
    }

    private void UpdateTitleSubtitle()
    {
      Place place = this.Geo.place;
      if (!string.IsNullOrEmpty(place != null ? place.group_photo : null))
        this._groupPhotoUri = new Uri(place.group_photo);
      if (!string.IsNullOrEmpty(this.Geo.AttachmentTitle) && !string.IsNullOrEmpty(this.Geo.AttachmentSubtitle))
      {
        this.textBlockTitle.Text = this.Geo.AttachmentTitle;
        this.textBlockSubtitle.Text = this.Geo.AttachmentSubtitle;
      }
      else
      {
        if (place != null)
        {
          string str1 = "";
          string str2 = "";
          if (!string.IsNullOrEmpty(place.title))
            str1 = place.title.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
          if (!string.IsNullOrEmpty(place.address))
            str2 = place.address.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
          if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2))
          {
            this.Geo.AttachmentTitle = str1;
            this.Geo.AttachmentSubtitle = str2;
            this.textBlockTitle.Text = str1;
            this.textBlockSubtitle.Text = str2;
            return;
          }
        }
        this.textBlockTitle.Text = "...";
        this.textBlockSubtitle.Text = "...";
        this.ReverseGeocode((Action<string, string>) ((title, subtitle) => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (string.IsNullOrEmpty(title))
            title = CommonResources.MapAttachment_Place;
          if (string.IsNullOrEmpty(subtitle))
            subtitle = CommonResources.MapAttachment_CountryNotIdentified;
          this.Geo.AttachmentTitle = title.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
          this.Geo.AttachmentSubtitle = subtitle.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ");
        }))));
      }
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, this._mapUri);
      VeryLowProfileImageLoader.SetUriSource(this.imageGroupPhoto, this._groupPhotoUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, (Uri) null);
      VeryLowProfileImageLoader.SetUriSource(this.imageGroupPhoto, (Uri) null);
    }

    public override void ShownOnScreen()
    {
      DateTime now;
      if (this._groupPhotoUri != (Uri) null && this._groupPhotoUri.IsAbsoluteUri)
      {
        string originalString = this._groupPhotoUri.OriginalString;
        now = DateTime.Now;
        long ticks = now.Ticks;
        VeryLowProfileImageLoader.SetPriority(originalString, ticks);
      }
      if (!(this._mapUri != (Uri) null) || !this._mapUri.IsAbsoluteUri)
        return;
      string originalString1 = this._mapUri.OriginalString;
      now = DateTime.Now;
      long ticks1 = now.Ticks;
      VeryLowProfileImageLoader.SetPriority(originalString1, ticks1);
    }

    private void GridPlace_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      Place place = this.Geo.place;
      long groupId = place != null ? place.group_id : 0L;
      if (groupId <= 0L)
        return;
      Navigator.Current.NavigateToGroup(groupId, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPlaceFullAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) this.FindName("canvas");
      this.rectanglePlaceholder = (Rectangle) this.FindName("rectanglePlaceholder");
      this.imageMap = (Image) this.FindName("imageMap");
      this.imageMapIcon = (Image) this.FindName("imageMapIcon");
      this.rectMapBorderBottom = (Rectangle) this.FindName("rectMapBorderBottom");
      this.gridPlace = (Grid) this.FindName("gridPlace");
      this.imageGroupPhoto = (Image) this.FindName("imageGroupPhoto");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockSubtitle = (TextBlock) this.FindName("textBlockSubtitle");
      this.rectBorder = (Rectangle) this.FindName("rectBorder");
    }
  }
}
