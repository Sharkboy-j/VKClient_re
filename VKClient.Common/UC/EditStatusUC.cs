using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class EditStatusUC : UserControl
  {
    internal TextBox textBoxText;
    internal Button buttonSave;
    private bool _contentLoaded;

    public TextBox TextBoxText
    {
      get
      {
        return this.textBoxText;
      }
    }

    public Button ButtonSave
    {
      get
      {
        return this.buttonSave;
      }
    }

    public EditStatusUC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.EditStatusUC_Loaded);
    }

    private void EditStatusUC_Loaded(object sender, RoutedEventArgs e)
    {
      this.textBoxText.Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/EditStatusUC.xaml", UriKind.Relative));
      this.textBoxText = (TextBox) this.FindName("textBoxText");
      this.buttonSave = (Button) this.FindName("buttonSave");
    }
  }
}
