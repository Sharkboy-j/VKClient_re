using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep2UC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxCountry;
    internal TextBox textBoxPhoneNumber;
    internal TextBlock textBlockPhoneNumberWatermark;
    private bool _contentLoaded;

    private RegistrationPhoneNumberViewModel RegistrationPhoneNumberVM
    {
      get
      {
        return this.DataContext as RegistrationPhoneNumberViewModel;
      }
    }

    public bool ShowingPopup { get; set; }

    public RegistrationStep2UC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.RegistrationStep2UC_Loaded);
    }

    private void RegistrationStep2UC_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBlockPhoneNumberWatermark.Visibility = this.textBoxPhoneNumber.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void textBoxTextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      this.textBlockPhoneNumberWatermark.Visibility = this.textBoxPhoneNumber.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateSource(TextBox textBox)
    {
      textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void textBoxCountry_Tap(object sender, GestureEventArgs e)
    {
      this.ShowingPopup = true;
      CountryPickerUC.Show(this.RegistrationPhoneNumberVM.Country, false, (Action<Country>) (c =>
      {
        this.ShowingPopup = false;
        this.RegistrationPhoneNumberVM.Country = c;
        this.textBoxPhoneNumber.Focus();
      }), (Action) (() => this.ShowingPopup = false));
    }

    private void textBoxPhoneNumber_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      RegistrationPage registrationPage = FramePageUtils.CurrentPage as RegistrationPage;
      if (registrationPage == null)
        return;
      registrationPage.RegistrationVM.CompleteCurrentStep();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep2UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.textBoxCountry = (TextBox) this.FindName("textBoxCountry");
      this.textBoxPhoneNumber = (TextBox) this.FindName("textBoxPhoneNumber");
      this.textBlockPhoneNumberWatermark = (TextBlock) this.FindName("textBlockPhoneNumberWatermark");
    }
  }
}
