using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.DatePicker;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class SettingsEditProfilePage : PageBase
  {
    private readonly ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/check.png", UriKind.Relative),
      Text = CommonResources.AppBarMenu_Save
    };
    private readonly ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private bool _isInitialized;
    private ApplicationBar _mainAppBar;
    internal GenericHeaderUC ucHeader;
    internal ScrollViewer scrollViewer;
    internal StackPanel stackPanel;
    internal ContextMenu PhotoMenu;
    internal MyDatePicker datePicker;
    internal TextBoxPanelControl textBoxPanel;
    private bool _contentLoaded;

    private SettingsEditProfileViewModel VM
    {
      get
      {
        return this.DataContext as SettingsEditProfileViewModel;
      }
    }

    public SettingsEditProfilePage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.Settings_EditProfile_Title.ToUpperInvariant();
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click += new EventHandler(this._appBarButtonCheck_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._mainAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonCheck);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonCancel);
      this.ApplicationBar = (IApplicationBar) this._mainAppBar;
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = true;
      this.scrollViewer.ScrollToOffsetWithAnimation(((UIElement) sender).GetRelativePosition((UIElement) this.stackPanel).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = false;
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      this.VM.Save();
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        SettingsEditProfileViewModel profileViewModel = new SettingsEditProfileViewModel();
        profileViewModel.Reload();
        profileViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.DataContext = (object) profileViewModel;
        this.UpdateAppBar();
        this._isInitialized = true;
      }
      this.HandleInputParams();
    }

    private void HandleInputParams()
    {
      List<User> userList = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
      if (userList != null && userList.Count >= 1)
        this.VM.Partner = userList[0];
      List<Stream> streamList = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      Rect rect = new Rect();
      if (ParametersRepository.Contains("UserPicSquare"))
        rect = (Rect) ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
      if (streamList == null || streamList.Count <= 0)
        return;
      this.VM.UploadUserPhoto(streamList[0], rect);
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanSave;
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanSave"))
        return;
      this.UpdateAppBar();
    }

    private void textBoxFirstNameChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
    }

    private void textBoxLastNameChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
    }

    private void UpdateSource(TextBox textBox)
    {
      textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void BirthdayTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, (object) this.datePicker, (object[]) null);
    }

    private void RemovePartnerTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.Partner = (User) null;
    }

    private void ChoosePartnerTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      int sexFilter = 0;
      if (this.VM.RelationshipType.id == 3 || this.VM.RelationshipType.id == 4)
        sexFilter = this.VM.IsMale ? 1 : 2;
      Navigator.Current.NavigateToPickUser(false, 0L, true, 0, PickUserMode.PickForPartner, CommonResources.Settings_EditProfile_ChooseAPartner2, sexFilter);
    }

    private void CancelNameRequestButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.CancelNameRequest();
    }

    private void ChoosePhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.DoChoosePhoto();
    }

    private void ChosePhotoMenuClick(object sender, RoutedEventArgs e)
    {
      this.DoChoosePhoto();
    }

    private void DoChoosePhoto()
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
    }

    private void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show(CommonResources.DeleteConfirmation, CommonResources.DeleteOnePhoto, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.VM.DeletePhoto();
    }

    private void GridPhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.PhotoMenu.IsOpen = true;
    }

    private void CountryPicker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      CountryPickerUC.Show(this.VM.Country, true, (Action<Country>) (country => this.VM.Country = country), null);
    }

    private void CityPicker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.VM.Country == null || this.VM.Country.id < 1L)
        return;
      CityPickerUC.Show(this.VM.Country.id, this.VM.City, true, (Action<City>) (city => this.VM.City = city), null);
    }

    private void PartnerTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.VM.Partner == null)
        return;
      Navigator.Current.NavigateToUserProfile(this.VM.Partner.id, this.VM.Partner.Name, "", false);
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (!e.Key.IsDigit())
        return;
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SettingsEditProfilePage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) this.FindName("scrollViewer");
      this.stackPanel = (StackPanel) this.FindName("stackPanel");
      this.PhotoMenu = (ContextMenu) this.FindName("PhotoMenu");
      this.datePicker = (MyDatePicker) this.FindName("datePicker");
      this.textBoxPanel = (TextBoxPanelControl) this.FindName("textBoxPanel");
    }
  }
}
