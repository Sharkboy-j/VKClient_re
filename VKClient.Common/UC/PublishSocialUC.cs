using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class PublishSocialUC : UserControl
  {
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof (PublishSocialType), typeof (PublishSocialUC), new PropertyMetadata(new PropertyChangedCallback(PublishSocialUC.OnTypeChanged)));
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof (bool), typeof (PublishSocialUC), new PropertyMetadata(new PropertyChangedCallback(PublishSocialUC.OnIsCheckedChanged)));
    internal Image imageIcon;
    private bool _contentLoaded;

    public PublishSocialType Type
    {
      get
      {
        return (PublishSocialType) this.GetValue(PublishSocialUC.TypeProperty);
      }
      set
      {
        this.SetValue(PublishSocialUC.TypeProperty, (object) value);
      }
    }

    public bool IsChecked
    {
      get
      {
        return (bool) this.GetValue(PublishSocialUC.IsCheckedProperty);
      }
      set
      {
        this.SetValue(PublishSocialUC.IsCheckedProperty, (object) value);
      }
    }

    public PublishSocialUC()
    {
      this.InitializeComponent();
    }

    private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PublishSocialUC publishSocialUc = d as PublishSocialUC;
      if (publishSocialUc == null || !(e.NewValue is PublishSocialType))
        return;
      switch ((PublishSocialType) e.NewValue)
      {
        case PublishSocialType.Twitter:
          MultiResImageLoader.SetUriSource(publishSocialUc.imageIcon, "/Resources/NewPost/ImportTwitter.png");
          break;
        case PublishSocialType.Facebook:
          MultiResImageLoader.SetUriSource(publishSocialUc.imageIcon, "/Resources/NewPost/ImportFacebook.png");
          break;
      }
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PublishSocialUC publishSocialUc = d as PublishSocialUC;
      if (publishSocialUc == null)
        return;
      bool flag = (bool) e.NewValue;
      publishSocialUc.imageIcon.Opacity = flag ? 1.0 : 0.5;
    }

    private void OnTapped(object sender, GestureEventArgs e)
    {
      this.IsChecked = !this.IsChecked;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/PublishSocialUC.xaml", UriKind.Relative));
      this.imageIcon = (Image) this.FindName("imageIcon");
    }
  }
}
