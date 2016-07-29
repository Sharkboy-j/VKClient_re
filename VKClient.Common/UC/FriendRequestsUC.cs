using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class FriendRequestsUC : UserControl
  {
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof (FriendRequests), typeof (FriendRequestsUC), new PropertyMetadata(new PropertyChangedCallback(FriendRequestsUC.OnModelChanged)));
    private FriendRequests _model;
    internal TextBlock TitleBlock;
    internal Border ShowAllBlock;
    internal ContentControl RequestView;
    private bool _contentLoaded;

    public FriendRequests Model
    {
      get
      {
        return (FriendRequests) this.GetValue(FriendRequestsUC.ModelProperty);
      }
      set
      {
        this.SetValue(FriendRequestsUC.ModelProperty, (object) value);
      }
    }

    public FriendRequestsUC()
    {
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((FriendRequestsUC) d).UpdateDataView((FriendRequests) e.NewValue);
    }

    public void UpdateDataView(FriendRequests model)
    {
      this._model = model;
      if (this._model == null || model.count == 0)
        return;
      this.TitleBlock.Text = model.are_suggested_friends ? UIStringFormatterHelper.FormatNumberOfSomething(model.count, CommonResources.SuggestedFriendOneFrm, CommonResources.SuggestedFriendTwoFrm, CommonResources.SuggestedFriendFiveFrm, true, null, false) : UIStringFormatterHelper.FormatNumberOfSomething(model.count, CommonResources.OneFriendRequestFrm, CommonResources.TwoFourFriendRequestsFrm, CommonResources.FiveFriendRequestsFrm, true, null, false);
      this.ShowAllBlock.Visibility = model.count > 1 ? Visibility.Visible : Visibility.Collapsed;
      if (model.requests[0].RequestHandledAction == null)
        model.requests[0].RequestHandledAction = (Action<FriendRequests>) (requests => ((FriendsViewModel) this.DataContext).RequestsViewModel = requests);
      this.RequestView.Content = (object) new FriendRequestUC()
      {
        Model = model.requests[0],
        Profiles = model.profiles.ToArray(),
        IsSuggestedFriend = new bool?(model.are_suggested_friends)
      };
    }

    private void ShowAllBlock_OnTapped(object sender, GestureEventArgs e)
    {
      if (this._model == null)
        return;
      ParametersRepository.SetParameterForId("FriendRequestsUC", (object) this);
      Navigator.Current.NavigateToFriendRequests(this._model.are_suggested_friends);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/FriendRequestsUC.xaml", UriKind.Relative));
      this.TitleBlock = (TextBlock) this.FindName("TitleBlock");
      this.ShowAllBlock = (Border) this.FindName("ShowAllBlock");
      this.RequestView = (ContentControl) this.FindName("RequestView");
    }
  }
}
