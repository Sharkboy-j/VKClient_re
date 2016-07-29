using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class ShareContentActionsUC : UserControl
  {
    private bool _contentLoaded;

    public event EventHandler ShareWallPostItemSelected;

    public event EventHandler ShareCommunityItemSelected;

    public ShareContentActionsUC()
    {
      this.InitializeComponent();
    }

    private void ShareWallPostItem_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.ShareWallPostItemSelected == null)
        return;
      this.ShareWallPostItemSelected((object) this, EventArgs.Empty);
    }

    private void ShareCommunityItem_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.ShareCommunityItemSelected == null)
        return;
      this.ShareCommunityItemSelected((object) this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ShareContentActionsUC.xaml", UriKind.Relative));
    }
  }
}
