using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common
{
  public class SDKAuthPage : PageBase
  {
    private const string REDIRECT_URL = "https://oauth.vk.com/blank.html";
    private bool _isInitialized;
    private string _clientId;
    private string _scope;
    private string _callbackUri;
    private bool _revoke;
    private bool _handled;
    private string _guid;
    internal Grid LayoutRoot;
    internal ProgressBar progressBar;
    internal TextBlock errorTextBlock;
    internal WebBrowser webBrowser;
    private bool _contentLoaded;

    public SDKAuthPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
      this.DataContext = (object) new ViewModelBase();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);
      if (this._handled)
      {
        Navigator.Current.NavigateToMainPage();
      }
      else
      {
        if (!this._isInitialized)
        {
          this._clientId = this.NavigationContext.QueryString["ClientId"].ToString();
          this._scope = this.NavigationContext.QueryString["Scope"].ToString();
          this._callbackUri = this.NavigationContext.QueryString["RedirectUri"].ToString();
          this._guid = this.NavigationContext.QueryString["SDKGUID"];
          this._revoke = this.NavigationContext.QueryString["Revoke"].ToString() == bool.TrueString;
          this.InitializeWebBrowser();
          this._isInitialized = true;
        }
        SystemTray.IsVisible = false;
      }
    }

    private void InitializeWebBrowser()
    {
      string uriString = string.Format("https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri={2}&display=windows_mobile&v={3}&response_type=token&access_token={4}&revoke={5}&sdk_guid={6}", (object) this._clientId, (object) this._scope, (object) "https://oauth.vk.com/blank.html", (object) VKConstants.API_VERSION, (object) AppGlobalStateManager.Current.GlobalState.AccessToken, (object) (this._revoke ? 1 : 0), (object) this._guid);
      this.webBrowser.NavigationFailed += new NavigationFailedEventHandler(this.BrowserOnNavigationFailed);
      this.webBrowser.Navigating += new EventHandler<NavigatingEventArgs>(this.BrowserOnNavigating);
      this.webBrowser.LoadCompleted += new LoadCompletedEventHandler(this.BrowserOnLoadCompleted);
      this.webBrowser.Navigate(new Uri(uriString));
    }

    private void BrowserOnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
      this.progressBar.Visibility = Visibility.Collapsed;
      this.errorTextBlock.Visibility = Visibility.Visible;
    }

    private void BrowserOnLoadCompleted(object sender, NavigationEventArgs e)
    {
      this.webBrowser.LoadCompleted -= new LoadCompletedEventHandler(this.BrowserOnLoadCompleted);
      this.webBrowser.Visibility = Visibility.Visible;
      this.progressBar.Visibility = Visibility.Collapsed;
    }

    private void BrowserOnNavigating(object sender, NavigatingEventArgs e)
    {
      string absoluteUri = e.Uri.AbsoluteUri;
      if (!absoluteUri.StartsWith("https://oauth.vk.com/blank.html"))
        return;
      this._handled = true;
      this.webBrowser.Visibility = Visibility.Collapsed;
      Navigator.Current.NavigateFromSDKAuthPage(this._callbackUri + "?" + absoluteUri.Substring(absoluteUri.IndexOf('#') + 1));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SDKAuthPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.progressBar = (ProgressBar) this.FindName("progressBar");
      this.errorTextBlock = (TextBlock) this.FindName("errorTextBlock");
      this.webBrowser = (WebBrowser) this.FindName("webBrowser");
    }
  }
}
