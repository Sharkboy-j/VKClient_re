using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class FriendsSearchUC : UserControl
  {
    //private bool _isInitialized;
    internal ExtendedLongListSelector listBoxUsers;
    private bool _contentLoaded;

    public FriendsSearchUC()
    {
      this.InitializeComponent();
    }

    private void UsersList_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      (this.DataContext as FriendsSearchViewModel).SearchVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void FriendsProvider_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SubscriptionItemHeader subscriptionItemHeader = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (subscriptionItemHeader == null || subscriptionItemHeader.TapAction == null)
        return;
      subscriptionItemHeader.TapAction();
    }

    public void ScrollToTop()
    {
      this.listBoxUsers.ScrollToTop();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/FriendsSearchUC.xaml", UriKind.Relative));
      this.listBoxUsers = (ExtendedLongListSelector) this.FindName("listBoxUsers");
    }
  }
}
