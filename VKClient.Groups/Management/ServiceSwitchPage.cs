using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public partial class ServiceSwitchPage : PageBase
  {

    public ServiceSwitchViewModel ViewModel
    {
      get
      {
        return this.DataContext as ServiceSwitchViewModel;
      }
    }

    public ServiceSwitchPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      this.DataContext = (object) new ServiceSwitchViewModel((CommunityService) int.Parse(this.NavigationContext.QueryString["Service"]), (CommunityServiceState) int.Parse(this.NavigationContext.QueryString["CurrentState"]));
    }

    private void Disabled_OnClicked(object sender, GestureEventArgs e)
    {
      this.ViewModel.SaveResult(CommunityServiceState.Disabled);
    }

    private void Opened_OnClicked(object sender, GestureEventArgs e)
    {
      this.ViewModel.SaveResult(CommunityServiceState.Opened);
    }

    private void Limited_OnClicked(object sender, GestureEventArgs e)
    {
      this.ViewModel.SaveResult(CommunityServiceState.Limited);
    }

    private void Closed_OnClicked(object sender, GestureEventArgs e)
    {
      this.ViewModel.SaveResult(CommunityServiceState.Closed);
    }

  }
}
