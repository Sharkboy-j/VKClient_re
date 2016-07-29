using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
    public partial class GroupPage : PageBase
  {
    private ApplicationBar _defaultAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private readonly ApplicationBarIconButton _appBarButtonRefresh = new ApplicationBarIconButton()
    {
      Text = CommonResources.AppBar_Refresh,
      IconUri = AppBarResources.RefreshUri
    };
    private readonly ApplicationBarIconButton _appBarButtonAddNews = new ApplicationBarIconButton()
    {
      Text = CommonResources.MainPage_News_AddNews,
      IconUri = AppBarResources.AddNewsUri
    };
    private readonly ApplicationBarIconButton _appBarButtonPin = new ApplicationBarIconButton()
    {
      Text = CommonResources.PinToStart,
      IconUri = AppBarResources.PinToStartUri
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemFaveUnfave = new ApplicationBarMenuItem()
    {
      Text = CommonResources.AddToBookmarks
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemSubscribeUnsubscribe = new ApplicationBarMenuItem()
    {
      Text = CommonResources.SubscribeToNews
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemLeaveGroup = new ApplicationBarMenuItem();
    private readonly ApplicationBarMenuItem _appBarMenuItemPinToStart = new ApplicationBarMenuItem()
    {
      Text = CommonResources.PinToStart
    };
    private bool _isInitialized;
    private bool _forbidOverrideGoBack;
    private long _gid;

    public GroupViewModel GroupVM
    {
      get
      {
        return this.DataContext as GroupViewModel;
      }
    }

    public GroupPage()
    {
      this.InitializeComponent();
      this.wallPanel.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollViewer), false);
      this.scrollViewer.BindViewportBoundsTo((FrameworkElement) this.stackPanelMain);
      this.Header.OnHeaderTap = (Action) (() =>
      {
        if (this.pivot.SelectedItem != this.pivotItemMain)
          return;
        this.wallPanel.ScrollToBottom(false);
      });
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.wallPanel);
      this.wallPanel.OnRefresh = (Action) (() => this.GroupVM.LoadGroupData(true, false));
      this.RegisterForCleanup((IMyVirtualizingPanel) this.wallPanel);
      this.wallPanel.DeltaOffset = -400.0;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._defaultAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._appBarButtonRefresh.Click += new EventHandler(this.AppBarButtonRefresh_OnClick);
      this._appBarMenuItemLeaveGroup.Click += new EventHandler(this.AppBarMenuItemLeaveGroup_OnClick);
      this._appBarMenuItemPinToStart.Click += new EventHandler(this.AppBarMenuItemPinToStart_OnClick);
      this._appBarButtonAddNews.Click += new EventHandler(this.AppBarButtonAddNews_OnClick);
      this._appBarButtonPin.Click += new EventHandler(this.AppBarButtonPin_OnClick);
      this._appBarMenuItemFaveUnfave.Click += new EventHandler(this.AppBarMenuItemFaveUnfave_OnClick);
      this._appBarMenuItemSubscribeUnsubscribe.Click += new EventHandler(this._appBarMenuItemSubscribeUnsubscribe_Click);
    }

    private void _appBarMenuItemSubscribeUnsubscribe_Click(object sender, EventArgs e)
    {
      this.GroupVM.SubscribeUnsubscribe();
    }

    private void AppBarMenuItemFaveUnfave_OnClick(object sender, EventArgs e)
    {
      this.GroupVM.FaveUnfave();
    }

    private void AppBarButtonAddNews_OnClick(object sender, EventArgs e)
    {
      this.GroupVM.HandleActionButton(ActionButtonType.WriteOnWall);
    }

    private void AppBarButtonPin_OnClick(object sender, EventArgs e)
    {
      this.GroupVM.PinToStart();
    }

    private void AppBarMenuItemPinToStart_OnClick(object sender, EventArgs e)
    {
      this.GroupVM.PinToStart();
    }

    private void AppBarMenuItemLeaveGroup_OnClick(object sender, EventArgs e)
    {
      this.GroupVM.HandleActionButton(ActionButtonType.Leave);
    }

    private void AppBarButtonRefresh_OnClick(object sender, EventArgs e)
    {
      this.GroupVM.LoadGroupData(true, false);
    }

    private void UpdateAppBar()
    {
      if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
        return;
      bool flag = SecondaryTileManager.Instance.TileExistsFor(this._gid, true);
      if (!this._defaultAppBar.MenuItems.Contains((object) this._appBarMenuItemPinToStart) && !flag)
        this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemPinToStart);
      if (flag)
        this._defaultAppBar.MenuItems.Remove((object) this._appBarMenuItemPinToStart);
      if (!this._defaultAppBar.MenuItems.Contains((object) this._appBarMenuItemFaveUnfave))
        this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemFaveUnfave);
      if (!this._defaultAppBar.MenuItems.Contains((object) this._appBarMenuItemSubscribeUnsubscribe))
        this._defaultAppBar.MenuItems.Insert(0, (object) this._appBarMenuItemSubscribeUnsubscribe);
      this._appBarMenuItemFaveUnfave.Text = this.GroupVM.IsFavorite ? CommonResources.RemoveFromBookmarks : CommonResources.AddToBookmarks;
      this._appBarMenuItemSubscribeUnsubscribe.Text = this.GroupVM.IsSubscribed ? CommonResources.UnsubscribeFromNews : CommonResources.SubscribeToNews;
      if ((this.GroupVM.CanPost || this.GroupVM.CanSuggestAPost) && !this._defaultAppBar.Buttons.Contains((object) this._appBarButtonAddNews))
        this._defaultAppBar.Buttons.Add((object) this._appBarButtonAddNews);
      if (!this.GroupVM.CanPost && !this.GroupVM.CanSuggestAPost)
        this._defaultAppBar.Buttons.Remove((object) this._appBarButtonAddNews);
      if (this.GroupVM.CanPost)
        this._appBarButtonAddNews.Text = CommonResources.MainPage_News_AddNews;
      else if (this.GroupVM.CanSuggestAPost)
        this._appBarButtonAddNews.Text = CommonResources.SuggestedNews_SuggestAPost;
      if (this._defaultAppBar.MenuItems.Count > 0 || this._defaultAppBar.Buttons.Count > 0)
      {
        this.ApplicationBar = (IApplicationBar) this._defaultAppBar;
        this.ApplicationBar.Mode = this._defaultAppBar.Buttons.Count == 0 ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
      }
      else
        this.ApplicationBar = (IApplicationBar) null;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._gid = long.Parse(this.NavigationContext.QueryString["GroupId"]);
        string name = this.NavigationContext.QueryString["Name"];
        if (this.NavigationContext.QueryString.ContainsKey("ForbidOverrideGoBack"))
          bool.TryParse(this.NavigationContext.QueryString["ForbidOverrideGoBack"], out this._forbidOverrideGoBack);
        GroupViewModel groupViewModel = new GroupViewModel(this._gid, name);
        groupViewModel.PropertyChanged += new PropertyChangedEventHandler(this.gvm_PropertyChanged);
        this.DataContext = (object) groupViewModel;
        groupViewModel.LoadGroupData(false, false);
        this._isInitialized = true;
      }
      this.ProcessInputParameters();
      CurrentMediaSource.AudioSource = StatisticsActionSource.wall_group;
      CurrentMediaSource.VideoSource = StatisticsActionSource.wall_group;
      CurrentMediaSource.GifPlaySource = StatisticsActionSource.wall_group;
      CurrentNewsFeedSource.Source = ViewPostSource.GroupWall;
    }

    private void ProcessInputParameters()
    {
      Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
      if (group == null)
        return;
      foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>) this.GroupVM.WallVM.Collection)
      {
        WallPostItem wallPostItem = virtualizable as WallPostItem;
        if (wallPostItem != null && wallPostItem.LikesAndCommentsItem != null && wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(group.id, group.name))
          break;
      }
    }

    private void gvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "LeaveButtonVisibility") && !(e.PropertyName == "LeaveButtonText") && (!(e.PropertyName == "CanPost") && !(e.PropertyName == "IsFavorite")) && !(e.PropertyName == "IsSubscribed"))
        return;
      this.UpdateAppBar();
    }

    private void ActionButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GroupVM.HandleActionButton(((sender as FrameworkElement).DataContext as ActionButton).ButtonType);
    }

    private void Button_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GroupVM.HandleNavigateButton(((sender as FrameworkElement).DataContext as NavigateButton).ButtonType);
    }

    private void ButtonInformation_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ((sender as FrameworkElement).DataContext as InformationRow).Navigate();
    }

    private void AllPosts_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GroupVM.ShowAllPosts = true;
    }

    private void GroupPosts_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.GroupVM.ShowAllPosts = false;
    }

    private void Avatar_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (!this.GroupVM.HaveAvatar)
        return;
      Navigator.Current.NavigateToImageViewer("-6", 5, this._gid, true, -1, 0, new List<Photo>(), (Func<int, Image>) (ind => (Image) null));
    }
  }
}
