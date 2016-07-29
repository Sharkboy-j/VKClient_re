using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewsLinkMediumUC : NewsLinkUCBase
  {
    private const double SNIPPET_MIN_HEIGHT = 100.0;
    private const double SNIPPET_MAX_HEIGHT = 152.0;
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

    public NewsLinkMediumUC()
    {
      this.InitializeComponent();
      this.panelProductRating.Visibility = Visibility.Collapsed;
      this.textBlockPrice.Visibility = Visibility.Collapsed;
      this.ucRating.Visibility = Visibility.Collapsed;
      this.textBlockVotesCount.Visibility = Visibility.Collapsed;
      this.textBlockCaption.Visibility = Visibility.Collapsed;
      this.buttonAction.Visibility = Visibility.Collapsed;
    }

    public override void Initialize(Link link, double width)
    {
      this._url = link.url;
      this.ComposeContentTextInlines(link);
      this.textBlockContent.Width = width - (this.canvasImageContainer.Width + this.gridContent.Margin.Left + this.gridContent.Margin.Right);
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
        this.textBlockContent.MaxHeight -= this.textBlockContent.LineHeight;
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
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.panelProductRating);
      }
      if (!string.IsNullOrEmpty(link.caption))
      {
        this.textBlockContent.MaxHeight -= this.textBlockContent.LineHeight;
        this.textBlockCaption.Visibility = Visibility.Visible;
        this.textBlockCaption.Text = link.caption;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.textBlockCaption);
      }
      LinkButton button = link.button;
      if (!string.IsNullOrWhiteSpace(button != null ? button.title : null))
      {
        this.textBlockContent.MaxHeight -= this.textBlockContent.LineHeight;
        this.buttonAction.Visibility = Visibility.Visible;
        this.buttonAction.Content = (object) button.title;
        this._actionButtonUrl = button.url;
        this._actualHeight = this._actualHeight + this.GetElementTotalHeight((FrameworkElement) this.buttonAction);
      }
      this._actualHeight = this._actualHeight + Math.Min(this.textBlockContent.ActualHeight, this.textBlockContent.MaxHeight);
      this._actualHeight = Math.Min(this._actualHeight, 152.0);
      this._actualHeight = Math.Max(this._actualHeight, 100.0);
      this._actualHeight = this._actualHeight + (this.gridContent.Margin.Top + this.gridContent.Margin.Bottom);
      this.canvasImageContainer.Height = this._actualHeight;
      this.imagePreview.Height = this._actualHeight;
      this._imageUri = link.photo.GetAppropriateForScaleFactor(this._actualHeight, 1).ConvertToUri();
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewsLinkMediumUC.xaml", UriKind.Relative));
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
