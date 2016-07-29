using Microsoft.Phone.Shell;
using System;
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
    public partial class ServicesPage : PageBase
  {
    private bool _isInitialized;

    public ServicesViewModel ViewModel
    {
      get
      {
        return this.DataContext as ServicesViewModel;
      }
    }

    public ServicesPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        ServicesViewModel viewModel = new ServicesViewModel(long.Parse(this.NavigationContext.QueryString["CommunityId"]));
        this.DataContext = (object) viewModel;
        ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton()
        {
          IconUri = new Uri("/Resources/check.png", UriKind.Relative),
          Text = CommonResources.AppBarMenu_Save
        };
        ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton()
        {
          IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
          Text = CommonResources.AppBar_Cancel
        };
        applicationBarIconButton1.Click += (EventHandler) ((p, f) =>
        {
          this.Focus();
          viewModel.SaveChanges();
        });
        applicationBarIconButton2.Click += (EventHandler) ((p, f) => Navigator.Current.GoBack());
        this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
        this.ApplicationBar.Buttons.Add((object) applicationBarIconButton1);
        this.ApplicationBar.Buttons.Add((object) applicationBarIconButton2);
        viewModel.Reload();
        this._isInitialized = true;
      }
      else
      {
        if (!ParametersRepository.Contains("CommunityManagementService"))
          return;
        CommunityService communityService = (CommunityService) ParametersRepository.GetParameterForIdAndReset("CommunityManagementService");
        CommunityServiceState communityServiceState = (CommunityServiceState) ParametersRepository.GetParameterForIdAndReset("CommunityManagementServiceNewState");
        switch (communityService)
        {
          case CommunityService.Wall:
            this.ViewModel.WallOrComments = communityServiceState;
            break;
          case CommunityService.Photos:
            this.ViewModel.Photos = communityServiceState;
            break;
          case CommunityService.Videos:
            this.ViewModel.Videos = communityServiceState;
            break;
          case CommunityService.Audios:
            this.ViewModel.Audios = communityServiceState;
            break;
          case CommunityService.Documents:
            this.ViewModel.Documents = communityServiceState;
            break;
          case CommunityService.Discussions:
            this.ViewModel.Discussions = communityServiceState;
            break;
        }
      }
    }

    private void WallOption_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Wall, this.ViewModel.WallOrComments);
    }

    private void PhotosOption_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Photos, this.ViewModel.Photos);
    }

    private void VideosOption_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Videos, this.ViewModel.Videos);
    }

    private void AudiosOption_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Audios, this.ViewModel.Audios);
    }

    private void DocumentsOption_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Documents, this.ViewModel.Documents);
    }

    private void DiscussionsOption_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Discussions, this.ViewModel.Discussions);
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
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
