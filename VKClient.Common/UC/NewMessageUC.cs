using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Shared.ImagePreview;
using VKClient.Common.Stickers.AutoSuggest;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.Stickers.Views;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewMessageUC : UserControl, IHandle<StickersAutoSuggestDictionary.AutoSuggestDictionaryUpdatedEvent>, IHandle, IHandle<PreviewCompletedEvent>, IHandle<HasStickersUpdatesChangedEvent>, IHandle<StickersSettings.StickersListUpdatedEvent>, IHandle<StickersSettings.StickersKeyboardOpenRequestEvent>, IHandle<StickersSettings.StickersItemTapEvent>
  {
    private string _lastKeyForAutoSuggest = "";
    private bool _isEnabled = true;
    private int _adminLevel;
    private PhoneApplicationPage _parentPage;
    private bool _lastAutoSuggestStickersEnabled;
    private string _replyAutoForm;
    private ImageBrush _keyboardBrush;
    private ImageBrush _emojiBrush;
    private bool _panelInitialized;
    private SwipeThroughControl _stickersSlideView;
    internal StackPanel panelReply;
    internal CheckBox checkBoxAsCommunity;
    internal TextBlock textBlockReply;
    internal ReplyUserUC ucReplyUser;
    internal ScrollViewer scrollNewMessage;
    internal NewPostUC ucNewPost;
    internal Border borderEmoji;
    internal Ellipse ellipseHasStickersUpdates;
    internal Border borderSend;
    internal StickersAutoSuggestUC ucStickersAutoSuggest;
    internal TextBoxPanelControl panelControl;
    private bool _contentLoaded;

    private bool HaveRightsToPostOnBehalfOfCommunity
    {
      get
      {
        return this._adminLevel > 1;
      }
    }

    public ScrollViewer ScrollNewMessage
    {
      get
      {
        return this.scrollNewMessage;
      }
    }

    public NewPostUC UCNewPost
    {
      get
      {
        return this.ucNewPost;
      }
    }

    public Action OnAddAttachTap { get; set; }

    public Action OnSendTap { get; set; }

    public TextBox TextBoxNewComment
    {
      get
      {
        return this.UCNewPost.textBoxPost;
      }
    }

    public ReplyUserUC ReplyUserUC
    {
      get
      {
        return this.ucReplyUser;
      }
    }

    public TextBoxPanelControl PanelControl
    {
      get
      {
        return this.panelControl;
      }
    }

    public bool FromGroupChecked
    {
      get
      {
        if (this.checkBoxAsCommunity.IsChecked.HasValue)
          return this.checkBoxAsCommunity.IsChecked.Value;
        return false;
      }
    }

    public NewMessageUC()
    {
      this.InitializeComponent();
      this.SetAdminLevel(0);
      this.panelControl.BindTextBox(this.TextBoxNewComment);
      this.panelControl.IsFocusedChanged += new EventHandler<bool>(this.IsFocusedChanged);
      this.panelControl.IsEmojiOpenedChanged += new EventHandler<bool>(this.IsEmojiOpenedChanged);
      this.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
      this.UpdateSendButton(false);
      this.UpdateAutoSuggestVisibility();
      this.ucStickersAutoSuggest.AutoSuggestStickerSendingCallback = new Action(this.OnAutoSuggestStickerSending);
      this.ucStickersAutoSuggest.AutoSuggestStickerSentCallback = new Action(this.OnAutoSuggestStickerSent);
      this.Loaded += new RoutedEventHandler(this.NewMessageUC_Loaded);
      this.UpdateHasStickersUpdatesState();
      this._lastAutoSuggestStickersEnabled = AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled;
      EventAggregator.Current.Subscribe((object) this);
    }

    public void SetAdminLevel(int adminLevel)
    {
      this._adminLevel = adminLevel;
      if (this.HaveRightsToPostOnBehalfOfCommunity)
      {
        this.panelReply.Visibility = Visibility.Visible;
        this.checkBoxAsCommunity.Visibility = Visibility.Visible;
        this.textBlockReply.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.checkBoxAsCommunity.Visibility = Visibility.Collapsed;
        this.textBlockReply.Visibility = Visibility.Visible;
        if (!string.IsNullOrEmpty(this.ucReplyUser.Title))
          return;
        this.panelReply.Visibility = Visibility.Collapsed;
        this.ucReplyUser.Visibility = Visibility.Collapsed;
      }
    }

    private void NewMessageUC_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._parentPage != null)
        return;
      this._parentPage = (PhoneApplicationPage) FramePageUtils.CurrentPage;
      if (this._parentPage == null)
        return;
      this._parentPage.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(this._parentPage_OrientationChanged);
    }

    private void _parentPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateAutoSuggestVisibility();
    }

    private void OnAutoSuggestStickerSending()
    {
      this.ucNewPost.ForceFocusIfNeeded();
    }

    private void OnAutoSuggestStickerSent()
    {
      this.TextBoxNewComment.Text = "";
      this._replyAutoForm = "";
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateAutoSuggest(false);
    }

    private void UpdateAutoSuggest(bool force = false)
    {
      string text = this.TextBoxNewComment.Text;
      if (!string.IsNullOrEmpty(this._replyAutoForm) && text.StartsWith(this._replyAutoForm))
        text = text.Substring(this._replyAutoForm.Length);
      string str = StickersAutoSuggestDictionary.Instance.PrepareTextForLookup(text);
      if (((str != this._lastKeyForAutoSuggest ? 1 : (this._lastAutoSuggestStickersEnabled != AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled ? 1 : 0)) | (force ? 1 : 0)) != 0)
      {
        this.ucStickersAutoSuggest.SetData(StickersAutoSuggestDictionary.Instance.GetAutoSuggestItemsFor(str), str);
        this._lastKeyForAutoSuggest = str;
        this._lastAutoSuggestStickersEnabled = AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled;
      }
      this.UpdateAutoSuggestVisibility();
    }

    private void UpdateAutoSuggestVisibility()
    {
      this.ucStickersAutoSuggest.ShowHide((this.ucNewPost.IsFocused || this.panelControl.IsOpen) && (this._parentPage == null || this._parentPage.Orientation != PageOrientation.Landscape && this._parentPage.Orientation != PageOrientation.LandscapeLeft && this._parentPage.Orientation != PageOrientation.LandscapeRight) && this.ucStickersAutoSuggest.HasItemsToShow);
    }

    public void SetReplyAutoForm(string replyAutoForm)
    {
      this._replyAutoForm = replyAutoForm;
    }

    public void UpdateSendButton(bool isEnabled)
    {
      this._isEnabled = isEnabled;
      MetroInMotion.SetTilt((DependencyObject) this.borderSend, !isEnabled ? 0.0 : 2.1);
    }

    private void IsEmojiOpenedChanged(object sender, bool e)
    {
      ImageBrush imageBrush;
      if (this.panelControl.IsOpen)
      {
        if (this._keyboardBrush == null)
        {
          this._keyboardBrush = new ImageBrush();
          ImageLoader.SetImageBrushMultiResSource(this._keyboardBrush, "/Resources/Keyboard32px.png");
        }
        imageBrush = this._keyboardBrush;
      }
      else
      {
        if (this._emojiBrush == null)
        {
          this._emojiBrush = new ImageBrush();
          ImageLoader.SetImageBrushMultiResSource(this._emojiBrush, "/Resources/Smile32px.png");
        }
        imageBrush = this._emojiBrush;
      }
      this.borderEmoji.OpacityMask = (Brush) imageBrush;
      this.UpdateAutoSuggest(false);
    }

    private void IsFocusedChanged(object sender, bool e)
    {
      this.ucNewPost.IsFocused = this.panelControl.IsTextBoxTargetFocused;
      this.UpdateAutoSuggest(false);
    }

    private void AddAttachTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Action onAddAttachTap = this.OnAddAttachTap;
      if (onAddAttachTap == null)
        return;
      onAddAttachTap();
    }

    private void SendTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnSendTap == null || !this._isEnabled)
        return;
      this.ucNewPost.ForceFocusIfNeeded();
      this.OnSendTap();
    }

    private void Smiles_OnMouseEnter(object sender, MouseEventArgs e)
    {
      this.InitPanel();
      if (!this.panelControl.IsOpen)
        this.OpenPanel();
      else
        this.ucNewPost.TextBoxPost.Focus();
    }

    private void InitPanel()
    {
      if (this._panelInitialized)
        return;
      this._stickersSlideView = new SwipeThroughControl();
      this._stickersSlideView.BackspaceTapCallback = new Action(this.HandleBackspaceTap);
      this.panelControl.InitializeWithChildControl((FrameworkElement) this._stickersSlideView);
      this._stickersSlideView.CreateSingleElement = (Func<Control>) (() =>
      {
        SpriteListControl spriteListControl = new SpriteListControl();
        FramePageUtils.CurrentPage.RegisterForCleanup((IMyVirtualizingPanel) spriteListControl.MyPanel);
        spriteListControl.MyPanel.LoadedHeightDownwards = spriteListControl.MyPanel.LoadedHeightDownwardsNotScrolling = 600.0;
        spriteListControl.MyPanel.LoadedHeightUpwards = spriteListControl.MyPanel.LoadedHeightUpwardsNotScrolling = 300.0;
        spriteListControl.MyPanel.LoadUnloadThreshold = 100.0;
        return (Control) spriteListControl;
      });
      this._panelInitialized = true;
      this.ReloadStickersItems(true, false);
    }

    private void HandleBackspaceTap()
    {
      TextBox textBoxNewComment = this.TextBoxNewComment;
      string s = textBoxNewComment != null ? textBoxNewComment.Text : null;
      if (string.IsNullOrEmpty(s) || s.Length <= 0)
        return;
      int num = 1;
      if (s.Length > 1 && char.IsSurrogatePair(s, s.Length - 2))
        num = 2;
      this.TextBoxNewComment.Text = s.Substring(0, s.Length - num);
    }

    private void ReloadStickersItems(bool reloadSystemItems = false, bool keepPosition = true)
    {
      if (!this._panelInitialized)
        return;
      StoreProduct storeProduct = null;
      if (keepPosition)
        storeProduct = this.GetCurrentSelectedProduct();
      this._stickersSlideView.Items = new ObservableCollection<object>((IEnumerable<object>) StickersSettings.Instance.CreateSpriteListItemData());
      if (reloadSystemItems)
      {
        this._stickersSlideView.HeaderItems = new List<object>((IEnumerable<object>) StickersSettings.Instance.CreateStoreSpriteListItem());
        this._stickersSlideView.FooterItems = new List<object>((IEnumerable<object>) StickersSettings.Instance.CreateSettingsSpriteListItem());
      }
      if (!keepPosition || storeProduct == null)
        return;
      this.TrySlideToStickersPack((long) storeProduct.id);
    }

    private void OpenPanel()
    {
      this.panelControl.IsOpen = true;
      this.Focus();
      this.MarkUpdatesAsViewed(false);
    }

    private void ReplyPanel_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ucNewPost.ForceFocusIfNeeded();
    }

    private void UcReplyUser_OnTitleChanged(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(this.ucReplyUser.Title))
      {
        this.ucReplyUser.Visibility = Visibility.Visible;
        this.panelReply.Visibility = Visibility.Visible;
        this.textBlockReply.Visibility = this.HaveRightsToPostOnBehalfOfCommunity ? Visibility.Collapsed : Visibility.Visible;
      }
      else
      {
        this.ucReplyUser.Visibility = Visibility.Collapsed;
        if (!this.HaveRightsToPostOnBehalfOfCommunity)
        {
          this.panelReply.Visibility = Visibility.Collapsed;
          this.textBlockReply.Visibility = Visibility.Visible;
        }
        else
          this.textBlockReply.Visibility = Visibility.Collapsed;
      }
    }

    private bool TrySlideToStickersPack(long productId)
    {
      this.InitPanel();
      int num = -1;
      ObservableCollection<object> items = this._stickersSlideView.Items;
      for (int index = 0; index < items.Count; ++index)
      {
        StoreProduct stickerProduct = ((SpriteListItemData) items[index]).StickerProduct;
        if (stickerProduct != null && (long) stickerProduct.id == productId)
        {
          num = index;
          break;
        }
      }
      if (num < 0)
        return false;
      if (this._stickersSlideView.SelectedIndex != num)
        this._stickersSlideView.SelectedIndex = num;
      return true;
    }

    private StoreProduct GetCurrentSelectedProduct()
    {
      SwipeThroughControl swipeThroughControl = this._stickersSlideView;
      if ((swipeThroughControl != null ? swipeThroughControl.Items : null) == null || this._stickersSlideView.Items.Count == 0)
        return null;
      return ((SpriteListItemData) this._stickersSlideView.Items[this._stickersSlideView.SelectedIndex]).StickerProduct;
    }

    private void TryOpenStickersPackKeyboard(long productId, int delay = 0)
    {
      if (this._parentPage != FramePageUtils.CurrentPage || !this.TrySlideToStickersPack(productId))
        return;
      Action openPanel = (Action) (() =>
      {
        if (this.panelControl.IsOpen)
          return;
        this.OpenPanel();
      });
      if (delay <= 0)
        openPanel();
      else
        new DelayedExecutor(delay).AddToDelayedExecution((Action) (() => openPanel()));
    }

    private void OpenKeyboardOrPopup(StockItemHeader stockItemHeader)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this._parentPage != FramePageUtils.CurrentPage)
          return;
        if (this.IsHitTestVisible)
        {
          this.TryOpenStickersPackKeyboard((long) stockItemHeader.ProductId, 0);
        }
        else
        {
          CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.message;
          StickersPackView.Show(stockItemHeader, "message");
        }
      }));
    }

    private void MarkUpdatesAsViewed(bool force = false)
    {
      if (!force && this.ellipseHasStickersUpdates.Visibility != Visibility.Visible)
        return;
      AppGlobalStateManager.Current.GlobalState.HasStickersUpdates = false;
      this.UpdateHasStickersUpdatesState();
      StoreService.Instance.MarkUpdatesAsViewed();
    }

    private void UpdateHasStickersUpdatesState()
    {
      Execute.ExecuteOnUIThread((Action) (() => this.ellipseHasStickersUpdates.Visibility = AppGlobalStateManager.Current.GlobalState.HasStickersUpdates ? Visibility.Visible : Visibility.Collapsed));
    }

    public void Handle(StickersAutoSuggestDictionary.AutoSuggestDictionaryUpdatedEvent message)
    {
      this.UpdateAutoSuggest(true);
    }

    public void Handle(PreviewCompletedEvent message)
    {
      this.ucNewPost.ForceFocusIfNeeded();
    }

    public void Handle(HasStickersUpdatesChangedEvent message)
    {
      if (this.panelControl.IsOpen)
        this.MarkUpdatesAsViewed(true);
      else
        this.UpdateHasStickersUpdatesState();
    }

    public void Handle(StickersSettings.StickersListUpdatedEvent message)
    {
      this.ReloadStickersItems(false, true);
    }

    public void Handle(StickersSettings.StickersKeyboardOpenRequestEvent message)
    {
      if (this._parentPage != FramePageUtils.CurrentPage)
        return;
      new DelayedExecutor(1000).AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.OpenKeyboardOrPopup(message.StockItemHeader)))));
    }

    public void Handle(StickersSettings.StickersItemTapEvent message)
    {
      this.OpenKeyboardOrPopup(message.StockItemHeader);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewMessageUC.xaml", UriKind.Relative));
      this.panelReply = (StackPanel) this.FindName("panelReply");
      this.checkBoxAsCommunity = (CheckBox) this.FindName("checkBoxAsCommunity");
      this.textBlockReply = (TextBlock) this.FindName("textBlockReply");
      this.ucReplyUser = (ReplyUserUC) this.FindName("ucReplyUser");
      this.scrollNewMessage = (ScrollViewer) this.FindName("scrollNewMessage");
      this.ucNewPost = (NewPostUC) this.FindName("ucNewPost");
      this.borderEmoji = (Border) this.FindName("borderEmoji");
      this.ellipseHasStickersUpdates = (Ellipse) this.FindName("ellipseHasStickersUpdates");
      this.borderSend = (Border) this.FindName("borderSend");
      this.ucStickersAutoSuggest = (StickersAutoSuggestUC) this.FindName("ucStickersAutoSuggest");
      this.panelControl = (TextBoxPanelControl) this.FindName("panelControl");
    }
  }
}
