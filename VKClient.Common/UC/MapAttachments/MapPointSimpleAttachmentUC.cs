using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using VKClient.Common.Framework;

namespace VKClient.Common.UC.MapAttachments
{
  public class MapPointSimpleAttachmentUC : MapAttachmentUCBase
  {
    private Uri _mapUri;
    internal Canvas canvas;
    internal Rectangle rectanglePlaceholder;
    internal Image imageMap;
    internal Image imageMapIcon;
    private bool _contentLoaded;

    public MapPointSimpleAttachmentUC()
    {
      this.InitializeComponent();
    }

    public static double CalculateTotalHeight(double width)
    {
      return MapAttachmentUCBase.GetMapHeight(width);
    }

    public override void OnReady()
    {
      double mapHeight = MapAttachmentUCBase.GetMapHeight(this.Width);
      this._mapUri = this.GetMapUri();
      this.canvas.Width = this.Width;
      this.canvas.Height = mapHeight;
      this.rectanglePlaceholder.Width = this.Width;
      this.rectanglePlaceholder.Height = mapHeight;
      this.imageMap.Width = this.Width;
      this.imageMap.Height = mapHeight;
      Canvas.SetLeft((UIElement) this.imageMapIcon, this.Width / 2.0 - this.imageMapIcon.Width / 2.0);
      Canvas.SetTop((UIElement) this.imageMapIcon, mapHeight / 2.0 - this.imageMapIcon.Height);
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, this._mapUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imageMap, (Uri) null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._mapUri != (Uri) null) || !this._mapUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._mapUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/MapAttachments/MapPointSimpleAttachmentUC.xaml", UriKind.Relative));
      this.canvas = (Canvas) this.FindName("canvas");
      this.rectanglePlaceholder = (Rectangle) this.FindName("rectanglePlaceholder");
      this.imageMap = (Image) this.FindName("imageMap");
      this.imageMapIcon = (Image) this.FindName("imageMapIcon");
    }
  }
}
