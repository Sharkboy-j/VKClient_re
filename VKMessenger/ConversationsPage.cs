using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKMessenger.Library;
using VKMessenger.Views;

namespace VKMessenger
{
  public partial class ConversationsPage : PageBase
  {
    private ApplicationBar _appBarConversations = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private bool _isInitialized;
    private static ConversationsUC _conversationsUCInstance;
    private static int TotalCount;

    public static ConversationsUC ConversationsUCInstance
    {
      get
      {
        if (ConversationsPage._conversationsUCInstance == null)
        {
          ConversationsPage._conversationsUCInstance = new ConversationsUC();
          ConversationsPage._conversationsUCInstance.PreventFromClearing = true;
        }
        return ConversationsPage._conversationsUCInstance;
      }
      set
      {
        ConversationsPage._conversationsUCInstance = value;
      }
    }

    public ConversationsPage()
    {
      ++ConversationsPage.TotalCount;
      this.InitializeComponent();
      this.BuildAppBar();
      this.Header.TextBlockTitle.Text = CommonResources.Messages_Title;
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    ~ConversationsPage()
    {
      --ConversationsPage.TotalCount;
    }

    private void OnHeaderTap()
    {
      ConversationsPage.ConversationsUCInstance.conversationsListBox.ScrollToBottom(false);
    }

    private void BuildAppBar()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton(new Uri("/Resources/appbar.add.rest.png", UriKind.Relative));
      applicationBarIconButton1.Click += new EventHandler(this.appBarButtonAdd_Click);
      applicationBarIconButton1.Text = CommonResources.AppBar_Add;
      this._appBarConversations.Buttons.Add((object) applicationBarIconButton1);
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton(new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative));
      applicationBarIconButton2.Text = CommonResources.AppBar_Search;
      this._appBarConversations.Buttons.Add((object) applicationBarIconButton2);
      applicationBarIconButton2.Click += new EventHandler(this.appBarButtonSearch_Click);
      this.ApplicationBar = (IApplicationBar) this._appBarConversations;
    }

    private void appBarButtonSearch_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToConversationsSearch();
    }

    private void appBarButtonRefresh_Click(object sender, EventArgs e)
    {
      ConversationsViewModel.Instance.RefreshConversations(false);
    }

    private void appBarButtonAdd_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToPickUser(false, 0L, false, 0, PickUserMode.PickForMessage, "", 0);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this.DataContext = (object) new ConversationsViewModelTemp();
        this.ContentPanel.Children.Add((UIElement) ConversationsPage.ConversationsUCInstance);
        this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) ConversationsPage.ConversationsUCInstance.conversationsListBox);
        ConversationsPage.ConversationsUCInstance.conversationsListBox.OnRefresh = (Action) (() => ConversationsPage.ConversationsUCInstance.ConversationsVM.RefreshConversations(false));
        if (ShareContentDataProviderManager.RetrieveDataProvider() is ShareExternalContentDataProvider)
          this.NavigationService.ClearBackStack();
        this._isInitialized = true;
      }
      ConversationsPage.ConversationsUCInstance.PrepareForViewIfNeeded();
    }

    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
    {
      base.OnRemovedFromJournal(e);
      this.ContentPanel.Children.Remove((UIElement) ConversationsPage.ConversationsUCInstance);
    }
  }
}
