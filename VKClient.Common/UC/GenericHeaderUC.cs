using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GenericHeaderUC : UserControl, INotifyPropertyChanged
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (GenericHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GenericHeaderUC.Title_OnChanged)));
    public static readonly DependencyProperty OptionsMenuProperty = DependencyProperty.Register("OptionsMenu", typeof (OptionsMenu), typeof (GenericHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GenericHeaderUC.OptionsMenu_OnChanged)));
    private bool _isFirstTimeMenuShown = true;
    private Action _onTitleTap;
    private bool _hideSandwitchButton;
    private bool _supportMenu;
    private bool _isMenuShown;
    private bool _isAnimating;
    private IApplicationBar _savedAppBar;
    private Rectangle _overlay;
    private bool _savedSuppressMenu;
    private string _iconSource;
    private const double HEADER_MARGIN = 16.0;
    private const int OPACITY_ANIMATION_DURATION = 267;
    internal Grid Header;
    internal Rectangle rectBackground;
    internal Grid borderSandwich;
    internal Border borderCounter;
    internal Border borderCounterStroke;
    internal TextBlock textBlockCounter;
    internal Border borderNewsfeedPromoOverlay;
    internal StackPanel stackPanel;
    internal TextBlock textBlockTitle;
    internal Border borderIcon;
    internal Border borderMenuOpenIcon;
    internal ItemsControl itemsControlOptionsMenu;
    private bool _contentLoaded;

    public Action OnHeaderTap { get; set; }

    public Action OnTitleTap
    {
      get
      {
        return this._onTitleTap;
      }
      set
      {
        this._onTitleTap = value;
        if (this._onTitleTap != null)
        {
          MetroInMotion.SetTilt((DependencyObject) this.stackPanel, 1.5);
          this.stackPanel.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockTitle_OnTap);
        }
        else
        {
          MetroInMotion.SetTilt((DependencyObject) this.stackPanel, 0.0);
          this.stackPanel.Tap -= new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockTitle_OnTap);
        }
      }
    }

    public bool HideSandwitchButton
    {
      get
      {
        return this._hideSandwitchButton;
      }
      set
      {
        this._hideSandwitchButton = value;
        this.borderSandwich.Visibility = this._hideSandwitchButton ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Brush HeaderBackgroundBrush
    {
      get
      {
        return (Brush) (this.rectBackground.Fill as SolidColorBrush);
      }
      set
      {
        this.rectBackground.Fill = value;
        this.textBlockCounter.Foreground = value;
        this.borderCounterStroke.BorderBrush = value;
      }
    }

    public Border BorderIcon
    {
      get
      {
        return this.borderIcon;
      }
    }

    public bool SupportMenu
    {
      get
      {
        return this._supportMenu;
      }
      set
      {
        this._supportMenu = value;
        this.OnTitleTap = !this._supportMenu ? null : new Action(this.ShowHideMenu);
        this.UpdateMenuIcons();
      }
    }

    public FrameworkElement MenuElement { get; set; }

    public Action InitializeMenuCallback { get; set; }

    public Action BeforeOpenMenuCallback { get; set; }

    public Action AfterCloseMenuCallback { get; set; }

    public Grid ContentPanel { get; set; }

    public TextBlock TextBlockTitle
    {
      get
      {
        return this.textBlockTitle;
      }
    }

    public string IconSource
    {
      get
      {
        return this._iconSource;
      }
      set
      {
        this._iconSource = value;
        this.OnPropertyChanged("IconSource");
        this.borderIcon.Visibility = string.IsNullOrWhiteSpace(this._iconSource) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string Title
    {
      get
      {
        return (string) this.GetValue(GenericHeaderUC.TitleProperty);
      }
      set
      {
        this.SetValue(GenericHeaderUC.TitleProperty, (object) value);
      }
    }

    public OptionsMenu OptionsMenu
    {
      get
      {
        return (OptionsMenu) this.GetValue(GenericHeaderUC.OptionsMenuProperty);
      }
      set
      {
        this.SetValue(GenericHeaderUC.OptionsMenuProperty, (object) value);
      }
    }

    private bool IsDarkTheme
    {
      get
      {
        return (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
      }
    }

    public event EventHandler<OptionsMenuItemType> OptionsMenuItemSelected;

    public event PropertyChangedEventHandler PropertyChanged;

    public GenericHeaderUC()
    {
      this.InitializeComponent();
      this.borderCounter.DataContext = (object) new MenuViewModel();
      this.OptionsMenu = new OptionsMenu();
    }

    private void UpdateMenuIcons()
    {
      RotateTransform target = this.borderMenuOpenIcon.RenderTransform as RotateTransform;
      int num = this._isMenuShown ? -180 : 0;
      target.Animate(target.Angle, (double) num, (object) RotateTransform.AngleProperty, 250, new int?(0), null, null);
      this.borderMenuOpenIcon.Visibility = this._supportMenu ? Visibility.Visible : Visibility.Collapsed;
    }

    public void InitializeMenu(FrameworkElement menuElement, Grid contentPanel, Action initElementCallback = null, Action beforeOpenCallback = null, Action afterCloseCallback = null)
    {
      this.MenuElement = menuElement;
      this.ContentPanel = contentPanel;
      this.InitializeMenuCallback = initElementCallback;
      this.BeforeOpenMenuCallback = beforeOpenCallback;
      this.AfterCloseMenuCallback = afterCloseCallback;
      this.SupportMenu = true;
    }

    public void ShowHideMenu()
    {
      if (this._isAnimating)
        return;
      if (!(this.MenuElement.RenderTransform is TranslateTransform))
        this.MenuElement.RenderTransform = (Transform) new TranslateTransform();
      if (this.MenuElement.CacheMode == null)
        this.MenuElement.CacheMode = (CacheMode) new BitmapCache();
      TranslateTransform translateTransform1 = this.MenuElement.RenderTransform as TranslateTransform;
      if (this._overlay == null)
        this.CreateOverlay();
      if (!this._isMenuShown)
      {
        if (this._isFirstTimeMenuShown && this.InitializeMenuCallback != null)
        {
          this.InitializeMenuCallback();
          this._isFirstTimeMenuShown = false;
        }
        if (this.BeforeOpenMenuCallback != null)
          this.BeforeOpenMenuCallback();
        this.MenuElement.Visibility = Visibility.Visible;
        this.MenuElement.UpdateLayout();
        translateTransform1.Y = -this.MenuElement.ActualHeight - this.Header.ActualHeight;
        this._isAnimating = true;
        List<AnimationInfo> animInfoList = new List<AnimationInfo>();
        AnimationInfo animationInfo1 = new AnimationInfo();
        animationInfo1.duration = 250;
        CubicEase cubicEase = new CubicEase();
        int num1 = 0;
        cubicEase.EasingMode = (EasingMode) num1;
        animationInfo1.easing = (IEasingFunction) cubicEase;
        double y = translateTransform1.Y;
        animationInfo1.from = y;
        double num2 = 0.0;
        animationInfo1.to = num2;
        TranslateTransform translateTransform2 = translateTransform1;
        animationInfo1.target = (DependencyObject) translateTransform2;
        DependencyProperty dependencyProperty1 = TranslateTransform.YProperty;
        animationInfo1.propertyPath = (object) dependencyProperty1;
        animInfoList.Add(animationInfo1);
        AnimationInfo animationInfo2 = new AnimationInfo();
        animationInfo2.duration = 250;
        animationInfo2.from = 0.0;
        animationInfo2.to = 1.0;
        Rectangle rectangle = this._overlay;
        animationInfo2.target = (DependencyObject) rectangle;
        DependencyProperty dependencyProperty2 = UIElement.OpacityProperty;
        animationInfo2.propertyPath = (object) dependencyProperty2;
        animInfoList.Add(animationInfo2);
        int? startTime = new int?(0);
        Action completed = (Action) (() => this._isAnimating = false);
        AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        PageBase currentPage = FramePageUtils.CurrentPage;
        if (currentPage != null)
        {
          this._savedAppBar = currentPage.ApplicationBar;
          currentPage.ApplicationBar = (IApplicationBar) null;
          currentPage.BackKeyPress += new EventHandler<CancelEventArgs>(this.HideMenuOnBackKeyPress);
          this._savedSuppressMenu = currentPage.SuppressMenu;
          currentPage.SuppressMenu = true;
        }
        if (this.ContentPanel != null)
        {
          int index = this.ContentPanel.Children.IndexOf((UIElement) this.MenuElement);
          if (index >= 0)
            this.ContentPanel.Children.Insert(index, (UIElement) this._overlay);
          else
            this.ContentPanel.Children.Add((UIElement) this._overlay);
        }
      }
      else
      {
        this._isAnimating = true;
        List<AnimationInfo> animInfoList = new List<AnimationInfo>();
        AnimationInfo animationInfo1 = new AnimationInfo();
        animationInfo1.duration = 250;
        ExponentialEase exponentialEase = new ExponentialEase();
        int num1 = 0;
        exponentialEase.EasingMode = (EasingMode) num1;
        double num2 = 6.0;
        exponentialEase.Exponent = num2;
        animationInfo1.easing = (IEasingFunction) exponentialEase;
        double num3 = 0.0;
        animationInfo1.from = num3;
        double num4 = -this.MenuElement.ActualHeight - this.Header.ActualHeight;
        animationInfo1.to = num4;
        TranslateTransform translateTransform2 = translateTransform1;
        animationInfo1.target = (DependencyObject) translateTransform2;
        DependencyProperty dependencyProperty1 = TranslateTransform.YProperty;
        animationInfo1.propertyPath = (object) dependencyProperty1;
        animInfoList.Add(animationInfo1);
        AnimationInfo animationInfo2 = new AnimationInfo();
        animationInfo2.duration = 250;
        animationInfo2.from = 0.0;
        animationInfo2.to = 1.0;
        Rectangle rectangle = this._overlay;
        animationInfo2.target = (DependencyObject) rectangle;
        DependencyProperty dependencyProperty2 = UIElement.OpacityProperty;
        animationInfo2.propertyPath = (object) dependencyProperty2;
        animInfoList.Add(animationInfo2);
        int? startTime = new int?(0);
        Action completed = (Action) (() =>
        {
          this._isAnimating = false;
          this.MenuElement.Visibility = Visibility.Collapsed;
          if (this.AfterCloseMenuCallback == null)
            return;
          this.AfterCloseMenuCallback();
        });
        AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        PageBase currentPage = FramePageUtils.CurrentPage;
        if (currentPage != null)
        {
          currentPage.ApplicationBar = this._savedAppBar;
          currentPage.BackKeyPress -= new EventHandler<CancelEventArgs>(this.HideMenuOnBackKeyPress);
          currentPage.SuppressMenu = this._savedSuppressMenu;
        }
        Grid contentPanel = this.ContentPanel;
        if (contentPanel != null)
          contentPanel.Children.Remove((UIElement) this._overlay);
      }
      this._isMenuShown = !this._isMenuShown;
      this.UpdateMenuIcons();
    }

    private void CreateOverlay()
    {
      Rectangle rectangle1 = new Rectangle();
      SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb((byte) 100, (byte) 0, (byte) 0, (byte) 0));
      rectangle1.Fill = (Brush) solidColorBrush;
      Rectangle rectangle2 = rectangle1;
      Grid.SetColumnSpan((FrameworkElement) rectangle2, 100);
      Grid.SetRowSpan((FrameworkElement) rectangle2, 100);
      rectangle2.UseOptimizedManipulationRouting = false;
      rectangle2.Tap += (EventHandler<System.Windows.Input.GestureEventArgs>) ((s, e) =>
      {
        this.ShowHideMenu();
        e.Handled = true;
      });
      rectangle2.ManipulationStarted += (EventHandler<ManipulationStartedEventArgs>) ((s, e) => e.Handled = true);
      rectangle2.ManipulationDelta += (EventHandler<ManipulationDeltaEventArgs>) ((s, e) => e.Handled = true);
      rectangle2.ManipulationCompleted += (EventHandler<ManipulationCompletedEventArgs>) ((s, e) => e.Handled = true);
      this._overlay = rectangle2;
    }

    private void HideMenuOnBackKeyPress(object sender, CancelEventArgs e)
    {
      this.ShowHideMenu();
      e.Cancel = true;
    }

    private void TextBlockTitle_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._onTitleTap == null)
        return;
      e.Handled = true;
      this._onTitleTap();
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((GenericHeaderUC) d).textBlockTitle.Text = e.NewValue as string ?? "";
    }

    private void Header_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Action onHeaderTap = this.OnHeaderTap;
      if (onHeaderTap == null)
        return;
      onHeaderTap();
    }

    private static void OptionsMenu_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((GenericHeaderUC) d).UpdateOptionsMenu();
    }

    private void UpdateOptionsMenu()
    {
      this.itemsControlOptionsMenu.ItemsSource = (IEnumerable) this.OptionsMenu;
    }

    private void OptionsMenuItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      OptionsMenuItem optionsMenuItem = ((FrameworkElement) sender).DataContext as OptionsMenuItem;
      if (optionsMenuItem == null)
        return;
      e.Handled = true;
      EventHandler<OptionsMenuItemType> eventHandler = this.OptionsMenuItemSelected;
      if (eventHandler == null)
        return;
      int num = (int) optionsMenuItem.Type;
      eventHandler((object) this, (OptionsMenuItemType) num);
    }

    public void SetMenu(List<MenuItem> menuItems)
    {
      if (menuItems == null || menuItems.Count == 0)
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      contextMenu1.Background = (Brush) solidColorBrush1;
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      contextMenu1.Foreground = (Brush) solidColorBrush2;
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in menuItems)
        contextMenu2.Items.Add((object) menuItem);
      ContextMenuService.SetContextMenu((DependencyObject) this, contextMenu2);
    }

    public void ShowMenu()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    public void CleanupBinding()
    {
      this.textBlockTitle.ClearValue(TextBlock.TextProperty);
    }

    public double GetTitleMarginLeft()
    {
      Thickness margin = this.borderSandwich.Margin;
      double num = margin.Left + this.borderSandwich.ActualWidth;
      margin = this.borderSandwich.Margin;
      double right = margin.Right;
      return num + right + 16.0;
    }

    public double GetTitleWidth()
    {
      return this.stackPanel.ActualWidth - 32.0;
    }

    public void ShowNewsfeedPromoOverlay()
    {
      if (!this.IsDarkTheme)
        return;
      this.borderNewsfeedPromoOverlay.Animate(this.borderNewsfeedPromoOverlay.Opacity, 1.0, (object) UIElement.OpacityProperty, 267, new int?(), null, null);
    }

    public void HideNewsfeedPromoOverlay()
    {
      if (!this.IsDarkTheme)
        return;
      this.borderNewsfeedPromoOverlay.Animate(this.borderNewsfeedPromoOverlay.Opacity, 0.0, (object) UIElement.OpacityProperty, 267, new int?(), null, null);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
      if (changedEventHandler == null)
        return;
      PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
      changedEventHandler((object) this, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GenericHeaderUC.xaml", UriKind.Relative));
      this.Header = (Grid) this.FindName("Header");
      this.rectBackground = (Rectangle) this.FindName("rectBackground");
      this.borderSandwich = (Grid) this.FindName("borderSandwich");
      this.borderCounter = (Border) this.FindName("borderCounter");
      this.borderCounterStroke = (Border) this.FindName("borderCounterStroke");
      this.textBlockCounter = (TextBlock) this.FindName("textBlockCounter");
      this.borderNewsfeedPromoOverlay = (Border) this.FindName("borderNewsfeedPromoOverlay");
      this.stackPanel = (StackPanel) this.FindName("stackPanel");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.borderIcon = (Border) this.FindName("borderIcon");
      this.borderMenuOpenIcon = (Border) this.FindName("borderMenuOpenIcon");
      this.itemsControlOptionsMenu = (ItemsControl) this.FindName("itemsControlOptionsMenu");
    }
  }
}
