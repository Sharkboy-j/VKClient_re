using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.UC
{
  public class PickerFlyout : IFlyout
  {
    private static readonly object Lockobj = new object();
    private bool _isOverlayApplied = true;
    public bool HideOnNavigation = true;
    private const string NoneStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0\"\r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"1\"/>\r\n        </Storyboard>";
    private Panel _popupContainer;
    private Frame _rootVisual;
    private PageBase _page;
    private Grid _childPanel;
    private Grid _overlay;
    //private FrameworkElement _child;
    private IApplicationBar _applicationBar;
    private bool _deferredShowToLoaded;
    private UIElement _controlToFadeout;
    private bool _wasMenuSuppressed;

    public bool IsOverlayApplied
    {
      get
      {
        return this._isOverlayApplied;
      }
      set
      {
        this._isOverlayApplied = value;
      }
    }

    public FrameworkElement Child { get; set; }

    public double VerticalOffset { get; set; }

    internal double ControlVerticalOffset { get; set; }

    public bool BackButtonPressed { get; set; }

    public Brush BackgroundBrush { get; set; }

    public bool IsOpen { get; set; }

    protected internal bool IsBackKeyOverride { get; set; }

    public bool HasPopup { get; set; }

    internal PageBase Page
    {
      get
      {
        return this._page ?? (this._page = this.RootVisual.GetFirstLogicalChildByType<PageBase>(false));
      }
    }

    internal Frame RootVisual
    {
      get
      {
        return this._rootVisual ?? (this._rootVisual = Application.Current.RootVisual as Frame);
      }
    }

    public bool KeepAppBar { get; set; }

    public bool SetStatusBarBackground { get; set; }

    internal Panel PopupContainer
    {
      get
      {
        if (this._popupContainer == null)
        {
          IEnumerable<ContentPresenter> logicalChildrenByType1 = this.RootVisual.GetLogicalChildrenByType<ContentPresenter>(false);
          for (int index = 0; index < logicalChildrenByType1.Count<ContentPresenter>(); ++index)
          {
            IEnumerable<Panel> logicalChildrenByType2 = logicalChildrenByType1.ElementAt<ContentPresenter>(index).GetLogicalChildrenByType<Panel>(false);
            if (logicalChildrenByType2.Any<Panel>())
            {
              this._popupContainer = logicalChildrenByType2.First<Panel>();
              break;
            }
          }
        }
        return this._popupContainer;
      }
    }

    public event EventHandler Closed;

    public event EventHandler Opened;

    public PickerFlyout()
    {
      this.BackButtonPressed = false;
      this.BackgroundBrush = (Brush) new SolidColorBrush(Color.FromArgb((byte) 100, (byte) 0, (byte) 0, (byte) 0));
    }

    private void InitializePopup()
    {
      this._childPanel = this.CreateGrid();
      if (this.IsOverlayApplied)
      {
        this._overlay = this.CreateGrid();
        this._overlay.UseOptimizedManipulationRouting = false;
        this._overlay.Tap += (EventHandler<GestureEventArgs>) ((s, e) => this.Hide());
        if (this.BackgroundBrush != null)
          this._overlay.Background = this.BackgroundBrush;
      }
      if (this.PopupContainer != null)
      {
        if (this._overlay != null)
          this.PopupContainer.Children.Add((UIElement) this._overlay);
        this.PopupContainer.Children.Add((UIElement) this._childPanel);
        this._childPanel.Children.Add((UIElement) this.Child);
      }
      else
      {
        this._deferredShowToLoaded = true;
        this.RootVisual.Loaded += new RoutedEventHandler(this.RootVisualDeferredShowLoaded);
      }
    }

    private Grid CreateGrid()
    {
      Grid grid1 = new Grid();
      string @string = Guid.NewGuid().ToString();
      grid1.Name = @string;
      Grid grid2 = grid1;
      Grid.SetColumnSpan((FrameworkElement) grid2, int.MaxValue);
      Grid.SetRowSpan((FrameworkElement) grid2, int.MaxValue);
      grid2.Opacity = 0.0;
      this.CalculateVerticalOffset((Panel) grid2);
      return grid2;
    }

    internal void CalculateVerticalOffset()
    {
      this.CalculateVerticalOffset((Panel) this._childPanel);
    }

    internal void CalculateVerticalOffset(Panel panel)
    {
      if (panel == null)
        return;
      int num = 0;
      if (SystemTray.IsVisible && SystemTray.Opacity < 1.0 && SystemTray.Opacity > 0.0)
        num += 32;
      panel.Margin = new Thickness(0.0, this.VerticalOffset + (double) num + this.ControlVerticalOffset, 0.0, 0.0);
    }

    private void RootVisualDeferredShowLoaded(object sender, RoutedEventArgs e)
    {
      this.RootVisual.Loaded -= new RoutedEventHandler(this.RootVisualDeferredShowLoaded);
      this._deferredShowToLoaded = false;
      this.Show(null);
    }

    protected internal void SetAlignmentsOnOverlay(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
      if (this._childPanel == null)
        return;
      this._childPanel.HorizontalAlignment = horizontalAlignment;
      this._childPanel.VerticalAlignment = verticalAlignment;
    }

    public void Show(UIElement controlToFadeout = null)
    {
      this._controlToFadeout = controlToFadeout;
      lock (PickerFlyout.Lockobj)
      {
        if (this.Page == null)
          return;
        this.IsOpen = true;
        this.InitializePopup();
        if (this._deferredShowToLoaded)
          return;
        if (!this.IsBackKeyOverride)
          this.Page.BackKeyPress += new EventHandler<CancelEventArgs>(this.OnBackKeyPress);
        this.Page.NavigationService.Navigated += new NavigatedEventHandler(this.OnNavigated);
        if (this.SetStatusBarBackground)
          SystemTray.BackgroundColor = (Application.Current.Resources["PhoneChromeBrush"] as SolidColorBrush).Color;
        if (!this.KeepAppBar)
        {
          this._applicationBar = this.Page.ApplicationBar;
          this.Page.ApplicationBar = null;
        }
        this.AnimateParentControl(true);
        int completedStoryboards = 0;
        Action local_3 = (Action) (() =>
        {
          ++completedStoryboards;
          if (completedStoryboards != 3)
            return;
          this.Page.Flyouts.Add((IFlyout) this);
          if (this.Opened == null)
            return;
          this.Opened((object) this, null);
        });
        this.RunShowStoryboard((FrameworkElement) this._overlay, local_3);
        this.RunShowStoryboard((FrameworkElement) this._childPanel, local_3);
        this.RunShowStoryboard(this.Child, local_3);
        this._wasMenuSuppressed = this.Page.SuppressMenu;
        this.Page.SuppressMenu = true;
      }
    }

    private void AnimateParentControl(bool fadeout)
    {
      if (this._controlToFadeout == null)
        return;
      AnimationUtils.Animate(fadeout ? 0.5 : 1.0, (DependencyObject) this._controlToFadeout, "Opacity", 0.25);
      if (fadeout)
        return;
      this._controlToFadeout.Visibility = Visibility.Visible;
    }

    private void RunShowStoryboard(FrameworkElement element, Action completionCallback)
    {
      if (element == null)
      {
        if (completionCallback == null)
          return;
        completionCallback();
      }
      else
      {
        Storyboard storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0\"\r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"1\"/>\r\n        </Storyboard>") as Storyboard;
        if (storyboard != null)
        {
          this.Page.Dispatcher.BeginInvoke((Action) (() =>
          {
            foreach (Timeline child in (PresentationFrameworkCollection<Timeline>) storyboard.Children)
              Storyboard.SetTarget(child, (DependencyObject) element);
            storyboard.Completed += (EventHandler) ((s, e) =>
            {
              if (completionCallback == null)
                return;
              completionCallback();
            });
            storyboard.Begin();
          }));
        }
        else
        {
          element.Opacity = 1.0;
          if (completionCallback == null)
            return;
          completionCallback();
        }
      }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      if (!e.IsNavigationInitiator || !this.HideOnNavigation)
        return;
      this.Hide();
    }

    public void Hide()
    {
      if (!this.IsOpen)
        return;
      this.Page.SuppressMenu = this._wasMenuSuppressed;
      this.HideStoryboardCompleted(null, (EventArgs) null);
    }

    private void RunHideStoryboard(FrameworkElement element, Action completionCallback)
    {
      completionCallback();
    }

    private void HideStoryboardCompleted(object sender, EventArgs e)
    {
      this.IsOpen = false;
      try
      {
        if (this.SetStatusBarBackground)
          SystemTray.BackgroundColor = (Application.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
        if (this.Page != null)
        {
          if (this.Page.Flyouts.Contains((IFlyout) this))
            this.Page.Flyouts.Remove((IFlyout) this);
          this.Page.BackKeyPress -= new EventHandler<CancelEventArgs>(this.OnBackKeyPress);
          this.Page.NavigationService.Navigated -= new NavigatedEventHandler(this.OnNavigated);
          if (this._applicationBar != null)
          {
            this.Page.ApplicationBar = this._applicationBar;
            this._applicationBar = (IApplicationBar) null;
          }
          this.AnimateParentControl(false);
          this._page = (PageBase) null;
        }
      }
      catch
      {
      }
      try
      {
        if (this.PopupContainer != null)
        {
          if (this.PopupContainer.Children != null)
          {
            if (this._overlay != null)
              this.PopupContainer.Children.Remove((UIElement) this._overlay);
            this.PopupContainer.Children.Remove((UIElement) this._childPanel);
          }
        }
      }
      catch
      {
      }
      try
      {
        EventHandler eventHandler = this.Closed;
        if (eventHandler == null)
          return;
        eventHandler((object)this, null);
      }
      catch
      {
      }
    }

    public void ChangeChild(FrameworkElement newChild, Action callback = null)
    {
      this._childPanel.Children.Remove((UIElement) this.Child);
      this.Child = newChild;
      this._childPanel.Children.Add((UIElement) this.Child);
      this.RunShowStoryboard(this.Child, callback);
    }

    public void OnBackKeyPress(object sender, CancelEventArgs e)
    {
      if (!this.Page.CanNavigateBack)
        return;
      if (this.HasPopup)
      {
        e.Cancel = true;
      }
      else
      {
        if (!this.IsOpen)
          return;
        e.Cancel = true;
        this.BackButtonPressed = true;
        this.Hide();
      }
    }
  }
}
