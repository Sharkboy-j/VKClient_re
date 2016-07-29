using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
//using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Backend;
using VKMessenger.Library;
using VKMessenger.Library.Events;
using VKMessenger.Library.VirtItems;

namespace VKMessenger.Views
{
  public class ConversationPage : PageBase, IScroll, INotifyPropertyChanged, IHandle<MessageActionEvent>, IHandle, IHandle<SpriteElementTapEvent>, IHandle<StickerItemTapEvent>
  {
    private readonly ApplicationBar _defaultAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private readonly ApplicationBar _appBarAttachments = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private readonly ApplicationBar _appBarSelection = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor
    };
    private readonly PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
    {
      ShowCamera = true
    };
    private readonly ApplicationBarIconButton _appBarButtonSend = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.send.text.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Send
    };
    private readonly ApplicationBarIconButton _appBarButtonAttachImage = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.feature.camera.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_AttachImage
    };
    private readonly ApplicationBarIconButton _appBarButtonAddAttachment = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/attach.png", UriKind.Relative),
      Text = CommonResources.NewPost_AppBar_AddAttachment
    };
    private readonly ApplicationBarIconButton _appBarButtonAttachments = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.attachments-1.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Attachments
    };
    private readonly ApplicationBarIconButton _appBarButtonСhoose = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.manage.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Choose
    };
    private readonly ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Cancel
    };
    private readonly ApplicationBarIconButton _appBarButtonForward = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.forward.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Forward
    };
    private readonly ApplicationBarIconButton _appBarButtonDelete = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.delete.rest.png", UriKind.Relative),
      Text = CommonResources.Conversation_AppBar_Delete
    };
    private readonly ApplicationBarIconButton _appBarButtonEmojiToggle = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.smile.png", UriKind.Relative),
      Text = "emoji"
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemDisableEnableNotifications = new ApplicationBarMenuItem()
    {
      Text = CommonResources.TurnOffNotifications
    };
    private readonly ApplicationBarMenuItem _appbarMenuItemPinToStart = new ApplicationBarMenuItem()
    {
      Text = CommonResources.PinToStart
    };
    private readonly ApplicationBarMenuItem _appbarMenuItemShowMaterials = new ApplicationBarMenuItem()
    {
      Text = CommonResources.Messenger_ShowMaterials
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemManageChat = new ApplicationBarMenuItem()
    {
      Text = CommonResources.Conversation_AppBar_ManageChat
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemDeleteDialog = new ApplicationBarMenuItem()
    {
      Text = CommonResources.Conversation_AppBar_DeleteDialog
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemRefresh = new ApplicationBarMenuItem()
    {
      Text = CommonResources.Conversation_AppBar_Refresh
    };
    private readonly ApplicationBarMenuItem _appBarMenuItemAddMember = new ApplicationBarMenuItem()
    {
      Text = CommonResources.Conversation_AppBar_AddMember
    };
    //private GeoCoordinate _position;
    private bool _isInitialized;
    //private bool _needScrollBottom;
    private readonly DateTime _createdTimestamp;
    private ConversationItems _conversationItems;
    private static int TotalCount;
    private PickerUC _pickerUC;
    private long _userOrChatId;
    private bool _isChat;
    private long _startMessageId;
    //private bool _loadedStartMessageId;
    private bool _isCurrent;
    //private bool _canDettachProductAttachment;
    private IShareContentDataProvider _shareContentDataProvider;
    private bool _needCleanupOnNavigatedFrom;
    private bool _shouldScrollToUnreadItem;
    private long _messageIdToScrollTo;
    internal Grid LayoutRoot;
    internal Grid gridHeader;
    internal StackPanel TitlePanel;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockSubtitleVertical;
    internal ContextMenu FriendOptionsMenu;
    internal MenuItem menuItemRefresh;
    internal MenuItem menuItemPinToStart;
    internal MenuItem menuItemShowMaterials;
    internal MenuItem menuItemDisableEnableNotifications;
    internal MenuItem menuItemAddMember;
    internal MenuItem menuItemDeleteDialog;
    internal Grid ContentPanel;
    internal ViewportControl myScroll;
    internal MyVirtualizingPanel2 myPanel;
    internal NewMessageUC ucNewMessage;
    private bool _contentLoaded;

    private ConversationPage.Mode CurrentMode
    {
      get
      {
        return !this.ConversationVM.IsInSelectionMode ? ConversationPage.Mode.Default : ConversationPage.Mode.Selection;
      }
      set
      {
        this.ConversationVM.IsInSelectionMode = value == ConversationPage.Mode.Selection;
        this.UpdateAppBar();
      }
    }

    public ConversationViewModel ConversationVM
    {
      get
      {
        return this.DataContext as ConversationViewModel;
      }
    }

    public ConversationItems ConversationItems
    {
      get
      {
        return this._conversationItems;
      }
      set
      {
        this._conversationItems = value;
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs("ConversationItems"));
      }
    }

    private NewPostUC ucNewPost
    {
      get
      {
        return this.ucNewMessage.UCNewPost;
      }
    }

    private ScrollViewer scrollNewMessage
    {
      get
      {
        return this.ucNewMessage.ScrollNewMessage;
      }
    }

    private TextBox textBoxNewMessage
    {
      get
      {
        return this.ucNewPost.TextBoxPost;
      }
    }

    public bool IsManipulating
    {
      get
      {
        return (uint) this.myScroll.ManipulationState > 0U;
      }
    }

    public double VerticalOffset
    {
      get
      {
        return this.myScroll.Viewport.Y;
      }
    }

    public bool IsHorizontalOrientation
    {
      get
      {
        if (this.Orientation != PageOrientation.Landscape && this.Orientation != PageOrientation.LandscapeLeft)
          return this.Orientation == PageOrientation.LandscapeRight;
        return true;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ConversationPage()
    {
      ++ConversationPage.TotalCount;
      this.InitializeComponent();
      this.myScroll.BindViewportBoundsTo((FrameworkElement) this.myPanel);
      this.Loaded += new RoutedEventHandler(this.ConversationPage_Loaded);
      this.myPanel.Compression += new MyVirtualizingPanel2.OnCompression(this.OnCompression);
      this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
      this._appBarButtonSend.Click += new EventHandler(this._appBarButtonSend_Click);
      this._appBarButtonAttachImage.Click += new EventHandler(this._appBatButtonAttachImage_Click);
      this._appBarButtonEmojiToggle.Click += new EventHandler(this._appBarButtonEmojiToggle_Click);
      this._appBarButtonAddAttachment.Click += new EventHandler(this._appBarButtonAddAttachment_Click);
      this._appBarButtonAttachments.Click += new EventHandler(this._appBarButtonAttachments_Click);
      this._appBarButtonСhoose.Click += new EventHandler(this._appBarButtonСhoose_Click);
      this._appBarButtonForward.Click += new EventHandler(this._appBarButtonForward_Click);
      this._appBarButtonDelete.Click += new EventHandler(this._appBarButtonDelete_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._appBarMenuItemManageChat.Click += new EventHandler(this._appBarMenuItemManageChat_Click);
      this._appBarMenuItemDeleteDialog.Click += new EventHandler(this._appBarMenuItemDeleteDialog_Click);
      this._appBarMenuItemRefresh.Click += new EventHandler(this._appBarMenuItemRefresh_Click);
      this._appBarMenuItemAddMember.Click += new EventHandler(this._appBarMenuItemAddMember_Click);
      this._appBarMenuItemDisableEnableNotifications.Click += new EventHandler(this._appBarMenuItemDisableEnableNotifications_Click);
      this._appbarMenuItemPinToStart.Click += new EventHandler(this._appbarMenuItemPinToStart_Click);
      this._appbarMenuItemShowMaterials.Click += new EventHandler(this._appbarMenuItemShowMaterials_Click);
      this._createdTimestamp = DateTime.Now;
      this.myPanel.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.myScroll), true);
      this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(this.ConversationPage_OrientationChanged);
      this.myPanel.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.myPanel_ScrollPositionChanged);
      this.myPanel.ManuallyLoadMore = true;
      this.myPanel.KeepScrollPositionWhenAddingItems = true;
      this.RegisterForCleanup((IMyVirtualizingPanel) this.myPanel);
      this.ucNewPost.TextBoxPost.TextChanged += new TextChangedEventHandler(this.textBoxNewMessage_TextChanged);
      this.ucNewPost.TextBoxPost.GotFocus += new RoutedEventHandler(this.textBoxNewMessage_GotFocus);
      this.ucNewPost.TextBoxPost.LostFocus += new RoutedEventHandler(this.textBoxNewMessage_LostFocus);
      this.ucNewPost.TextBlockWatermarkText.Text = CommonResources.Group_SendAMessage;
      this.ucNewMessage.OnAddAttachTap = (Action) (() => this.AddAttachTap(null, (System.Windows.Input.GestureEventArgs) null));
      this.ucNewMessage.OnSendTap = (Action) (() => this.SendTap(null, (System.Windows.Input.GestureEventArgs) null));
      this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelOpenClosed);
      Binding binding = new Binding("OutboundMessageVm.Attachments");
      this.ucNewPost.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
      this.ucNewPost.OnImageDeleteTap = (Action<object>) (sender =>
      {
        FrameworkElement frameworkElement = sender as FrameworkElement;
        if (frameworkElement != null)
          this.ConversationVM.OutboundMessageVm.RemoveAttachment(frameworkElement.DataContext as IOutboundAttachment);
        this.UpdateAppBar();
      });
      this.SuppressOpenMenuTapArea = true;
    }

    ~ConversationPage()
    {
      --ConversationPage.TotalCount;
    }

    private void PanelOpenClosed(object sender, bool e)
    {
      this.UpdateHeaderVisibility();
    }

    private void UpdateHeaderVisibility()
    {
      this.gridHeader.Visibility = !FramePageUtils.IsHorizontal || !this.ucNewMessage.PanelControl.IsOpen && !this.ucNewMessage.PanelControl.IsTextBoxTargetFocused ? Visibility.Visible : Visibility.Collapsed;
    }

    private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
    {
    }

    protected override void TextBoxPanelIsOpenedChanged(object sender, bool e)
    {
      this.UpdateAppBar();
    }

    private void myPanel_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
      if (e.ScrollHeight != 0.0 && e.ScrollHeight - e.CurrentPosition < VKConstants.LoadMoreNewsThreshold)
      {
        this.ConversationVM.LoadMoreConversations((Action<bool>) null);
      }
      else
      {
        if (e.ScrollHeight == 0.0 || e.CurrentPosition >= 100.0)
          return;
        this.ConversationVM.LoadNewerConversations((Action<bool>) null);
      }
    }

    private void _appbarMenuItemPinToStart_Click(object sender, EventArgs e)
    {
      this.ConversationVM.PinToStart();
    }

    private void _appbarMenuItemShowMaterials_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToConversationMaterials(MessagesService.Instance.GetPeerId(this.ConversationVM.UserOrCharId, this.ConversationVM.IsChat));
    }

    private void _appBarMenuItemDisableEnableNotifications_Click(object sender, EventArgs e)
    {
      this.ConversationVM.DisableEnableNotifications((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!res)
          new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
        this.UpdateAppBar();
      }))));
    }

    private void ConversationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateMargins();
      this.UpdateHeaderVisibility();
      bool p = e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight;
      SystemTray.IsVisible = !p;
      foreach (IMyVirtualizingPanel panel in this._panels)
        panel.RespondToOrientationChange(p);
    }

    private void UpdateMargins()
    {
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (e.Cancel || this.CurrentMode != ConversationPage.Mode.Selection)
        return;
      this.CurrentMode = ConversationPage.Mode.Default;
      e.Cancel = true;
    }

    private void _appBarButtonAddAttachment_Click(object sender, EventArgs e)
    {
      this._pickerUC = PickerUC.PickAttachmentTypeAndNavigate(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, null, (Action) (() => Navigator.Current.NavigateToPhotoPickerPhotos(10, false, false)));
    }

    private void _appBarMenuItemAddMember_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToPickUser(true, this._userOrChatId, false, 0, PickUserMode.PickForMessage, "", 0);
    }

    private void _appBarMenuItemRefresh_Click(object sender, EventArgs e)
    {
      this.ConversationVM.RefreshConversations();
    }

    private void _appBarMenuItemDeleteDialog_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteDialog, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ConversationVM.DeleteDialog();
      ObservableCollection<ConversationHeader> conversations = ConversationsViewModel.Instance.Conversations;
      ConversationHeader conversationHeader = conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>) (c =>
      {
        if (c.IsChat == this._isChat)
          return c.UserOrChatId == this._userOrChatId;
        return false;
      }));
      if (conversationHeader != null)
        conversations.Remove(conversationHeader);
      this.NavigationService.GoBackSafe();
    }

    private void _appBarMenuItemManageChat_Click(object sender, EventArgs e)
    {
      this.ManageChatIfApplicable();
    }

    private void ManageChatIfApplicable()
    {
      if (!this.ConversationVM.IsChat)
        return;
      this.NavigationService.Navigate(new Uri(string.Format("/VKMessenger;component/Views/ChatEditPage.xaml?chat_id={0}", (object) this.ConversationVM.UserOrCharId), UriKind.Relative));
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
      Logger.Instance.Info("Back from photo chooser");
      if (e.TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", (object) e.ChosenPhoto);
    }

    private void UpdateAppBar()
    {
      if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
        return;
      if (this.CurrentMode == ConversationPage.Mode.Selection)
      {
        this.ApplicationBar = (IApplicationBar) this._appBarSelection;
        ApplicationBarIconButton applicationBarIconButton1 = this._appBarButtonDelete;
        ApplicationBarIconButton applicationBarIconButton2 = this._appBarButtonForward;
        ObservableCollection<MessageViewModel> messages = this.ConversationVM.Messages;
        int num1;
        bool flag = (num1 = messages.Any<MessageViewModel>((Func<MessageViewModel, bool>) (m => m.IsSelected)) ? 1 : 0) != 0;
        applicationBarIconButton2.IsEnabled = num1 != 0;
        int num2 = flag ? 1 : 0;
        applicationBarIconButton1.IsEnabled = num2 != 0;
      }
      else
      {
        int num1 = this.ConversationVM.OutboundMessageVm != null ? this.ConversationVM.OutboundMessageVm.Attachments.Count : 0;
        int num2 = 0;
        if (num1 > 0 || num2 > 0)
          this._appBarButtonAttachments.IconUri = new Uri(string.Format("./Resources/appbar.attachments-{0}.rest.png", (object) Math.Min(num1 + num2, 10)), UriKind.Relative);
        this.UpdateSendButtonState();
        this.ApplicationBar = (IApplicationBar) null;
      }
      this.ucNewMessage.Visibility = this.CurrentMode == ConversationPage.Mode.Selection ? Visibility.Collapsed : Visibility.Visible;
      this._appbarMenuItemPinToStart.IsEnabled = !SecondaryTileManager.Instance.TileExistsForConversation(this._userOrChatId, this._isChat);
      this._appBarMenuItemDisableEnableNotifications.Text = this.ConversationVM.AreNotificationsDisabled ? CommonResources.TurnOnNotifications : CommonResources.TurnOffNotifications;
    }

    private void UpdateSendButtonState()
    {
      this._appBarButtonSend.IsEnabled = (this.ConversationVM.OutboundMessageVm != null ? this.ConversationVM.OutboundMessageVm.Attachments.Count : 0) > 0 || !string.IsNullOrWhiteSpace(this.textBoxNewMessage.Text);
      this.ucNewMessage.UpdateSendButton(this._appBarButtonSend.IsEnabled);
    }

    private void _appBarButtonСhoose_Click(object sender, EventArgs e)
    {
      this.CurrentMode = ConversationPage.Mode.Selection;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      this.CurrentMode = ConversationPage.Mode.Default;
    }

    private void _appBarButtonDelete_Click(object sender, EventArgs e)
    {
      List<MessageViewModel> list = this.ConversationVM.Messages.Where<MessageViewModel>((Func<MessageViewModel, bool>) (m => m.IsSelected)).ToList<MessageViewModel>();
      if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, list.Count == 1 ? CommonResources.Conversation_DeleteMessage : CommonResources.Conversation_DeleteMessages, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ConversationVM.DeleteMessages(list, null);
      this.ConversationVM.IsInSelectionMode = false;
      this.UpdateAppBar();
      this.Focus();
    }

    private void _appBarButtonForward_Click(object sender, EventArgs e)
    {
      List<Message> list = this.ConversationVM.Messages.Where<MessageViewModel>((Func<MessageViewModel, bool>) (m => m.IsSelected)).Select<MessageViewModel, Message>((Func<MessageViewModel, Message>) (m => m.Message)).Where<Message>((Func<Message, bool>) (m => (uint) m.mid > 0U)).ToList<Message>();
      ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
      contentDataProvider.ForwardedMessages = list;
      contentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
      this.ConversationVM.IsInSelectionMode = false;
      this.UpdateAppBar();
      Navigator.Current.NavigateToPickConversation();
    }

    private void _appBarButtonAttachments_Click(object sender, EventArgs e)
    {
      this.NavigationService.Navigate(new Uri(string.Format("/VKMessenger;component/Views/ManageAttachmentsPage.xaml?{0}={1}&{2}={3}", (object) NavigationParametersNames.IsChat, (object) this.ConversationVM.IsChat, (object) NavigationParametersNames.UserOrChatId, (object) this.ConversationVM.UserOrCharId), UriKind.Relative));
    }

    private void _appBatButtonAttachImage_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(this.ConversationVM.OutboundMessageVm.NumberOfAttAllowedToAdd, false, false);
    }

    private void ConversationPage_Loaded(object sender, RoutedEventArgs e)
    {
      Logger.Instance.Info("Conversation page loaded in {0} ms. ", (object) (DateTime.Now - this._createdTimestamp).TotalMilliseconds);
      Logger.Instance.Info("ConversationPage_Loaded");
      Stopwatch stopwatch = Stopwatch.StartNew();
      if (this.ConversationItems == null)
      {
        this.ConversationItems = new ConversationItems(this.ConversationVM);
        if (this._shouldScrollToUnreadItem)
        {
          this.ScrollToUnreadItem();
          this._shouldScrollToUnreadItem = false;
        }
        if (this._messageIdToScrollTo != 0L)
        {
          this.ScrollToMessageId(this._messageIdToScrollTo);
          this._messageIdToScrollTo = 0L;
        }
      }
      this.myPanel.DataContext = (object) this;
      this.myPanel.SetBinding(MyVirtualizingPanel2.ItemsSourceProperty, new Binding("ConversationItems.Messages"));
      stopwatch.Stop();
      Logger.Instance.Info("MyPanel set context in {0} ms.", (object) stopwatch.ElapsedMilliseconds);
      this.HandleOrientationChange();
    }

    private void BuildAppBar()
    {
      this._defaultAppBar.Opacity = 0.9;
      this._appBarSelection.Opacity = 0.99;
      this._appBarSelection.StateChanged += new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged);
      this._defaultAppBar.StateChanged += new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged);
      this._appBarAttachments.StateChanged += new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged);
      this._appBarAttachments.Opacity = 0.9;
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonSend);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonEmojiToggle);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonAddAttachment);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonСhoose);
      this._defaultAppBar.MenuItems.Add((object) this._appbarMenuItemPinToStart);
      this._appBarAttachments.MenuItems.Add((object) this._appbarMenuItemPinToStart);
      this._defaultAppBar.MenuItems.Add((object) this._appbarMenuItemShowMaterials);
      this._appBarAttachments.MenuItems.Add((object) this._appbarMenuItemShowMaterials);
      this._appBarAttachments.Buttons.Add((object) this._appBarButtonSend);
      this._appBarAttachments.Buttons.Add((object) this._appBarButtonEmojiToggle);
      this._appBarAttachments.Buttons.Add((object) this._appBarButtonAttachments);
      this._appBarAttachments.Buttons.Add((object) this._appBarButtonСhoose);
      this._appBarSelection.Buttons.Add((object) this._appBarButtonForward);
      this._appBarSelection.Buttons.Add((object) this._appBarButtonDelete);
      this._appBarSelection.Buttons.Add((object) this._appBarButtonCancel);
      this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemRefresh);
      this._appBarAttachments.MenuItems.Add((object) this._appBarMenuItemRefresh);
      if (this._isChat)
      {
        this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemManageChat);
        this._appBarAttachments.MenuItems.Add((object) this._appBarMenuItemManageChat);
        this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemDisableEnableNotifications);
        this._appBarAttachments.MenuItems.Add((object) this._appBarMenuItemDisableEnableNotifications);
      }
      else
      {
        this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemAddMember);
        this._appBarAttachments.MenuItems.Add((object) this._appBarMenuItemAddMember);
      }
      this._defaultAppBar.MenuItems.Add((object) this._appBarMenuItemDeleteDialog);
      this._appBarAttachments.MenuItems.Add((object) this._appBarMenuItemDeleteDialog);
    }

    private void _defaultAppBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
    {
    }

    private void _appBarButtonSend_Click(object sender, EventArgs e)
    {
      this.SendMessage();
    }

    private void SendMessage()
    {
      this.ConversationVM.SendMessage(this.textBoxNewMessage.Text);
      this.textBoxNewMessage.Text = string.Empty;
      this.ScrollToBottom(true, false);
      this.UpdateAppBar();
    }

    private void textBoxNewMessage_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void textBoxNewMessage_LostFocus(object sender, RoutedEventArgs e)
    {
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      base.HandleOnNavigatedTo(e);
      this._isCurrent = true;
      this.UpdateMargins();
      EventAggregator.Current.Subscribe((object) this);
      if (!this._isInitialized)
      {
        this._userOrChatId = long.Parse(this.NavigationContext.QueryString[NavigationParametersNames.UserOrChatId]);
        this._isChat = this.NavigationContext.QueryString[NavigationParametersNames.IsChat].ToLowerInvariant() == bool.TrueString.ToLowerInvariant();
        this._startMessageId = !this.NavigationContext.QueryString.ContainsKey("MessageId") ? 0L : long.Parse(this.NavigationContext.QueryString["MessageId"]);
        if (this._startMessageId == 0L)
          this._startMessageId = -1L;
        this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
        if (this._shareContentDataProvider is ShareExternalContentDataProvider)
        {
          this.NavigationService.ClearBackStack();
          this.SuppressMenu = true;
        }
        ConversationViewModel vm = ConversationViewModelCache.Current.GetVM(this._userOrChatId, this._isChat, false);
        if (this._startMessageId <= 0L)
          vm.TrimMessages();
        else
          vm.Messages.Clear();
        this.textBoxNewMessage.Text = vm.OutboundMessageVm.MessageText ?? "";
        vm.PropertyChanged += new PropertyChangedEventHandler(this.cvm_PropertyChanged);
        this.DataContext = (object) vm;
        this._isInitialized = true;
        //if (vm.Messages != null && vm.Messages.Count > 0)
        //  this._needScrollBottom = true;
        if (e.IsNavigationInitiator && this.NavigationContext.QueryString.ContainsKey("FromLookup") && this.NavigationContext.QueryString["FromLookup"] == bool.TrueString)
          this.NavigationService.RemoveBackEntrySafe();
        bool flag = this.NavigationContext.QueryString.ContainsKey("IsContactProductSellerMode") && string.Equals(this.NavigationContext.QueryString["IsContactProductSellerMode"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
        this.ConversationVM.CanDettachProductAttachment = !flag;
        OutboundMessageViewModel outboundMessageVm = this.ConversationVM.OutboundMessageVm;
        if (outboundMessageVm != null)
        {
          ObservableCollection<IOutboundAttachment> attachments = outboundMessageVm.Attachments;
          if (attachments != null)
          {
            foreach (IOutboundAttachment outboundAttachment in (Collection<IOutboundAttachment>) attachments)
            {
              OutboundProductAttachment productAttachment = outboundAttachment as OutboundProductAttachment;
              if (productAttachment != null && !productAttachment.CanDettach)
              {
                outboundMessageVm.RemoveAttachment((IOutboundAttachment) productAttachment);
                if (this.textBoxNewMessage.Text == CommonResources.ContactSellerMessage)
                {
                  this.textBoxNewMessage.Text = "";
                  break;
                }
                break;
              }
            }
            if (flag)
            {
              while (attachments.Count > 0)
                outboundMessageVm.RemoveAttachment(attachments[0]);
            }
          }
        }
        List<Message> messageList = (List<Message>) ParametersRepository.GetParameterForIdAndReset("MessagesToForward");
        if (messageList != null && messageList.Count > 0)
        {
          this.NavigationService.RemoveBackEntrySafe();
          this.ConversationVM.AddForwardedMessagesToOutboundMessage((IList<Message>) messageList);
        }
        this.BuildAppBar();
      }
      else
      {
        Logger.Instance.Info("FORCE SET VM");
        ConversationViewModelCache.Current.SetVM(this.ConversationVM, false);
      }
      this.ConversationVM.IsOnScreen = true;
      this.ConversationVM.EnsureConversationIsUpToDate(e.NavigationMode == NavigationMode.New, this._startMessageId, null);
      this._startMessageId = -1L;
      this.ConversationVM.LoadHeaderInfoAsync();
      this.ConversationVM.AddAttachmentsFromRepository();
      this.ConversationVM.Scroll = (IScroll) this;
      this.UpdateAppBar();
      if (ParametersRepository.Contains(NavigationParametersNames.NewMessageContents))
      {
        string @string = ParametersRepository.GetParameterForIdAndReset(NavigationParametersNames.NewMessageContents).ToString();
        if (!string.IsNullOrWhiteSpace(@string))
        {
          this.textBoxNewMessage.Text = @string;
          this.UpdateAppBar();
        }
      }

      if (ParametersRepository.Contains(NavigationParametersNames.Graffiti))//NEW: 4.8.0
      {
          GraffitiAttachmentItem graffitiAttachmentItem = ParametersRepository.GetParameterForIdAndReset(NavigationParametersNames.Graffiti) as GraffitiAttachmentItem;
          if (graffitiAttachmentItem != null)
          {
              this.ConversationVM.SendGraffiti(graffitiAttachmentItem);
              this.ScrollToBottom(true, false);
          }
      }


      DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(this.DeviceNetworkInformation_NetworkAvailabilityChanged);
      Logger.Instance.Info("ConversationPage, ViewModel hash code={0}", (object) this.ConversationVM.GetHashCode());
      CurrentMediaSource.AudioSource = StatisticsActionSource.messages;
      CurrentMediaSource.VideoSource = StatisticsActionSource.messages;
      CurrentMediaSource.GifPlaySource = StatisticsActionSource.messages;
      CurrentMarketItemSource.Source = MarketItemSource.im;
      stopwatch.Stop();
      if (e.NavigationMode != NavigationMode.Back)
        return;
      this.ScrollToBottomIfNeeded();
    }

    private void ScrollToBottomIfNeeded()
    {
      MessageViewModel messageViewModel = this.ConversationVM.Messages.LastOrDefault<MessageViewModel>();
      if (messageViewModel == null || messageViewModel.Message.@out != 1 || string.IsNullOrEmpty(messageViewModel.Message.action))
        return;
      this.ScrollToBottom(false, false);
    }

    private void cvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "AreNotificationsDisabled"))
        return;
      this.UpdateAppBar();
    }

    private void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
    {
      if (e.NotificationType != NetworkNotificationType.InterfaceConnected)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.ConversationVM == null)
          return;
        this.ConversationVM.EnsureConversationIsUpToDate(false, 0L, (Action<bool>) null);
      }));
    }

    protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      base.HandleOnNavigatingFrom(e);
      this.ConversationVM.RemoveUnreadMessagesItem();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      this._isCurrent = false;
      EventAggregator.Current.Unsubscribe((object) this);
      DeviceNetworkInformation.NetworkAvailabilityChanged -= new EventHandler<NetworkNotificationEventArgs>(this.DeviceNetworkInformation_NetworkAvailabilityChanged);
      this.ConversationVM.Scroll = (IScroll) null;
      this.ConversationVM.IsOnScreen = false;
      this.ConversationVM.OutboundMessageVm.MessageText = this.textBoxNewMessage.Text;
      this.ConversationVM.IsInSelectionMode = false;
      ConversationViewModelCache.Current.SetVM(this.ConversationVM, e.NavigationMode == NavigationMode.Back);
      if (this._needCleanupOnNavigatedFrom)
      {
        this.Cleanup();
        this._needCleanupOnNavigatedFrom = false;
      }
      if (e.NavigationMode != NavigationMode.Back || !(this._shareContentDataProvider is ShareExternalContentDataProvider))
        return;
      ObservableCollection<IOutboundAttachment> attachments = this.ConversationVM.OutboundMessageVm.Attachments;
      for (int index = 0; index < attachments.Count; ++index)
      {
        if (attachments[index] is OutboundUploadDocumentAttachment)
        {
          attachments.RemoveAt(index);
          --index;
        }
      }
    }

    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
    {
      base.OnRemovedFromJournal(e);
      if (!this._isCurrent)
        this.Cleanup();
      else
        this._needCleanupOnNavigatedFrom = true;
    }

    private void Cleanup()
    {
      if (this.ConversationItems != null)
        this.ConversationItems.Cleanup();
      if (this.ConversationVM != null)
        this.ConversationVM.PropertyChanged -= new PropertyChangedEventHandler(this.cvm_PropertyChanged);
      this.ucNewPost.ItemsControlAttachments.ClearValue(ItemsControl.ItemsSourceProperty);
      this.DataContext = null;
    }

    private void textBoxNewMessage_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSendButtonState();
      this.ConversationVM.UserIsTyping();
    }

    public void ScrollToUnreadItem()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.ConversationItems != null)
          this.ScrollToItem(this.ConversationItems.Messages.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (ci => (ci as MessageItem).MVM.Message.action == ConversationViewModel.UNREAD_ITEM_ACTION)));
        else
          this._shouldScrollToUnreadItem = true;
      }));
    }

    public void ScrollToMessageId(long messageId)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.ConversationItems != null)
          this.ScrollToItem(this.ConversationItems.Messages.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (ci => (long) (ci as MessageItem).MVM.Message.id == messageId)));
        else
          this._messageIdToScrollTo = messageId;
      }));
    }

    private void ScrollToItem(IVirtualizable item)
    {
      if (item == null)
        return;
      this.myPanel.ScrollTo(Math.Max(0.0, this.myPanel.GetScrollOffsetForItem(this.ConversationItems.Messages.IndexOf(item)) + item.FixedHeight - (this.Orientation == PageOrientation.Landscape || this.Orientation == PageOrientation.LandscapeLeft || this.Orientation == PageOrientation.LandscapeRight ? 200.0 : 400.0)));
    }

    public void ScrollToBottom(bool animated = true, bool onlyIfInTheBottom = false)
    {
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        if (!(!onlyIfInTheBottom | this.VerticalOffset < 50.0))
          return;
        if (animated)
          this.myPanel.ScrollToBottom(false);
        else
          this.myPanel.ScrollTo(0.0);
      }));
    }

    private void PageBase_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.HandleOrientationChange();
    }

    private void HandleOrientationChange()
    {
      bool flag = this.Orientation == PageOrientation.LandscapeRight || this.Orientation == PageOrientation.LandscapeLeft;
      this.scrollNewMessage.MaxHeight = flag ? 100.0 : 168.0;
      this.gridHeader.Height = flag ? 88.0 : 112.0;
    }

    private void Title_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ConversationVM.IsChat)
        this.ManageChatIfApplicable();
      else if (this.ConversationVM.UserOrCharId > 0L)
      {
        Navigator.Current.NavigateToUserProfile(this.ConversationVM.UserOrCharId, this.ConversationVM.Title, "", false);
      }
      else
      {
        if (this.ConversationVM.UserOrCharId <= -2000000000L)
          return;
        Navigator.Current.NavigateToGroup(-this.ConversationVM.UserOrCharId, this.ConversationVM.Title, false);
      }
    }

    public void Handle(MessageActionEvent message)
    {
      if ((long) message.Message.UserOrChatId != this.ConversationVM.UserOrCharId || message.Message.IsChat != this.ConversationVM.IsChat)
        return;
      switch (message.MessageActionType)
      {
        case MessageActionType.Quote:
          this.ConversationVM.AddForwardedMessagesToOutboundMessage((IList<Message>) new List<Message>()
          {
            message.Message.Message
          });
          this.UpdateAppBar();
          break;
        case MessageActionType.Forward:
          ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
          contentDataProvider.ForwardedMessages = new List<Message>()
          {
            message.Message.Message
          };
          contentDataProvider.StoreDataToRepository();
          ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
          Navigator.Current.NavigateToPickConversation();
          break;
        case MessageActionType.Delete:
          bool refreshConversations = message.Message == this.ConversationVM.Messages.LastOrDefault<MessageViewModel>();
          ConversationViewModel conversationVm = this.ConversationVM;
          List<MessageViewModel> messageViewModels = new List<MessageViewModel>();
          messageViewModels.Add(message.Message);
          Action callback = (Action) (() =>
          {
            if (!refreshConversations)
              return;
            ConversationsViewModel.Instance.RefreshConversations(true);
          });
          conversationVm.DeleteMessages(messageViewModels, callback);
          this.Focus();
          break;
        case MessageActionType.SelectUnselect:
          message.Message.IsSelected = !message.Message.IsSelected;
          this.UpdateAppBar();
          break;
        case MessageActionType.EnterSelectMode:
          message.Message.IsSelected = true;
          this._appBarButtonСhoose_Click(null, (EventArgs) null);
          break;
      }
    }

    private void OnCompression(object sender, CompressionEventArgs e)
    {
      if (e.Type == CompressionType.Top)
        this.ConversationVM.LoadNewerConversations((Action<bool>) null);
      if (e.Type != CompressionType.Bottom)
        return;
      this.ConversationVM.LoadMoreConversations((Action<bool>) null);
    }

    private void ArrowDownTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ConversationVM.RefreshConversations();
    }

    public void Handle(SpriteElementTapEvent data)
    {
      if (!this._isCurrent)
        return;
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        int selectionStart = this.textBoxNewMessage.SelectionStart;
        this.textBoxNewMessage.Text = this.textBoxNewMessage.Text.Insert(selectionStart, data.Data.ElementCode);
        this.textBoxNewMessage.Select(selectionStart + data.Data.ElementCode.Length, 0);
      }));
    }

    public void Handle(StickerItemTapEvent message)
    {
      if (!this._isCurrent)
        return;
      this.ConversationVM.SendSticker(message.StickerItem, message.Referrer);
      this.ScrollToBottom(true, false);
    }

    private void SendTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this._appBarButtonSend.IsEnabled)
        return;
      this.SendMessage();
    }

    private void AddAttachTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
        ConversationViewModel conversationVm = this.ConversationVM;
        ConversationInfo conversationInfo = new ConversationInfo(conversationVm.IsChat, conversationVm.UserOrCharId, conversationVm.User, conversationVm.Chat);
        AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryGraffitiAndLocation, this.ConversationVM.OutboundMessageVm.NumberOfAttAllowedToAdd, (Action)(() =>
        {
            this.ConversationVM.AddAttachmentsFromRepository();
            this.UpdateAppBar();
        }), this.ConversationVM.OutboundMessageVm.HaveGeoAttachment, 0L, 0, conversationInfo);
    }

    private void OptionsButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.PrepareMenu();
      this.FriendOptionsMenu.IsOpen = true;
    }

    private void PrepareMenu()
    {
      this.menuItemPinToStart.Visibility = !this._defaultAppBar.MenuItems.Contains((object) this._appbarMenuItemPinToStart) || !this._appbarMenuItemPinToStart.IsEnabled ? Visibility.Collapsed : Visibility.Visible;
      this.menuItemDisableEnableNotifications.Visibility = this._defaultAppBar.MenuItems.Contains((object) this._appBarMenuItemDisableEnableNotifications) ? Visibility.Visible : Visibility.Collapsed;
      this.menuItemDisableEnableNotifications.Header = (object) this._appBarMenuItemDisableEnableNotifications.Text;
      if (this.ConversationVM.CanAddMembers && this._defaultAppBar.MenuItems.Contains((object) this._appBarMenuItemAddMember))
        this.menuItemAddMember.Visibility = Visibility.Visible;
      else
        this.menuItemAddMember.Visibility = Visibility.Collapsed;
      this.menuItemDeleteDialog.Visibility = this._defaultAppBar.MenuItems.Contains((object) this._appBarMenuItemDeleteDialog) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MenuPinToStartClick(object sender, RoutedEventArgs e)
    {
      this._appbarMenuItemPinToStart_Click(null, (EventArgs) null);
    }

    private void MenuShowMaterialsClick(object sender, RoutedEventArgs e)
    {
      this._appbarMenuItemShowMaterials_Click(null, (EventArgs) null);
    }

    private void MenuDisableEnableNotificationsClick(object sender, RoutedEventArgs e)
    {
      this._appBarMenuItemDisableEnableNotifications_Click(null, (EventArgs) null);
    }

    private void MenuAddMemberClick(object sender, RoutedEventArgs e)
    {
      this._appBarMenuItemAddMember_Click(null, (EventArgs) null);
    }

    private void MenuDeleteDialogClick(object sender, RoutedEventArgs e)
    {
      this._appBarMenuItemDeleteDialog_Click(null, (EventArgs) null);
    }

    public void ScrollToOffset(double offset)
    {
      this.myPanel.ScrollTo(offset);
    }

    private void MenuRefreshClick(object sender, RoutedEventArgs e)
    {
      this.ConversationVM.RefreshConversations();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ConversationPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.gridHeader = (Grid) this.FindName("gridHeader");
      this.TitlePanel = (StackPanel) this.FindName("TitlePanel");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockSubtitleVertical = (TextBlock) this.FindName("textBlockSubtitleVertical");
      this.FriendOptionsMenu = (ContextMenu) this.FindName("FriendOptionsMenu");
      this.menuItemRefresh = (MenuItem) this.FindName("menuItemRefresh");
      this.menuItemPinToStart = (MenuItem) this.FindName("menuItemPinToStart");
      this.menuItemShowMaterials = (MenuItem) this.FindName("menuItemShowMaterials");
      this.menuItemDisableEnableNotifications = (MenuItem) this.FindName("menuItemDisableEnableNotifications");
      this.menuItemAddMember = (MenuItem) this.FindName("menuItemAddMember");
      this.menuItemDeleteDialog = (MenuItem) this.FindName("menuItemDeleteDialog");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.myScroll = (ViewportControl) this.FindName("myScroll");
      this.myPanel = (MyVirtualizingPanel2) this.FindName("myPanel");
      this.ucNewMessage = (NewMessageUC) this.FindName("ucNewMessage");
    }

    public enum Mode
    {
      Default,
      Selection,
    }
  }
}
