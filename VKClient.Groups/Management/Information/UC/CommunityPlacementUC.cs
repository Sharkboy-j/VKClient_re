using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
    public partial class CommunityPlacementUC : UserControl
  {
    public CommunityPlacementViewModel ViewModel
    {
      get
      {
        return this.DataContext as CommunityPlacementViewModel;
      }
    }

    public CommunityPlacementUC()
    {
      this.InitializeComponent();
    }

    private void OnClicked(object sender, GestureEventArgs e)
    {
      if (this.ViewModel.EditButtonVisibility != Visibility.Collapsed)
        return;
      this.EditButton_OnClicked(sender, e);
    }

    private void EditButton_OnClicked(object sender, GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      e.Handled = true;
      this.ViewModel.NavigateToPlacementSelection();
    }

  }
}
