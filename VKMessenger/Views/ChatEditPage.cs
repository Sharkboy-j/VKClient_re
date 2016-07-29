using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ChatEditPage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonAdd = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Add
    };
    private ApplicationBarIconButton _appBarButtonSave = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/check.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Save
    };
    private ApplicationBarIconButton _appBarButtonChange = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.manage.rest.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Change
    };
    private ApplicationBarIconButton _appBarButtonExclude = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.minus.rest.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Exclude
    };
    private ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Cancel
    };
    private ApplicationBar _defaultAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private ApplicationBar _editAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private ApplicationBarMenuItem _appBarMenuItemLeave = new ApplicationBarMenuItem()
    {
      Text = CommonResources.ChatEdit_AppBar_Leave
    };
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal TextBox textBoxChatName;
    internal MultiselectList listBoxChatParticipants;
    private bool _contentLoaded;

    private bool IsInEditMode
    {
      get
      {
        return this.listBoxChatParticipants.IsSelectionEnabled;
      }
    }

    private bool HaveSelectedItems
    {
      get
      {
        if (this.listBoxChatParticipants.SelectedItems != null)
          return this.listBoxChatParticipants.SelectedItems.Count > 0;
        return false;
      }
    }

    private ChatEditViewModel ChatEditVM
    {
      get
      {
        return this.DataContext as ChatEditViewModel;
      }
    }

    public ChatEditPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonAdd.Click += new EventHandler(this._appBarButtonAdd_Click);
      this._appBarButtonSave.Click += new EventHandler(this._appBarButtonSave_Click);
      this._appBarButtonChange.Click += new EventHandler(this._appBarButtonChange_Click);
      this._appBarButtonExclude.Click += new EventHandler(this._appBarButtonExclude_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._appBarMenuItemLeave.Click += new EventHandler(this._appBarMenuItemLeave_Click);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonAdd);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonSave);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonChange);
      this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemLeave);
      this._editAppBar.Buttons.Add((object) this._appBarButtonExclude);
      this._editAppBar.Buttons.Add((object) this._appBarButtonCancel);
      this._editAppBar.MenuItems.Add((object) this._appBarMenuItemLeave);
    }

    private void UpdateAppBar()
    {
      if (this.IsInEditMode)
      {
        this._appBarButtonExclude.IsEnabled = this.HaveSelectedItems;
        this.ApplicationBar = (IApplicationBar) this._editAppBar;
      }
      else
      {
        this._appBarButtonSave.IsEnabled = this.ChatEditVM.CanSaveChat();
        this.ApplicationBar = (IApplicationBar) this._defaultAppBar;
      }
    }

    private void _appBarMenuItemLeave_Click(object sender, EventArgs e)
    {
      this.ChatEditVM.LeaveChat();
      this.NavigationService.RemoveBackEntrySafe();
      this.NavigationService.GoBackSafe();
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      this.listBoxChatParticipants.IsSelectionEnabled = false;
      this.UpdateAppBar();
    }

    private void _appBarButtonExclude_Click(object sender, EventArgs e)
    {
      List<ChatParticipant> chatParticipantList1 = new List<ChatParticipant>();
      bool flag1 = false;
      bool flag2 = false;
      if (this.listBoxChatParticipants.SelectedItems != null && this.listBoxChatParticipants.SelectedItems.Count > 0)
      {
        int count = this.listBoxChatParticipants.SelectedItems.Count;
        foreach (object selectedItem in (IEnumerable) this.listBoxChatParticipants.SelectedItems)
          chatParticipantList1.Add(selectedItem as ChatParticipant);
        flag2 = chatParticipantList1.Any<ChatParticipant>((Func<ChatParticipant, bool>) (m => (long) m.ChatUser.uid == AppGlobalStateManager.Current.LoggedInUserId));
        List<ChatParticipant> chatParticipantList2 = this.ChatEditVM.ExcludeMembers(chatParticipantList1);
        if (count == chatParticipantList2.Count)
          ExtendedMessageBox.ShowSafe(CommonResources.ChatEdit_CannotExclude);
        else if (chatParticipantList2.Count > 0)
          ExtendedMessageBox.ShowSafe(CommonResources.ChatEdit_CannotExcludeSome);
        else
          flag1 = true;
      }
      if (flag2)
      {
        this.NavigationService.RemoveBackEntrySafe();
        this.NavigationService.GoBackSafe();
      }
      else
      {
        if (!flag1)
          return;
        this.listBoxChatParticipants.IsSelectionEnabled = false;
        this.UpdateAppBar();
      }
    }

    private void _appBarButtonChange_Click(object sender, EventArgs e)
    {
      this.listBoxChatParticipants.IsSelectionEnabled = true;
      this.UpdateAppBar();
    }

    private void _appBarButtonSave_Click(object sender, EventArgs e)
    {
      this.ChatEditVM.SaveChat();
      this.Focus();
      this.UpdateAppBar();
    }

    private void _appBarButtonAdd_Click(object sender, EventArgs e)
    {
      ObservableCollection<ChatParticipant> chatParticipants = this.ChatEditVM.ChatParticipants;
      Func<ChatParticipant, bool> func = (Func<ChatParticipant, bool>) (c =>
      {
        if (c.ChatUser != null)
          return (long) c.ChatUser.uid == AppGlobalStateManager.Current.LoggedInUserId;
        return false;
      });

      Func<ChatParticipant, bool> predicate = new Func<ChatParticipant, bool>(c => { return c.ChatUser != null && (long)c.ChatUser.uid == AppGlobalStateManager.Current.LoggedInUserId; });
	
      int currentCountInChat = chatParticipants.Any<ChatParticipant>(predicate) ? this.ChatEditVM.ChatParticipants.Count - 1 : this.ChatEditVM.ChatParticipants.Count;
      if (currentCountInChat < VKConstants.MaxChatCount)
      {
        Navigator.Current.NavigateToPickUser(false, 0L, true, currentCountInChat, PickUserMode.PickForMessage, "", 0);
      }
      else
      {
        int num = (int) MessageBox.Show(CommonResources.NoMoreThan30MayTakePartInConversation);
      }
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        ChatEditViewModel chatEditViewModel = new ChatEditViewModel(long.Parse(this.NavigationContext.QueryString["chat_id"]));
        this.DataContext = (object) chatEditViewModel;
        chatEditViewModel.LoadChatInfoAsync();
        this._isInitialized = true;
      }
      if (e.NavigationMode == NavigationMode.Back)
      {
        List<User> selectedUsers = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
        if (selectedUsers != null)
          this.ChatEditVM.AddUsersToChat(selectedUsers);
      }
      this.HandleInputParameters();
      this.UpdateAppBar();
    }

    private void HandleInputParameters()
    {
      List<Stream> streamList = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      Rect rect = new Rect();
      if (ParametersRepository.Contains("UserPicSquare"))
        rect = (Rect) ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
      if (streamList == null || streamList.Count <= 0)
        return;
      this.ChatEditVM.UpdateChatPhoto(streamList[0], rect);
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (!this.listBoxChatParticipants.IsSelectionEnabled)
        return;
      this.listBoxChatParticipants.IsSelectionEnabled = false;
      e.Cancel = true;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.ChatEditVM.ChatName = this.textBoxChatName.Text;
      this.UpdateAppBar();
    }

    private void listBoxChatParticipants_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void listBoxChatParticipants_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.listBoxChatParticipants.IsSelectionEnabled)
        return;
      try
      {
        MultiselectItem multiselectItem = this.listBoxChatParticipants.ItemContainerGenerator.ContainerFromItem((object) ((sender as FrameworkElement).DataContext as ChatParticipant)) as MultiselectItem;
        int num = !multiselectItem.IsSelected ? 1 : 0;
        multiselectItem.IsSelected = num != 0;
      }
      catch
      {
      }
    }

    private void Friend_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.listBoxChatParticipants.IsSelectionEnabled)
        return;
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      ChatParticipant chatParticipant = frameworkElement.DataContext as ChatParticipant;
      if (chatParticipant == null)
        return;
      if (chatParticipant.IsUser)
        Navigator.Current.NavigateToUserProfile((long) chatParticipant.ChatUser.uid, chatParticipant.ChatUser.Name, "", false);
      else
        Navigator.Current.NavigateToGroup((long) chatParticipant.ChatUser.id, chatParticipant.FullName, false);
    }

    private void DeletePhoto_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ChatEditVM.DeleteChatPhoto();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ChatEditPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.textBoxChatName = (TextBox) this.FindName("textBoxChatName");
      this.listBoxChatParticipants = (MultiselectList) this.FindName("listBoxChatParticipants");
    }
  }
}
