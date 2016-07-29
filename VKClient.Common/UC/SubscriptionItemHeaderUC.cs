using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class SubscriptionItemHeaderUC : UserControl
  {
    private bool _contentLoaded;

    public SubscriptionItemHeaderUC()
    {
      this.InitializeComponent();
    }

    private void Item_OnTap(object sender, GestureEventArgs e)
    {
      SubscriptionItemHeader subscriptionItemHeader = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (subscriptionItemHeader == null || subscriptionItemHeader.TapAction == null)
        return;
      subscriptionItemHeader.TapAction();
    }

    private void Subscribe_OnTap(object sender, GestureEventArgs e)
    {
      SubscriptionItemHeader subscriptionItemHeader = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (subscriptionItemHeader == null || subscriptionItemHeader.SubscribeAction == null)
        return;
      subscriptionItemHeader.SubscribeAction();
    }

    private void Unsubscribe_OnTap(object sender, GestureEventArgs e)
    {
      SubscriptionItemHeader subscriptionItemHeader = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (subscriptionItemHeader == null || subscriptionItemHeader.UnsubscribeAction == null)
        return;
      subscriptionItemHeader.UnsubscribeAction();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/SubscriptionItemHeaderUC.xaml", UriKind.Relative));
    }
  }
}
