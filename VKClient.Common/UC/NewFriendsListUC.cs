using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class NewFriendsListUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxTitle;
    internal Button buttonCreate;
    internal Button buttonSave;
    private bool _contentLoaded;

    public bool IsNew { get; set; }

    public Visibility IsEditListVisibility
    {
      get
      {
        return !this.IsNew ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility IsNewListVisibility
    {
      get
      {
        return !this.IsNew ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public NewFriendsListUC()
    {
      this.InitializeComponent();
      this.UpdateButtonEnabled();
    }

    public void Initialize(bool isNew)
    {
      this.IsNew = isNew;
      this.DataContext = (object) this;
      this.UpdateButtonEnabled();
    }

    private void textBoxTitle_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      this.UpdateButtonEnabled();
    }

    private void UpdateButtonEnabled()
    {
      this.buttonCreate.IsEnabled = this.buttonSave.IsEnabled = !string.IsNullOrWhiteSpace(this.textBoxTitle.Text);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewFriendsListUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.textBoxTitle = (TextBox) this.FindName("textBoxTitle");
      this.buttonCreate = (Button) this.FindName("buttonCreate");
      this.buttonSave = (Button) this.FindName("buttonSave");
    }
  }
}
