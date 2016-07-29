using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;

namespace VKClient.Groups.UC
{
    public partial class LinkUC : UserControl
  {
    public LinkUC()
    {
      this.InitializeComponent();
    }

    private void ActionButton_OnClicked(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      ((LinkHeader) this.DataContext).ActionButtonAction((FrameworkElement) this);
    }

    private void ActionButton_OnPressed(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }
  }
}
