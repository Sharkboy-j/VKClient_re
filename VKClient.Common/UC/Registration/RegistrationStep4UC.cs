using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep4UC : UserControl
  {
    internal Grid LayoutRoot;
    internal PasswordBox passwordBox;
    internal TextBlock textBlockWatermark;
    private bool _contentLoaded;

    public RegistrationStep4UC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.RegistrationStep4UC_Loaded);
    }

    private void RegistrationStep4UC_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBlockWatermark.Visibility = this.passwordBox.Password == string.Empty ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.UpdateSource(sender as PasswordBox);
      this.textBlockWatermark.Visibility = this.passwordBox.Password == string.Empty ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateSource(PasswordBox textBox)
    {
      textBox.GetBindingExpression(PasswordBox.PasswordProperty).UpdateSource();
    }

    private void passwordBox_KeyDown(object sender, KeyEventArgs e)
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep4UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.passwordBox = (PasswordBox) this.FindName("passwordBox");
      this.textBlockWatermark = (TextBlock) this.FindName("textBlockWatermark");
    }
  }
}
