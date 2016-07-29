using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class MapAttachmentPage : PageBase
  {
    private readonly MapLayer _mapLayerPushpin = new MapLayer();
    private readonly MapOverlay _mapOverlayPushpin = new MapOverlay()
    {
      PositionOrigin = new Point(0.5, 1.0)
    };
    private readonly GeoCoordinateWatcher _watcher = new GeoCoordinateWatcher();
    private bool _isInitialized;
    private bool _shouldPick;
    private Image _pinImage;
    private GeoCoordinate _lastPosition;
    internal ProgressIndicator progressIndicator;
    internal Map map;
    private bool _contentLoaded;

    public MapAttachmentPage()
    {
      try
      {
        this.InitializeComponent();
        Image image = new Image();
        double num1 = 44.0;
        image.Height = num1;
        double num2 = 28.0;
        image.Width = num2;
        this._pinImage = image;
        MultiResImageLoader.SetUriSource(this._pinImage, "Resources/MapPin.png");
        this._watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(this._watcher_PositionChanged);
        this._watcher.StatusChanged += (EventHandler<GeoPositionStatusChangedEventArgs>) ((sender, args) =>
        {
          if (args.Status != GeoPositionStatus.Disabled)
            return;
          GeolocationHelper.HandleDisabledLocationSettings();
        });
      }
      catch (Exception ex)
      {
        Logger.Instance.ErrorAndSaveToIso("Failed to create MapAttachmentPage", ex);
      }
    }

    private void Map_OnLoaded(object sender, RoutedEventArgs e)
    {
      MapsSettings.ApplicationContext.ApplicationId = "55677f7c-3dab-4a57-95b2-4efd44a0e692";
      MapsSettings.ApplicationContext.AuthenticationToken = "1jh4FPILRSo9J1ADKx2CgA";
      this._mapLayerPushpin.Add(this._mapOverlayPushpin);
      this.map.Layers.Add(this._mapLayerPushpin);
    }

    private void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
    {
      Logger.Instance.Info("WatcherPositionChanged Enter");
      this.MoveMapToPosition(e.Position.Location);
      this._watcher.Stop();
      this.SetProgressIndicator(false);
      Logger.Instance.Info("WatcherPositionChanged Exit");
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      try
      {
        base.HandleOnNavigatedTo(e);
        if (this._isInitialized)
          return;
        this.DataContext = (object) new ViewModelBase();
        this._shouldPick = this.NavigationContext.QueryString.ContainsKey("Pick") && this.NavigationContext.QueryString["Pick"] == true.ToString();
        if (!this._shouldPick)
        {
          this.MoveMapToPosition(new GeoCoordinate(double.Parse(this.NavigationContext.QueryString["latitude"], (IFormatProvider) CultureInfo.InvariantCulture), double.Parse(this.NavigationContext.QueryString["longitude"], (IFormatProvider) CultureInfo.InvariantCulture)));
        }
        else
        {
          try
          {
            if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked || !AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
            {
              bool flag = MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
              AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
              AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
            }
            if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
              return;
            this._watcher.Start();
            this.SetProgressIndicator(true);
          }
          catch (Exception ex)
          {
            if (ex.HResult == -2147467260)
              GeolocationHelper.HandleDisabledLocationSettings();
          }
        }
        this.InitializeAppBar();
        this._isInitialized = true;
      }
      catch (Exception ex)
      {
        Logger.Instance.ErrorAndSaveToIso("Failed to OnNavigatedTo MapAttachmentPage", ex);
      }
    }

    protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      Logger.Instance.Info("MapAttachmentHandleOnNavigatingFrom Enter");
      base.HandleOnNavigatingFrom(e);
      ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
      {
        try
        {
          this._watcher.Stop();
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("failed to stop watcher", ex);
        }
      }));
      Logger.Instance.Info("MapAttachmentHandleOnNavigatingFrom Exit");
    }

    private void SetProgressIndicator(bool show)
    {
      this.progressIndicator.IsVisible = show;
      this.progressIndicator.IsIndeterminate = show;
    }

    private void MoveMapToPosition(GeoCoordinate geoCoordinate)
    {
      try
      {
        this.map.Center = geoCoordinate;
        this.map.ZoomLevel = 16.0;
        this._mapOverlayPushpin.Content = (object) this._pinImage;
        this._mapOverlayPushpin.GeoCoordinate = geoCoordinate;
        this._lastPosition = geoCoordinate;
      }
      catch (Exception ex)
      {
        Logger.Instance.ErrorAndSaveToIso("Failed to MoveMapToPosition MapAttachmentPage", ex);
      }
    }

    private void InitializeAppBar()
    {
      if (this._shouldPick)
      {
        ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton()
        {
          IconUri = new Uri("/Resources/check.png", UriKind.Relative),
          Text = CommonResources.Conversation_AppBar_AttachLocation
        };
        applicationBarIconButton1.Click += new EventHandler(this._appBarButtonSave_Click);
        ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton()
        {
          IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
          Text = CommonResources.ChatEdit_AppBar_Cancel
        };
        applicationBarIconButton2.Click += new EventHandler(this._appBarButtonCancel_Click);
        ApplicationBar applicationBar = new ApplicationBar()
        {
          BackgroundColor = VKConstants.AppBarBGColor,
          ForegroundColor = VKConstants.AppBarFGColor
        };
        applicationBar.Buttons.Add((object) applicationBarIconButton1);
        applicationBar.Buttons.Add((object) applicationBarIconButton2);
        this.ApplicationBar = (IApplicationBar) applicationBar;
      }
      else
        this.ApplicationBar = (IApplicationBar) null;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      this.NavigationService.GoBackSafe();
    }

    private void _appBarButtonSave_Click(object sender, EventArgs e)
    {
      ParametersRepository.SetParameterForId("NewPositionToBeAttached", (object) this._lastPosition);
      this.NavigationService.GoBackSafe();
    }

    private void Map_OnTap(object sender, GestureEventArgs e)
    {
      if (!this._shouldPick)
        return;
      GeoCoordinate geoCoordinate = this.map.ConvertViewportPointToGeoCoordinate(e.GetPosition((UIElement) this.map));
      this._mapOverlayPushpin.Content = (object) this._pinImage;
      this._mapOverlayPushpin.GeoCoordinate = geoCoordinate;
      this._lastPosition = geoCoordinate;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/MapAttachmentPage.xaml", UriKind.Relative));
      this.progressIndicator = (ProgressIndicator) this.FindName("progressIndicator");
      this.map = (Map) this.FindName("map");
    }
  }
}
