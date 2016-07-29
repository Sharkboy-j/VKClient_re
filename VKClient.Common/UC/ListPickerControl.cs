using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.UC
{
  public class ListPickerControl : Control
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ListPickerControl), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty ParentElementProperty = DependencyProperty.Register("ParentElement", typeof (FrameworkElement), typeof (ListPickerControl), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IList), typeof (ListPickerControl), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof (object), typeof (ListPickerControl), new PropertyMetadata(new PropertyChangedCallback(ListPickerControl.SelectedItem_OnChanged)));
    public static readonly DependencyProperty SelectedItemStrProperty = DependencyProperty.Register("SelectedItemStr", typeof (string), typeof (ListPickerControl), new PropertyMetadata((object) "Subtitle"));
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (ListPickerControl), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemPrefixProperty = DependencyProperty.Register("ItemPrefix", typeof (string), typeof (ListPickerControl), new PropertyMetadata(null));
    public static readonly DependencyProperty PickerMaxHeightProperty = DependencyProperty.Register("PickerMaxHeight", typeof (double), typeof (ListPickerControl), new PropertyMetadata((object) 480.0));
    public static readonly DependencyProperty PickerWidthProperty = DependencyProperty.Register("PickerWidth", typeof (double), typeof (ListPickerControl), new PropertyMetadata((object) 320.0));
    private const double DEFAULT_PICKER_MAX_HEIGHT = 480.0;
    private const double DEFAULT_PICKER_WIDTH = 320.0;
    private const double MAX_PICKER_RIGHT_BORDER_POSITION = 474.0;
    private PhoneApplicationPage _page;
    private Frame _frame;

    public string Title
    {
      get
      {
        return (string) this.GetValue(ListPickerControl.TitleProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.TitleProperty, (object) value);
      }
    }

    public FrameworkElement ParentElement
    {
      get
      {
        return (FrameworkElement) this.GetValue(ListPickerControl.ParentElementProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.ParentElementProperty, (object) value);
      }
    }

    public IList ItemsSource
    {
      get
      {
        return (IList) this.GetValue(ListPickerControl.ItemsSourceProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.ItemsSourceProperty, (object) value);
      }
    }

    public object SelectedItem
    {
      get
      {
        return this.GetValue(ListPickerControl.SelectedItemProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.SelectedItemProperty, value);
      }
    }

    public string SelectedItemStr
    {
      get
      {
        return (string) this.GetValue(ListPickerControl.SelectedItemStrProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.SelectedItemStrProperty, (object) value);
      }
    }

    public DataTemplate ItemTemplate
    {
      get
      {
        return (DataTemplate) this.GetValue(ListPickerControl.ItemTemplateProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.ItemTemplateProperty, (object) value);
      }
    }

    public string ItemPrefix
    {
      get
      {
        return (string) this.GetValue(ListPickerControl.ItemPrefixProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.ItemPrefixProperty, (object) value);
      }
    }

    public double PickerMaxHeight
    {
      get
      {
        return (double) this.GetValue(ListPickerControl.PickerMaxHeightProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.PickerMaxHeightProperty, (object) value);
      }
    }

    public double PickerWidth
    {
      get
      {
        return (double) this.GetValue(ListPickerControl.PickerWidthProperty);
      }
      set
      {
        this.SetValue(ListPickerControl.PickerWidthProperty, (object) value);
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        return this._page ?? (this._page = VKClient.Common.Framework.CodeForFun.TemplatedVisualTreeExtensions.GetFirstLogicalChildByType<PhoneApplicationPage>(this.Frame, false));
      }
    }

    private Frame Frame
    {
      get
      {
        return this._frame ?? (this._frame = Application.Current.RootVisual as Frame);
      }
    }

    public ListPickerControl()
    {
      this.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.Picker_OnTap);
    }

    private static void SelectedItem_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((ListPickerControl) d).SelectedItemStr = e.NewValue != null ? e.NewValue.ToString() : "";
    }

    private void Picker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement1 = this.ParentElement ?? (FrameworkElement) this.Page;
      Point point1 = this.TransformToVisual((UIElement) frameworkElement1).Transform(new Point(0.0, 0.0));
      point1.X -= 16.0;
      if (point1.X + this.PickerWidth > 474.0)
        point1.X = 474.0 - this.PickerWidth;
      Grid grid1 = new Grid();
      int num1 = 0;
      grid1.VerticalAlignment = (VerticalAlignment) num1;
      int num2 = 0;
      grid1.HorizontalAlignment = (HorizontalAlignment) num2;
      Grid grid2 = grid1;
      ObservableCollection<ListPickerListItem> observableCollection = new ObservableCollection<ListPickerListItem>();
      ListPickerListItem listPickerListItem1 = (ListPickerListItem) null;
      foreach (object fromObj in (IEnumerable) this.ItemsSource)
      {
        ListPickerListItem listPickerListItem2 = new ListPickerListItem(fromObj)
        {
          Prefix = this.ItemPrefix
        };
        observableCollection.Add(listPickerListItem2);
        object selectedItem = this.SelectedItem;
        if (fromObj == selectedItem)
        {
          listPickerListItem2.IsSelected = true;
          listPickerListItem1 = listPickerListItem2;
        }
      }
      ListPickerItemsUC listPickerItemsUc = new ListPickerItemsUC();
      listPickerItemsUc.ItemsSource = observableCollection;
      listPickerItemsUc.SelectedItem = listPickerListItem1;
      DataTemplate itemTemplate = this.ItemTemplate;
      listPickerItemsUc.ItemTemplate = itemTemplate;
      double pickerMaxHeight = this.PickerMaxHeight;
      listPickerItemsUc.PickerMaxHeight = pickerMaxHeight;
      double pickerWidth = this.PickerWidth;
      listPickerItemsUc.PickerWidth = pickerWidth;
      FrameworkElement frameworkElement2 = frameworkElement1;
      listPickerItemsUc.ParentElement = frameworkElement2;
      Point point2 = point1;
      listPickerItemsUc.ShowPosition = point2;
      ListPickerItemsUC picker = listPickerItemsUc;
      grid2.Children.Add((UIElement) picker);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      Grid grid3 = grid2;
      dialogService.Child = (FrameworkElement) grid3;
      DialogService ds = dialogService;
      picker.listBox.Tap += (EventHandler<System.Windows.Input.GestureEventArgs>) ((o, args) =>
      {
        ListPickerListItem listPickerListItem2 = picker.listBox.SelectedItem as ListPickerListItem;
        if (listPickerListItem2 != null)
          this.SelectedItem = this.ItemsSource[picker.ItemsSource.IndexOf(listPickerListItem2)];
        picker.AnimClipHide.Completed += (EventHandler) ((sender1, eventArgs) => ds.Hide());
        picker.AnimClipHide.Begin();
      });
      picker.Setup();
      ds.Opened += (EventHandler) ((o, args) => picker.AnimClip.Begin());
      ds.Show(null);
    }
  }
}
