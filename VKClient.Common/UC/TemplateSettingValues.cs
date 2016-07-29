using System.Windows;

namespace VKClient.Common.UC
{
  public class TemplateSettingValues : DependencyObject
  {
    public static readonly DependencyProperty MaxSideLengthProperty = DependencyProperty.Register("MaxSideLength", typeof (double), typeof (TemplateSettingValues), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register("EllipseDiameter", typeof (double), typeof (TemplateSettingValues), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty EllipseOffsetProperty = DependencyProperty.Register("EllipseOffset", typeof (Thickness), typeof (TemplateSettingValues), new PropertyMetadata((object) new Thickness()));

    public double MaxSideLength
    {
      get
      {
        return (double) this.GetValue(TemplateSettingValues.MaxSideLengthProperty);
      }
      set
      {
        this.SetValue(TemplateSettingValues.MaxSideLengthProperty, (object) value);
      }
    }

    public double EllipseDiameter
    {
      get
      {
        return (double) this.GetValue(TemplateSettingValues.EllipseDiameterProperty);
      }
      set
      {
        this.SetValue(TemplateSettingValues.EllipseDiameterProperty, (object) value);
      }
    }

    public Thickness EllipseOffset
    {
      get
      {
        return (Thickness) this.GetValue(TemplateSettingValues.EllipseOffsetProperty);
      }
      set
      {
        this.SetValue(TemplateSettingValues.EllipseOffsetProperty, (object) value);
      }
    }

    public TemplateSettingValues(double width, int ellipseDiameterFactor)
      : this()
    {
      this.EllipseDiameter = width / (ellipseDiameterFactor != 0 ? (double) ellipseDiameterFactor : 1.0);
      this.EllipseOffset = new Thickness(this.EllipseDiameter);
    }

    public TemplateSettingValues()
    {
      this.MaxSideLength = 400.0;
    }
  }
}
