using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VKMessenger.Framework
{
  public class BoolToStyleConverter : IValueConverter
  {
    public Style TrueStyle { get; set; }

    public Style FalseStyle { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(bool) value)
        return (object) this.FalseStyle;
      return (object) this.TrueStyle;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
