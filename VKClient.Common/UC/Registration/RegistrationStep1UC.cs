using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep1UC : UserControl
  {
    internal Grid LayoutRoot;
    internal ContextMenu PhotoMenu;
    internal TextBox textBoxFirstName;
    internal TextBlock textBlockFirstNameWatermark;
    internal TextBox textBoxLastName;
    internal TextBlock textBlockLastNameWatermark;
    private bool _contentLoaded;

    private RegistrationProfileViewModel VM
    {
      get
      {
        return this.DataContext as RegistrationProfileViewModel;
      }
    }

    public RegistrationStep1UC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.RegistrationStep1UC_Loaded);
    }

    private void RegistrationStep1UC_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBlockFirstNameWatermark.Visibility = this.textBoxFirstName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible;
      this.textBlockLastNameWatermark.Visibility = this.textBoxLastName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ChoosePhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.DoChoosePhoto();
    }

    private void ChosePhotoMenuClick(object sender, RoutedEventArgs e)
    {
      this.DoChoosePhoto();
    }

    private void DoChoosePhoto()
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
    }

    private void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
    {
      this.VM.DeletePhoto();
    }

    private void GridPhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.PhotoMenu.IsOpen = true;
    }

    private void textBoxFirstNameChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      this.textBlockFirstNameWatermark.Visibility = this.textBoxFirstName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void textBoxLastNameChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      this.textBlockLastNameWatermark.Visibility = this.textBoxLastName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateSource(TextBox textBox)
    {
      textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void TermsClick(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://vk.com/terms", true, false);
    }

    private void PrivacyClick(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://vk.com/privacy", true, false);
    }

    private void textBoxFirstName_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key.IsDigit())
        e.Handled = true;
      if (e.Key != Key.Enter)
        return;
      this.textBoxLastName.Focus();
    }

    private void textBoxLastName_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key.IsDigit())
        e.Handled = true;
      if (e.Key != Key.Enter)
        return;
      PageBase currentPage = FramePageUtils.CurrentPage;
      if (currentPage == null)
        return;
      currentPage.Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep1UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.PhotoMenu = (ContextMenu) this.FindName("PhotoMenu");
      this.textBoxFirstName = (TextBox) this.FindName("textBoxFirstName");
      this.textBlockFirstNameWatermark = (TextBlock) this.FindName("textBlockFirstNameWatermark");
      this.textBoxLastName = (TextBox) this.FindName("textBoxLastName");
      this.textBlockLastNameWatermark = (TextBlock) this.FindName("textBlockLastNameWatermark");
    }
  }
}
