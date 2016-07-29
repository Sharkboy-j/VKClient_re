using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Games;
using VKClient.Common.Stickers.AutoSuggest;
using VKClient.Common.Utils;
using VKClient.Common.VideoCatalog;
using VKClient.Library;
using VKClient.Video.VideoCatalog;
using VKMessenger;
using VKMessenger.Library;
using VKMessenger.Views;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace VKClient
{
    public partial class App : Application, IAppStateInfo
    {
        private static string _imageDictionaryKey = "ImageDict";
        public static TelemetryClient TelemetryClient;
        private static CustomUriMapper _uriMapper;
        private bool phoneApplicationInitialized;
        //private bool _wasReset;
        private bool _handlingPreLoginNavigation;
        private App.SessionType sessionType;
        private bool wasRelaunched;

        public PhoneApplicationFrame RootFrame { get; private set; }

        public ShareOperation ShareOperation { get; set; }

        public StartState StartState { get; private set; }

        private IsolatedStorageSettings Settings
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings;
            }
        }

        public App()
        {
            //this.InitializeTelemetry();
            Logger.Instance.Info("App() check 1");
            this.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(this.App_UnhandledException);
            this.InitializeComponent();
            Logger.Instance.Info("App() check 2");
            ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
            Logger.Instance.Info("App() check 3");
            this.ApplyThemeBasedOnSettings(themeSettings);
            Logger.Instance.Info("App() check 4");
            this.InitializePhoneApplication();
            Logger.Instance.Info("App() check 5");
            this.InitializeLanguage(themeSettings);
            Logger.Instance.Info("App() check 6");
            this.InitializeServiceLocator();
            Logger.Instance.Info("App() check 7");
            IPageDataRequesteeInfo pageDataRequestee = PageBase.CurrentPageDataRequestee;
        }

        private void InitializeTelemetry()
        {
            try
            {
                bool flag = true;
                //if (1 != 0)
                //{
                TelemetryConfiguration.Active.InstrumentationKey = "0e558d17-1207-46e2-a99d-f3224bfef5ba";
                flag = (DateTime.Now.Ticks / 10L ^ 7L) % 10L == 0L;
                //}
                //else
                //{
                //  PageViewTelemetryModule viewTelemetryModule = new PageViewTelemetryModule();
                //  viewTelemetryModule.Initialize(TelemetryConfiguration.Active);
                //  TelemetryConfiguration.Active.TelemetryModules.Add((object) viewTelemetryModule);
                //}
                if (!flag)
                    TelemetryConfiguration.Active.DisableTelemetry = true;
                TelemetryConfiguration.Active.TelemetryInitializers.Add((ITelemetryInitializer)new VKTelemetryInitializer());
                App.TelemetryClient = new TelemetryClient();
            }
            catch
            {
            }
        }

        private void InitializeLanguage(ThemeSettings settings)
        {
            try
            {
                string languageCultureString = settings.LanguageCultureString;
                if (languageCultureString != string.Empty)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCultureString);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCultureString);
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(languageCultureString);
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(languageCultureString);
                }
                AppliedSettingsInfo.AppliedLanguageSetting = settings.LanguageSettings;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("App.InitializeLanguage failed", ex);
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw;
            }
        }

        private void ApplyThemeBasedOnSettings(ThemeSettings settings)
        {
            Logger.Instance.Info("App.ApplyThemeBasedOnSettings check 1");
            ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.None;
            if (settings.BackgroundSettings == 1)
                settings.BackgroundSettings = 3;
            switch (settings.BackgroundSettings)
            {
                case 0:
                    if ((double)Application.Current.Resources["PhoneDarkThemeOpacity"] == 1.0)
                    {
                        ThemeManager.ToDarkTheme();
                        break;
                    }
                    ThemeManager.ToLightTheme();
                    break;
                case 2:
                    ThemeManager.ToDarkTheme();
                    break;
                case 3:
                    ThemeManager.ToLightTheme();
                    break;
            }
            AppliedSettingsInfo.AppliedBGSetting = settings.BackgroundSettings;
            Logger.Instance.Info("App.ApplyThemeBasedOnSettings check 2");
            Logger.Instance.Info("App.ApplyThemeBasedOnSettings check 3");
        }

        private void InitializeServiceLocator()
        {
            ServiceLocator.Register<IAppStateInfo>((IAppStateInfo)this);
            ServiceLocator.Register<IVideoCatalogItemUCFactory>((IVideoCatalogItemUCFactory) new VideoCatalogItemUCFactory());
            ServiceLocator.Register<IConversationsUCFactory>((IConversationsUCFactory)new ConversationsUCFactory());
            ServiceLocator.Register<IBackendConfirmationHandler>((IBackendConfirmationHandler)new MessageBoxBackendConfirmationHandler());
            ServiceLocator.Register<IBackendNotEnoughMoneyHandler>((IBackendNotEnoughMoneyHandler)new BackendNotEnoughMoneyHandler());
        }

        private void App_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Logger.Instance.ErrorAndSaveToIso("UNHANDLED", e.ExceptionObject);
            e.Handled = true;
            this.ReportException(e.ExceptionObject);
        }

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            this.RemoveCurrentDeactivationSettings();
            Logger.Instance.Info("App.Application_Launching check 1");
            this.PerformInitialization();
            Logger.Instance.Info("App.Application_Launching check 2");
            this.RestoreState(true);
            Logger.Instance.Info("App.Application_Launching check 3");
            PushNotificationsManager.Instance.Initialize();
            Logger.Instance.Info("App.Application_Launching check 5");
            PlaylistManager.Initialize();
            Logger.Instance.Info("App.Application_Launching check 6");
            BaseDataManager.Instance.NeedRefreshBaseData = true;
            App._uriMapper.NeedHandleActivation = true;
            ContactsManager.Instance.EnsureInSyncAsync(false);
            ContactsSyncManager.Instance.Sync(null);
            ShareLaunchingEventArgs launchingEventArgs = e as ShareLaunchingEventArgs;
            if (launchingEventArgs == null)
                return;
            this.ShareOperation = launchingEventArgs.ShareTargetActivatedEventArgs.ShareOperation;
        }

        private void PerformInitialization()
        {
            MemoryInfo.Initialize();
            Navigator.Current = (INavigator)new NavigatorImpl();
            JsonWebRequest.GetCurrentPageDataRequestee = (Func<IPageDataRequesteeInfo>)(() => PageBase.CurrentPageDataRequestee);
            BirthdaysNotificationManager.Instance.Initialize();
            TileManager.Instance.Initialize();
            TileScheduledUpdate.Instance.Initialize();
            TileManager.Instance.ResetContent();
            MessengerStateManagerInstance.Current = (IMessengerStateManager)new MessengerStateManager();
            SubscriptionFromPostManager.Instance.Restore();
            MessengerStateManagerInstance.Current.AppStartedTime = DateTime.Now;
            AudioEventTranslator.Initialize();
            IsolatedStorageSettings.ApplicationSettings["ScaleFactor"] = (object)Application.Current.Host.Content.ScaleFactor;
            BGAudioPlayerWrapper.InitializeInstance();
            InstalledPackagesFinder.Instance.Initialize();
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            Logger.Instance.Info("App.Application_Activated check 1, InstancePreserved=" + e.IsApplicationInstancePreserved.ToString());
            if (!e.IsApplicationInstancePreserved)
                this.RestoreSessionType();
            if (e.IsApplicationInstancePreserved)
            {
                this.StartState = StartState.Reactivated;
                if (!ConversationsViewModel.IsInstanceNull)
                    ConversationsViewModel.Instance.NeedRefresh = true;
                NetworkStatusInfo.Instance.RetrieveNetworkStatus();
            }
            else
            {
                this.PerformInitialization();
                this.StartState = StartState.TombstonedThenRessurected;
                this.RestoreState(false);
            }
            Logger.Instance.Info("App.Application_Activated check 3");
            PushNotificationsManager.Instance.Initialize();
            Logger.Instance.Info("App.Application_Activated check 4");
            PlaylistManager.Initialize();
            Logger.Instance.Info("App.Application_Activated check 5");
            ContactsManager.Instance.EnsureInSyncAsync(false);
            BaseDataManager.Instance.NeedRefreshBaseData = true;
            App._uriMapper.NeedHandleActivation = true;
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Logger.Instance.Info("App.Application_Deactivated check 1");
            this.SaveCurrentDeactivationSettings();
            this.RespondToDeactivationOrClose();
            Logger.Instance.Info("App.Application_Deactivated check 2");
            EventAggregator.Current.Publish((object)new ApplicationDeactivatedEvent());
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            this.RemoveCurrentDeactivationSettings();
            this.RespondToDeactivationOrClose();
        }

        private void RespondToDeactivationOrClose()
        {
            AppGlobalStateManager.Current.GlobalState.LastDeactivatedTime = DateTime.Now;
            BGAudioPlayerWrapper.Instance.RespondToAppDeactivation();
            this.SaveState();
            TileManager.Instance.ResetContent();
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Logger.Instance.ErrorAndSaveToIso("RootFrame_NavigationFailed", e.Exception);
            if (!Debugger.IsAttached)
                return;
            Debugger.Break();
        }

        private void RestoreState(bool initialLaunch)
        {
            AppGlobalStateManager.Current.Initialize(true);
            CacheManager.TryDeserialize((IBinarySerializable)ImageCache.Current, App._imageDictionaryKey, CacheManager.DataType.CachedData);
            CountersManager.Current.Restore();
            AudioCacheManager.Instance.EnsureCachingInRunning();
            ConversationsViewModelUpdatesListener.Listen();
            ConversationViewModelCache.Current.SubscribeToUpdates();
            MediaLRUCache instance = MediaLRUCache.Instance;
            StickersAutoSuggestDictionary.Instance.RestoreStateAsync();
        }

        private void SaveState()
        {
            ConversationsViewModel.Save();
            CountersManager.Current.Save();
            AppGlobalStateManager.Current.SaveState();
            CacheManager.TrySerialize((IBinarySerializable)ImageCache.Current, App._imageDictionaryKey, false, CacheManager.DataType.CachedData);
            VeryLowProfileImageLoader.SaveState();
            SubscriptionFromPostManager.Instance.Save();
            ConversationViewModelCache.Current.FlushToPersistentStorage();
            AudioCacheManager.Instance.Save();
            MediaLRUCache.Instance.Save();
            StickersAutoSuggestDictionary.Instance.SaveState();
        }

        private void InitializePhoneApplication()
        {
            if (this.phoneApplicationInitialized)
                return;
            TransitionFrame transitionFrame = new TransitionFrame();
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
            transitionFrame.Background = (Brush)solidColorBrush;
            this.RootFrame = (PhoneApplicationFrame)transitionFrame;
            this.RootFrame.Navigated += new NavigatedEventHandler(this.CompleteInitializePhoneApplication);
            this.RootFrame.Navigated += new NavigatedEventHandler(this.RootFrame_Navigated);
            this.RootFrame.NavigationStopped += new NavigationStoppedEventHandler(this.RootFrame_NavigationStopped);
            App._uriMapper = new CustomUriMapper();
            this.RootFrame.UriMapper = (UriMapperBase)App._uriMapper;
            this.RootFrame.NavigationFailed += new NavigationFailedEventHandler(this.RootFrame_NavigationFailed);
            this.RootFrame.Navigating += new NavigatingCancelEventHandler(this.RootFrame_Navigating);
            PhoneApplicationService.Current.ContractActivated += new EventHandler<IActivatedEventArgs>(this.Application_ContractActivated);
            this.phoneApplicationInitialized = true;
        }

        private void Application_ContractActivated(object sender, IActivatedEventArgs e)
        {
            FileOpenPickerContinuationEventArgs continuationEventArgs = e as FileOpenPickerContinuationEventArgs;
            if (continuationEventArgs == null)
                return;
            if (((IDictionary<string, object>)continuationEventArgs.ContinuationData).ContainsKey("FilePickedType"))
                ParametersRepository.SetParameterForId("FilePickedType", (object)(AttachmentType)((IDictionary<string, object>)continuationEventArgs.ContinuationData)["FilePickedType"]);
            ParametersRepository.SetParameterForId("FilePicked", (object)continuationEventArgs);
        }

        private void RootFrame_NavigationStopped(object sender, NavigationEventArgs e)
        {
            Logger.Instance.Info("App.RootFrame_NavigationStopped Mode={1}, Uri={0}", (object)e.Uri.ToString(), (object)e.NavigationMode);
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Logger.Instance.Info("App.RootFrame_Navigated Mode={1}, Uri={0}", (object)e.Uri.ToString(), (object)e.NavigationMode);
            if (e.NavigationMode == NavigationMode.Reset)
                this.RootFrame.Navigated += new NavigatedEventHandler(this.ClearBackStackAfterReset);
            EventAggregator.Current.Publish((object)new RootFrameNavigatedEvent()
            {
                Uri = e.Uri
            });
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Logger.Instance.Info("App.RootFrame_Navigating Mode={1}, Uri={0}", (object)e.Uri.ToString(), (object)e.NavigationMode);
            string @string = e.Uri.ToString();
            if (@string == "app://external/")
                return;
            if (!@string.Contains("/LoginPage.xaml") && !@string.Contains("/ValidatePage.xaml") && (!@string.Contains("/WelcomePage.xaml") && !@string.Contains("/RegistrationPage.xaml")) && (!@string.Contains("/Auth2FAPage.xaml") && !@string.Contains("/PhotoPickerPhotos.xaml")))
            {
                if (AppGlobalStateManager.Current.IsUserLoginRequired() && !this._handlingPreLoginNavigation)
                {
                    e.Cancel = true;
                    this.RootFrame.Dispatcher.BeginInvoke((Action)(() => this.RootFrame.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative))));
                }
                else if (@string.Contains("TileLoggedInUserId") && @string.Contains("IsChat=True"))
                {
                    int startIndex1 = @string.IndexOf("TileLoggedInUserId");
                    int startIndex2 = @string.IndexOf("=", startIndex1) + 1;
                    long result = 0;
                    if (long.TryParse(@string.Substring(startIndex2), out result) && result != AppGlobalStateManager.Current.LoggedInUserId)
                    {
                        e.Cancel = true;
                        this.RootFrame.Dispatcher.BeginInvoke((Action)(() => this.RootFrame.Navigate(new Uri("/VKClient.Common;component/NewsPage.xaml", UriKind.Relative))));
                    }
                }
            }
            this._handlingPreLoginNavigation = false;
            if (this.sessionType == App.SessionType.None && e.NavigationMode == NavigationMode.New)
            {
                if (this.IsDeepLink(e.Uri.ToString()))
                    this.sessionType = App.SessionType.DeepLink;
                else if (e.Uri.ToString().Contains("/NewsPage.xaml"))
                    this.sessionType = App.SessionType.Home;
            }
            if (e.NavigationMode == NavigationMode.Reset)
            {
                if ((e.Uri.OriginalString.Contains("RegistrationPage.xaml") || e.Uri.OriginalString.Contains("LoginPage.xaml") || e.Uri.OriginalString.Contains("Auth2FAPage.xaml")) && AppGlobalStateManager.Current.IsUserLoginRequired())
                    this._handlingPreLoginNavigation = true;
                this.wasRelaunched = true;
            }
            else
            {
                if (e.NavigationMode != NavigationMode.New || !this.wasRelaunched)
                    return;
                this.wasRelaunched = false;
                if (this.IsDeepLink(e.Uri.ToString()))
                {
                    this.sessionType = App.SessionType.DeepLink;
                }
                else
                {
                    if (!e.Uri.ToString().Contains("/NewsPage.xaml"))
                        return;
                    if (this.sessionType == App.SessionType.DeepLink)
                    {
                        this.sessionType = App.SessionType.Home;
                        e.Cancel = true;
                        this.RootFrame.Navigated -= new NavigatedEventHandler(this.ClearBackStackAfterReset);
                    }
                    else
                    {
                        e.Cancel = true;
                        this.RootFrame.Navigated -= new NavigatedEventHandler(this.ClearBackStackAfterReset);
                    }
                }
            }
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            if (this.RootVisual != this.RootFrame)
                this.RootVisual = (UIElement)this.RootFrame;
            this.RootFrame.Navigated -= new NavigatedEventHandler(this.CompleteInitializePhoneApplication);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
        }

        private void TextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
        }

        public bool AddOrUpdateValue(string Key, object value)
        {
            bool flag = false;
            try
            {
                if (this.Settings.Contains(Key))
                {
                    if (this.Settings[Key] != value)
                    {
                        this.Settings[Key] = value;
                        flag = true;
                    }
                }
                else
                {
                    this.Settings.Add(Key, value);
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("App.AddOrUpdateValue failed.", ex);
            }
            return flag;
        }

        public void RemoveValue(string Key)
        {
            if (!this.Settings.Contains(Key))
                return;
            this.Settings.Remove(Key);
        }

        public void SaveCurrentDeactivationSettings()
        {
            if (!this.AddOrUpdateValue("SessionType", (object)this.sessionType))
                return;
            this.Settings.Save();
        }

        public void RemoveCurrentDeactivationSettings()
        {
            this.RemoveValue("SessionType");
            this.Settings.Save();
        }

        private void RestoreSessionType()
        {
            if (!this.Settings.Contains("SessionType"))
                return;
            this.sessionType = (App.SessionType)this.Settings["SessionType"];
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            this.RootFrame.Navigated -= new NavigatedEventHandler(this.ClearBackStackAfterReset);
        }

        private bool IsDeepLink(string uri)
        {
            if (uri.Contains("ClearBackStack") || uri.Contains("/NewsPage.xaml") && uri.Contains("Action") || (uri.Contains("/NewsPage.xaml") && uri.Contains("uid=") || uri.Contains("/NewsPage.xaml") && uri.Contains("type=")) || (uri.Contains("/NewsPage.xaml") && uri.Contains("like_type=") || uri.Contains("/NewsPage.xaml") && uri.Contains("group_id=") || uri.Contains("/NewsPage.xaml") && uri.Contains("from_id=")))
                return true;
            if (uri.Contains("/NewsPage.xaml"))
                return uri.Contains("url=");
            return false;
        }

        public void ReportException(Exception exc)
        {
            if (exc == null)
                return;
            try
            {
                App.TelemetryClient.TrackException(exc);
            }
            catch
            {
            }
        }

        public void HandleSuccessfulLogin(AutorizationData logInInfo, bool navigate = true)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                AppGlobalStateManager.Current.HandleUserLogin(logInInfo);
                ConversationsViewModel.Instance = new ConversationsViewModel();
                ConversationsPage.ConversationsUCInstance = (ConversationsUC)null;
                ContactsManager.Instance.EnsureInSyncAsync(false);
                if (!navigate)
                    return;
                Navigator.Current.NavigateToMainPage();
            }));
        }

        private void ButtonTryAgain_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
            ISupportReload supportReload = ((FrameworkElement)sender).DataContext as ISupportReload;
            if (supportReload == null)
                return;
            supportReload.Reload();
        }

        private enum SessionType
        {
            None,
            Home,
            DeepLink,
        }
    }
}
