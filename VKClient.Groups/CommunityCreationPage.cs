using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
  public partial class CommunityCreationPage : PageBase
  {
    private bool _isInitialized;

    public CommunityCreationPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      CommunityCreationViewModel viewModel = new CommunityCreationViewModel(this.NavigationService);
      this.DataContext = (object) viewModel;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      applicationBarIconButton1.IconUri = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.Text = CommonResources.AppBarMenu_Save;
      int num = 0;
      applicationBarIconButton1.IsEnabled = num != 0;
      ApplicationBarIconButton appBarButtonSave = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton()
      {
        IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
        Text = CommonResources.AppBar_Cancel
      };
      appBarButtonSave.Click += (EventHandler) ((p, f) =>
      {
        this.Focus();
        viewModel.CreateCommunity();
      });
      applicationBarIconButton2.Click += (EventHandler) ((p, f) => Navigator.Current.GoBack());
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = viewModel.IsFormCompleted && viewModel.IsFormEnabled);
      this.ApplicationBar.Buttons.Add((object) appBarButtonSave);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton2);
      this._isInitialized = true;
    }

    private void NameBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.NameBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void TermsLink_OnClicked(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://vk.com/terms", false, false);
    }

    private void NameBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.Focus();
    }

    private void NameBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      this.Viewer.ScrollToOffsetWithAnimation(((UIElement) sender).GetRelativePosition((UIElement) this.ViewerContent).Y - 38.0, 0.2, false);
    }

    private void NameBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = false;
    }
  }
}
