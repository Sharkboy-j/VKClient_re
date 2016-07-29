using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class GamesCatalogHeaderUC : UserControl
  {
    internal StackPanel panelContent;
    internal TextBlock textBlockTitle;
    internal Border borderNew;
    private bool _contentLoaded;

    public GamesCatalogHeaderUC()
    {
      this.InitializeComponent();
    }

    private void BorderNew_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.textBlockTitle.MaxWidth = this.panelContent.ActualWidth - (this.borderNew.ActualWidth > 0.0 ? this.borderNew.ActualWidth + this.borderNew.Margin.Left : 0.0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GamesCatalogHeaderUC.xaml", UriKind.Relative));
      this.panelContent = (StackPanel) this.FindName("panelContent");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.borderNew = (Border) this.FindName("borderNew");
    }
  }
}
