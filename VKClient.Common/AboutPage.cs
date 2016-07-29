using Microsoft.Phone.Tasks;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
    public class AboutPage : PageBase
    {
        internal GenericHeaderUC Header;
        internal TextBlock VersionBlock;
        private bool _contentLoaded;

        public AboutPage()
        {
            this.InitializeComponent();
            this.Header.TextBlockTitle.Text = CommonResources.Menu_About.ToUpper();
            this.VersionBlock.Text = string.Format(CommonResources.About_Version, AppInfo.Version);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            this.DataContext = new ViewModelBase();
        }

        private void OnFeedbackClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToGroup(35502680, null, false);
        }

        private void OnRateClicked(object sender, GestureEventArgs e)
        {
            new MarketplaceReviewTask().Show();
        }

        private void OnPrivacyClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToWebUri("https://m.vk.com/privacy", false, false);
        }

        private void OnTermsClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToWebUri("https://m.vk.com/terms", false, false);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/AboutPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)this.FindName("Header");
            this.VersionBlock = (TextBlock)this.FindName("VersionBlock");
        }
    }
}
