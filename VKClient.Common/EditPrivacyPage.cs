using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class EditPrivacyPage : PageBase
  {
    private bool _isInitialized;
    private EditPrivacyPageInputData _inputData;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector mainList;
    private bool _contentLoaded;

    private EditPrivacyViewModel VM
    {
      get
      {
        return this.DataContext as EditPrivacyViewModel;
      }
    }

    public EditPrivacyPage()
    {
      this.InitializeComponent();
      this.Header.TextBlockTitle.Text = CommonResources.Privacy_Title.ToUpperInvariant();
      this.Header.HideSandwitchButton = true;
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._inputData = ParametersRepository.GetParameterForIdAndReset("EditPrivacyInputData") as EditPrivacyPageInputData;
        if (this._inputData == null)
        {
          Navigator.Current.GoBack();
        }
        else
        {
          this.DataContext = (object) new EditPrivacyViewModel(this._inputData.PrivacyForEdit, this._inputData.UpdatePrivacyCallback);
          this._isInitialized = true;
        }
      }
      this.HandleInputParams();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      if (e.NavigationMode != NavigationMode.Back || this._inputData == null || (this.VM == null || !this.VM.IsOKState))
        return;
      this._inputData.UpdatePrivacyCallback(this.VM.GetAsPrivacyInfo());
    }

    private void HandleInputParams()
    {
      List<User> pickedUsers = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
      List<FriendsList> pickedLists = ParametersRepository.GetParameterForIdAndReset("SelectedLists") as List<FriendsList>;
      if (this.VM == null || pickedLists == null && pickedUsers == null)
        return;
      this.VM.HandlePickedUsers(pickedUsers, pickedLists);
    }

    private void Grid_Tap(object sender, GestureEventArgs e)
    {
    }

    private void PickMoreUsersTap(object sender, GestureEventArgs e)
    {
      Group<FriendHeader> g = (e.OriginalSource as FrameworkElement).DataContext as Group<FriendHeader>;
      if (g == null || this.VM == null)
        return;
      this.VM.InitiatePickUsersFor(g);
    }

    private void RemoveUserOrListTap(object sender, GestureEventArgs e)
    {
      FriendHeader fh = (e.OriginalSource as FrameworkElement).DataContext as FriendHeader;
      if (this.VM == null)
        return;
      this.VM.Remove(fh);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/EditPrivacyPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.mainList = (ExtendedLongListSelector) this.FindName("mainList");
    }
  }
}
