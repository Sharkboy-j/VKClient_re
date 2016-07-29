using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public partial class BlockDurationPicker : PageBase
  {

    public BlockDurationPickerViewModel ViewModel
    {
      get
      {
        return this.DataContext as BlockDurationPickerViewModel;
      }
    }

    public BlockDurationPicker()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      this.DataContext = (object) new BlockDurationPickerViewModel(int.Parse(this.NavigationContext.QueryString["DurationUnixTime"]));
    }

    private void CurrentDuration_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void Forever_OnClicked(object sender, GestureEventArgs e)
    {
      ParametersRepository.SetParameterForId("BlockDurationUnixTime", (object) 0);
      Navigator.Current.GoBack();
    }

    private void ForYear_OnClicked(object sender, GestureEventArgs e)
    {
      string paramId = "BlockDurationUnixTime";
      DateTime dateTime = this.ViewModel.TimeNow;
      dateTime = dateTime.AddYears(1);
      int local = Extensions.DateTimeToUnixTimestamp(dateTime.ToUniversalTime(), true);
      ParametersRepository.SetParameterForId(paramId, (object) local);
      Navigator.Current.GoBack();
    }

    private void ForMonth_OnClicked(object sender, GestureEventArgs e)
    {
      string paramId = "BlockDurationUnixTime";
      DateTime dateTime = this.ViewModel.TimeNow;
      dateTime = dateTime.AddMonths(1);
      int local = Extensions.DateTimeToUnixTimestamp(dateTime.ToUniversalTime(), true);
      ParametersRepository.SetParameterForId(paramId, (object) local);
      Navigator.Current.GoBack();
    }

    private void ForWeek_OnClicked(object sender, GestureEventArgs e)
    {
      string paramId = "BlockDurationUnixTime";
      DateTime dateTime = this.ViewModel.TimeNow;
      dateTime = dateTime.AddDays(7.0);
      int local = Extensions.DateTimeToUnixTimestamp(dateTime.ToUniversalTime(), true);
      ParametersRepository.SetParameterForId(paramId, (object) local);
      Navigator.Current.GoBack();
    }

    private void ForDay_OnClicked(object sender, GestureEventArgs e)
    {
      string paramId = "BlockDurationUnixTime";
      DateTime dateTime = this.ViewModel.TimeNow;
      dateTime = dateTime.AddDays(1.0);
      int local = Extensions.DateTimeToUnixTimestamp(dateTime.ToUniversalTime(), true);
      ParametersRepository.SetParameterForId(paramId, (object) local);
      Navigator.Current.GoBack();
    }

    private void ForHour_OnClicked(object sender, GestureEventArgs e)
    {
      string paramId = "BlockDurationUnixTime";
      DateTime dateTime = this.ViewModel.TimeNow;
      dateTime = dateTime.AddHours(1.0);
      int local = Extensions.DateTimeToUnixTimestamp(dateTime.ToUniversalTime(), true);
      ParametersRepository.SetParameterForId(paramId, (object) local);
      Navigator.Current.GoBack();
    }

  }
}
