using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information
{
    public partial class InformationPage : PageBase
  {
    private bool _isInitialized;

    public InformationViewModel ViewModel
    {
      get
      {
        return this.DataContext as InformationViewModel;
      }
    }

    public InformationPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      InformationViewModel viewModel = new InformationViewModel(long.Parse(this.NavigationContext.QueryString["CommunityId"]));
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
        viewModel.SaveChanges();
      });
      applicationBarIconButton2.Click += (EventHandler) ((p, f) => Navigator.Current.GoBack());
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = viewModel.IsFormEnabled && viewModel.IsFormCompleted);
      this.ApplicationBar.Buttons.Add((object) appBarButtonSave);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton2);
      viewModel.OnTextBoxGotFocus += (RoutedEventHandler) ((o, args) =>
      {
        this.TextBoxPanel.IsOpen = true;
        this.Viewer.ScrollToOffsetWithAnimation(((UIElement) o).GetRelativePosition((UIElement) this.ViewerContent).Y - 38.0, 0.2, false);
      });
      viewModel.OnTextBoxLostFocus += (RoutedEventHandler) ((o, args) => this.TextBoxPanel.IsOpen = false);
      viewModel.Reload();
      this._isInitialized = true;
    }

  }
}
