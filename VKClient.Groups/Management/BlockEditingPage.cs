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
    public partial class BlockEditingPage : PageBase
  {
    private bool _isInitialized;

    public BlockEditingViewModel ViewModel
    {
      get
      {
        return this.DataContext as BlockEditingViewModel;
      }
    }

    public BlockEditingPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        BlockEditingViewModel viewModel = new BlockEditingViewModel(long.Parse(this.NavigationContext.QueryString["CommunityId"]), (User) ParametersRepository.GetParameterForIdAndReset("CommunityManagementBlockEditingUser"), (User) ParametersRepository.GetParameterForIdAndReset("CommunityManagementBlockEditingManager"), bool.Parse(this.NavigationContext.QueryString["IsEditing"]), bool.Parse(this.NavigationContext.QueryString["IsOpenedWithoutPicker"]));
        this.DataContext = (object) viewModel;
        ApplicationBarIconButton appBarButtonSave = new ApplicationBarIconButton()
        {
          IconUri = new Uri("/Resources/check.png", UriKind.Relative),
          Text = CommonResources.AppBarMenu_Save
        };
        ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton()
        {
          IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
          Text = CommonResources.AppBar_Cancel
        };
        appBarButtonSave.Click += (EventHandler) ((p, f) =>
        {
          this.Focus();
          viewModel.SaveChanges(this.NavigationService);
        });
        applicationBarIconButton.Click += (EventHandler) ((p, f) => Navigator.Current.GoBack());
        this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
        viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = viewModel.IsFormEnabled);
        this.ApplicationBar.Buttons.Add((object) appBarButtonSave);
        this.ApplicationBar.Buttons.Add((object) applicationBarIconButton);
        this._isInitialized = true;
      }
      else
      {
        if (!ParametersRepository.Contains("BlockDurationUnixTime"))
          return;
        this.ViewModel.UpdateDuration((int) ParametersRepository.GetParameterForIdAndReset("BlockDurationUnixTime"));
      }
    }

    private void ManagerName_OnClicked(object sender, RoutedEventArgs e)
    {
      this.ViewModel.GoToManagerProfile();
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.Focus();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void BlockDurationPicker_OnClicked(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      Navigator.Current.NavigateToCommunityManagementBlockDurationPicker(this.ViewModel.DurationUnixTime);
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
