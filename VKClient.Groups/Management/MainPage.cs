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
    public partial class MainPage : PageBase
    {

        public MainViewModel ViewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            long id = long.Parse(this.NavigationContext.QueryString["CommunityId"]);

            string temp = this.NavigationContext.QueryString["CommunityType"].ToString();
            string temp2 = this.NavigationContext.QueryString["CommunityType"];

            GroupType groupType = (GroupType)int.Parse(temp);
            bool flag = this.NavigationContext.QueryString["IsAdministrator"].ToLower() == "true";
            int num1 = (int)groupType;
            int num2 = flag ? 1 : 0;
            this.DataContext = (object)new MainViewModel(id, (GroupType)num1, num2 != 0);
        }

        private void Information_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementInformation(this.ViewModel.Id);
        }

        private void Services_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServices(this.ViewModel.Id);
        }

        private void Managers_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementManagers(this.ViewModel.Id, this.ViewModel.Type);
        }

        private void Requests_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementRequests(this.ViewModel.Id);
        }

        private void Invitations_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementInvitations(this.ViewModel.Id);
        }

        private void Members_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunitySubscribers(this.ViewModel.Id, this.ViewModel.Type, true, false, false);
        }

        private void Blacklist_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementBlacklist(this.ViewModel.Id, this.ViewModel.Type);
        }

        private void Links_OnClicked(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementLinks(this.ViewModel.Id);
        }

    }
}
