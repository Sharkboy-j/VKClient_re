using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using Windows.System;

namespace VKClient.Common
{
  public class UsersSearchNearbyPage : PageBase
  {
    private UsersSearchNearbyViewModel _viewModel;
    internal VisualStateGroup CommonStates;
    internal VisualState Normal;
    internal VisualState Disabled;
    internal GenericHeaderUC ucHeader;
    internal ProgressRing progressRing;
    internal TextBlock textBlockDescription;
    internal TextBlock textBlockDisabled;
    internal Button buttonOpenSettings;
    internal ExtendedLongListSelector listUsers;
    private bool _contentLoaded;

    public UsersSearchNearbyPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_Nearby;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      this._viewModel = new UsersSearchNearbyViewModel();
      this.DataContext = (object) this._viewModel;
      this._viewModel.LoadGeoposition(new Action<GeoPositionStatus>(this.HandlePositionStatus));
    }

    private void HandlePositionStatus(GeoPositionStatus status)
    {
      if (status == GeoPositionStatus.Disabled)
      {
        this._viewModel.StopLoading();
        VisualStateManager.GoToState((Control) this, "Disabled", false);
      }
      else
      {
        VisualStateManager.GoToState((Control) this, "Normal", false);
        if (status != GeoPositionStatus.Ready)
          return;
        this._viewModel.StartLoading();
      }
    }

    private void ButtonOpenSettings_OnClick(object sender, RoutedEventArgs e)
    {
      Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UsersSearchNearbyPage.xaml", UriKind.Relative));
      this.CommonStates = (VisualStateGroup) this.FindName("CommonStates");
      this.Normal = (VisualState) this.FindName("Normal");
      this.Disabled = (VisualState) this.FindName("Disabled");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.progressRing = (ProgressRing) this.FindName("progressRing");
      this.textBlockDescription = (TextBlock) this.FindName("textBlockDescription");
      this.textBlockDisabled = (TextBlock) this.FindName("textBlockDisabled");
      this.buttonOpenSettings = (Button) this.FindName("buttonOpenSettings");
      this.listUsers = (ExtendedLongListSelector) this.FindName("listUsers");
    }
  }
}
