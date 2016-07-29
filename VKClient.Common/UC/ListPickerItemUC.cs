using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ListPickerItemUC : UserControl
  {
    internal VisualStateGroup CommonStates;
    internal VisualState Normal;
    internal VisualState Selected;
    internal TextBlock textBlock;
    private bool _contentLoaded;

    public ListPickerItemUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ListPickerItemUC.xaml", UriKind.Relative));
      this.CommonStates = (VisualStateGroup) this.FindName("CommonStates");
      this.Normal = (VisualState) this.FindName("Normal");
      this.Selected = (VisualState) this.FindName("Selected");
      this.textBlock = (TextBlock) this.FindName("textBlock");
    }
  }
}
