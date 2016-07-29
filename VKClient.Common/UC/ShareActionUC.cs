using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class ShareActionUC : UserControl
  {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof (string), typeof (ShareActionUC), new PropertyMetadata(new PropertyChangedCallback(ShareActionUC.OnIconChanged)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ShareActionUC), new PropertyMetadata(new PropertyChangedCallback(ShareActionUC.OnTitleChanged)));
    internal ImageBrush imageBrushIcon;
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Icon
    {
      get
      {
        return (string) this.GetValue(ShareActionUC.IconProperty);
      }
      set
      {
        this.SetValue(ShareActionUC.IconProperty, (object) value);
      }
    }

    public string Title
    {
      get
      {
        return (string) this.GetValue(ShareActionUC.TitleProperty);
      }
      set
      {
        this.SetValue(ShareActionUC.TitleProperty, (object) value);
      }
    }

    public ShareActionUC()
    {
      this.InitializeComponent();
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ShareActionUC shareActionUc = d as ShareActionUC;
      if (shareActionUc == null)
        return;
      string str = e.NewValue as string;
      if (string.IsNullOrEmpty(str))
        return;
      ImageLoader.SetImageBrushMultiResSource(shareActionUc.imageBrushIcon, str);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ShareActionUC shareActionUc = d as ShareActionUC;
      if (shareActionUc == null)
        return;
      string str = e.NewValue as string;
      if (string.IsNullOrEmpty(str))
        return;
      shareActionUc.textBlockTitle.Text = str;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ShareActionUC.xaml", UriKind.Relative));
      this.imageBrushIcon = (ImageBrush) this.FindName("imageBrushIcon");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
    }
  }
}
