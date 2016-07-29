using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public partial class ManagerEditingPage : PageBase
  {
    private bool _isInitialized;

    public ManagerEditingViewModel ViewModel
    {
      get
      {
        return this.DataContext as ManagerEditingViewModel;
      }
    }

    public ManagerEditingPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long communityId = long.Parse(this.NavigationContext.QueryString["CommunityId"]);
      User manager = (User) ParametersRepository.GetParameterForIdAndReset("CommunityManager");
      bool isEditing = this.NavigationContext.QueryString.ContainsKey("IsContact");
      bool fromPicker = this.NavigationContext.QueryString.ContainsKey("FromPicker") && bool.Parse(this.NavigationContext.QueryString["FromPicker"]);
      bool isContact = false;
      string position = "";
      string email = "";
      string phone = "";
      if (isEditing)
      {
        isContact = this.NavigationContext.QueryString["IsContact"].ToLower() == "true";
        position = Extensions.ForUI(this.NavigationContext.QueryString["Position"]);
        email = Extensions.ForUI(this.NavigationContext.QueryString["Email"]);
        phone = Extensions.ForUI(this.NavigationContext.QueryString["Phone"]);
      }
      ManagerEditingViewModel viewModel = new ManagerEditingViewModel(communityId, manager, isContact, position, email, phone, isEditing, fromPicker);
      this.DataContext = (object) viewModel;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      applicationBarIconButton1.IconUri = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.Text = CommonResources.AppBarMenu_Save;
      int num = viewModel.IsFormCompleted ? 1 : 0;
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
        viewModel.SaveChanges(false, this.NavigationService);
      });
      applicationBarIconButton2.Click += (EventHandler) ((p, f) => Navigator.Current.GoBack());
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = viewModel.IsFormEnabled && viewModel.IsFormCompleted);
      this.ApplicationBar.Buttons.Add((object) appBarButtonSave);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton2);
      this._isInitialized = true;
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.Focus();
    }

    private void RemoveFromManagers_OnClicked(object sender, GestureEventArgs e)
    {
      if (!this.ViewModel.IsFormEnabled || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.RemovingFromManagers, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.ViewModel.SaveChanges(true, this.NavigationService);
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      this.Viewer.ScrollToOffsetWithAnimation(((UIElement) sender).GetRelativePosition((UIElement) this.ViewerContent).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = false;
    }
  }
}
