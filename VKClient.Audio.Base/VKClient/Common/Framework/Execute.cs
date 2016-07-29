using System;
using System.Windows;

namespace VKClient.Common.Framework
{
  public class Execute
  {
    public static void ExecuteOnUIThread(Action action)
    {
      if (Deployment.Current.Dispatcher.CheckAccess())
        action();
      else
        Deployment.Current.Dispatcher.BeginInvoke(action);
    }
  }
}
