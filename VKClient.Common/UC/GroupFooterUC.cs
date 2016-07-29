using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class GroupFooterUC : UserControl
  {
    public static readonly DependencyProperty FooterTextProperty = DependencyProperty.Register("FooterText", typeof (string), typeof (GroupFooterUC), new PropertyMetadata(new PropertyChangedCallback(GroupFooterUC.OnFooterTextChanged)));
    internal TextBlock textBlockFooter;
    private bool _contentLoaded;

    public string FooterText
    {
      get
      {
        return (string) this.GetValue(GroupFooterUC.FooterTextProperty);
      }
      set
      {
        this.SetValue(GroupFooterUC.FooterTextProperty, (object) value);
      }
    }

    public event EventHandler MoreTapped;

    public GroupFooterUC()
    {
      this.InitializeComponent();
    }

    private static void OnFooterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GroupFooterUC groupFooterUc = d as GroupFooterUC;
      if (groupFooterUc == null)
        return;
      string str = e.NewValue as string;
      groupFooterUc.textBlockFooter.Text = !string.IsNullOrEmpty(str) ? str : "";
    }

    private void More_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.MoreTapped == null)
        return;
      this.MoreTapped((object) this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GroupFooterUC.xaml", UriKind.Relative));
      this.textBlockFooter = (TextBlock) this.FindName("textBlockFooter");
    }
  }
}
