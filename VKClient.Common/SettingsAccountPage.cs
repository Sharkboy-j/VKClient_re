using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class SettingsAccountPage : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    private bool _contentLoaded;

    private SettingsAccountViewModel VM
    {
      get
      {
        return this.DataContext as SettingsAccountViewModel;
      }
    }

    public SettingsAccountPage()
    {
      this.InitializeComponent();
      this.ucHeader.textBlockTitle.Text = CommonResources.NewSettings_Account.ToUpperInvariant();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        SettingsAccountViewModel accountViewModel = new SettingsAccountViewModel();
        accountViewModel.LoadData();
        this.DataContext = (object) accountViewModel;
        this._isInitialized = true;
      }
      this.HandleInputParameters();
    }

    private void HandleInputParameters()
    {
      ValidationUserResponse validationResponse = ParametersRepository.GetParameterForIdAndReset("ValidationResponse") as ValidationUserResponse;
      if (validationResponse == null)
        return;
      this.VM.HandleValidationResponse(validationResponse);
    }

    private void PhoneNumberTap(object sender, GestureEventArgs e)
    {
      this.VM.HandlePhoneNumberTap();
    }

    private void EmailTap(object sender, GestureEventArgs e)
    {
      this.VM.HandleEmailTap();
    }

    private void ChangePasswordTap(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToChangePassword();
    }

    private void ShortNameTap(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToChangeShortName(this.VM.ShortName);
    }

    private void NewsFilterTap(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToManageSources(ManageSourcesMode.ManageHiddenNewsSources);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SettingsAccountPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
    }
  }
}
