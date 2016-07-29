using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class SuggestedPostponedPostsPage : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal Grid ContentPanel;
    internal ViewportControl scrollNews;
    internal StackPanel stackPanel;
    internal MyVirtualizingPanel2 panelNews;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private SuggestedPostponedPostsViewModel VM
    {
      get
      {
        return this.DataContext as SuggestedPostponedPostsViewModel;
      }
    }

    public SuggestedPostponedPostsPage()
    {
      this.InitializeComponent();
      this.panelNews.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollNews), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelNews);
      this.panelNews.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.panelNews_ScrollPositionChanged);
      this.scrollNews.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelNews);
      this.panelNews.OnRefresh = (Action) (() => this.VM.WallVM.LoadData(true, false, (Action<BackendResult<WallData, ResultCode>>) null, false));
    }

    private void panelNews_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      SuggestedPostponedPostsViewModel postponedPostsViewModel = new SuggestedPostponedPostsViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, (SuggestedPostponedMode) int.Parse(this.NavigationContext.QueryString["Mode"]));
      this.DataContext = (object) postponedPostsViewModel;
      postponedPostsViewModel.WallVM.LoadData(false, false, (Action<BackendResult<WallData, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SuggestedPostponedPostsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.scrollNews = (ViewportControl) this.FindName("scrollNews");
      this.stackPanel = (StackPanel) this.FindName("stackPanel");
      this.panelNews = (MyVirtualizingPanel2) this.FindName("panelNews");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
    }
  }
}
