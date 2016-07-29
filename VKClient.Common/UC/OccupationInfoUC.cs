using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class OccupationInfoUC : UserControl
  {
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof (OccupationType), typeof (OccupationInfoUC), new PropertyMetadata(new PropertyChangedCallback(OccupationInfoUC.OnTypeChanged)));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof (string), typeof (OccupationInfoUC), new PropertyMetadata(new PropertyChangedCallback(OccupationInfoUC.OnDescriptionChanged)));
    public static readonly DependencyProperty GroupImageProperty = DependencyProperty.Register("GroupImage", typeof (string), typeof (OccupationInfoUC), new PropertyMetadata(new PropertyChangedCallback(OccupationInfoUC.OnGroupImageChanged)));
    internal ScrollableTextBlock textBlockDescription;
    internal Image imageGroup;
    private bool _contentLoaded;

    public OccupationType Type
    {
      get
      {
        return (OccupationType) this.GetValue(OccupationInfoUC.TypeProperty);
      }
      set
      {
        this.SetValue(OccupationInfoUC.TypeProperty, (object) value);
      }
    }

    public string Description
    {
      get
      {
        return (string) this.GetValue(OccupationInfoUC.DescriptionProperty);
      }
      set
      {
        this.SetValue(OccupationInfoUC.DescriptionProperty, (object) value);
      }
    }

    public string GroupImage
    {
      get
      {
        return (string) this.GetValue(OccupationInfoUC.GroupImageProperty);
      }
      set
      {
        this.SetValue(OccupationInfoUC.GroupImageProperty, (object) value);
      }
    }

    public OccupationInfoUC()
    {
      this.InitializeComponent();
      this.imageGroup.Visibility = Visibility.Collapsed;
      this.textBlockDescription.Text = "";
    }

    private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      OccupationInfoUC occupationInfoUc = d as OccupationInfoUC;
      if (occupationInfoUc == null)
        return;
      string str = e.NewValue as string;
      occupationInfoUc.textBlockDescription.Text = str;
    }

    private static void OnGroupImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      OccupationInfoUC occupationInfoUc = d as OccupationInfoUC;
      if (occupationInfoUc == null)
        return;
      string str = e.NewValue as string;
      if (!string.IsNullOrEmpty(str))
      {
        ImageLoader.SetUriSource(occupationInfoUc.imageGroup, str);
        occupationInfoUc.imageGroup.Visibility = Visibility.Visible;
      }
      else
      {
        ImageLoader.SetUriSource(occupationInfoUc.imageGroup, "");
        occupationInfoUc.imageGroup.Visibility = Visibility.Collapsed;
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/OccupationInfoUC.xaml", UriKind.Relative));
      this.textBlockDescription = (ScrollableTextBlock) this.FindName("textBlockDescription");
      this.imageGroup = (Image) this.FindName("imageGroup");
    }
  }
}
