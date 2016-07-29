using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GroupHeaderUC : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.Title_OnChanged)));
    public static readonly DependencyProperty CounterProperty = DependencyProperty.Register("Counter", typeof (int), typeof (GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.Counter_OnChanged)));
    public static readonly DependencyProperty IsShowAllVisibleProperty = DependencyProperty.Register("IsShowAllVisible", typeof (bool), typeof (GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.IsShowAllVisible_OnChanged)));
    public static readonly DependencyProperty ShowAllTitleProperty = DependencyProperty.Register("ShowAllTitle", typeof (string), typeof (GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.ShowAllTitle_OnChanged)));
    public static readonly DependencyProperty IsTopSeparatorVisibleProperty = DependencyProperty.Register("IsTopSeparatorVisible", typeof (bool), typeof (GroupHeaderUC), new PropertyMetadata((object) true, new PropertyChangedCallback(GroupHeaderUC.IsTopSeparatorVisible_OnChanged)));
    internal Rectangle rectTopSeparator;
    internal Border gridContainer;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockCounter;
    internal TextBlock textBlockShowAll;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) this.GetValue(GroupHeaderUC.TitleProperty);
      }
      set
      {
        this.SetValue(GroupHeaderUC.TitleProperty, (object) value);
      }
    }

    public int Counter
    {
      get
      {
        return (int) this.GetValue(GroupHeaderUC.CounterProperty);
      }
      set
      {
        this.SetValue(GroupHeaderUC.CounterProperty, (object) value);
      }
    }

    public bool IsShowAllVisible
    {
      get
      {
        return (bool) this.GetValue(GroupHeaderUC.IsShowAllVisibleProperty);
      }
      set
      {
        this.SetValue(GroupHeaderUC.IsShowAllVisibleProperty, (object) value);
      }
    }

    public string ShowAllTitle
    {
      get
      {
        return (string) this.GetValue(GroupHeaderUC.ShowAllTitleProperty);
      }
      set
      {
        this.SetValue(GroupHeaderUC.ShowAllTitleProperty, (object) value);
      }
    }

    public bool IsTopSeparatorVisible
    {
      get
      {
        return (bool) this.GetValue(GroupHeaderUC.IsTopSeparatorVisibleProperty);
      }
      set
      {
        this.SetValue(GroupHeaderUC.IsTopSeparatorVisibleProperty, (object) value);
      }
    }

    public event RoutedEventHandler HeaderTap;

    public GroupHeaderUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = "";
      this.textBlockCounter.Text = "";
      this.textBlockShowAll.Visibility = Visibility.Collapsed;
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((GroupHeaderUC) d).textBlockTitle.Text = e.NewValue as string ?? "";
    }

    private static void Counter_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is int))
        return;
      ((GroupHeaderUC) d).textBlockCounter.Text = UIStringFormatterHelper.FormatForUIShort((long) (int) e.NewValue);
    }

    private static void IsShowAllVisible_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GroupHeaderUC groupHeaderUc = d as GroupHeaderUC;
      if (groupHeaderUc == null)
        return;
      if ((bool) e.NewValue)
      {
        MetroInMotion.SetTilt((DependencyObject) groupHeaderUc.gridContainer, 1.5);
        groupHeaderUc.textBlockShowAll.Visibility = Visibility.Visible;
      }
      else
      {
        MetroInMotion.SetTilt((DependencyObject) groupHeaderUc.gridContainer, 0.0);
        groupHeaderUc.textBlockShowAll.Visibility = Visibility.Collapsed;
      }
    }

    private static void ShowAllTitle_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((GroupHeaderUC) d).textBlockShowAll.Text = e.NewValue as string ?? "";
    }

    private static void IsTopSeparatorVisible_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((GroupHeaderUC) d).rectTopSeparator.Visibility = (bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowAll_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.HeaderTap == null || !this.IsShowAllVisible)
        return;
      this.HeaderTap((object) this, (RoutedEventArgs) e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GroupHeaderUC.xaml", UriKind.Relative));
      this.rectTopSeparator = (Rectangle) this.FindName("rectTopSeparator");
      this.gridContainer = (Border) this.FindName("gridContainer");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockCounter = (TextBlock) this.FindName("textBlockCounter");
      this.textBlockShowAll = (TextBlock) this.FindName("textBlockShowAll");
    }
  }
}
