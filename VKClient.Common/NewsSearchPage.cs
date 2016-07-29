using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class NewsSearchPage : PageBase
  {
    private bool _isInitialized;
    private bool _isEmptySearch;
    internal NewsSearchUC ucNewsSearch;
    private bool _contentLoaded;

    public NewsSearchPage()
    {
      this.InitializeComponent();
      this.SuppressOpenMenuTapArea = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this.DataContext = (object) new ViewModelBase();
        IDictionary<string, string> queryString = this.NavigationContext.QueryString;
        string query = queryString.ContainsKey("Query") ? HttpUtility.UrlDecode(queryString["Query"]) : "";
        this._isEmptySearch = string.IsNullOrEmpty(query);
        this.ucNewsSearch.Init(0L, query, false);
        this._isInitialized = true;
      }
      CurrentMediaSource.VideoSource = StatisticsActionSource.search;
      CurrentMediaSource.AudioSource = StatisticsActionSource.search;
      CurrentMediaSource.GifPlaySource = StatisticsActionSource.search;
      CurrentNewsFeedSource.FeedSource = NewsSourcesPredefined.Search;
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      if (!string.IsNullOrEmpty(this.ucNewsSearch.TextBoxSearch.Text) && !this.ImageViewerDecorator.IsShown && (this.Flyouts.Count == 0 && this._isEmptySearch))
      {
        this.ucNewsSearch.TextBoxSearch.Text = "";
        this.Focus();
        e.Cancel = true;
      }
      base.OnBackKeyPress(e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/NewsSearchPage.xaml", UriKind.Relative));
      this.ucNewsSearch = (NewsSearchUC) this.FindName("ucNewsSearch");
    }
  }
}
