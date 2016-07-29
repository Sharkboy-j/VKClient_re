using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsLinkUC : NewsLinkUCBase
  {
    private const double MIN_IMAGE_RATIO = 1.5;
    private Uri _imageUri;
    private string _url;
    private string _actionButtonUrl;
    private double _actualHeight;
    internal Canvas canvasImageContainer;
    internal Image imagePreview;
    internal Grid gridContent;
    internal TextBlock textBlockContent;
    internal StackPanel panelProductRating;
    internal TextBlock textBlockPrice;
    internal Rating ucRating;
    internal TextBlock textBlockVotesCount;
    internal TextBlock textBlockCaption;
    internal Button buttonAction;
    private bool _contentLoaded;

    public NewsLinkUC()
    {
      this.InitializeComponent();
      this.panelProductRating.Visibility = Visibility.Collapsed;
      this.textBlockPrice.Visibility = Visibility.Collapsed;
      this.ucRating.Visibility = Visibility.Collapsed;
      this.textBlockVotesCount.Visibility = Visibility.Collapsed;
      this.textBlockCaption.Visibility = Visibility.Collapsed;
      Grid.SetColumnSpan((FrameworkElement) this.textBlockContent, 2);
      this.buttonAction.Visibility = Visibility.Collapsed;
      this.buttonAction.VerticalAlignment = VerticalAlignment.Center;
    }

    public override void Initialize(Link link, double width)
    {
      double val1 = (double) link.photo.width / (double) link.photo.height;
      double num1 = Math.Max(val1, 1.5);
      double num2 = width;
      double num3 = Math.Round(num2 / num1);
      this._imageUri = ExtensionsBase.ConvertToUri(link.photo.GetAppropriateForScaleFactor(num2 / val1, 1));
      this.canvasImageContainer.Height = num3;
      this.canvasImageContainer.Width = num2;
      this.imagePreview.Height = num3;
      this.imagePreview.Width = num2;
      this._actualHeight = this._actualHeight + num3;
      this.ComposeContentTextInlines(link);
      this._url = link.url;
      double num4 = width - this.gridContent.Margin.Left - this.gridContent.Margin.Right;
      LinkButton button = link.button;
      if (button != null)
      {
        this.buttonAction.Visibility = Visibility.Visible;
        this.buttonAction.Content = (object) button.title;
        this._actionButtonUrl = button.url;
        this.buttonAction.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double num5 = (double) (int) new TextBlock()
        {
          FontFamily = new FontFamily("Segoe WP Semibold"),
          FontSize = 20.0,
          Text = button.title
        }.ActualWidth + this.buttonAction.Margin.Left + -this.buttonAction.Margin.Right + 32.0;
        num4 -= num5;
      }
      this.textBlockContent.Width = num4;
      LinkProduct product = link.product;
      string str;
      if (product == null)
      {
        str = null;
      }
      else
      {
        Price price = product.price;
        str = price != null ? price.text : null;
      }
      bool flag1 = !string.IsNullOrEmpty(str);
      bool flag2 = link.rating != null;
      if (flag1 | flag2)
      {
        this.panelProductRating.Visibility = Visibility.Visible;
        if (flag1)
        {
          this.textBlockPrice.Visibility = Visibility.Visible;
          this.textBlockPrice.Text = link.product.price.text;
        }
        if (flag2)
        {
          this.ucRating.Visibility = Visibility.Visible;
          this.ucRating.Value = link.rating.stars;
          long reviewsCount = link.rating.reviews_count;
          if (reviewsCount > 0L)
          {
            this.textBlockVotesCount.Visibility = Visibility.Visible;
            this.textBlockVotesCount.Text = string.Format("({0})", (object) UIStringFormatterHelper.FormatForUIVeryShort(reviewsCount));
          }
        }
        this.buttonAction.VerticalAlignment = VerticalAlignment.Bottom;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.panelProductRating);
      }
      if (!string.IsNullOrEmpty(link.caption))
      {
        this.textBlockCaption.Visibility = Visibility.Visible;
        this.textBlockCaption.Text = link.caption;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.textBlockCaption);
      }
      this._actualHeight = this._actualHeight + Math.Min(this.textBlockContent.ActualHeight, this.textBlockContent.MaxHeight);
      this._actualHeight = this._actualHeight + (this.gridContent.Margin.Top + this.gridContent.Margin.Bottom);
    }

    private void ComposeContentTextInlines(Link link)
    {
      this.textBlockContent.Inlines.Clear();
      Run run1 = new Run();
      run1.Text = !string.IsNullOrWhiteSpace(link.title) ? link.title : CommonResources.Link;
      FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
      run1.FontFamily = fontFamily;
      this.textBlockContent.Inlines.Add((Inline) run1);
      if (string.IsNullOrWhiteSpace(link.description))
        return;
      Run run2 = new Run()
      {
        Text = link.description
      };
      if (this.textBlockContent.Inlines.Count > 0)
        this.textBlockContent.Inlines.Add((Inline) new LineBreak());
      this.textBlockContent.Inlines.Add((Inline) run2);
    }

    public override double CalculateTotalHeight()
    {
      return this._actualHeight;
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (string.IsNullOrEmpty(this._url))
        return;
      Navigator.Current.NavigateToWebUri(this._url, false, false);
    }

    private void ActionButton_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (string.IsNullOrEmpty(this._actionButtonUrl))
        return;
      e.Handled = true;
      Navigator.Current.NavigateToWebUri(this._actionButtonUrl, false, false);
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imagePreview, this._imageUri);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.imagePreview, (Uri) null);
    }

    public override void ShownOnScreen()
    {
      if (!(this._imageUri != (Uri) null) || !this._imageUri.IsAbsoluteUri)
        return;
      VeryLowProfileImageLoader.SetPriority(this._imageUri.OriginalString, DateTime.Now.Ticks);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewsLinkUC.xaml", UriKind.Relative));
      this.canvasImageContainer = (Canvas) this.FindName("canvasImageContainer");
      this.imagePreview = (Image) this.FindName("imagePreview");
      this.gridContent = (Grid) this.FindName("gridContent");
      this.textBlockContent = (TextBlock) this.FindName("textBlockContent");
      this.panelProductRating = (StackPanel) this.FindName("panelProductRating");
      this.textBlockPrice = (TextBlock) this.FindName("textBlockPrice");
      this.ucRating = (Rating) this.FindName("ucRating");
      this.textBlockVotesCount = (TextBlock) this.FindName("textBlockVotesCount");
      this.textBlockCaption = (TextBlock) this.FindName("textBlockCaption");
      this.buttonAction = (Button) this.FindName("buttonAction");
    }
  }
}
