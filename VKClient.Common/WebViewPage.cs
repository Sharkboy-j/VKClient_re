using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Framework;

namespace VKClient.Common
{
  public class WebViewPage : PhoneApplicationPage
  {
    private bool _isInitialized;
    internal WebBrowser WebView;
    private bool _contentLoaded;

    public WebViewPage()
    {
      this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._isInitialized = true;
      this.WebView.Navigate(new Uri(HttpUtility.UrlDecode(this.NavigationContext.QueryString["Uri"])));
    }

    private void WebView_OnNavigating(object sender, NavigatingEventArgs e)
    {
      if (!e.Uri.AbsoluteUri.Contains("blank.html"))
        return;
      if (this.NavigationService.CanGoBack)
        this.NavigationService.GoBackSafe();
      else
        Navigator.Current.NavigateToMainPage();
    }

    private void WebView_OnLoadCompleted(object sender, NavigationEventArgs e)
    {
      this.WebView.LoadCompleted -= new LoadCompletedEventHandler(this.WebView_OnLoadCompleted);
      this.WebView.Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/WebViewPage.xaml", UriKind.Relative));
      this.WebView = (WebBrowser) this.FindName("WebView");
    }
  }
}
