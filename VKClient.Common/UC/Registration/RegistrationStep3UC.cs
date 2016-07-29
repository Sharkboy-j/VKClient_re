using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep3UC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxConfirmationCode;
    internal TextBlock textBlockConfirmationCodeWatermark;
    private bool _contentLoaded;

    public RegistrationStep3UC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.RegistrationStep3UC_Loaded);
    }

    private void RegistrationStep3UC_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBlockConfirmationCodeWatermark.Visibility = this.textBoxConfirmationCode.Text == string.Empty ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      this.textBlockConfirmationCodeWatermark.Visibility = this.textBoxConfirmationCode.Text == string.Empty ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GridCallTap(object sender, GestureEventArgs e)
    {
      (this.DataContext as RegistrationConfirmationCodeViewModel).RequestCall();
    }

    private void UpdateSource(TextBox textBox)
    {
      textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void textBoxConfirmationCode_KeyDown(object sender, KeyEventArgs e)
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep3UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.textBoxConfirmationCode = (TextBox) this.FindName("textBoxConfirmationCode");
      this.textBlockConfirmationCodeWatermark = (TextBlock) this.FindName("textBlockConfirmationCodeWatermark");
    }
  }
}
