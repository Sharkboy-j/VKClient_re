using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class GenericPageLoadInfoUC : UserControl
  {
    private bool _contentLoaded;

    public GenericPageLoadInfoUC()
    {
      this.InitializeComponent();
    }

    private void ButtonRetry_OnClick(object sender, RoutedEventArgs e)
    {
      ViewModelStatefulBase modelStatefulBase = this.DataContext as ViewModelStatefulBase;
      if (modelStatefulBase == null)
        return;
      modelStatefulBase.Reload();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GenericPageLoadInfoUC.xaml", UriKind.Relative));
    }
  }
}
