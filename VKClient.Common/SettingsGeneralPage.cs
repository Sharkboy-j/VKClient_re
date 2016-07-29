using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using Windows.System;

namespace VKClient.Common
{
    public class SettingsGeneralPage : PageBase
    {
        private bool _isInitialized;
        internal Grid LayoutRoot;
        internal GenericHeaderUC Header;
        internal Grid ContentPanel;
        private bool _contentLoaded;

        private SettingsGeneralViewModel VM
        {
            get
            {
                return this.DataContext as SettingsGeneralViewModel;
            }
        }

        public SettingsGeneralPage()
        {
            this.InitializeComponent();
            this.Header.textBlockTitle.Text = CommonResources.NewSettings_General.ToUpperInvariant();
        }

        private async void ConfigureLockScreenTap(object sender, GestureEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
        }

        private void ClearMusicCacheTap(object sender, GestureEventArgs e)
        {
            this.VM.ClearMusicCache();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            this.DataContext = (object)new SettingsGeneralViewModel();
            this._isInitialized = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/SettingsGeneralPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.Header = (GenericHeaderUC)this.FindName("Header");
            this.ContentPanel = (Grid)this.FindName("ContentPanel");
        }
    }
}
