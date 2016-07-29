using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ConversationsUC : ConversationsUCBase
  {
    private static List<ConversationsUC> _previousInstances = new List<ConversationsUC>();
    private List<UserControlVirtualizable> _headerUCPool = new List<UserControlVirtualizable>();
    //private int _cnt;
    private bool _isShown;
    private FrameworkElement _element;
    internal Grid LayoutRoot;
    internal ViewportControl scrollConversations;
    internal StackPanel stackPanelConversations;
    internal MyVirtualizingPanel2 conversationsListBox;
    private bool _contentLoaded;

    public ConversationsViewModel ConversationsVM
    {
      get
      {
        return this.DataContext as ConversationsViewModel;
      }
    }

    public bool IsLookup { get; set; }

    public bool PreventFromClearing { get; set; }

    public ConversationsUC()
    {
      this.InitializeComponent();
      this.DataContext = (object) ConversationsViewModel.Instance;
      Logger.Instance.Info("ConversationUC created");
      this.Loaded += new RoutedEventHandler(this.ConversationsUC_Loaded);
      this.scrollConversations.BindViewportBoundsTo((FrameworkElement) this.stackPanelConversations);
      this.conversationsListBox.LoadUnloadThreshold = 100.0;
      this.conversationsListBox.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.conversationsListBox_ScrollPositionChanged);
      this.conversationsListBox.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollConversations), false);
      this.conversationsListBox.CreateVirtItemFunc = (Func<object, IVirtualizable>) (obj => (IVirtualizable) new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
      {
        if (this.ConversationsVM.ConversationsGenCol.Refreshing)
          this._headerUCPool.Clear();
        bool haveEmoji = (obj as ConversationHeader).HaveEmoji;
        UserControlVirtualizable controlVirtualizable = this.IsShareContentMode ? this._headerUCPool.FirstOrDefault<UserControlVirtualizable>((Func<UserControlVirtualizable, bool>) (ch => ch is ConversationHeaderShareUC)) : (haveEmoji ? this._headerUCPool.FirstOrDefault<UserControlVirtualizable>((Func<UserControlVirtualizable, bool>) (ch => ch is ConversationHeaderUCEmoji)) : this._headerUCPool.FirstOrDefault<UserControlVirtualizable>((Func<UserControlVirtualizable, bool>) (ch => ch is ConversationHeaderUC)));
        if (controlVirtualizable != null)
        {
          this._headerUCPool.Remove(controlVirtualizable);
        }
        else
        {
          if (!this.IsShareContentMode)
          {
            if (haveEmoji)
            {
              ConversationHeaderUCEmoji conversationHeaderUcEmoji = new ConversationHeaderUCEmoji();
              int num = this.IsLookup ? 1 : 0;
              conversationHeaderUcEmoji.IsLookup = num != 0;
              controlVirtualizable = (UserControlVirtualizable) conversationHeaderUcEmoji;
            }
            else
            {
              ConversationHeaderUC conversationHeaderUc = new ConversationHeaderUC();
              int num = this.IsLookup ? 1 : 0;
              conversationHeaderUc.IsLookup = num != 0;
              controlVirtualizable = (UserControlVirtualizable) conversationHeaderUc;
            }
            controlVirtualizable.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.ChucOnTap);
          }
          else
          {
            ConversationHeaderShareUC conversationHeaderShareUc = new ConversationHeaderShareUC();
            int num = this.IsLookup ? 1 : 0;
            conversationHeaderShareUc.IsLookup = num != 0;
            controlVirtualizable = (UserControlVirtualizable) conversationHeaderShareUc;
            controlVirtualizable.Tap += (EventHandler<System.Windows.Input.GestureEventArgs>) ((sender, args) => this.OnConversationTap((Action) (() => this.ChucOnTap(sender, args))));
          }
          controlVirtualizable.Width = 480.0;
        }
        controlVirtualizable.DataContext = obj;
        return controlVirtualizable;
      }), (Func<double>) (() => !this.IsShareContentMode ? 102.0 : 74.0), (Action<UserControlVirtualizable>) (uc =>
      {
        uc.DataContext = null;
        this._headerUCPool.Add(uc);
      }), 0.0, false));
    }

    private void ChucOnTap(object sender, System.Windows.Input.GestureEventArgs args)
    {
      ConversationHeader conversationHeader = (sender as FrameworkElement).DataContext as ConversationHeader;
      if (conversationHeader == null)
        return;
      bool isChat = true;
      long userOrChatId = (long) conversationHeader._message.chat_id;
      if (userOrChatId == 0L)
      {
        isChat = false;
        userOrChatId = (long) conversationHeader._message.uid;
      }
      Navigator.Current.NavigateToConversation(userOrChatId, isChat, this.IsLookup, "", 0L, false);
    }

    private void HandleTap(object s)
    {
      ConversationHeader conversationHeader = (s as FrameworkElement).DataContext as ConversationHeader;
      if (conversationHeader == null)
        return;
      bool isChat = true;
      long userOrChatId = (long) conversationHeader._message.chat_id;
      if (userOrChatId == 0L)
      {
        isChat = false;
        userOrChatId = (long) conversationHeader._message.uid;
      }
      Navigator.Current.NavigateToConversation(userOrChatId, isChat, this.IsLookup, "", 0L, false);
    }

    private void conversationsListBox_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
    }

    private void ConversationsUC_Loaded(object sender, RoutedEventArgs e)
    {
      foreach (ConversationsUC previousInstance in ConversationsUC._previousInstances)
      {
        if (previousInstance != this)
          previousInstance.DataContext = null;
      }
      ConversationsUC._previousInstances.Clear();
      if (!this.PreventFromClearing)
        ConversationsUC._previousInstances.Add(this);
      Logger.Instance.Info("ConversationUC loaded");
    }

    public override void PrepareForViewIfNeeded()
    {
      if (this.ConversationsVM == null || this._isShown && !this.ConversationsVM.NeedRefresh)
        return;
      ConversationsViewModel.Instance.RefreshConversations(true);
      this._isShown = true;
    }

    private void OnCompression(object sender, CompressionEventArgs e)
    {
      if (e.Type != CompressionType.Bottom)
        return;
      this.ConversationsVM.LoadMoreConversations((Action) (() => {}));
    }

    private void conversationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void DeleteConversation(ConversationHeader ch)
    {
      if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteDialog, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ConversationsVM.DeleteConversation(ch);
    }

    private void MenuItemTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      DependencyObject reference = (DependencyObject) frameworkElement;
      while (!(reference is ContextMenu))
        reference = VisualTreeHelper.GetParent(reference);
      ContextMenu contextMenu = reference as ContextMenu;
      int num = 0;
      contextMenu.IsOpen = num != 0;
      ConversationHeader ch = contextMenu.DataContext as ConversationHeader;
      if (!(frameworkElement.DataContext is MenuItemData))
        return;
      MenuItemData menuItemData = frameworkElement.DataContext as MenuItemData;
      if (menuItemData.Tag == "delete")
        this.DeleteConversation(ch);
      if (!(menuItemData.Tag == "disableEnable"))
        return;
      this.ConversationsVM.SetInProgressMain(true, "");
      ch.DisableEnableNotifications((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.ConversationsVM.SetInProgressMain(false, "");
        if (res)
          return;
        ExtendedMessageBox.ShowSafe(CommonResources.Error);
      }))));
    }

    public override void SetListHeader(FrameworkElement element)
    {
      if (this._element != null)
        this.stackPanelConversations.Children.Remove((UIElement) this._element);
      if (element == null)
        return;
      this._element = element;
      this.stackPanelConversations.Children.Insert(0, (UIElement) this._element);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ConversationsUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.scrollConversations = (ViewportControl) this.FindName("scrollConversations");
      this.stackPanelConversations = (StackPanel) this.FindName("stackPanelConversations");
      this.conversationsListBox = (MyVirtualizingPanel2) this.FindName("conversationsListBox");
    }
  }
}
