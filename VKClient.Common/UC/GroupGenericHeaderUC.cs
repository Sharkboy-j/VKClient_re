using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class GroupGenericHeaderUC : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (GroupGenericHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupGenericHeaderUC.OnTitleChanged)));
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) this.GetValue(GroupGenericHeaderUC.TitleProperty);
      }
      set
      {
        this.SetValue(GroupGenericHeaderUC.TitleProperty, (object) value);
      }
    }

    public GroupGenericHeaderUC()
    {
      this.InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GroupGenericHeaderUC groupGenericHeaderUc = (GroupGenericHeaderUC) d;
      string str = e.NewValue as string;
      groupGenericHeaderUc.textBlockTitle.Text = !string.IsNullOrEmpty(str) ? str.ToUpperInvariant() : "";
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GroupGenericHeaderUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
    }
  }
}
