using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ReplyUserUC : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ReplyUserUC), new PropertyMetadata(new PropertyChangedCallback(ReplyUserUC.OnTitleChanged)));
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) this.GetValue(ReplyUserUC.TitleProperty);
      }
      set
      {
        this.SetValue(ReplyUserUC.TitleProperty, (object) value);
      }
    }

    public event EventHandler TitleChanged;

    public ReplyUserUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = "";
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ReplyUserUC replyUserUc = d as ReplyUserUC;
      if (replyUserUc == null)
        return;
      string str = e.NewValue as string;
      replyUserUc.textBlockTitle.Text = !string.IsNullOrEmpty(str) ? str : "";
      replyUserUc.FireTitleChangedEvent();
    }

    private void FireTitleChangedEvent()
    {
      if (this.TitleChanged == null)
        return;
      this.TitleChanged((object) this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ReplyUserUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
    }
  }
}
