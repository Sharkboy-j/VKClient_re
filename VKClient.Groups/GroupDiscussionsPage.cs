using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
    public partial class GroupDiscussionsPage : PageBase
    {
        //private readonly int OFFSET_KNOB = 40;
        private ApplicationBarIconButton _appBarButtonRefresh = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.refresh.rest.png", UriKind.Relative),
            Text = CommonResources.AppBar_Refresh
        };
        private ApplicationBarIconButton _appBarButtonAdd = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.add.rest.png", UriKind.Relative),
            Text = CommonResources.AppBar_Add
        };
        private ApplicationBar _defaultAppBar = new ApplicationBar()
        {
            BackgroundColor = VKConstants.AppBarBGColor,
            ForegroundColor = VKConstants.AppBarFGColor,
            Opacity = 0.9
        };
        private bool _isInitialized;

        public GroupDiscussionsViewModel GroupDiscussionsVM
        {
            get
            {
                return this.DataContext as GroupDiscussionsViewModel;
            }
        }

        public GroupDiscussionsPage()
        {
            this.InitializeComponent();
            this.BuildAppBar();
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxThemeHeaders);
            this.listBoxThemeHeaders.OnRefresh = (Action)(() => this.GroupDiscussionsVM.LoadData(true, false));
            this.Header.OnHeaderTap = (Action)(() => this.listBoxThemeHeaders.ScrollToTop());
        }

        private void BuildAppBar()
        {
            this._appBarButtonRefresh.Click += new EventHandler(this._appBarButtonRefresh_Click);
            this._appBarButtonAdd.Click += new EventHandler(this._appBarButtonAdd_Click);
            this._defaultAppBar.Opacity = 0.9;
        }

        private void _appBarButtonAdd_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToNewWallPost(this.GroupDiscussionsVM.GroupId, true, this.GroupDiscussionsVM.AdminLevel, this.GroupDiscussionsVM.IsPublicPage, true, false);
        }

        private void _appBarButtonRefresh_Click(object sender, EventArgs e)
        {
            this.GroupDiscussionsVM.LoadData(true, false);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                long gid = long.Parse(this.NavigationContext.QueryString["GroupId"]);
                int num1 = int.Parse(this.NavigationContext.QueryString["AdminLevel"]);
                bool flag1 = this.NavigationContext.QueryString["IsPublicPage"] == bool.TrueString;
                bool flag2 = this.NavigationContext.QueryString["CanCreateTopic"] == bool.TrueString;
                int adminLevel = num1;
                int num2 = flag1 ? 1 : 0;
                int num3 = flag2 ? 1 : 0;
                GroupDiscussionsViewModel discussionsViewModel = new GroupDiscussionsViewModel(gid, adminLevel, num2 != 0, num3 != 0);
                this.DataContext = (object)discussionsViewModel;
                discussionsViewModel.LoadData(false, false);
                this._isInitialized = true;
            }
            this.UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            if (!this.GroupDiscussionsVM.CanCreateDiscussion || this._defaultAppBar.Buttons.Contains((object)this._appBarButtonAdd))
                return;
            this._defaultAppBar.Buttons.Insert(0, (object)this._appBarButtonAdd);
            this.ApplicationBar = (IApplicationBar)this._defaultAppBar;
        }

        private void listBoxThemeHeaders_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.GroupDiscussionsVM.DiscussionsVM.LoadMoreIfNeeded((object)(e.ContentPresenter.Content as ThemeHeader));
        }

        private void listBoxThemeHeaders_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            this.listBoxThemeHeaders.SelectedItem = null;
        }

        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ThemeHeader header = (sender as FrameworkElement).DataContext as ThemeHeader;
            if (header == null)
                return;
            this.NavigateToDiscussion(false, header);
        }

        private void Grid_Tap_2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ThemeHeader header = (sender as FrameworkElement).DataContext as ThemeHeader;
            if (header == null)
                return;
            this.NavigateToDiscussion(true, header);
        }

        private void NavigateToDiscussion(bool loadFromEnd, ThemeHeader header)
        {
            this.GroupDiscussionsVM.NavigateToDiscusson(loadFromEnd, header);
        }
    }
}
