using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKMessenger.Views
{
  public class PickConversationPage : PageBase
  {
    private readonly DialogService _de = new DialogService();
    private readonly ApplicationBarIconButton _appBarButtonSearch = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Search
    };
    private bool _isInitialized;
    private IShareContentDataProvider _shareContentDataProvider;
    internal Grid LayoutRoot;
    internal Grid ContentPanel;
    internal ConversationsUC conversationsUC;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    public PickConversationPage()
    {
      this.InitializeComponent();
      this.RegisterForCleanup((IMyVirtualizingPanel) this.conversationsUC.conversationsListBox);
      this.conversationsUC.IsLookup = true;
      this.BuildAppBar();
      this.Header.TextBlockTitle.Text = CommonResources.ChooseConversation;
      this.Header.HideSandwitchButton = true;
      this.Header.OnHeaderTap = (Action) (() => this.conversationsUC.conversationsListBox.ScrollToBottom(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.conversationsUC.conversationsListBox);
      this.conversationsUC.conversationsListBox.OnRefresh = (Action) (() => this.conversationsUC.ConversationsVM.RefreshConversations(false));
      this.SuppressMenu = true;
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = new ApplicationBar()
      {
        BackgroundColor = VKConstants.AppBarBGColor,
        ForegroundColor = VKConstants.AppBarFGColor,
        Opacity = 0.9
      };
      this._appBarButtonSearch.Click += new EventHandler(this._appBarButtonSearch_Click);
      applicationBar.Buttons.Add((object) this._appBarButtonSearch);
      this.ApplicationBar = (IApplicationBar) applicationBar;
    }

    private void _appBarButtonSearch_Click(object sender, EventArgs e)
    {
      GenericSearchUC searchUC = new GenericSearchUC();
      searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 32.0, 0.0, 0.0);
      searchUC.Initialize<User, FriendHeader>((ISearchDataProvider<User, FriendHeader>) new ConversationsSearchDataProvider(), (Action<object, object>) ((listBox, selectedItem) =>
      {
        FriendHeader friendHeader = selectedItem as FriendHeader;
        if (friendHeader == null)
          return;
        bool isChat = friendHeader.UserId < 0L;
        Navigator.Current.NavigateToConversation(Math.Abs(friendHeader.UserId), isChat, true, "", 0L, false);
      }), Application.Current.Resources["FriendItemTemplate"] as DataTemplate);
      searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler) ((s, ev) => this.ContentPanel.Visibility = searchUC.SearchTextBox.Text != "" ? Visibility.Collapsed : Visibility.Visible);
      this._de.HideOnNavigation = false;
      this._de.AnimationType = DialogService.AnimationTypes.None;
      this._de.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      this._de.Child = (FrameworkElement) searchUC;
      this._de.Show((UIElement) this.ContentPanel);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this.DataContext = (object) new ConversationsViewModelTemp();
      this.conversationsUC.PrepareForViewIfNeeded();
      this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
      this._isInitialized = true;
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      if (e.NavigationMode == NavigationMode.Back || this._shareContentDataProvider == null)
        return;
      this._shareContentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider(this._shareContentDataProvider);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/PickConversationPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.conversationsUC = (ConversationsUC) this.FindName("conversationsUC");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
    }
  }
}
