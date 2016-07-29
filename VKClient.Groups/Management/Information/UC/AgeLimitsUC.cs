using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
    public partial class AgeLimitsUC : UserControl
  {
    public AgeLimitsViewModel ViewModel
    {
      get
      {
        return this.DataContext as AgeLimitsViewModel;
      }
    }

    public AgeLimitsUC()
    {
      this.InitializeComponent();
    }

    private void SetAgeLimitsButton_OnClicked(object sender, GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      this.ViewModel.FullFormVisibility = Visibility.Visible;
    }

    private void MoreInformation_OnClicked(object sender, GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      string uri = "https://m.vk.com/agelimits?api_view=1";
      string lang = LangHelper.GetLang();
      if (!string.IsNullOrEmpty(lang))
        uri += string.Format("&lang={0}", (object) lang);
      Navigator.Current.NavigateToWebUri(uri, true, false);
    }

  }
}
