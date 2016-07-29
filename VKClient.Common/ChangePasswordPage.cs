using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class ChangePasswordPage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/check.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Save
    };
    private ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private ApplicationBar _appBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    internal PasswordBox textBoxOldPassword;
    internal PasswordBox textBoxNewPassword;
    internal PasswordBox textBoxConfirmNewPassword;
    private bool _contentLoaded;

    private ChangePasswordViewModel VM
    {
      get
      {
        return this.DataContext as ChangePasswordViewModel;
      }
    }

    public ChangePasswordPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.TextBlockTitle.Text = CommonResources.Settings_ChangePassword.ToUpperInvariant();
      this.SuppressMenu = true;
      this.ucHeader.HideSandwitchButton = true;
      this.Loaded += new RoutedEventHandler(this.ChangePasswordPage_Loaded);
    }

    private void ChangePasswordPage_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBoxOldPassword.Focus();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click += new EventHandler(this._appBarButtonCheck_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._appBar.Buttons.Add((object) this._appBarButtonCheck);
      this._appBar.Buttons.Add((object) this._appBarButtonCancel);
      this.ApplicationBar = (IApplicationBar) this._appBar;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      this.VM.UpdatePassword((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() => this.UpdateAppBar()))));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this.DataContext = (object) new ChangePasswordViewModel();
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanUpdatePassword;
    }

    private void textBoxOldPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.VM.OldPassword = this.textBoxOldPassword.Password;
      this.UpdateAppBar();
    }

    private void textBoxNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.VM.NewPassword = this.textBoxNewPassword.Password;
      this.UpdateAppBar();
    }

    private void textBoxConfirmNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.VM.ConfirmNewPassword = this.textBoxConfirmNewPassword.Password;
      this.UpdateAppBar();
    }

    private void textBoxOldPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.textBoxNewPassword.Focus();
    }

    private void textBoxNewPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.textBoxConfirmNewPassword.Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/ChangePasswordPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.textBoxOldPassword = (PasswordBox) this.FindName("textBoxOldPassword");
      this.textBoxNewPassword = (PasswordBox) this.FindName("textBoxNewPassword");
      this.textBoxConfirmNewPassword = (PasswordBox) this.FindName("textBoxConfirmNewPassword");
    }
  }
}
