using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library.FriendsImport;

namespace VKClient.Common.UC
{
  public class FriendsImportUC : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (FriendsImportUC), new PropertyMetadata(new PropertyChangedCallback(FriendsImportUC.OnTitleChanged)));
    private FriendsImportViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) this.GetValue(FriendsImportUC.TitleProperty);
      }
      set
      {
        this.SetValue(FriendsImportUC.TitleProperty, (object) value);
      }
    }

    public FriendsImportUC()
    {
      this.InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FriendsImportUC friendsImportUc = (FriendsImportUC) d;
      string str = e.NewValue as string;
      friendsImportUc.ucHeader.TextBlockTitle.Text = !string.IsNullOrEmpty(str) ? str.ToUpperInvariant() : "";
    }

    public void SetFriendsImportProvider(IFriendsImportProvider provider)
    {
      this._viewModel = new FriendsImportViewModel(provider);
      this._viewModel.LoadData();
      this.DataContext = (object) this._viewModel;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/FriendsImportUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
    }
  }
}
