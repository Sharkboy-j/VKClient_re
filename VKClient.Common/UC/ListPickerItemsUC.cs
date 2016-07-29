using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class ListPickerItemsUC : UserControl
  {
    private const double MarginTop = 40.0;
    private const double MarginBottom = 16.0;
    private const double ElementHeight = 64.0;
    private const double ListMarginTop = 13.0;
    private const double ListMarginBottom = 27.0;
    private double _verticalOffset;
    internal Storyboard AnimClip;
    internal Storyboard AnimClipHide;
    internal Grid containerGrid;
    internal TranslateTransform transform;
    internal RectangleGeometry rectGeometry;
    internal ScaleTransform scaleTransform;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    public ObservableCollection<ListPickerListItem> ItemsSource { get; set; }

    public DataTemplate ItemTemplate { get; set; }

    public ListPickerListItem SelectedItem { get; set; }

    public double PickerMaxHeight { get; set; }

    public double PickerWidth { get; set; }

    public Point ShowPosition { get; set; }

    public FrameworkElement ParentElement { get; set; }

    public ListPickerItemsUC()
    {
      this.InitializeComponent();
      this.listBox.Opacity = 0.0;
      this.scaleTransform.ScaleY = 0.0;
    }

    public void Setup()
    {
      if (this.ItemTemplate != null)
        this.listBox.ItemTemplate = this.ItemTemplate;
      this.listBox.ItemsSource = (IList) this.ItemsSource;
      int selectedIndex = this.ItemsSource.IndexOf(this.SelectedItem);
      double val2 = 13.0 + (double) this.ItemsSource.Count * 64.0 + 27.0;
      double num = this.PickerMaxHeight > 0.0 ? Math.Min(this.PickerMaxHeight, val2) : val2;
      double pickerWidth = this.PickerWidth;
      this._verticalOffset = this.ShowPosition.Y;
      if (selectedIndex > -1)
      {
        this.listBox.SizeChanged += (SizeChangedEventHandler) ((o, eventArgs) =>
        {
          this.listBox.SelectedItem = (object) this.ItemsSource[selectedIndex];
          this.ScrollToSelectedItem(selectedIndex);
        });
        this._verticalOffset = this._verticalOffset - 64.0;
      }
      if (this._verticalOffset < 40.0)
        this._verticalOffset = 40.0;
      else if (this._verticalOffset + num > this.ParentElement.ActualHeight - 16.0)
        this._verticalOffset = this._verticalOffset - (this._verticalOffset + num - this.ParentElement.ActualHeight + 16.0);
      this.containerGrid.Height = num;
      this.containerGrid.Width = pickerWidth;
      this.containerGrid.Margin = new Thickness(this.ShowPosition.X, this._verticalOffset, 0.0, 0.0);
      this.scaleTransform.CenterY = this.ShowPosition.Y - this._verticalOffset;
      this.rectGeometry.Rect = new Rect()
      {
        X = 0.0,
        Y = 0.0,
        Height = num,
        Width = pickerWidth
      };
    }

    private void ScrollToSelectedItem(int selectedIndex)
    {
      if (selectedIndex == this.ItemsSource.Count - 1)
      {
        this.listBox.ScrollTo(this.listBox.ItemsSource[selectedIndex]);
      }
      else
      {
        double num = this.ShowPosition.Y - this._verticalOffset;
        this.listBox.ScrollToPosition(ListPickerItemsUC.GetItemOffset(selectedIndex) - num);
      }
    }

    private static double GetItemOffset(int itemIndex)
    {
      return 13.0 + 64.0 * (double) itemIndex;
    }

    private void ListBox_OnTap(object sender, GestureEventArgs e)
    {
      foreach (ListPickerListItem listPickerListItem in (Collection<ListPickerListItem>) this.ItemsSource)
        listPickerListItem.IsSelected = false;
      ListPickerListItem listPickerListItem1 = this.listBox.SelectedItem as ListPickerListItem;
      if (listPickerListItem1 == null)
        return;
      listPickerListItem1.IsSelected = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ListPickerItemsUC.xaml", UriKind.Relative));
      this.AnimClip = (Storyboard) this.FindName("AnimClip");
      this.AnimClipHide = (Storyboard) this.FindName("AnimClipHide");
      this.containerGrid = (Grid) this.FindName("containerGrid");
      this.transform = (TranslateTransform) this.FindName("transform");
      this.rectGeometry = (RectangleGeometry) this.FindName("rectGeometry");
      this.scaleTransform = (ScaleTransform) this.FindName("scaleTransform");
      this.listBox = (ExtendedLongListSelector) this.FindName("listBox");
    }
  }
}
