using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ConversationHeaderUC : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public bool IsLookup { get; set; }

    public ConversationHeaderUC()
    {
      this.InitializeComponent();
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
      ConversationHeader conversation = contextMenu.DataContext as ConversationHeader;
      if (!(frameworkElement.DataContext is MenuItemData))
        return;
      MenuItemData menuItemData = frameworkElement.DataContext as MenuItemData;
      if (menuItemData.Tag == "delete" && MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteDialog, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        ConversationsViewModel.Instance.DeleteConversation(conversation);
      if (!(menuItemData.Tag == "disableEnable"))
        return;
      ConversationsViewModel.Instance.SetInProgressMain(true, "");
      conversation.DisableEnableNotifications((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        ConversationsViewModel.Instance.SetInProgressMain(false, "");
        if (res)
          return;
        ExtendedMessageBox.ShowSafe(CommonResources.Error);
      }))));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ConversationHeaderUC.xaml", UriKind.Relative));
    }
  }
}
