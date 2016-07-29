using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKMessenger.Views
{
    public class MapAttachmentPage : PageBase
    {
        private GeoCoordinateWatcher _watcher = new GeoCoordinateWatcher();
        private bool _isInitialized;
        private bool _shouldPick;
        private CredentialsProvider _credentialsProvider;
        private GeoCoordinate _lastPosition;
        internal ProgressIndicator progressIndicator;
        internal Map map;
        internal MapLayer pushpinLayer;
        private bool _contentLoaded;

        public MapAttachmentPage()
        {
            this.InitializeComponent();
            this._credentialsProvider = (CredentialsProvider)new ApplicationIdCredentialsProvider("AsAOCzjdoO4A8lKbpU4hZzrs4piUJ0g4jQZ-FbL4AUmy_cbfoOQaqN5usCNwG0Ua");
            this.map.CredentialsProvider = this._credentialsProvider;
            this._watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(this._watcher_PositionChanged);
        }

        private void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.MoveMapToPosition(e.Position.Location);
            this._watcher.Stop();
            this.SetProgressIndicator(false);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            this._shouldPick = this.NavigationContext.QueryString.ContainsKey("Pick");
            if (!this._shouldPick)
            {
                this.MoveMapToPosition(new GeoCoordinate(double.Parse(this.NavigationContext.QueryString["latitude"], (IFormatProvider)CultureInfo.InvariantCulture), double.Parse(this.NavigationContext.QueryString["longitude"], (IFormatProvider)CultureInfo.InvariantCulture)));
            }
            else
            {
                if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked)
                {
                    bool flag = false;
                    if (MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        flag = true;
                    AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
                    AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
                }
                if (AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
                {
                    this._watcher.Start();
                    this.SetProgressIndicator(true);
                }
            }
            this.InitializeAppBar();
            this._isInitialized = true;
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.HandleOnNavigatingFrom(e);
            this._watcher.Stop();
        }

        private void SetProgressIndicator(bool show)
        {
            this.progressIndicator.IsVisible = show;
            this.progressIndicator.IsIndeterminate = show;
        }

        private void MoveMapToPosition(GeoCoordinate geoCoordinate)
        {
            this.map.AnimationLevel = AnimationLevel.UserInput;
            this.map.Center = geoCoordinate;
            this.map.ZoomLevel = 15.0;
            Image image = new Image();
            image.Source = (ImageSource)new BitmapImage(new Uri("/VKMessenger;component/Resources/Map_Pin.png", UriKind.Relative));
            image.Stretch = Stretch.None;
            PositionOrigin origin = PositionOrigin.BottomCenter;
            this._lastPosition = geoCoordinate;
            this.pushpinLayer.Children.Clear();
            this.pushpinLayer.AddChild((UIElement)image, geoCoordinate, origin);
        }

        private void InitializeAppBar()
        {
            if (this._shouldPick)
            {
                ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("./Resources/appbar.save.rest.png", UriKind.Relative),
                    Text = CommonResources.ChatEdit_AppBar_Save
                };
                applicationBarIconButton1.Click += new EventHandler(this._appBarButtonSave_Click);
                ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton()
                {
                    IconUri = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative),
                    Text = CommonResources.ChatEdit_AppBar_Cancel
                };
                applicationBarIconButton2.Click += new EventHandler(this._appBarButtonCancel_Click);
                ApplicationBar applicationBar = new ApplicationBar()
                {
                    BackgroundColor = VKConstants.AppBarBGColor,
                    ForegroundColor = VKConstants.AppBarFGColor
                };
                applicationBar.Buttons.Add((object)applicationBarIconButton1);
                applicationBar.Buttons.Add((object)applicationBarIconButton2);
                this.ApplicationBar = (IApplicationBar)applicationBar;
            }
            else
                this.ApplicationBar = null;
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBackSafe();
        }

        private void _appBarButtonSave_Click(object sender, EventArgs e)
        {
            ParametersRepository.SetParameterForId("NewPositionToBeAttached", (object)this._lastPosition);
            this.NavigationService.GoBackSafe();
        }

        private void pushpinLayer_Tap(object sender, GestureEventArgs e)
        {
            if (!this._shouldPick)
                return;
            Point position = e.GetPosition((UIElement)this.map);
            GeoCoordinate geoCoordinate = new GeoCoordinate();
            GeoCoordinate location = this.map.ViewportPointToLocation(position);
            this.pushpinLayer.Children.Clear();
            Image image = new Image();
            image.Source = (ImageSource)new BitmapImage(new Uri("/VKMessenger;component/Resources/Map_Pin.png", UriKind.Relative));
            image.Stretch = Stretch.None;
            PositionOrigin origin = PositionOrigin.BottomCenter;
            this.pushpinLayer.AddChild((UIElement)image, location, origin);
            this._lastPosition = location;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKMessenger;component/Views/MapAttachmentPage.xaml", UriKind.Relative));
            this.progressIndicator = (ProgressIndicator)this.FindName("progressIndicator");
            this.map = (Map)this.FindName("map");
            this.pushpinLayer = (MapLayer)this.FindName("pushpinLayer");
        }
    }
}
