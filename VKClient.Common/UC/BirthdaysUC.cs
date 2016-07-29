using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class BirthdaysUC : UserControl
  {
    internal Grid LayoutRoot;
    internal Grid gridHeader;
    private bool _contentLoaded;

    public BirthdaysUC()
    {
      this.InitializeComponent();
      this.DataContext = (object) new UpcomingBirthdaysViewModel();
    }

    private void HeaderTap(object sender, GestureEventArgs e)
    {
      MenuUC.Instance.NavigateToBirthdays(false);
    }

    private void HeaderHold(object sender, GestureEventArgs e)
    {
      MenuUC.Instance.NavigateToBirthdays(true);
    }

    private void Grid_Tap(object sender, GestureEventArgs e)
    {
      BirthdayInfo birthdayInfo = (sender as FrameworkElement).DataContext as BirthdayInfo;
      MenuUC.Instance.NavigateToUserProfile(birthdayInfo.friend.uid, birthdayInfo.friend.Name, false);
    }

    private void Grid_Hold(object sender, GestureEventArgs e)
    {
      BirthdayInfo birthdayInfo = (sender as FrameworkElement).DataContext as BirthdayInfo;
      MenuUC.Instance.NavigateToUserProfile(birthdayInfo.friend.uid, birthdayInfo.friend.Name, true);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/BirthdaysUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.gridHeader = (Grid) this.FindName("gridHeader");
    }
  }
}
