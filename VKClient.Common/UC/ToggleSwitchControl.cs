using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace VKClient.Common.UC
{
  [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
  [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
  [TemplateVisualState(GroupName = "CheckStates", Name = "Checked")]
  [TemplateVisualState(GroupName = "CheckStates", Name = "Unchecked")]
  [TemplatePart(Name = "SwitchRoot", Type = typeof (Grid))]
  [TemplatePart(Name = "BorderSwitchForeground", Type = typeof (Border))]
  [TemplatePart(Name = "SwitchTrack", Type = typeof (Grid))]
  [TemplatePart(Name = "SwitchThumb", Type = typeof (FrameworkElement))]
  public class ToggleSwitchControl : ToggleButton
  {
    public static readonly DependencyProperty SwitchBackgroundProperty = DependencyProperty.Register("SwitchBackground", typeof (Brush), typeof (ToggleSwitchControl), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty SwitchForegroundProperty = DependencyProperty.Register("SwitchForeground", typeof (Brush), typeof (ToggleSwitchControl), new PropertyMetadata((PropertyChangedCallback) null));
    private const string CommonStates = "CommonStates";
    private const string NormalState = "Normal";
    private const string DisabledState = "Disabled";
    private const string CheckStates = "CheckStates";
    private const string CheckedState = "Checked";
    private const string UncheckedState = "Unchecked";
    private const string SwitchRootPart = "SwitchRoot";
    private const string SwitchBorderForeground = "BorderSwitchForeground";
    private const string SwitchTrackPart = "SwitchTrack";
    private const string SwitchThumbPart = "SwitchThumb";
    private TranslateTransform _thumbTranslation;
    private Grid _root;
    private Border _borderForeground;
    private Grid _track;
    private FrameworkElement _thumb;
    private const double _uncheckedTranslation = 0.0;
    private double _checkedTranslation;
    private double _dragTranslation;
    private bool _wasDragged;

    public Brush SwitchBackground
    {
      get
      {
        return (Brush) this.GetValue(ToggleSwitchControl.SwitchBackgroundProperty);
      }
      set
      {
        this.SetValue(ToggleSwitchControl.SwitchBackgroundProperty, (object) value);
      }
    }

    public Brush SwitchForeground
    {
      get
      {
        return (Brush) this.GetValue(ToggleSwitchControl.SwitchForegroundProperty);
      }
      set
      {
        this.SetValue(ToggleSwitchControl.SwitchForegroundProperty, (object) value);
      }
    }

    private double Translation
    {
      get
      {
        return this._thumbTranslation.X;
      }
      set
      {
        if (this._thumbTranslation != null)
          this._thumbTranslation.X = value;
        if (this._borderForeground == null || this._checkedTranslation == 0.0)
          return;
        this._borderForeground.Opacity = value / this._checkedTranslation;
      }
    }

    public ToggleSwitchControl()
    {
      this.DefaultStyleKey = (object) typeof (ToggleSwitchControl);
    }

    private void ChangeVisualState(bool useTransitions)
    {
      bool? isChecked = this.IsChecked;
      if ((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        VisualStateManager.GoToState((Control) this, "Checked", useTransitions);
      else
        VisualStateManager.GoToState((Control) this, "Unchecked", useTransitions);
    }

    protected override void OnToggle()
    {
      bool? isChecked = this.IsChecked;
      this.IsChecked = new bool?((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 0) == 0);
      this.ChangeVisualState(true);
    }

    public override void OnApplyTemplate()
    {
      if (this._track != null)
        this._track.SizeChanged -= new SizeChangedEventHandler(this.OnSizeChanged);
      if (this._thumb != null)
        this._thumb.SizeChanged -= new SizeChangedEventHandler(this.OnSizeChanged);
      if (this._root != null)
      {
        this._root.ManipulationStarted -= new EventHandler<ManipulationStartedEventArgs>(this.OnManipulationStarted);
        this._root.ManipulationDelta -= new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta);
        this._root.ManipulationCompleted -= new EventHandler<ManipulationCompletedEventArgs>(this.OnManipulationCompleted);
      }
      base.OnApplyTemplate();
      this._root = this.GetTemplateChild("SwitchRoot") as Grid;
      this._borderForeground = this.GetTemplateChild("BorderSwitchForeground") as Border;
      this._track = this.GetTemplateChild("SwitchTrack") as Grid;
      this._thumb = this.GetTemplateChild("SwitchThumb") as FrameworkElement;
      FrameworkElement frameworkElement = this._thumb;
      this._thumbTranslation = (frameworkElement != null ? frameworkElement.RenderTransform : (Transform) null) as TranslateTransform;
      if (this._root != null && this._track != null && (this._thumb != null && this._thumbTranslation != null))
      {
        this._root.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this.OnManipulationStarted);
        this._root.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta);
        this._root.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.OnManipulationCompleted);
        this._track.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
        this._thumb.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      }
      this.ChangeVisualState(false);
    }

    private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
      this._dragTranslation = this.Translation;
      this.Translation = this._dragTranslation;
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
      double x = e.DeltaManipulation.Translation.X;
      if ((Math.Abs(x) >= Math.Abs(e.DeltaManipulation.Translation.Y) ? 1 : 0) != 1 || x == 0.0)
        return;
      this._wasDragged = true;
      this._dragTranslation = this._dragTranslation + x;
      this.Translation = Math.Max(0.0, Math.Min(this._checkedTranslation, this._dragTranslation));
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      bool flag = false;
      if (this._wasDragged)
      {
        bool? isChecked = this.IsChecked;
        if (this.Translation != ((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 0) != 0 ? this._checkedTranslation : 0.0))
          flag = true;
      }
      else
        flag = true;
      if (flag)
        this.OnClick();
      this._wasDragged = false;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._track.Clip = (Geometry) new RectangleGeometry()
      {
        Rect = new Rect(0.0, 0.0, this._track.ActualWidth, this._track.ActualHeight)
      };
      this._checkedTranslation = this._track.ActualWidth - this._thumb.ActualWidth - this._thumb.Margin.Left - this._thumb.Margin.Right;
    }
  }
}
