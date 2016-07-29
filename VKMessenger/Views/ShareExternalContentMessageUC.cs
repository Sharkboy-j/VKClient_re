using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Utils;

namespace VKMessenger.Views
{
  public class ShareExternalContentMessageUC : UserControl
  {
    private double _savedHeight;
    internal ScrollViewer scrollViewerMessage;
    internal TextBox textBoxMessage;
    internal TextBlock textBlockWatermarkText;
    private bool _contentLoaded;

    public ShareExternalContentMessageUC()
    {
      this.InitializeComponent();
    }

    private void TextBoxMessage_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.textBlockWatermarkText.Opacity = this.textBoxMessage.Text == "" ? 1.0 : 0.0;
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        double num = this.textBoxMessage.ActualHeight - this.textBoxMessage.Padding.Bottom;
        if (this._savedHeight > 0.0)
        {
          bool flag = false;
          if (num < this._savedHeight && this.scrollViewerMessage.ExtentHeight == this.scrollViewerMessage.VerticalOffset + this.scrollViewerMessage.ViewportHeight)
            flag = true;
          if (!flag)
            this.scrollViewerMessage.ScrollToOffsetWithAnimation(this.scrollViewerMessage.VerticalOffset + num - this._savedHeight, 0.15, false);
        }
        this._savedHeight = num;
      }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKMessenger;component/Views/ShareExternalContentMessageUC.xaml", UriKind.Relative));
      this.scrollViewerMessage = (ScrollViewer) this.FindName("scrollViewerMessage");
      this.textBoxMessage = (TextBox) this.FindName("textBoxMessage");
      this.textBlockWatermarkText = (TextBlock) this.FindName("textBlockWatermarkText");
    }
  }
}
