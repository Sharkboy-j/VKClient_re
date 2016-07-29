using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace VKClient.Common.UC
{
  public class PhotoFeedMoveTutorialUC : NewsfeedPromoUC
  {
    internal Grid gridBackground;
    internal Grid gridCutArea;
    internal Polygon polygonTriangle;
    internal Grid gridMessage;
    private bool _contentLoaded;

    public Action BackgroundTapCallback { get; set; }

    protected override FrameworkElement GridCutArea
    {
      get
      {
        return (FrameworkElement) this.gridCutArea;
      }
    }

    protected override Grid GridBackground
    {
      get
      {
        return this.gridBackground;
      }
    }

    protected override FrameworkElement GridMessage
    {
      get
      {
        return (FrameworkElement) this.gridMessage;
      }
    }

    protected override Polygon PolygonTriangle
    {
      get
      {
        return this.polygonTriangle;
      }
    }

    public PhotoFeedMoveTutorialUC()
    {
      this.InitializeComponent();
    }

    private void GridBackground_OnTap(object sender, GestureEventArgs e)
    {
      Action backgroundTapCallback = this.BackgroundTapCallback;
      if (backgroundTapCallback == null)
        return;
      backgroundTapCallback();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/PhotoFeedMoveTutorialUC.xaml", UriKind.Relative));
      this.gridBackground = (Grid) this.FindName("gridBackground");
      this.gridCutArea = (Grid) this.FindName("gridCutArea");
      this.polygonTriangle = (Polygon) this.FindName("polygonTriangle");
      this.gridMessage = (Grid) this.FindName("gridMessage");
    }
  }
}
