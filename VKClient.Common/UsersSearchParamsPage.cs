using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class UsersSearchParamsPage : PageBase
  {
    private readonly ApplicationBarIconButton _appBarButtonSave = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/check.png", UriKind.Relative),
      Text = CommonResources.AppBarMenu_Save
    };
    private readonly ApplicationBarIconButton _appBarButtonReset = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private bool _isIntialized;
    private UsersSearchParamsViewModel _viewModel;
    internal Storyboard ShowCustomAgeAnimation;
    internal Storyboard HideCustomAgeAnimation;
    internal GenericHeaderUC ucHeader;
    internal Border customAgeContainer;
    private bool _contentLoaded;

    public UsersSearchParamsPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_SearchParameters;
      this.ucHeader.HideSandwitchButton = true;
      this.SuppressMenu = true;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      applicationBar.Buttons.Add((object) this._appBarButtonSave);
      this._appBarButtonSave.Click += new EventHandler(this.AppBarButtonSave_OnClick);
      applicationBar.Buttons.Add((object) this._appBarButtonReset);
      this._appBarButtonReset.Click += new EventHandler(this.AppBarButtonReset_OnClick);
      this.ApplicationBar = (IApplicationBar) applicationBar;
    }

    private void AppBarButtonSave_OnClick(object sender, EventArgs eventArgs)
    {
      this._viewModel.Save();
      Navigator.Current.GoBack();
    }

    private void AppBarButtonReset_OnClick(object sender, EventArgs eventArgs)
    {
      Navigator.Current.GoBack();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isIntialized)
        return;
      this._viewModel = new UsersSearchParamsViewModel(ParametersRepository.GetParameterForIdAndReset("UsersSearchParams") as SearchParams);
      this.DataContext = (object) this._viewModel;
      this._isIntialized = true;
    }

    private void CountryPicker_OnTap(object sender, GestureEventArgs e)
    {
      CountryPickerUC.Show(this._viewModel.Country, true, (Action<Country>) (country => this._viewModel.Country = country), null);
    }

    private void CityPicker_OnTap(object sender, GestureEventArgs e)
    {
      if (this._viewModel.Country == null)
        return;
      CityPickerUC.Show(this._viewModel.Country.id, this._viewModel.City, true, (Action<City>) (city => this._viewModel.City = city), null);
    }

    private void AnyAgeCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
    {
      this.ShowCustomAgeAnimation.Begin();
    }

    private void AnyAgeCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
      this.HideCustomAgeAnimation.Begin();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UsersSearchParamsPage.xaml", UriKind.Relative));
      this.ShowCustomAgeAnimation = (Storyboard) this.FindName("ShowCustomAgeAnimation");
      this.HideCustomAgeAnimation = (Storyboard) this.FindName("HideCustomAgeAnimation");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.customAgeContainer = (Border) this.FindName("customAgeContainer");
    }
  }
}
