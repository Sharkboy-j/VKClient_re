using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class AttachmentSubPickerUC : UserControl
  {
    private static int _instancesCount;
    internal TextBlock textBlockTitle;
    internal ItemsControl itemsControl;
    private bool _contentLoaded;

    public event AttachmentSubItemSelectedEventHandler ItemSelected;

    public AttachmentSubPickerUC()
    {
      this.InitializeComponent();
      ++AttachmentSubPickerUC._instancesCount;
    }

    ~AttachmentSubPickerUC()
    {
      --AttachmentSubPickerUC._instancesCount;
    }

    private void Item_OnTapped(object sender, GestureEventArgs e)
    {
      AttachmentPickerItem picketItem = (sender as FrameworkElement).DataContext as AttachmentPickerItem;
      if (picketItem == null || this.ItemSelected == null)
        return;
      this.ItemSelected(picketItem);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/AttachmentSubPickerUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.itemsControl = (ItemsControl) this.FindName("itemsControl");
    }
  }
}
