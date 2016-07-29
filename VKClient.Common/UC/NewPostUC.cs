using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewPostUC : UserControl
  {
    private bool _isLoaded;
    private ScrollViewer _scroll;
    private double savedHeight;
    internal Grid LayoutRoot;
    internal TextBox textBoxPost;
    internal TextBlock textBlockWatermarkText;
    internal ItemsControl itemsControlAttachments;
    private bool _contentLoaded;

    public TextBox TextBoxPost
    {
      get
      {
        return this.textBoxPost;
      }
    }

    public TextBlock TextBlockWatermarkText
    {
      get
      {
        return this.textBlockWatermarkText;
      }
    }

    public ItemsControl ItemsControlAttachments
    {
      get
      {
        return this.itemsControlAttachments;
      }
    }

    public Action<object> OnImageDeleteTap { get; set; }

    public Action OnAddAttachmentTap { get; set; }

    public bool IsFocused { get; set; }

    public NewPostUC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.NewPostUC_Loaded);
    }

    private void NewPostUC_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._isLoaded)
        return;
      this._scroll = this.Ancestors<ScrollViewer>().FirstOrDefault<DependencyObject>() as ScrollViewer;
      this.textBlockWatermarkText.Opacity = this.textBoxPost.Text == "" ? 1.0 : 0.0;
      this._isLoaded = true;
    }

    private void textBoxPost_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      this.textBlockWatermarkText.Opacity = this.textBoxPost.Text == "" ? 1.0 : 0.0;
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        double num = this.textBoxPost.ActualHeight - this.textBoxPost.Padding.Bottom;
        if (this.savedHeight > 0.0)
        {
          bool flag = false;
          if (num < this.savedHeight && this._scroll.ExtentHeight == this._scroll.VerticalOffset + this._scroll.ViewportHeight)
            flag = true;
          if (!flag)
            this._scroll.ScrollToOffsetWithAnimation(this._scroll.VerticalOffset + num - this.savedHeight, 0.15, false);
        }
        this.savedHeight = num;
      }));
    }

    private void Image_Delete_Tap(object sender, GestureEventArgs e)
    {
      if (this.OnImageDeleteTap == null)
        return;
      this.OnImageDeleteTap(sender);
      this.ForceFocusIfNeeded();
    }

    private void AddAttachmentTap(object sender, GestureEventArgs e)
    {
      if (this.OnAddAttachmentTap == null)
        return;
      this.OnAddAttachmentTap();
    }

    private void Image_Tap(object sender, GestureEventArgs e)
    {
      this.ForceFocusIfNeeded();
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      IOutboundAttachment attachment = frameworkElement.DataContext as IOutboundAttachment;
      WallPostViewModel wallPostViewModel = this.DataContext as WallPostViewModel;
      if (wallPostViewModel == null || attachment == null)
        return;
      wallPostViewModel.UploadAttachment(attachment, null);
    }

    private void Grid_Tap(object sender, GestureEventArgs e)
    {
      this.ForceFocusIfNeeded();
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      IOutboundAttachment attachment = frameworkElement.DataContext as IOutboundAttachment;
      if (attachment is IHandleTap)
      {
        (attachment as IHandleTap).OnTap();
      }
      else
      {
        if (attachment == null)
          return;
        WallPostViewModel wallPostViewModel = this.DataContext as WallPostViewModel;
        if (wallPostViewModel == null || attachment == null)
          return;
        wallPostViewModel.UploadAttachment(attachment, null);
      }
    }

    public void ForceFocusIfNeeded()
    {
      if (!this.IsFocused)
        return;
      this.textBoxPost.Focus();
      TextBoxPanelControl textBoxPanelControl = FramePageUtils.CurrentPage.Descendants<TextBoxPanelControl>().FirstOrDefault<DependencyObject>() as TextBoxPanelControl;
      if (textBoxPanelControl == null)
        return;
      textBoxPanelControl.IgnoreNextLostGotFocus();
    }

    private void Rectangle_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      rectangle.Opacity = 0.3;
    }

    private void Rectangle_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      rectangle.Opacity = 0.2;
    }

    private void Rectangle_ManipulationStarted2(object sender, ManipulationStartedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      rectangle.Opacity = 0.05;
    }

    private void Rectangle_ManipulationCompleted2(object sender, ManipulationCompletedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      rectangle.Opacity = 0.0;
    }

    private void itemsControlAttachments_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ICollection collection = this.itemsControlAttachments.ItemsSource as ICollection;
      if (collection == null)
        return;
      bool flag = collection.Count > 0;
      this.textBoxPost.Padding = flag ? new Thickness(0.0, 0.0, 0.0, 100.0) : new Thickness();
      this.itemsControlAttachments.Margin = flag ? new Thickness(-6.0, -105.0, -6.0, 6.0) : new Thickness(-6.0, -5.0, -6.0, 6.0);
    }

    private void TextBlock_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      TextBlock textName = (TextBlock) sender;
      OutboundAttachmentBase outboundAttachmentBase = textName.DataContext as OutboundAttachmentBase;
      if (outboundAttachmentBase == null)
        return;
      textName.CorrectText(outboundAttachmentBase.Width - 8.0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewPostUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.textBoxPost = (TextBox) this.FindName("textBoxPost");
      this.textBlockWatermarkText = (TextBlock) this.FindName("textBlockWatermarkText");
      this.itemsControlAttachments = (ItemsControl) this.FindName("itemsControlAttachments");
    }
  }
}
