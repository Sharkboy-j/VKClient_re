using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class PublicRejectUC : UserControlVirtualizable
  {
    internal Button buttonPublish;
    internal Button buttonDelete;
    private bool _contentLoaded;

    public PublicRejectUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/PublicRejectUC.xaml", UriKind.Relative));
      this.buttonPublish = (Button) this.FindName("buttonPublish");
      this.buttonDelete = (Button) this.FindName("buttonDelete");
    }
  }
}
