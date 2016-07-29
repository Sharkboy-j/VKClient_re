using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class BirthdaysPage : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector listBoxBirthdays;
    internal GenericHeaderUC Header;
    private bool _contentLoaded;

    public BirthdaysPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      BirthdaysViewModel birthdaysViewModel = new BirthdaysViewModel();
      this.DataContext = (object) birthdaysViewModel;
      birthdaysViewModel.BithdaysGroupsViewModel.LoadData(false, false, (Action<BackendResult<List<User>, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void Grid_Tap(object sender, GestureEventArgs e)
    {
      BirthdayInfo birthdayInfo = (sender as FrameworkElement).DataContext as BirthdayInfo;
      Navigator.Current.NavigateToUserProfile(birthdayInfo.friend.uid, birthdayInfo.friend.Name, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/BirthdaysPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.listBoxBirthdays = (ExtendedLongListSelector) this.FindName("listBoxBirthdays");
      this.Header = (GenericHeaderUC) this.FindName("Header");
    }
  }
}
