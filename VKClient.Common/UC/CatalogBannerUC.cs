using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;

namespace VKClient.Common.UC
{
  public class CatalogBannerUC : UserControl, ISupportDataContext
  {
    public static readonly DependencyProperty CatalogBannerProperty = DependencyProperty.Register("CatalogBanner", typeof (GameHeader), typeof (CatalogBannerUC), new PropertyMetadata(new PropertyChangedCallback(CatalogBannerUC.OnCatalogBannerChanged)));
    internal Image imageBanner;
    private bool _contentLoaded;

    public GameHeader CatalogBanner
    {
      get
      {
        return (GameHeader) this.GetValue(CatalogBannerUC.CatalogBannerProperty);
      }
      set
      {
        this.SetValue(CatalogBannerUC.CatalogBannerProperty, (object) value);
      }
    }

    public CatalogBannerUC()
    {
      this.InitializeComponent();
    }

    private static void OnCatalogBannerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      CatalogBannerUC catalogBannerUc = d as CatalogBannerUC;
      if (catalogBannerUc == null)
        return;
      GameHeader gameHeader = e.NewValue as GameHeader;
      if (gameHeader == null)
        ImageLoader.SetUriSource(catalogBannerUc.imageBanner, "");
      else
        ImageLoader.SetUriSource(catalogBannerUc.imageBanner, gameHeader.Game.banner_896);
    }

    public void SetDataContext(object obj)
    {
      GameHeader gameHeader = obj as GameHeader;
      if (gameHeader != null)
        this.CatalogBanner = gameHeader;
      else
        this.CatalogBanner = (GameHeader) null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/CatalogBannerUC.xaml", UriKind.Relative));
      this.imageBanner = (Image) this.FindName("imageBanner");
    }
  }
}
