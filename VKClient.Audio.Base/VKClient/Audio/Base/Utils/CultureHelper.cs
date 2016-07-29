using System.Threading;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.Utils
{
  public static class CultureHelper
  {
    public static CultureName GetCurrent()
    {
      string str = ThemeSettingsManager.GetThemeSettings().LanguageCultureString;
      if (str == "")
        str = Thread.CurrentThread.CurrentUICulture.ToString();
      if (str.StartsWith("en"))
        return CultureName.EN;
      if (str.StartsWith("ru"))
        return CultureName.RU;
      if (str.StartsWith("uk"))
        return CultureName.UK;
      if (str.StartsWith("be"))
        return CultureName.BE;
      if (str.StartsWith("pt"))
        return CultureName.PT;
      return str.StartsWith("kk") ? CultureName.KZ : CultureName.NONE;
    }
  }
}
