using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Library.Registration;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.UC.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class RegistrationPage : PageBase
  {
    private ApplicationBar _appBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/check.png", UriKind.Relative),
      Text = CommonResources.ChatEdit_AppBar_Save
    };
    private readonly DelayedExecutor _de = new DelayedExecutor(300);
    private bool _isInitialized;
    private string _registrationVMFileID;
    internal GenericHeaderUC Header;
    internal Rectangle rectProgress;
    internal RegistrationStep1UC ucRegistrationStep1;
    internal RegistrationStep2UC ucRegistrationStep2;
    internal RegistrationStep3UC ucRegistrationStep3;
    internal RegistrationStep4UC ucRegistrationStep4;
    private bool _contentLoaded;

    public RegistrationViewModel RegistrationVM
    {
      get
      {
        return this.DataContext as RegistrationViewModel;
      }
    }

    public RegistrationPage()
    {
      this.InitializeComponent();
      this.Header.HideSandwitchButton = true;
      this.SuppressMenu = true;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click += new EventHandler(this._appBarButtonCheck_Click);
      this._appBar.Buttons.Add((object) this._appBarButtonCheck);
      this.ApplicationBar = (IApplicationBar) this._appBar;
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      switch (this.RegistrationVM.CurrentStep)
      {
        case 1:
          if (this.ucRegistrationStep1.textBoxFirstName.Text.Length < 2 || this.ucRegistrationStep1.textBoxLastName.Text.Length < 2)
          {
            new GenericInfoUC().ShowAndHideLater(CommonResources.Registration_WrongName, null);
            return;
          }
          break;
        case 4:
          if (this.ucRegistrationStep4.passwordBox.Password.Length < 6)
          {
            new GenericInfoUC().ShowAndHideLater(CommonResources.Registration_ShortPassword, null);
            return;
          }
          break;
      }
      this.RegistrationVM.CompleteCurrentStep();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._registrationVMFileID = this.NavigationContext.QueryString["SessionId"];
        RegistrationViewModel registrationViewModel = new RegistrationViewModel();
        CacheManager.TryDeserialize((IBinarySerializable) registrationViewModel, this._registrationVMFileID, CacheManager.DataType.CachedData);
        registrationViewModel.OnMovedForward = (Action) (() => this.HandleMoveBackOrForward());
        this.DataContext = (object) registrationViewModel;
        this.HandleMoveBackOrForward();
        this._isInitialized = true;
        registrationViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.UpdateButtonIsEnabled();
      }
      this.HandleInputParams();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      CacheManager.TrySerialize((IBinarySerializable) this.RegistrationVM, this._registrationVMFileID, false, CacheManager.DataType.CachedData);
    }

    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
    {
      base.OnRemovedFromJournal(e);
      CacheManager.TryDelete(this._registrationVMFileID, CacheManager.DataType.CachedData);
    }

    private void HandleMoveBackOrForward()
    {
      int num1 = this.RegistrationVM.CurrentStep - 1;
      bool flag = num1 <= 3;
      if (!flag)
        this.NavigationService.ClearBackStack();
      this.rectProgress.Width = flag ? 120.0 : 240.0;
      double num2 = flag ? (double) (120 * num1) : (double) (240 * (num1 - 4));
      TranslateTransform translateTransform = this.rectProgress.RenderTransform as TranslateTransform;
      TranslateTransform target = translateTransform;
      double x = translateTransform.X;
      double to = num2;
      DependencyProperty dependencyProperty = TranslateTransform.XProperty;
      int duration = 250;
      int? startTime = new int?(0);
      CubicEase cubicEase = new CubicEase();
      int num3 = 2;
      cubicEase.EasingMode = (EasingMode) num3;
      int num4 = 0;
      target.Animate(x, to, (object) dependencyProperty, duration, startTime, (IEasingFunction) cubicEase, null, num4 != 0);
      switch (num1)
      {
        case 0:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (string.IsNullOrEmpty(this.ucRegistrationStep1.textBoxFirstName.Text))
            {
              this.ucRegistrationStep1.textBoxFirstName.Focus();
            }
            else
            {
              if (!string.IsNullOrEmpty(this.ucRegistrationStep1.textBoxLastName.Text))
                return;
              this.ucRegistrationStep1.textBoxLastName.Focus();
            }
          }))));
          break;
        case 1:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (!string.IsNullOrEmpty(this.ucRegistrationStep2.textBoxPhoneNumber.Text))
              return;
            this.ucRegistrationStep2.textBoxPhoneNumber.Focus();
          }))));
          break;
        case 2:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (!string.IsNullOrEmpty(this.ucRegistrationStep3.textBoxConfirmationCode.Text))
              return;
            this.ucRegistrationStep3.textBoxConfirmationCode.Focus();
          }))));
          break;
        case 3:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (!string.IsNullOrEmpty(this.ucRegistrationStep4.passwordBox.Password))
              return;
            this.ucRegistrationStep4.passwordBox.Focus();
          }))));
          break;
      }
    }

    private void HandleInputParams()
    {
      List<Stream> streamList = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      Rect rect = new Rect();
      if (ParametersRepository.Contains("UserPicSquare"))
        rect = (Rect) ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
      if (streamList == null || streamList.Count <= 0)
        return;
      this.RegistrationVM.SetUserPhoto(streamList[0], rect);
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanCompleteCurrentStep"))
        return;
      this.UpdateButtonIsEnabled();
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (this.ucRegistrationStep2.ShowingPopup || !this.RegistrationVM.HandleBackKey())
        return;
      this.HandleMoveBackOrForward();
      e.Cancel = true;
    }

    private void UpdateButtonIsEnabled()
    {
      this._appBarButtonCheck.IsEnabled = this.RegistrationVM.CanCompleteCurrentStep;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/RegistrationPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.rectProgress = (Rectangle) this.FindName("rectProgress");
      this.ucRegistrationStep1 = (RegistrationStep1UC) this.FindName("ucRegistrationStep1");
      this.ucRegistrationStep2 = (RegistrationStep2UC) this.FindName("ucRegistrationStep2");
      this.ucRegistrationStep3 = (RegistrationStep3UC) this.FindName("ucRegistrationStep3");
      this.ucRegistrationStep4 = (RegistrationStep4UC) this.FindName("ucRegistrationStep4");
    }
  }
}
