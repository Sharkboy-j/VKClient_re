using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class NewsfeedNewPostUC : UserControl
  {
    private bool _contentLoaded;

    public NewsfeedNewPostUC()
    {
      this.InitializeComponent();
    }

    private void NewPost_OnTap(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToNewWallPost(0L, false, 0, false, false, false);
    }

    private void Photo_OnTap(object sender, GestureEventArgs e)
    {
      ParametersRepository.SetParameterForId("GoPickImage", (object) true);
      Navigator.Current.NavigateToNewWallPost(0L, false, 0, false, false, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewsfeedNewPostUC.xaml", UriKind.Relative));
    }
  }
}
