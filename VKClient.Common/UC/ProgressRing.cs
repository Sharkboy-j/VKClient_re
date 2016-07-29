using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ProgressRing : Control
  {
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof (bool), typeof (ProgressRing), new PropertyMetadata((object) false, new PropertyChangedCallback(ProgressRing.IsActiveChanged)));
    public static readonly DependencyProperty TemplateSettingsProperty = DependencyProperty.Register("TemplateSettings", typeof (TemplateSettingValues), typeof (ProgressRing), new PropertyMetadata((object) new TemplateSettingValues(100.0, 10)));
    public static readonly DependencyProperty EllipseDiameterFactorProperty = DependencyProperty.Register("EllipseDiameterFactor", typeof (int), typeof (ProgressRing), new PropertyMetadata((object) 10));
    private bool _hasAppliedTemplate;
    private const int DEFAULT_ELLIPSE_DIAMETER_FACTOR = 10;

    public bool IsActive
    {
      get
      {
        return (bool) this.GetValue(ProgressRing.IsActiveProperty);
      }
      set
      {
        this.SetValue(ProgressRing.IsActiveProperty, (object) value);
      }
    }

    public TemplateSettingValues TemplateSettings
    {
      get
      {
        return (TemplateSettingValues) this.GetValue(ProgressRing.TemplateSettingsProperty);
      }
      set
      {
        this.SetValue(ProgressRing.TemplateSettingsProperty, (object) value);
      }
    }

    public int EllipseDiameterFactor
    {
      get
      {
        return (int) this.GetValue(ProgressRing.EllipseDiameterFactorProperty);
      }
      set
      {
        this.SetValue(ProgressRing.EllipseDiameterFactorProperty, (object) value);
      }
    }

    public ProgressRing()
    {
      this.DefaultStyleKey = (object) typeof (ProgressRing);
      this.TemplateSettings = new TemplateSettingValues(60.0, this.EllipseDiameterFactor);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this._hasAppliedTemplate = true;
      this.UpdateState(this.IsActive);
    }

    private void UpdateState(bool isActive)
    {
      if (!this._hasAppliedTemplate)
        return;
      VisualStateManager.GoToState((Control) this, isActive ? "Active" : "Inactive", true);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      double width = 100.0;
      if (!DesignerProperties.IsInDesignTool)
        width = this.Width != double.NaN ? this.Width : availableSize.Width;
      this.TemplateSettings = new TemplateSettingValues(width, this.EllipseDiameterFactor);
      return base.MeasureOverride(availableSize);
    }

    private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      ((ProgressRing) d).UpdateState((bool) args.NewValue);
    }
  }
}
