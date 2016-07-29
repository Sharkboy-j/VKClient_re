using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class SettingsNotifications : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Grid ContentPanel;
    private bool _contentLoaded;

    private SettingsNotificationsViewModel VM
    {
      get
      {
        return this.DataContext as SettingsNotificationsViewModel;
      }
    }

    public SettingsNotifications()
    {
      this.InitializeComponent();
      this.Header.textBlockTitle.Text = CommonResources.NewSettings_Notifications.ToUpperInvariant();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this.DataContext = (object) new SettingsNotificationsViewModel();
      this._isInitialized = true;
    }

    private void DoNotDisturbClick(object sender, RoutedEventArgs e)
    {
      PickerUC.ShowPickerFor(new ObservableCollection<PickableItem>(SettingsNotificationsViewModel.DoNotDisturbOptions), (PickableItem) null, (Action<PickableItem>) (pi => this.VM.Disable((int) (pi.ID * 3600L))), (Action<PickableItem>) null, null, CommonResources.Settings_Notifications_DoNotDisturb.ToUpperInvariant());
    }

    private void ConfigureNewsSourcesTap(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToManageSources(ManageSourcesMode.ManagePushNotificationsSources);
    }

    private void CancelDNDClick(object sender, RoutedEventArgs e)
    {
      this.VM.Disable(0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SettingsNotifications.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
    }
  }
}
