using System;
using System.Globalization;
using System.Windows.Data;
using VKClient.Common.Localization;

namespace VKMessenger.Framework.Convertors
{
    public class BoolToSwitchConverter : IValueConverter
    {
        private string FalseValue = CommonResources.Settings_Disabled;
        private string TrueValue = CommonResources.Settings_Enabled;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return this.FalseValue;
            if (!(bool)value)
                return this.FalseValue;
            return this.TrueValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)(value != null ? (value.Equals((object)this.TrueValue) ? true : false) : false);
        }
    }
}
