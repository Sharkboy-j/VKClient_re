using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ScrollableTextBox : UserControl
  {
    private bool _contentLoaded;

    public ScrollableTextBox()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ScrollableTextBox.xaml", UriKind.Relative));
    }
  }
}
