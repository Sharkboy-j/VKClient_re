using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class GamesSlideViewItem : UserControl
  {
    internal Border Header;
    internal Grid Content;
    private bool _contentLoaded;

    public GamesSlideViewItem()
    {
      this.InitializeComponent();
    }

    public void SetState(bool state)
    {
    }

    public void SetDataContext(object dataContext)
    {
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GamesSlideViewItem.xaml", UriKind.Relative));
      this.Header = (Border) this.FindName("Header");
      this.Content = (Grid) this.FindName("Content");
    }
  }
}
