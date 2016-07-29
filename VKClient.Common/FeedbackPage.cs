using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
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
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FeedbackPage : PageBase
  {
    private ApplicationBar _defaultAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private ApplicationBarIconButton _appBarButtonRefresh = new ApplicationBarIconButton()
    {
      IconUri = new Uri("Resources/appbar.refresh.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Refresh
    };
    private bool _isInitilized;
    private bool _triggeredLoadingComments;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Pivot pivot;
    internal PivotItem pivotItemFeedback;
    internal ViewportControl scrollFeedback;
    internal StackPanel stackPanelFeedback;
    internal MyVirtualizingPanel2 panelFeedback;
    internal PivotItem pivotItemComments;
    internal ViewportControl scrollComments;
    internal StackPanel stackPanelComments;
    internal MyVirtualizingPanel2 panelComments;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private FeedbackViewModel FeedbackVM
    {
      get
      {
        return this.DataContext as FeedbackViewModel;
      }
    }

    public FeedbackPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.panelComments.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollComments), false);
      this.scrollComments.BindViewportBoundsTo((FrameworkElement) this.stackPanelComments);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelComments);
      this.panelFeedback.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollFeedback), false);
      this.scrollFeedback.BindViewportBoundsTo((FrameworkElement) this.stackPanelFeedback);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelFeedback);
      this.panelFeedback.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.panelFeedback_ScrollPositionChanged);
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    private void panelFeedback_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
      if (e.ScrollHeight == 0.0 || e.ScrollHeight - e.CurrentPosition >= VKConstants.LoadMoreNewsThreshold)
        return;
      this.FeedbackVM.LoadFeedback(false);
    }

    private void BuildAppBar()
    {
      this._appBarButtonRefresh.Click += new EventHandler(this._appBarButtonRefresh_Click);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonRefresh);
      this._defaultAppBar.Opacity = 0.9;
    }

    private void UpdateAppBar()
    {
    }

    private void _appBarButtonRefresh_Click(object sender, EventArgs e)
    {
      if (this.pivot.SelectedItem == this.pivotItemFeedback)
        this.FeedbackVM.LoadFeedback(true);
      else
        this.FeedbackVM.LoadComments(true);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitilized)
        return;
      FeedbackViewModel vm = new FeedbackViewModel(this.panelFeedback, this.panelComments);
      this.DataContext = (object) vm;
      vm.LoadFeedback(false);
      this.UpdateAppBar();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelFeedback);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelComments);
      this.panelFeedback.OnRefresh = (Action) (() => vm.FeedbackVM.LoadData(true, false, (Action<BackendResult<NotificationData, ResultCode>>) null, false));
      this.panelComments.OnRefresh = (Action) (() => vm.CommentsVM.LoadData(true, false, (Action<BackendResult<NewsFeedData, ResultCode>>) null, false));
      CountersManager.Current.ResetFeedback();
      this._isInitilized = true;
    }

    private void pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedItem != this.pivotItemComments || this._triggeredLoadingComments)
        return;
      this.FeedbackVM.LoadComments(false);
      this._triggeredLoadingComments = true;
    }

    public void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemFeedback)
      {
        this.panelFeedback.ScrollToBottom(false);
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemComments)
          return;
        this.panelComments.ScrollToBottom(false);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/FeedbackPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemFeedback = (PivotItem) this.FindName("pivotItemFeedback");
      this.scrollFeedback = (ViewportControl) this.FindName("scrollFeedback");
      this.stackPanelFeedback = (StackPanel) this.FindName("stackPanelFeedback");
      this.panelFeedback = (MyVirtualizingPanel2) this.FindName("panelFeedback");
      this.pivotItemComments = (PivotItem) this.FindName("pivotItemComments");
      this.scrollComments = (ViewportControl) this.FindName("scrollComments");
      this.stackPanelComments = (StackPanel) this.FindName("stackPanelComments");
      this.panelComments = (MyVirtualizingPanel2) this.FindName("panelComments");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
    }
  }
}
