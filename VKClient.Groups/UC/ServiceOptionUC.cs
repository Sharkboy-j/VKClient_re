using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Groups.UC
{
    public partial class ServiceOptionUC : UserControl
  {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof (string), typeof (ServiceOptionUC), new PropertyMetadata((object) "", new PropertyChangedCallback(ServiceOptionUC.IconPropertyChangedCallback)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ServiceOptionUC), new PropertyMetadata((object) "", new PropertyChangedCallback(ServiceOptionUC.TitlePropertyChangedCallback)));
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof (string), typeof (ServiceOptionUC), new PropertyMetadata((object) "", new PropertyChangedCallback(ServiceOptionUC.StatePropertyChangedCallback)));
   

    public string Icon
    {
      get
      {
        return (string) this.GetValue(ServiceOptionUC.IconProperty);
      }
      set
      {
        this.SetValue(ServiceOptionUC.IconProperty, (object) value);
      }
    }

    public string Title
    {
      get
      {
        return (string) this.GetValue(ServiceOptionUC.TitleProperty);
      }
      set
      {
        this.SetValue(ServiceOptionUC.TitleProperty, (object) value);
      }
    }

    public string State
    {
      get
      {
        return (string) this.GetValue(ServiceOptionUC.StateProperty);
      }
      set
      {
        this.SetValue(ServiceOptionUC.StateProperty, (object) value);
      }
    }

    public ServiceOptionUC()
    {
      this.InitializeComponent();
    }

    private static void IconPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      ServiceOptionUC serviceOptionUc = (ServiceOptionUC) sender;
      ImageBrush imageBrush = new ImageBrush();
      ImageLoader.SetImageBrushMultiResSource(imageBrush, (string) e.NewValue);
      serviceOptionUc.IconBorder.OpacityMask = (Brush) imageBrush;
    }

    private static void TitlePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      ((ServiceOptionUC) sender).TitleBlock.Text = (string) e.NewValue;
    }

    private static void StatePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      ((ServiceOptionUC) sender).StateBlock.Text = (string) e.NewValue;
    }
  }
}
