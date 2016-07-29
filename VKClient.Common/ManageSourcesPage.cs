using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Common
{
    public class ManageSourcesPage : PageBase
    {
        private bool _isInitialized;
        private ApplicationBarIconButton _appBarButtonDelete;
        private ApplicationBar _defaultAppBar;
        internal Grid LayoutRoot;
        internal GenericHeaderUC ucHeader;
        internal PullToRefreshUC ucPullToRefresh;
        internal Pivot pivot;
        internal PivotItem pivotItemFriends;
        internal ExtendedLongListSelector listBoxFriends;
        internal PivotItem pivotItemCommunities;
        internal ExtendedLongListSelector listBoxCommunities;
        private bool _contentLoaded;

        private ManageSourcesViewModel ManageSourcesVM
        {
            get
            {
                return this.DataContext as ManageSourcesViewModel;
            }
        }

        public ManageSourcesPage()
        {
            ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
            applicationBarIconButton.Text = CommonResources.Delete;
            Uri uri = new Uri("Resources/minus.png", UriKind.Relative);
            applicationBarIconButton.IconUri = uri;
            this._appBarButtonDelete = applicationBarIconButton;

            this.InitializeComponent();
            this.ucHeader.OnHeaderTap = (Action)(() =>
            {
                if (this.pivot.SelectedItem == this.pivotItemFriends)
                {
                    this.listBoxFriends.ScrollToTop();
                }
                else
                {
                    if (this.pivot.SelectedItem != this.pivotItemCommunities)
                        return;
                    this.listBoxCommunities.ScrollToTop();
                }
            });
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxCommunities);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxFriends);
            this.listBoxFriends.OnRefresh = (Action)(() => this.ManageSourcesVM.FriendsVM.LoadData(true, false, null, false));
            this.listBoxCommunities.OnRefresh = (Action)(() => this.ManageSourcesVM.GroupsVM.LoadData(true, false, null, false));
        }

        private void BuildAppBar()
        {
            this._defaultAppBar = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor,
                Opacity = 0.9
            };
            this._appBarButtonDelete.Click += new EventHandler(this._appBarButtonDelete_Click);
            this._defaultAppBar.Buttons.Add((object)this._appBarButtonDelete);
            this.ApplicationBar = (IApplicationBar)this._defaultAppBar;
        }

        private void UpdateAppBar()
        {
            this._appBarButtonDelete.IsEnabled = this.ManageSourcesVM.SelectedCount > 0;
        }

        private void _appBarButtonDelete_Click(object sender, EventArgs e)
        {
            this.ManageSourcesVM.DeleteSelected();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                ManageSourcesViewModel sourcesViewModel = new ManageSourcesViewModel((ManageSourcesMode)Enum.Parse(typeof(ManageSourcesMode), this.NavigationContext.QueryString["Mode"]));
                this.DataContext = (object)sourcesViewModel;
                sourcesViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
                sourcesViewModel.FriendsVM.LoadData(false, false, (Action<BackendResult<ProfilesAndGroups, ResultCode>>)null, false);
                sourcesViewModel.GroupsVM.LoadData(false, false, (Action<BackendResult<ProfilesAndGroups, ResultCode>>)null, false);
                this.BuildAppBar();
                this._isInitialized = true;
            }
            this.UpdateAppBar();
        }

        private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "SelectedCount"))
                return;
            this.UpdateAppBar();
        }

        private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            (sender as ExtendedLongListSelector).SelectedItem = null;
        }

        private void Friend_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!(sender is FrameworkElement))
                return;
            FriendHeader friendHeader = (sender as FrameworkElement).DataContext as FriendHeader;
            if (friendHeader == null)
                return;
            Navigator.Current.NavigateToUserProfile(friendHeader.UserId, friendHeader.User.Name, "", false);
        }

        private void Group_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!(sender is FrameworkElement))
                return;
            GroupHeader groupHeader = (sender as FrameworkElement).DataContext as GroupHeader;
            if (groupHeader == null)
                return;
            Navigator.Current.NavigateToGroup(groupHeader.Group.id, groupHeader.Group.name, false);
        }

        private void CheckBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/ManageSourcesPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.ucHeader = (GenericHeaderUC)this.FindName("ucHeader");
            this.ucPullToRefresh = (PullToRefreshUC)this.FindName("ucPullToRefresh");
            this.pivot = (Pivot)this.FindName("pivot");
            this.pivotItemFriends = (PivotItem)this.FindName("pivotItemFriends");
            this.listBoxFriends = (ExtendedLongListSelector)this.FindName("listBoxFriends");
            this.pivotItemCommunities = (PivotItem)this.FindName("pivotItemCommunities");
            this.listBoxCommunities = (ExtendedLongListSelector)this.FindName("listBoxCommunities");
        }
    }
}
