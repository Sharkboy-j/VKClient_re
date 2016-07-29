using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.Extensions;

namespace VKClient.Common.UC
{
  public class ToggleControl : UserControl
  {
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof (bool), typeof (ToggleControl), new PropertyMetadata(new PropertyChangedCallback(ToggleControl.IsChecked_OnChanged)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ToggleControl), new PropertyMetadata(new PropertyChangedCallback(ToggleControl.Title_OnChanged)));
    internal QuadraticEase EasingFunc;
    internal Storyboard AnimateChecked;
    internal Storyboard AnimateUnchecked;
    internal TextBlock textBlockTitle;
    internal ToggleSwitchControl controlToggleSwitch;
    private bool _contentLoaded;

    public bool IsChecked
    {
      get
      {
        return (bool) this.GetValue(ToggleControl.IsCheckedProperty);
      }
      set
      {
        this.SetValue(ToggleControl.IsCheckedProperty, (object) value);
      }
    }

    public string Title
    {
      get
      {
        return (string) this.GetValue(ToggleControl.TitleProperty);
      }
      set
      {
        this.SetValue(ToggleControl.TitleProperty, (object) value);
      }
    }

    public event EventHandler<bool> CheckedUnchecked;

    public ToggleControl()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = "";
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args)
    {
      double width = args.NewSize.Width;
      if (double.IsNaN(width) || double.IsInfinity(width))
        return;
      this.textBlockTitle.Text = this.Title;
      this.textBlockTitle.CorrectText(Math.Max(0.0, width - this.controlToggleSwitch.Width));
    }

    private static void IsChecked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ToggleControl toggleControl = (ToggleControl) d;
      toggleControl.UpdateToggle();
      toggleControl.FireCheckedEvent();
    }

    private void UpdateToggle()
    {
      this.controlToggleSwitch.Checked -= new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
      this.controlToggleSwitch.Unchecked -= new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
      this.controlToggleSwitch.IsChecked = new bool?(this.IsChecked);
      this.controlToggleSwitch.Checked += new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
      this.controlToggleSwitch.Unchecked += new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
    }

    private void FireCheckedEvent()
    {
      EventHandler<bool> eventHandler = this.CheckedUnchecked;
      if (eventHandler == null)
        return;
      int num = this.IsChecked ? 1 : 0;
      eventHandler((object) this, num != 0);
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((ToggleControl) d).UpdateTitle();
    }

    private void UpdateTitle()
    {
      this.textBlockTitle.Text = this.Title;
      if (double.IsNaN(this.ActualWidth) || double.IsInfinity(this.ActualWidth))
        return;
      this.textBlockTitle.CorrectText(Math.Max(0.0, this.ActualWidth - this.controlToggleSwitch.Width));
    }

    private void BorderToggleTitle_OnTap(object sender, GestureEventArgs e)
    {
      this.IsChecked = !this.IsChecked;
    }

    private void ControlToggleSwitch_OnCheckedUnchecked(object sender, RoutedEventArgs e)
    {
      bool? isChecked = this.controlToggleSwitch.IsChecked;
      this.IsChecked = isChecked.HasValue && isChecked.GetValueOrDefault();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ToggleControl.xaml", UriKind.Relative));
      this.EasingFunc = (QuadraticEase) this.FindName("EasingFunc");
      this.AnimateChecked = (Storyboard) this.FindName("AnimateChecked");
      this.AnimateUnchecked = (Storyboard) this.FindName("AnimateUnchecked");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.controlToggleSwitch = (ToggleSwitchControl) this.FindName("controlToggleSwitch");
    }
  }
}
