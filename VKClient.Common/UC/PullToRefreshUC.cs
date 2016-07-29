using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class PullToRefreshUC : UserControl
  {
    private double _previousP = -1.0;
    internal Grid LayoutRoot;
    internal Rectangle rectProgress;
    internal TextBlock textBlockTip;
    private bool _contentLoaded;

    public Brush ForegroundBrush
    {
      get
      {
        return this.textBlockTip.Foreground;
      }
      set
      {
        this.textBlockTip.Foreground = value;
        this.rectProgress.Fill = value;
      }
    }

    public PullToRefreshUC()
    {
      this.InitializeComponent();
    }

    public void TrackListBox(ISupportPullToRefresh lb)
    {
      lb.OnPullPercentageChanged = (Action) (() => this.OnPullPercentageChanged(lb));
      this.Update(0.0);
    }

    private void OnPullPercentageChanged(ISupportPullToRefresh lb)
    {
      this.Update(lb.PullPercentage);
    }

    private void Update(double p)
    {
      if (this._previousP == p)
        return;
      (this.rectProgress.RenderTransform as ScaleTransform).ScaleX = 1.0 + p / 100.0;
      this.rectProgress.Opacity = this.textBlockTip.Opacity = (p + 50.0) * 0.667 / 100.0;
      this.textBlockTip.Visibility = this.rectProgress.Visibility = p > 0.0 ? Visibility.Visible : Visibility.Collapsed;
      try
      {
        if (Application.Current.RootVisual is PhoneApplicationFrame)
        {
          if ((Application.Current.RootVisual as PhoneApplicationFrame).Content is PhoneApplicationPage)
            SystemTray.IsVisible = this.textBlockTip.Visibility != Visibility.Visible;
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("PullToRefreshUC Failed to set systemtray visibility", ex);
      }
      this._previousP = p;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/PullToRefreshUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.rectProgress = (Rectangle) this.FindName("rectProgress");
      this.textBlockTip = (TextBlock) this.FindName("textBlockTip");
    }
  }
}
