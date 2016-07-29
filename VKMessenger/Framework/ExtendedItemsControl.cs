using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace VKMessenger.Framework
{
  public class ExtendedItemsControl : ItemsControl
  {
    protected bool _isBouncy;
    private bool _alreadyHookedScrollEvents;

    public event ExtendedItemsControl.OnCompression Compression;

    public event ExtendedItemsControl.OnScrollStateChanged ScrollStateChanged;

    public ExtendedItemsControl()
    {
      this.Loaded += new RoutedEventHandler(this.ListBox_Loaded);
    }

    private void ListBox_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._alreadyHookedScrollEvents)
        return;
      this._alreadyHookedScrollEvents = true;
      this.AddHandler(UIElement.ManipulationCompletedEvent, (Delegate) new EventHandler<ManipulationCompletedEventArgs>(this.LB_ManipulationCompleted), true);
      ScrollBar scrollBar = (ScrollBar) this.FindElementRecursive((FrameworkElement) this, typeof (ScrollBar));
      ScrollViewer scrollViewer = (ScrollViewer) this.FindElementRecursive((FrameworkElement) this, typeof (ScrollViewer));
      if (scrollViewer == null)
        return;
      FrameworkElement element = VisualTreeHelper.GetChild((DependencyObject) scrollViewer, 0) as FrameworkElement;
      if (element == null)
        return;
      VisualStateGroup visualState1 = this.FindVisualState(element, "ScrollStates");
      if (visualState1 != null)
        visualState1.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(this.group_CurrentStateChanging);
      VisualStateGroup visualState2 = this.FindVisualState(element, "VerticalCompression");
      VisualStateGroup visualState3 = this.FindVisualState(element, "HorizontalCompression");
      if (visualState2 != null)
        visualState2.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(this.vgroup_CurrentStateChanging);
      if (visualState3 == null)
        return;
      visualState3.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(this.hgroup_CurrentStateChanging);
    }

    private void hgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
    {
      if (e.NewState.Name == "CompressionLeft")
      {
        this._isBouncy = true;
        if (this.Compression != null)
        {
          this.Compression((object) this, new CompressionEventArgs(CompressionType.Left));
        }
      }
      if (e.NewState.Name == "CompressionRight")
      {
        this._isBouncy = true;
        if (this.Compression != null)
        {
          this.Compression((object) this, new CompressionEventArgs(CompressionType.Right));
        }
      }
      if (!(e.NewState.Name == "NoHorizontalCompression"))
        return;
      this._isBouncy = false;
    }

    private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
    {
      if (e.NewState.Name == "Scrolling")
      {
        if (this.ScrollStateChanged == null)
          return;
        this.ScrollStateChanged((object) this, new ScrollStateChangedEventArgs(true));
      }
      else
      {
        if (this.ScrollStateChanged == null)
          return;
        this.ScrollStateChanged((object) this, new ScrollStateChangedEventArgs(false));
      }
    }

    private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
    {
      if (e.NewState.Name == "CompressionTop")
      {
        this._isBouncy = true;
        if (this.Compression != null)
        {
          this.Compression((object) this, new CompressionEventArgs(CompressionType.Top));
        }
      }
      if (e.NewState.Name == "CompressionBottom")
      {
        this._isBouncy = true;
        if (this.Compression != null)
        {
          this.Compression((object) this, new CompressionEventArgs(CompressionType.Bottom));
        }
      }
      if (!(e.NewState.Name == "NoVerticalCompression"))
        return;
      this._isBouncy = false;
    }

    private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      if (!this._isBouncy)
        return;
      this._isBouncy = false;
    }

    private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
    {
      int childrenCount = VisualTreeHelper.GetChildrenCount((DependencyObject) parent);
      UIElement uiElement = null;
      if (childrenCount > 0)
      {
        for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
        {
          object obj = (object) VisualTreeHelper.GetChild((DependencyObject) parent, childIndex);
          if (obj.GetType() == targetType)
            return obj as UIElement;
          uiElement = this.FindElementRecursive(VisualTreeHelper.GetChild((DependencyObject) parent, childIndex) as FrameworkElement, targetType);
        }
      }
      return uiElement;
    }

    private VisualStateGroup FindVisualState(FrameworkElement element, string name)
    {
      if (element == null)
        return (VisualStateGroup) null;
      foreach (VisualStateGroup visualStateGroup in (IEnumerable) VisualStateManager.GetVisualStateGroups(element))
      {
        if (visualStateGroup.Name == name)
          return visualStateGroup;
      }
      return (VisualStateGroup) null;
    }

    public void ScrollIntoView(object item)
    {
    }

    public delegate void OnCompression(object sender, CompressionEventArgs e);

    public delegate void OnScrollStateChanged(object sender, ScrollStateChangedEventArgs e);
  }
}
