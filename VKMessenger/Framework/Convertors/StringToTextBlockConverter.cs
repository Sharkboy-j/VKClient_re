using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using VKMessenger.Views;

namespace VKMessenger.Framework.Convertors
{
  public class StringToTextBlockConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) new TextBlock();
      string str1 = value as string;
      TextBlock textBlock = new TextBlock();
      textBlock.Style = (Style) Application.Current.Resources[(object) (string) parameter];
      textBlock.TextWrapping = TextWrapping.NoWrap;
      SolidColorBrush solidColorBrush1 = new SolidColorBrush((Color) Application.Current.Resources["PhoneAccentColor"]);
      char[] chArray = new char[1]{ ' ' };
      foreach (string str2 in ((IEnumerable<string>) str1.Split(chArray)).Where<string>((Func<string, bool>) (s => !string.IsNullOrWhiteSpace(s))))
      {
        bool flag = false;
        IList<string> stringList = (IList<string>) new List<string>();
        if (MessengerStateManagerInstance.Current.RootFrame.Content is ConversationsSearch)
          stringList = (IList<string>) MessengerStateManagerInstance.Current.ConversationSearchStrings;
        foreach (string str3 in (IEnumerable<string>) stringList)
        {
          if (str2.StartsWith(str3, StringComparison.CurrentCultureIgnoreCase))
          {
            InlineCollection inlines = textBlock.Inlines;
            Run run = new Run();
            run.Text = str2.Substring(0, str3.Length);
            SolidColorBrush solidColorBrush2 = solidColorBrush1;
            run.Foreground = (Brush) solidColorBrush2;
            inlines.Add((Inline) run);
            textBlock.Inlines.Add((Inline) new Run()
            {
              Text = str2.Substring(str3.Length)
            });
            flag = true;
            break;
          }
        }
        if (!flag)
          textBlock.Inlines.Add((Inline) new Run() { Text = str2 });
        textBlock.Inlines.Add((Inline) new Run() { Text = " " });
      }
      return (object) textBlock;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException("This converter cannot be used in two-way binding.");
    }
  }
}
