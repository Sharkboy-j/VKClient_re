using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework.DatePicker;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
    public partial class EventDatesUC : UserControl
  {

    public EventDatesViewModel ViewModel
    {
      get
      {
        return this.DataContext as EventDatesViewModel;
      }
    }

    public EventDatesUC()
    {
      this.InitializeComponent();
    }

    private void StartDatePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.StartDatePicker, (object[]) null);
    }

    private void StartTimePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.StartTimePicker, (object[]) null);
    }

    private void FinishDatePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.FinishDatePicker, (object[]) null);
    }

    private void FinishTimePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.FinishTimePicker, (object[]) null);
    }

    private void SetFinishTimeButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      this.ViewModel.FinishFieldsVisibility = Visibility.Visible;
    }

  }
}
