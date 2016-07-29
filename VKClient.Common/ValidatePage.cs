using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Framework;

namespace VKClient.Common
{
  public class ValidatePage : PhoneApplicationPage
  {
    private const string REDIRECT_URL = "https://oauth.vk.com/blank.html";
    private bool _isInitialized;
    private string _scopes;
    private bool _revoke;
    private string _validationUri;
    internal Grid LayoutRoot;
    internal WebBrowser webBrowser;
    private bool _contentLoaded;

    public ValidatePage()
    {
      this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);
      if (this._isInitialized)
        return;
      if (this.NavigationContext.QueryString.ContainsKey("ValidationUri"))
      {
        this._validationUri = this.NavigationContext.QueryString["ValidationUri"];
      }
      else
      {
        this._scopes = this.NavigationContext.QueryString["Scopes"];
        this._revoke = this.NavigationContext.QueryString["Revoke"] == bool.TrueString;
      }
      this.InitializeWebBrowser();
      this._isInitialized = true;
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      base.OnNavigatingFrom(e);
      if (e.NavigationMode != NavigationMode.Back || ParametersRepository.Contains("ValidationResponse"))
        return;
      ParametersRepository.SetParameterForId("ValidationResponse", (object) new ValidationUserResponse()
      {
        IsSucceeded = false
      });
    }

    private void InitializeWebBrowser()
    {
      if (string.IsNullOrEmpty(this._validationUri))
        return;
      string uriString = this._validationUri;
      this.webBrowser.NavigationFailed += new NavigationFailedEventHandler(this.BrowserOnNavigationFailed);
      this.webBrowser.Navigating += new EventHandler<NavigatingEventArgs>(this.BrowserOnNavigating);
      this.webBrowser.LoadCompleted += new LoadCompletedEventHandler(this.BrowserOnLoadCompleted);
      this.webBrowser.Navigate(new Uri(uriString));
    }

    private void BrowserOnLoadCompleted(object sender, NavigationEventArgs navigationEventArgs)
    {
      this.webBrowser.LoadCompleted -= new LoadCompletedEventHandler(this.BrowserOnLoadCompleted);
      this.webBrowser.Visibility = Visibility.Visible;
    }

    private void BrowserOnNavigating(object sender, NavigatingEventArgs args)
    {
      string absoluteUri = args.Uri.AbsoluteUri;
      if (!absoluteUri.StartsWith("https://oauth.vk.com/blank.html"))
        return;
      this.ProcessResultString(absoluteUri.Substring(absoluteUri.IndexOf('#') + 1));
    }

    private void ProcessResultString(string result)
    {
      Dictionary<string, string> dictionary = ValidatePage.ExploreQueryString(result);
      ValidationUserResponse validationUserResponse = new ValidationUserResponse();
      if (dictionary.ContainsKey("success"))
      {
        validationUserResponse.IsSucceeded = true;
        if (dictionary.ContainsKey("access_token"))
          validationUserResponse.access_token = dictionary["access_token"];
        if (dictionary.ContainsKey("user_id"))
        {
          long result1 = 0;
          if (long.TryParse(dictionary["user_id"], out result1))
            validationUserResponse.user_id = result1;
        }
        if (dictionary.ContainsKey("phone"))
          validationUserResponse.phone = dictionary["phone"];
        if (dictionary.ContainsKey("phone_status"))
          validationUserResponse.phone_status = dictionary["phone_status"];
        if (dictionary.ContainsKey("email"))
          validationUserResponse.phone = dictionary["email"];
        if (dictionary.ContainsKey("email_status"))
          validationUserResponse.phone_status = dictionary["email_status"];
      }
      ParametersRepository.SetParameterForId("ValidationResponse", (object) validationUserResponse);
      this.NavigationService.GoBackSafe();
    }

    private void BrowserOnNavigationFailed(object sender, NavigationFailedEventArgs navigationFailedEventArgs)
    {
    }

    public static Dictionary<string, string> ExploreQueryString(string queryString)
    {
      string[] strArray1 = queryString.Split('&');
      Dictionary<string, string> dictionary = new Dictionary<string, string>(strArray1.Length);
      foreach (string str in strArray1)
      {
        char[] chArray = new char[1]{ '=' };
        string[] strArray2 = str.Split(chArray);
        dictionary.Add(strArray2[0], strArray2[1]);
      }
      return dictionary;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/ValidatePage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.webBrowser = (WebBrowser) this.FindName("webBrowser");
    }
  }
}
