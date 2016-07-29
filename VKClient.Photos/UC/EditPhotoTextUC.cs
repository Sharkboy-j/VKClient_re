using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Photos.UC
{
    public partial class EditPhotoTextUC : UserControl
  {

    public Button ButtonSave
    {
      get
      {
        return this.buttonSave;
      }
    }

    public TextBox TextBoxText
    {
      get
      {
        return this.textBoxText;
      }
    }

    public EditPhotoTextUC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.EditPhotoTextUC_Loaded);
    }

    private void EditPhotoTextUC_Loaded(object sender, RoutedEventArgs e)
    {
      Deployment.Current.Dispatcher.BeginInvoke((Action) (() =>
      {
        this.textBoxText.Focus();
        this.textBoxText.Select(this.textBoxText.Text.Length, 0);
      }));
    }
  }
}
