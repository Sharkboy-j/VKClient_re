using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.UC
{
    public partial class CommunityInvitationUC : UserControl
  {
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof (CommunityInvitation), typeof (CommunityInvitationUC), new PropertyMetadata(new PropertyChangedCallback(CommunityInvitationUC.OnModelChanged)));
    public static readonly DependencyProperty NeedBottomSeparatorLineProperty = DependencyProperty.Register("NeedBottomSeparatorLine", typeof (bool), typeof (CommunityInvitationUC), new PropertyMetadata(new PropertyChangedCallback(CommunityInvitationUC.OnNeedBottomSeparatorLineChanged)));
    private CommunityInvitation _model;

    public CommunityInvitation Model
    {
      get
      {
        return (CommunityInvitation) this.GetValue(CommunityInvitationUC.ModelProperty);
      }
      set
      {
        this.SetValue(CommunityInvitationUC.ModelProperty, (object) value);
      }
    }

    public bool NeedBottomSeparatorLine
    {
      get
      {
        return (bool) this.GetValue(CommunityInvitationUC.NeedBottomSeparatorLineProperty);
      }
      set
      {
        this.SetValue(CommunityInvitationUC.NeedBottomSeparatorLineProperty, (object) value);
      }
    }

    public CommunityInvitationUC()
    {
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((CommunityInvitationUC) d).UpdateDataView((CommunityInvitation) e.NewValue);
    }

    private static void OnNeedBottomSeparatorLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((CommunityInvitationUC) d).BottomSeparatorRectangle.Visibility = (bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateDataView(CommunityInvitation model)
    {
      this._model = model;
      this.InvitationName.Text = model.community.name;
      ImageLoader.SetUriSource(this.InvitationPhoto, model.community.photo_100);
      int membersCount = model.community.members_count;
      this.InvitationMembersCount.Text = !(model.community.type != "public") ? UIStringFormatterHelper.FormatNumberOfSomething(membersCount, CommonResources.OneSubscriberFrm, CommonResources.TwoFourSubscribersFrm, CommonResources.FiveSubscribersFrm, true, null, false) : UIStringFormatterHelper.FormatNumberOfSomething(membersCount, CommonResources.OneMemberFrm, CommonResources.TwoFourMembersFrm, CommonResources.FiveMembersFrm, true, null, false);
      this.InvitationInviterSex.Text = (model.inviter.sex != 1 ? CommonResources.Communities_InvitationByM : CommonResources.Communities_InvitationByF) + " ";
      this.InvitationInviterName.Text = model.inviter.Name;
    }

    private void Invitation_OnTapped(object sender, GestureEventArgs e)
    {
      Group community = this._model.community;
      Navigator.Current.NavigateToGroup(community.id, community.name, false);
    }

    private void InvitationInviterName_OnTapped(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      Navigator.Current.NavigateToUserProfile(this._model.inviter.id, "", "", false);
    }

    private void Button_OnClicked(object sender, RoutedEventArgs e)
    {
      string str = string.Format("\r\n\r\nvar result=API.groups.{0}({{group_id:{1}}});;\r\nif (result==1)\r\n{{\r\n    var invitations=API.groups.getInvites({{count:1,\"fields\":\"members_count\"}});\r\n\r\n    var first_invitation_community=null;\r\n    var first_invitation_inviter=null;\r\n\r\n    if (invitations.items.length>0)\r\n    {{\r\n        first_invitation_community=invitations.items[0];\r\n        first_invitation_inviter=API.users.get({{user_ids:first_invitation_community.invited_by,fields:\"sex\"}})[0];\r\n    }}\r\n\r\n    return\r\n    {{\r\n        \"count\":invitations.count,\r\n        \"first_invitation\":\r\n        {{\r\n            \"community\":first_invitation_community,\r\n            \"inviter\":first_invitation_inviter\r\n        }}\r\n    }};\r\n}}\r\nreturn 0;\r\n\r\n", sender == this.JoinButton ? (object) "join" : (object) "leave", (object) this._model.community.id);
      CommunityInvitation model = this.Model;
      Action<BackendResult<CommunityInvitations, ResultCode>> action = (Action<BackendResult<CommunityInvitations, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          CommunityInvitations resultData = result.ResultData;
          model.InvitationHandledAction(resultData);
          CountersManager.Current.Counters.groups = resultData.count;
          EventAggregator.Current.Publish((object) new CountersChanged(CountersManager.Current.Counters));
        }
        PageBase.SetInProgress(false);
      })));
      PageBase.SetInProgress(true);
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str);
      Action<BackendResult<CommunityInvitations, ResultCode>> callback = action;
      object local = null;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.DispatchRequestToVK<CommunityInvitations>(methodName, parameters, callback, (Func<string, CommunityInvitations>) local, num1 != 0, num2 != 0, cancellationToken);
    }

    private void InviterName_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    private void Button_OnTapped(object sender, GestureEventArgs e)
    {
      e.Handled = true;
    }
  }
}
