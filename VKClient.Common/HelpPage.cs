using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class HelpPage : PageBase
  {
    private string _uri = "";
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal Grid ContentPanel;
    internal WebBrowser webBrowser;
    internal ProgressBar progressBar;
    internal GenericHeaderUC Header;
    private bool _contentLoaded;

    public HelpPage()
    {
      this.InitializeComponent();
      this.Header.TextBlockTitle.Text = CommonResources.Help.ToUpperInvariant();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._uri = AppGlobalStateManager.Current.GlobalState.SupportUri;
      this.DataContext = (object) new ViewModelBase();
      this._isInitialized = true;
      this.InitializeWebBrowser();
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (!this.webBrowser.CanGoBack)
        return;
      this.webBrowser.GoBack();
      e.Cancel = true;
    }

    private void InitializeWebBrowser()
    {
      if (string.IsNullOrEmpty(this._uri))
        return;
      string str = this._uri;
      this.webBrowser.NavigationFailed += new NavigationFailedEventHandler(this.BrowserOnNavigationFailed);
      this.webBrowser.Navigating += new EventHandler<NavigatingEventArgs>(this.BrowserOnNavigating);
      this.webBrowser.LoadCompleted += new LoadCompletedEventHandler(this.BrowserOnLoadCompleted);
      this.webBrowser.Navigate(new Uri(this._uri));
    }

    private void BrowserOnLoadCompleted(object sender, NavigationEventArgs navigationEventArgs)
    {
      this.webBrowser.LoadCompleted -= new LoadCompletedEventHandler(this.BrowserOnLoadCompleted);
      this.progressBar.Visibility = Visibility.Collapsed;
      this.webBrowser.Visibility = Visibility.Visible;
    }

    private void BrowserOnNavigating(object sender, NavigatingEventArgs args)
    {
    }

    private void BrowserOnNavigationFailed(object sender, NavigationFailedEventArgs navigationFailedEventArgs)
    {
      new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
      this.progressBar.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/HelpPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.webBrowser = (WebBrowser) this.FindName("webBrowser");
      this.progressBar = (ProgressBar) this.FindName("progressBar");
      this.Header = (GenericHeaderUC) this.FindName("Header");
    }
  }
}
