using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class FooterNewUC : UserControl
  {
    private bool _contentLoaded;

    public FooterNewUC()
    {
      this.InitializeComponent();
    }

    private void ButtonTryAgain_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      ISupportReload supportReload = ((FrameworkElement) sender).DataContext as ISupportReload;
      if (supportReload == null)
        return;
      supportReload.Reload();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/FooterNewUC.xaml", UriKind.Relative));
    }
  }
}
