using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class CareerItemInfoUC : UserControl
  {
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof (string), typeof (CareerItemInfoUC), new PropertyMetadata(new PropertyChangedCallback(CareerItemInfoUC.OnDescriptionChanged)));
    public static readonly DependencyProperty GroupImageProperty = DependencyProperty.Register("GroupImage", typeof (string), typeof (CareerItemInfoUC), new PropertyMetadata(new PropertyChangedCallback(CareerItemInfoUC.OnGroupImageChanged)));
    internal TextBlock textBlockDescription;
    internal Ellipse imageGroupPlaceholder;
    internal Image imageGroup;
    private bool _contentLoaded;

    public string Description
    {
      get
      {
        return (string) this.GetValue(CareerItemInfoUC.DescriptionProperty);
      }
      set
      {
        this.SetValue(CareerItemInfoUC.DescriptionProperty, (object) value);
      }
    }

    public string GroupImage
    {
      get
      {
        return (string) this.GetValue(CareerItemInfoUC.GroupImageProperty);
      }
      set
      {
        this.SetValue(CareerItemInfoUC.GroupImageProperty, (object) value);
      }
    }

    public CareerItemInfoUC()
    {
      this.InitializeComponent();
      this.imageGroupPlaceholder.Visibility = Visibility.Collapsed;
      this.imageGroup.Visibility = Visibility.Collapsed;
      this.textBlockDescription.Text = "";
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      CareerItemInfoUC careerItemInfoUc = d as CareerItemInfoUC;
      if (careerItemInfoUc == null)
        return;
      string str = e.NewValue as string;
      careerItemInfoUc.textBlockDescription.Text = str;
    }

    private static void OnGroupImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      CareerItemInfoUC careerItemInfoUc = d as CareerItemInfoUC;
      if (careerItemInfoUc == null)
        return;
      string str = e.NewValue as string;
      if (!string.IsNullOrEmpty(str))
      {
        ImageLoader.SetUriSource(careerItemInfoUc.imageGroup, str);
        careerItemInfoUc.imageGroupPlaceholder.Visibility = Visibility.Visible;
        careerItemInfoUc.imageGroup.Visibility = Visibility.Visible;
      }
      else
      {
        ImageLoader.SetUriSource(careerItemInfoUc.imageGroup, "");
        careerItemInfoUc.imageGroupPlaceholder.Visibility = Visibility.Collapsed;
        careerItemInfoUc.imageGroup.Visibility = Visibility.Collapsed;
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/CareerItemInfoUC.xaml", UriKind.Relative));
      this.textBlockDescription = (TextBlock) this.FindName("textBlockDescription");
      this.imageGroupPlaceholder = (Ellipse) this.FindName("imageGroupPlaceholder");
      this.imageGroup = (Image) this.FindName("imageGroup");
    }
  }
}
