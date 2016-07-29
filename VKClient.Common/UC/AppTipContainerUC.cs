using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class AppTipContainerUC : UserControl
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public Action OnTap { get; set; }

    public AppTipContainerUC()
    {
      this.InitializeComponent();
    }

    public void InitForSwipeFromLeftSideTip()
    {
      AppTipBubbleUC appTipBubbleUc = new AppTipBubbleUC();
      appTipBubbleUc.LayoutRoot.Width = 293.0;
      appTipBubbleUc.Margin = new Thickness(16.0, 96.0, 0.0, 0.0);
      appTipBubbleUc.imageTip.Width = 100.0;
      appTipBubbleUc.imageTip.Height = 92.0;
      ImageLoader.SetUriSource(appTipBubbleUc.imageTip, "/Resources/New/SwipeMenuTip.png");
      appTipBubbleUc.textBlockTip.Text = CommonResources.SwipeToOpenMenuTip;
      this.LayoutRoot.Children.Add((UIElement) appTipBubbleUc);
    }

    public void InitForPullToRefresh()
    {
      AppTipBubbleUC appTipBubbleUc = new AppTipBubbleUC();
      appTipBubbleUc.LayoutRoot.Width = 277.0;
      appTipBubbleUc.Margin = new Thickness(90.0, 96.0, 0.0, 0.0);
      appTipBubbleUc.imageTip.Width = 66.0;
      appTipBubbleUc.imageTip.Height = 117.0;
      ImageLoader.SetUriSource(appTipBubbleUc.imageTip, "/Resources/New/PullToRefreshTip.png");
      appTipBubbleUc.textBlockTip.Text = CommonResources.PullToRefreshTip;
      this.LayoutRoot.Children.Add((UIElement) appTipBubbleUc);
    }

    private void LayoutRoot_Tap(object sender, GestureEventArgs e)
    {
      if (this.OnTap == null)
        return;
      this.OnTap();
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/AppTipContainerUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
    }
  }
}
