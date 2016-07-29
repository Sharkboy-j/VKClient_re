using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class SettingsChangeShortNamePage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/check.png", UriKind.Relative), Text = CommonResources.ChatEdit_AppBar_Save };
    private ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private ApplicationBar _appBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    internal TextBox textBoxName;
    internal ContextMenu AtNameMenu;
    internal ContextMenu LinkMenu;
    private bool _contentLoaded;

    private SettingsChangeShortNameViewModel VM
    {
      get
      {
        return this.DataContext as SettingsChangeShortNameViewModel;
      }
    }

    public SettingsChangeShortNamePage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.TextBlockTitle.Text = CommonResources.Settings_ShortName.ToUpperInvariant();
      this.Loaded += new RoutedEventHandler(this.SettingsChangeShortNamePage_Loaded);
      this.ucHeader.HideSandwitchButton = true;
      this.SuppressMenu = true;
    }

    private void SettingsChangeShortNamePage_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBoxName.Focus();
      this.textBoxName.Select(this.textBoxName.Text.Length, 0);
      this.Loaded -= new RoutedEventHandler(this.SettingsChangeShortNamePage_Loaded);
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click += new EventHandler(this._appBarButtonCheck_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._appBar.Buttons.Add((object) this._appBarButtonCheck);
      this._appBar.Buttons.Add((object) this._appBarButtonCancel);
      this.ApplicationBar = (IApplicationBar) this._appBar;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      if (!this.VM.CanSave)
        return;
      this.VM.SaveShortName((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() => this.UpdateAppBar()))));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        SettingsChangeShortNameViewModel shortNameViewModel = new SettingsChangeShortNameViewModel(this.NavigationContext.QueryString["CurrentShortName"]);
        this.DataContext = (object) shortNameViewModel;
        this._isInitialized = true;
        shortNameViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
      }
      this.UpdateAppBar();
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanSave"))
        return;
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanSave;
    }

    private void textBoxName_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
    }

    private void UpdateSource(TextBox textBox)
    {
      textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void CopyAtName(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(this.VM.AtShortName);
    }

    private void AtNameTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.AtNameMenu.IsOpen = true;
    }

    private void CopyLink(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(this.VM.YourLink);
    }

    private void CopyLinkTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.LinkMenu.IsOpen = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SettingsChangeShortNamePage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.ContentPanel = (Grid) this.FindName("ContentPanel");
      this.textBoxName = (TextBox) this.FindName("textBoxName");
      this.AtNameMenu = (ContextMenu) this.FindName("AtNameMenu");
      this.LinkMenu = (ContextMenu) this.FindName("LinkMenu");
    }
  }
}
