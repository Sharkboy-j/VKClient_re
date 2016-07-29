using System;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKMessenger.Framework
{
  public class InAppToastNotification
  {
    public static void Show(string title, string message, Action callback, string imageSrc)
    {
      if (string.IsNullOrEmpty(message))
        return;
      Execute.ExecuteOnUIThread((Action) (() => AppNotificationUC.Instance.ShowAndHideLater(imageSrc, title, message, callback, null)));
    }
  }
}
