using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Groups.UC
{
    public partial class CommunityInvitationsUC : UserControl
  {
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof (CommunityInvitations), typeof (CommunityInvitationsUC), new PropertyMetadata(new PropertyChangedCallback(CommunityInvitationsUC.OnModelChanged)));
    

    public CommunityInvitations Model
    {
      get
      {
        return (CommunityInvitations) this.GetValue(CommunityInvitationsUC.ModelProperty);
      }
      set
      {
        this.SetValue(CommunityInvitationsUC.ModelProperty, (object) value);
      }
    }

    public CommunityInvitationsUC()
    {
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((CommunityInvitationsUC) d).UpdateDataView((CommunityInvitations) e.NewValue);
    }

    public void UpdateDataView(CommunityInvitations model)
    {
      if (model == null || model.count == 0)
        return;
      this.TitleBlock.Text = UIStringFormatterHelper.FormatNumberOfSomething(model.count, CommonResources.Communities_InvitationOneFrm, CommonResources.Communities_InvitationTwoFrm, CommonResources.Communities_InvitationFiveFrm, true, null, false);
      this.ShowAllBlock.Visibility = model.count > 1 ? Visibility.Visible : Visibility.Collapsed;
      model.first_invitation.InvitationHandledAction = (Action<CommunityInvitations>) (invitations => ((GroupsListViewModel) this.DataContext).InvitationsViewModel = invitations);
      ContentControl contentControl = this.InvitationView;
      CommunityInvitationUC communityInvitationUc = new CommunityInvitationUC();
      communityInvitationUc.Model = model.first_invitation;
      int num = 0;
      communityInvitationUc.NeedBottomSeparatorLine = num != 0;
      contentControl.Content = (object) communityInvitationUc;
    }

    public void UpdateDataView(CommunityInvitationsList invitationsList)
    {
      CommunityInvitation communityInvitation = (CommunityInvitation) null;
      if (((IEnumerable<Group>) invitationsList.invitations).Any<Group>())
      {
        User user = ((IEnumerable<User>) invitationsList.inviters).First<User>((Func<User, bool>) (i => i.id == ((IEnumerable<Group>) invitationsList.invitations).First<Group>().invited_by));
        communityInvitation = new CommunityInvitation()
        {
          community = ((IEnumerable<Group>) invitationsList.invitations).First<Group>(),
          inviter = user
        };
      }
      this.UpdateDataView(new CommunityInvitations()
      {
        count = invitationsList.count,
        first_invitation = communityInvitation
      });
    }

    private void ShowAllBlock_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.Model == null)
        return;
      ParametersRepository.SetParameterForId("CommunityInvitationsUC", (object) this);
      Navigator.Current.NavigateToGroupInvitations();
    }

  }
}
