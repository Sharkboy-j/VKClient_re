using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.UC
{
  public class ListPickerUC2 : UserControl
  {
    public static readonly DependencyProperty ListHeaderHeightProperty = DependencyProperty.Register("ListHeaderHeight", typeof (double), typeof (ListPickerUC2), new PropertyMetadata((object) 8.0));
    public static readonly DependencyProperty ListFooterHeightProperty = DependencyProperty.Register("ListFooterHeight", typeof (double), typeof (ListPickerUC2), new PropertyMetadata((object) 8.0));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IList), typeof (ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof (object), typeof (ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty PickerMaxHeightProperty = DependencyProperty.Register("PickerMaxHeight", typeof (double), typeof (ListPickerUC2), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty PickerMaxWidthProperty = DependencyProperty.Register("PickerMaxWidth", typeof (double), typeof (ListPickerUC2), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty PickerMarginProperty = DependencyProperty.Register("PickerMargin", typeof (Thickness), typeof (ListPickerUC2), new PropertyMetadata((object) new Thickness()));
    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (ListPickerUC2), new PropertyMetadata(null));
    private const double ITEM_DEFAULT_HEIGHT = 64.0;
    private const double LIST_HEADER_DEFAULT_HEIGHT = 8.0;
    private const double LIST_FOOTER_DEFAULT_HEIGHT = 8.0;
    private Point _position;
    private FrameworkElement _container;
    private static int _instancesCount;
    private DialogService _flyout;
    private bool _flyoutOpened;
    internal Storyboard AnimClip;
    internal Storyboard AnimClipHide;
    internal Grid containerGrid;
    internal TranslateTransform transform;
    internal RectangleGeometry rectGeometry;
    internal ScaleTransform scaleTransform;
    internal Grid gridListBoxContainer;
    internal ExtendedLongListSelector listBox;
    internal Canvas listHeader;
    internal Canvas listFooter;
    internal Border borderDisable;
    private bool _contentLoaded;

    public double ListHeaderHeight
    {
      get
      {
        return (double) this.GetValue(ListPickerUC2.ListHeaderHeightProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.ListHeaderHeightProperty, (object) value);
      }
    }

    public double ListFooterHeight
    {
      get
      {
        return (double) this.GetValue(ListPickerUC2.ListFooterHeightProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.ListFooterHeightProperty, (object) value);
      }
    }

    public IList ItemsSource
    {
      get
      {
        return (IList) this.GetValue(ListPickerUC2.ItemsSourceProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.ItemsSourceProperty, (object) value);
      }
    }

    public object SelectedItem
    {
      get
      {
        return this.GetValue(ListPickerUC2.SelectedItemProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.SelectedItemProperty, value);
      }
    }

    public DataTemplate ItemTemplate
    {
      get
      {
        return (DataTemplate) this.GetValue(ListPickerUC2.ItemTemplateProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.ItemTemplateProperty, (object) value);
      }
    }

    public double PickerMaxHeight
    {
      get
      {
        return (double) this.GetValue(ListPickerUC2.PickerMaxHeightProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.PickerMaxHeightProperty, (object) value);
      }
    }

    public double PickerMaxWidth
    {
      get
      {
        return (double) this.GetValue(ListPickerUC2.PickerMaxWidthProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.PickerMaxWidthProperty, (object) value);
      }
    }

    public Thickness PickerMargin
    {
      get
      {
        return (Thickness) this.GetValue(ListPickerUC2.PickerMarginProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.PickerMarginProperty, (object) value);
      }
    }

    public Brush BackgroundColor
    {
      get
      {
        return (Brush) this.GetValue(ListPickerUC2.BackgroundColorProperty);
      }
      set
      {
        this.SetValue(ListPickerUC2.BackgroundColorProperty, (object) value);
      }
    }

    public event EventHandler<object> ItemTapped;

    public event EventHandler Closed;

    public ListPickerUC2()
    {
      this.InitializeComponent();
      this.listBox.Opacity = 1.0;
      this.scaleTransform.ScaleY = 1.0;
      ++ListPickerUC2._instancesCount;
    }

    ~ListPickerUC2()
    {
    }

    public void Show(Point position, FrameworkElement container)
    {
      this._position = position;
      this._container = container;
      this.listBox.Opacity = 0.0;
      this.scaleTransform.ScaleY = 0.0;
      this.Setup();
      Grid grid1 = new Grid();
      int num1 = 0;
      grid1.VerticalAlignment = (VerticalAlignment) num1;
      int num2 = 0;
      grid1.HorizontalAlignment = (HorizontalAlignment) num2;
      Grid grid2 = grid1;
      grid2.Children.Add((UIElement) this);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      Grid grid3 = grid2;
      dialogService.Child = (FrameworkElement) grid3;
      this._flyout = dialogService;
      this._flyout.Opened += (EventHandler) ((sender, args) =>
      {
        this.UpdatePopupSize();
        this._flyoutOpened = true;
        this.AnimClip.Begin();
      });
      this._flyout.Closed += (EventHandler) ((sender, args) =>
      {
        this._flyoutOpened = false;
        this.ItemsSource = null;
        EventHandler eventHandler = this.Closed;
        if (eventHandler == null)
          return;
        EventArgs e = EventArgs.Empty;
        eventHandler((object) this, e);
      });
      this._flyout.Show(null);
    }

    public void Hide()
    {
      this.AnimClipHide.Completed += (EventHandler) ((sender, eventArgs) =>
      {
        DialogService dialogService = this._flyout;
        if (dialogService == null)
          return;
         dialogService.Hide();
      });
      this.AnimClipHide.Begin();
    }

    private void Setup()
    {
      if (this.ItemTemplate != null)
        this.listBox.ItemTemplate = this.ItemTemplate;
      this.listBox.ItemsSource = this.ItemsSource;
      this.listHeader.Height = this.ListHeaderHeight;
      this.listFooter.Height = this.ListFooterHeight;
      if (this.BackgroundColor == null)
        return;
      this.containerGrid.Background = this.BackgroundColor;
    }

    private void UpdatePopupSize()
    {
      this.UpdatePopupHeight();
      this.containerGrid.Width = this.PickerMaxWidth;
      this.containerGrid.Margin = new Thickness(this._position.X, this._position.Y, 0.0, 0.0);
      this.scaleTransform.CenterY = this._position.Y;
      this.rectGeometry.Rect = new Rect()
      {
        X = 0.0,
        Y = 0.0,
        Width = this.containerGrid.Width,
        Height = this.containerGrid.Height
      };
    }

    private void UpdatePopupHeight()
    {
      double val1_1 = this._container.ActualHeight - this.PickerMargin.Top - this.PickerMargin.Bottom;
      double val2 = Math.Min(val1_1, this.listBox.ActualHeight);
      double val1_2 = this.PickerMaxHeight > 0.0 ? Math.Min(this.PickerMaxHeight, val2) : val2;
      this.gridListBoxContainer.Height = val1_1;
      this.containerGrid.Height = Math.Max(val1_2, 0.0);
    }

    private void ScrollToSelectedItem(int selectedIndex)
    {
      if (selectedIndex == this.ItemsSource.Count - 1)
      {
        this.listBox.ScrollTo(this.listBox.ItemsSource[selectedIndex]);
      }
      else
      {
        double y = this._position.Y;
        this.listBox.ScrollToPosition(ListPickerUC2.GetItemOffset(selectedIndex) - y);
      }
    }

    private static double GetItemOffset(int itemIndex)
    {
      return 8.0 + 64.0 * (double) itemIndex;
    }

    public void DisableContent()
    {
      this.borderDisable.Visibility = Visibility.Visible;
    }

    private void ListBox_OnTap(object sender, GestureEventArgs e)
    {
      object selectedItem1 = this.listBox.SelectedItem;
      if (selectedItem1 != null)
      {
        this.SelectedItem = this.ItemsSource[this.ItemsSource.IndexOf(selectedItem1)];
        EventHandler<object> eventHandler = this.ItemTapped;
        if (eventHandler != null)
        {
          object selectedItem2 = this.SelectedItem;
          eventHandler((object) this, selectedItem2);
        }
      }
      this.Hide();
    }

    private void ListBox_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this._flyoutOpened)
        this.UpdatePopupSize();
      int selectedIndex = this.ItemsSource.IndexOf(this.SelectedItem);
      if (selectedIndex <= -1)
        return;
      this.listBox.SelectedItem = this.ItemsSource[selectedIndex];
      this.ScrollToSelectedItem(selectedIndex);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ListPickerUC2.xaml", UriKind.Relative));
      this.AnimClip = (Storyboard) this.FindName("AnimClip");
      this.AnimClipHide = (Storyboard) this.FindName("AnimClipHide");
      this.containerGrid = (Grid) this.FindName("containerGrid");
      this.transform = (TranslateTransform) this.FindName("transform");
      this.rectGeometry = (RectangleGeometry) this.FindName("rectGeometry");
      this.scaleTransform = (ScaleTransform) this.FindName("scaleTransform");
      this.gridListBoxContainer = (Grid) this.FindName("gridListBoxContainer");
      this.listBox = (ExtendedLongListSelector) this.FindName("listBox");
      this.listHeader = (Canvas) this.FindName("listHeader");
      this.listFooter = (Canvas) this.FindName("listFooter");
      this.borderDisable = (Border) this.FindName("borderDisable");
    }
  }
}
