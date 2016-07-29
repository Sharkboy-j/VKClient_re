using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Photos.Library;

namespace VKClient.Photos.UC
{
    public partial class CreateAlbumUC : UserControl
  {

    private CreateEditAlbumViewModel VM
    {
      get
      {
        return this.DataContext as CreateEditAlbumViewModel;
      }
    }

    public CreateAlbumUC()
    {
      this.InitializeComponent();
      this.ucPrivacyHeaderAlbumView.OnTap = (Action) (() => Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
      {
        PrivacyForEdit = this.VM.PrivacyViewVM,
        UpdatePrivacyCallback = (Action<PrivacyInfo>) (pi => this.VM.PrivacyViewVM = new EditPrivacyViewModel(this.VM.PrivacyViewVM.PrivacyQuestion, pi, "", (List<string>) null))
      }));
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      (this.DataContext as CreateEditAlbumViewModel).Save();
    }
  }
}
