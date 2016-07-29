using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class SharePostUC : UserControl
  {
    private double _savedHeight;
    internal TextBlock textBlockTitle;
    internal ScrollViewer scroll;
    internal TextBox textBoxText;
    internal TextBlock textBlockWatermarkText;
    internal ShareActionUC buttonShare;
    internal ShareActionUC buttonShareCommunity;
    private bool _contentLoaded;

    public string Text
    {
      get
      {
        return this.textBoxText.Text;
      }
    }

    public event EventHandler ShareTap;

    public event EventHandler ShareCommunityTap;

    public event EventHandler SendTap;

    public SharePostUC()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = CommonResources.ShareWallPost_Share.ToUpperInvariant();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.textBlockWatermarkText.Opacity = this.textBoxText.Text == "" ? 1.0 : 0.0;
      this.ScrollToCursor();
    }

    private void ScrollToCursor()
    {
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        double num = this.textBoxText.ActualHeight - this.textBoxText.Padding.Bottom;
        if (this._savedHeight > 0.0)
        {
          bool flag = false;
          if (num < this._savedHeight && this.scroll.ExtentHeight == this.scroll.VerticalOffset + this.scroll.ViewportHeight)
            flag = true;
          if (!flag)
            this.scroll.ScrollToOffsetWithAnimation(this.scroll.VerticalOffset + num - this._savedHeight, 0.15, false);
        }
        this._savedHeight = num;
      }));
    }

    public void SetShareEnabled(bool isEnabled)
    {
      this.buttonShare.IsEnabled = isEnabled;
      this.buttonShare.Opacity = isEnabled ? 1.0 : 0.4;
    }

    public void SetShareCommunityEnabled(bool isEnabled)
    {
      this.buttonShareCommunity.IsEnabled = isEnabled;
      this.buttonShareCommunity.Opacity = isEnabled ? 1.0 : 0.4;
    }

    private void ButtonShare_OnTap(object sender, GestureEventArgs e)
    {
      if (this.ShareTap == null)
        return;
      this.ShareTap((object) this, EventArgs.Empty);
    }

    private void ShareWithCommunity_OnTap(object sender, RoutedEventArgs e)
    {
      if (this.ShareCommunityTap == null)
      {
        Navigator.Current.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", true, 0L, 0L, "", false, "");
      }
      else
      {
        this.ShareCommunityTap((object) this, EventArgs.Empty);
      }
    }

    private void ButtonSend_OnTap(object sender, GestureEventArgs e)
    {
      if (this.SendTap == null)
        return;
      this.SendTap((object) this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/SharePostUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.scroll = (ScrollViewer) this.FindName("scroll");
      this.textBoxText = (TextBox) this.FindName("textBoxText");
      this.textBlockWatermarkText = (TextBlock) this.FindName("textBlockWatermarkText");
      this.buttonShare = (ShareActionUC) this.FindName("buttonShare");
      this.buttonShareCommunity = (ShareActionUC) this.FindName("buttonShareCommunity");
    }
  }
}
