using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class EarlierRepliesUC : UserControl
  {
    internal Grid gridViewedFeedback;
    private bool _contentLoaded;

    public EarlierRepliesUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/EarlierRepliesUC.xaml", UriKind.Relative));
      this.gridViewedFeedback = (Grid) this.FindName("gridViewedFeedback");
    }
  }
}
