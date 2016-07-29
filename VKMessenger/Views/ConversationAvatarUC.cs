using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKMessenger.Views
{
  public class ConversationAvatarUC : UserControl
  {
    internal Grid gridConversationHeader;
    private bool _contentLoaded;

    public ConversationAvatarUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ConversationAvatarUC.xaml", UriKind.Relative));
      this.gridConversationHeader = (Grid) this.FindName("gridConversationHeader");
    }
  }
}
