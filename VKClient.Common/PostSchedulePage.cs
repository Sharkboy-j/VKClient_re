using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Framework.DatePicker;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;

namespace VKClient.Common
{
  public class PostSchedulePage : PageBase
  {
    private readonly ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/check.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Save
    };
    private readonly ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private bool _isInitialized;
    private PostScheduleViewModel _viewModel;
    internal PostScheduleDatePicker datePicker;
    internal PostScheduleTimePicker timePicker;
    private bool _contentLoaded;

    public PostSchedulePage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = new ApplicationBar()
      {
        BackgroundColor = VKConstants.AppBarBGColor,
        ForegroundColor = VKConstants.AppBarFGColor,
        Opacity = 0.9
      };
      applicationBar.Buttons.Add((object) this._appBarButtonCheck);
      applicationBar.Buttons.Add((object) this._appBarButtonCancel);
      this._appBarButtonCheck.Click += new EventHandler(this.AppBarButtonCheck_Click);
      this._appBarButtonCancel.Click += new EventHandler(this.AppBarButtonCancel_Click);
      this.ApplicationBar = (IApplicationBar) applicationBar;
    }

    private void AppBarButtonCheck_Click(object sender, EventArgs e)
    {
      TimerAttachment timerAttachment = new TimerAttachment()
      {
        ScheduledPublishDateTime = this._viewModel.GetScheduledDateTime()
      };
      DateTime dateTime = timerAttachment.ScheduledPublishDateTime;
      int year1 = dateTime.Year;
      dateTime = DateTime.Now;
      int year2 = dateTime.Year;
      if (year1 - year2 > 0)
      {
        int num = (int) MessageBox.Show(CommonResources.PostSchedule_InvalidPublishDate, CommonResources.Error, MessageBoxButton.OK);
      }
      else
      {
        ParametersRepository.SetParameterForId("PickedTimer", (object) timerAttachment);
        Navigator.Current.GoBack();
      }
    }

    private void AppBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long ticks = long.Parse(this.NavigationContext.QueryString["PublishDateTime"]);
      this._viewModel = ticks > 0L ? new PostScheduleViewModel(new DateTime?(new DateTime(ticks))) : new PostScheduleViewModel(new DateTime?());
      this.DataContext = (object) this._viewModel;
      this._isInitialized = true;
    }

    private void DatePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.datePicker, (object[]) null);
    }

    private void TimePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.timePicker, (object[]) null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/PostSchedulePage.xaml", UriKind.Relative));
      this.datePicker = (PostScheduleDatePicker) this.FindName("datePicker");
      this.timePicker = (PostScheduleTimePicker) this.FindName("timePicker");
    }
  }
}
