using System;
using System.Windows;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class SearchParamsViewModel : ViewModelBase
  {
    private readonly ISupportSearchParams _parametersProvider;
    private SearchParams _searchParams;

    public SearchParams SearchParams
    {
      get
      {
        return this._searchParams;
      }
      set
      {
        this._searchParams = value;
        this.NotifyUIProperties();
      }
    }

    public string ParamsStr
    {
      get
      {
        return this._parametersProvider.ParametersSummaryStr;
      }
    }

    public bool IsAnySet
    {
      get
      {
        return this._searchParams.IsAnySet;
      }
    }

    public Visibility AnySetVisibility
    {
      get
      {
        return !this._searchParams.IsAnySet ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility SetParamsVisibility
    {
      get
      {
        return !this._searchParams.IsAnySet ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public SearchParamsViewModel(ISupportSearchParams parametersProvider)
    {
      this._parametersProvider = parametersProvider;
      this._searchParams = new SearchParams();
    }

    public void NavigateToParametersPage()
    {
      this._parametersProvider.OpenParametersPage();
    }

    public void Clear()
    {
      this._parametersProvider.ClearParameters();
      this.NotifyUIProperties();
    }

    private void NotifyUIProperties()
    {
      this.NotifyPropertyChanged<SearchParams>((System.Linq.Expressions.Expression<Func<SearchParams>>) (() => this.SearchParams));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ParamsStr));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.AnySetVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.SetParamsVisibility));
    }
  }
}
