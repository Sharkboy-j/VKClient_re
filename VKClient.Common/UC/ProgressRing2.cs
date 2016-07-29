using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ProgressRing2 : Control
  {
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof (bool), typeof (ProgressRing2), new PropertyMetadata((object) false, new PropertyChangedCallback(ProgressRing2.IsActiveChanged)));
    public static readonly DependencyProperty TemplateSettingsProperty = DependencyProperty.Register("TemplateSettings", typeof (TemplateSettingValues2), typeof (ProgressRing2), new PropertyMetadata((object) new TemplateSettingValues2(8.0)));
    private bool _hasAppliedTemplate;

    public bool IsActive
    {
      get
      {
        return (bool) this.GetValue(ProgressRing2.IsActiveProperty);
      }
      set
      {
        this.SetValue(ProgressRing2.IsActiveProperty, (object) value);
      }
    }

    public TemplateSettingValues2 TemplateSettings
    {
      get
      {
        return (TemplateSettingValues2) this.GetValue(ProgressRing2.TemplateSettingsProperty);
      }
      set
      {
        this.SetValue(ProgressRing2.TemplateSettingsProperty, (object) value);
      }
    }

    public ProgressRing2()
    {
      this.DefaultStyleKey = (object) typeof (ProgressRing2);
      this.TemplateSettings = new TemplateSettingValues2(8.0);
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

    private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      ((ProgressRing2) d).UpdateState((bool) args.NewValue);
    }
  }
}
