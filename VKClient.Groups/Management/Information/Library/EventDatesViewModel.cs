using System;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information.Library
{
  public sealed class EventDatesViewModel : ViewModelBase
  {
    private Visibility _visibility;
    private Visibility _finishFieldsVisibility;
    private DateTime _startDate;
    private DateTime _startTime;
    private DateTime _finishDate;
    private DateTime _finishTime;

    public InformationViewModel ParentViewModel { get; set; }

    public Visibility Visibility
    {
      get
      {
        return this._visibility;
      }
      set
      {
        this._visibility = value;
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Visibility));
      }
    }

    public Visibility FinishFieldsVisibility
    {
      get
      {
        return this._finishFieldsVisibility;
      }
      set
      {
        this._finishFieldsVisibility = value;
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FinishFieldsVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.SetFinishTimeButtonVisibility));
      }
    }

    public Visibility SetFinishTimeButtonVisibility
    {
      get
      {
        return (this.FinishFieldsVisibility == Visibility.Collapsed).ToVisiblity();
      }
    }

    public DateTime StartDate
    {
      get
      {
        return this._startDate;
      }
      set
      {
        this._startDate = value;
        this.NotifyPropertyChanged<DateTime>((System.Linq.Expressions.Expression<Func<DateTime>>) (() => this.StartDate));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.StartDateString));
      }
    }

    public DateTime StartTime
    {
      get
      {
        return this._startTime;
      }
      set
      {
        this._startTime = value;
        this.NotifyPropertyChanged<DateTime>((System.Linq.Expressions.Expression<Func<DateTime>>) (() => this.StartTime));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.StartTimeString));
      }
    }

    public DateTime FinishDate
    {
      get
      {
        return this._finishDate;
      }
      set
      {
        this._finishDate = value;
        this.NotifyPropertyChanged<DateTime>((System.Linq.Expressions.Expression<Func<DateTime>>) (() => this.FinishDate));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FinishDateString));
      }
    }

    public DateTime FinishTime
    {
      get
      {
        return this._finishTime;
      }
      set
      {
        this._finishTime = value;
        this.NotifyPropertyChanged<DateTime>((System.Linq.Expressions.Expression<Func<DateTime>>) (() => this.FinishTime));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FinishTimeString));
      }
    }

    public string StartDateString
    {
      get
      {
        string str = this.StartDate.ToString("dd MMM yyyy");
        if (str.StartsWith("0"))
          str = str.Substring(1);
        return str;
      }
    }

    public string FinishDateString
    {
      get
      {
        string str = this.FinishDate.ToString("dd MMM yyyy");
        if (str.StartsWith("0"))
          str = str.Substring(1);
        return str;
      }
    }

    public string StartTimeString
    {
      get
      {
        return this.StartTime.ToString("HH:mm");
      }
    }

    public string FinishTimeString
    {
      get
      {
        return this.FinishTime.ToString("HH:mm");
      }
    }

    public EventDatesViewModel(InformationViewModel parentViewModel)
    {
      this.ParentViewModel = parentViewModel;
    }

    public void Read(CommunitySettings information)
    {
      if (information.Type != GroupType.Event)
      {
        this.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.StartDate = this.StartTime = Extensions.UnixTimeStampToDateTime((double) information.start_date.Value, false);
        this.FinishDate = this.FinishTime = Extensions.UnixTimeStampToDateTime((double) information.finish_date.Value, false);
        if (this.FinishTime.Second != 1)
          return;
        this.FinishDate = this.FinishTime = this.StartDate.AddHours(4.0);
        this.FinishFieldsVisibility = Visibility.Collapsed;
      }
    }
  }
}
