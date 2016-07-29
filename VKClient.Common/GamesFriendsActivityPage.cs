using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class GamesFriendsActivityPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC HeaderUC;
    internal PullToRefreshUC PullToRefreshUC;
    internal ExtendedLongListSelector FriendsActivityListBox;
    private bool _contentLoaded;

    private GamesFriendsActivityViewModel VM
    {
      get
      {
        return this.DataContext as GamesFriendsActivityViewModel;
      }
    }

    public GamesFriendsActivityPage()
    {
      this.InitializeComponent();
      this.HeaderUC.textBlockTitle.Text = CommonResources.PageTitle_Games_FriendsActivity.ToUpperInvariant();
      this.PullToRefreshUC.TrackListBox((ISupportPullToRefresh) this.FriendsActivityListBox);
      this.FriendsActivityListBox.OnRefresh = (Action) (() => this.VM.FriendsActivityVM.LoadData(true, false, (Action<BackendResult<GamesFriendsActivityResponse, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long result = 0;
      if (this.NavigationContext.QueryString.ContainsKey("GameId"))
      {
        long.TryParse(this.NavigationContext.QueryString["GameId"], out result);
        if (result != 0L)
          this.FriendsActivityListBox.ItemTemplate = (DataTemplate) this.Resources["ShortItemTemplate"];
      }
      if (this.NavigationContext.QueryString.ContainsKey("GameName"))
      {
        string str = this.NavigationContext.QueryString["GameName"];
        if (!string.IsNullOrEmpty(str))
          this.HeaderUC.textBlockTitle.Text = str.ToUpperInvariant();
      }
      GamesFriendsActivityViewModel activityViewModel = new GamesFriendsActivityViewModel(result);
      activityViewModel.FriendsActivityVM.LoadData(false, false, (Action<BackendResult<GamesFriendsActivityResponse, ResultCode>>) null, false);
      this.DataContext = (object) activityViewModel;
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.FriendsActivityVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/GamesFriendsActivityPage.xaml", UriKind.Relative));
      this.HeaderUC = (GenericHeaderUC) this.FindName("HeaderUC");
      this.PullToRefreshUC = (PullToRefreshUC) this.FindName("PullToRefreshUC");
      this.FriendsActivityListBox = (ExtendedLongListSelector) this.FindName("FriendsActivityListBox");
    }
  }
}
