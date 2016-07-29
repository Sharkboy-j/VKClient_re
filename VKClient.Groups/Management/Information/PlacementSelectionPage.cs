using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information
{
    public partial class PlacementSelectionPage : PageBase
  {
    private bool _canGetPlacement = true;
    private readonly MapOverlay _mapOverlayPushpin = new MapOverlay()
    {
      PositionOrigin = new Point(0.5, 1.0)
    };
    private bool _isInitialized;

    public PlacementSelectionViewModel ViewModel
    {
      get
      {
        return this.DataContext as PlacementSelectionViewModel;
      }
    }

    public PlacementSelectionPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      PlacementSelectionViewModel viewModel = new PlacementSelectionViewModel(long.Parse(this.NavigationContext.QueryString["CommunityId"]), (Place) ParametersRepository.GetParameterForIdAndReset("PlacementSelectionPlace"));
      this.DataContext = (object) viewModel;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      applicationBarIconButton1.IconUri = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.Text = CommonResources.AppBarMenu_Save;
      int num = this.ViewModel.GeoCoordinate != (GeoCoordinate) null ? 1 : 0;
      applicationBarIconButton1.IsEnabled = num != 0;
      ApplicationBarIconButton appBarButtonSave = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton()
      {
        IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
        Text = CommonResources.AppBar_Cancel
      };
      appBarButtonSave.Click += (EventHandler) ((p, f) =>
      {
        this.Focus();
        viewModel.SaveChanges();
      });
      applicationBarIconButton2.Click += (EventHandler) ((p, f) => Navigator.Current.GoBack());
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = viewModel.IsFormEnabled && viewModel.GeoCoordinate != (GeoCoordinate) null);
      this.ApplicationBar.Buttons.Add((object) appBarButtonSave);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton2);
      try
      {
        if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked || !AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
        {
          bool flag = MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
          AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
          AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
        }
        if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
          this._canGetPlacement = false;
      }
      catch (Exception ex)
      {
        if (ex.HResult == -2147467260)
        {
          GeolocationHelper.HandleDisabledLocationSettings();
          this._canGetPlacement = false;
        }
      }
      this._isInitialized = true;
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.Focus();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void CountryPicker_OnClicked(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      this.ViewModel.ChooseCountry();
    }

    private void CityPicker_OnClicked(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      this.ViewModel.ChooseCity();
    }

    private void Map_OnLoaded(object sender, RoutedEventArgs e)
    {
      MapsSettings.ApplicationContext.ApplicationId = "55677f7c-3dab-4a57-95b2-4efd44a0e692";
      MapsSettings.ApplicationContext.AuthenticationToken = "1jh4FPILRSo9J1ADKx2CgA";
      MapLayer mapLayer = new MapLayer();
      MapOverlay mapOverlay = this._mapOverlayPushpin;
      mapLayer.Add(mapOverlay);
      this.Map.Layers.Add(mapLayer);
      if (this.ViewModel.GeoCoordinate != (GeoCoordinate) null)
      {
        this.SetPushpin(this.ViewModel.GeoCoordinate, true);
      }
      else
      {
        if (!this._canGetPlacement)
          return;
        try
        {
          GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
          watcher.PositionChanged += (EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>) ((o, args) =>
          {
            if (!(this.ViewModel.GeoCoordinate == (GeoCoordinate) null))
              return;
            PageBase.SetInProgress(false);
            this.SetPushpin(args.Position.Location, true);
            watcher.Stop();
          });
          watcher.StatusChanged += (EventHandler<GeoPositionStatusChangedEventArgs>) ((o, args) =>
          {
            if (args.Status != GeoPositionStatus.Disabled)
              return;
            GeolocationHelper.HandleDisabledLocationSettings();
          });
          watcher.Start();
          PageBase.SetInProgress(true);
        }
        catch
        {
        }
      }
    }

    private void Map_OnTapped(object sender, GestureEventArgs e)
    {
      if (!this.ViewModel.IsFormEnabled)
        return;
      this.SetPushpin(this.Map.ConvertViewportPointToGeoCoordinate(e.GetPosition((UIElement) this.Map)), false);
      PageBase.SetInProgress(false);
    }

    private void SetPushpin(GeoCoordinate geoCoordinate, bool needCenter)
    {
      Image image1 = new Image();
      double num1 = 44.0;
      image1.Height = num1;
      double num2 = 28.0;
      image1.Width = num2;
      Image image2 = image1;
      MultiResImageLoader.SetUriSource(image2, "/Resources/MapPin.png");
      this._mapOverlayPushpin.Content = (object) image2;
      this._mapOverlayPushpin.GeoCoordinate = geoCoordinate;
      this.ViewModel.GeoCoordinate = geoCoordinate;
      if (!needCenter)
        return;
      this.Map.Center = geoCoordinate;
      this.Map.ZoomLevel = 12.0;
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      this.Viewer.ScrollToOffsetWithAnimation(((UIElement) sender).GetRelativePosition((UIElement) this.ViewerContent).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = false;
    }

  }
}
