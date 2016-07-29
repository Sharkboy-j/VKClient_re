using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class ContactsSyncRequestUC : UserControl
  {
    internal GenericHeaderUC ucHeader;
    internal Grid gridFriends;
    internal ProgressRing progressRing;
    internal ItemsControl itemsControl;
    internal TextBlock textBlockFriendsCount;
    internal Button buttonContinue;
    private bool _contentLoaded;

    public ContactsSyncRequestUC()
    {
      this.InitializeComponent();
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.SupportMenu = false;
      this.ucHeader.Title = CommonResources.PageTitle_FindFriends;
    }

    private void LoadFriends()
    {
      UsersService.Instance.GetFeatureUsers(10, (Action<BackendResult<VKList<User>, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.progressRing.Visibility = Visibility.Collapsed;
        if (result.ResultCode == ResultCode.Succeeded)
        {
          List<User> items = result.ResultData.items;
          if (items.Count > 0)
          {
            this.itemsControl.ItemsSource = (IEnumerable) items.Take<User>(5);
            this.textBlockFriendsCount.Text = UIStringFormatterHelper.FormatNumberOfSomething(items.Count, CommonResources.OneFriendContactsSyncFrm, CommonResources.TwoFourFriendsContactsSyncFrm, CommonResources.FiveFriendsContactsSyncFrm, true, null, false);
          }
          else
            this.gridFriends.Visibility = Visibility.Collapsed;
        }
        else
          this.gridFriends.Visibility = Visibility.Collapsed;
      }))));
    }

    public static void OpenFriendsImportContacts(Action continueClickCallback)
    {
      if (AppGlobalStateManager.Current.GlobalState.AllowSendContacts)
      {
        if (continueClickCallback == null)
          return;
        continueClickCallback();
      }
      else
      {
        DialogService dialogService = new DialogService();
        dialogService.AnimationType = DialogService.AnimationTypes.None;
        dialogService.AnimationTypeChild = DialogService.AnimationTypes.SlideInversed;
        SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
        dialogService.BackgroundBrush = (Brush) solidColorBrush;
        DialogService popup = dialogService;
        ContactsSyncRequestUC child = new ContactsSyncRequestUC();
        child.buttonContinue.Click += (RoutedEventHandler) ((sender, args) =>
        {
          AppGlobalStateManager.Current.GlobalState.AllowSendContacts = true;
          EventAggregator.Current.Publish((object) new ContactsSyncEnabled());
          popup.Hide();
          if (continueClickCallback == null)
            return;
          continueClickCallback();
        });
        popup.Child = (FrameworkElement) child;
        popup.Opened += (EventHandler) ((sender, args) => child.LoadFriends());
        popup.Show(null);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/ContactsSyncRequestUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.gridFriends = (Grid) this.FindName("gridFriends");
      this.progressRing = (ProgressRing) this.FindName("progressRing");
      this.itemsControl = (ItemsControl) this.FindName("itemsControl");
      this.textBlockFriendsCount = (TextBlock) this.FindName("textBlockFriendsCount");
      this.buttonContinue = (Button) this.FindName("buttonContinue");
    }
  }
}
