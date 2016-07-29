using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class UserNotificationTemplateUC : UserControl
  {
    private List<User> _users;
    private string _thumb;
    internal Grid LayoutRoot;
    internal Grid gridInner;
    internal Image mainUserImage;
    internal Image imageIcon;
    internal TextBlock textBlockHeader;
    internal Canvas canvasAdditionalUserImages;
    internal Ellipse rect1;
    internal Image image1;
    internal Ellipse rect2;
    internal Image image2;
    internal Ellipse rect3;
    internal Image image3;
    internal Ellipse rect4;
    internal Image image4;
    internal Ellipse rect5;
    internal Image image5;
    internal TextBlock textBlockDate;
    internal Image imageThumb;
    internal TextSeparatorUC ucEarlierReplies;
    private bool _contentLoaded;

    public UserNotificationTemplateUC()
    {
      this.InitializeComponent();
      this.ucEarlierReplies.Text = CommonResources.ViewedFeedback;
    }

    public void LoadImages()
    {
      if (this._users == null)
        return;
      List<Image> imageList = new List<Image>()
      {
        this.image1,
        this.image2,
        this.image3,
        this.image4,
        this.image5
      };
      List<FrameworkElement> frameworkElementList = new List<FrameworkElement>()
      {
        (FrameworkElement) this.rect1,
        (FrameworkElement) this.rect2,
        (FrameworkElement) this.rect3,
        (FrameworkElement) this.rect4,
        (FrameworkElement) this.rect5
      };
      frameworkElementList.ForEach((Action<FrameworkElement>) (r => r.Visibility = Visibility.Collapsed));
      int val2 = Math.Min(this._users.Count - 1, imageList.Count);
      if (!string.IsNullOrEmpty(this._thumb))
        val2 = Math.Min(3, val2);
      for (int index = 0; index < val2; ++index)
      {
        ImageLoader.SetSourceForImage(imageList[index], this._users[index + 1].photo_max, false);
        imageList[index].Tag = (object) this._users[index + 1];
        imageList[index].Tap += new EventHandler<GestureEventArgs>(this.UserNotificationTemplateUC_Tap);
      }
      for (int count = this._users.Count; count < frameworkElementList.Count; ++count)
      {
        imageList[count].Visibility = Visibility.Collapsed;
        frameworkElementList[count].Visibility = Visibility.Collapsed;
      }
      if (this._thumb == null)
        return;
      ImageLoader.SetSourceForImage(this.imageThumb, this._thumb, false);
    }

    public double Configure(List<User> users, string actionText, string dateText, string hightlightedText, string thumb, NotificationType type, int totalUsersCount, bool showEarlierReplies)
    {
      this._users = users;
      this._thumb = thumb;
      this.imageThumb.Visibility = string.IsNullOrWhiteSpace(this._thumb) ? Visibility.Collapsed : Visibility.Visible;
      this.canvasAdditionalUserImages.Visibility = totalUsersCount > 1 ? Visibility.Visible : Visibility.Collapsed;
      this.ConfigureLeftSideImage(type);
      this.textBlockHeader.Width = this.CalculateTextBlockHeaderWidth();
      this.ConfigureHeaderText(users, actionText, hightlightedText, totalUsersCount);
      this.textBlockDate.Text = dateText;
      this.ucEarlierReplies.Visibility = showEarlierReplies ? Visibility.Visible : Visibility.Collapsed;
      Thickness margin = this.textBlockHeader.Margin;
      double num1 = margin.Top + this.textBlockHeader.ActualHeight;
      margin = this.textBlockHeader.Margin;
      double bottom = margin.Bottom;
      double num2 = num1 + bottom + (this.canvasAdditionalUserImages.Visibility == Visibility.Visible ? this.canvasAdditionalUserImages.Height : 0.0) + this.textBlockDate.ActualHeight + 20.0;
      if (showEarlierReplies)
        num2 += 46.0;
      this.LayoutRoot.Height = num2;
      return num2;
    }

    private void ConfigureHeaderText(List<User> users, string actionText, string hightlightedText, int totalUsersCount)
    {
      if (users.Count <= 0)
        return;
      this.textBlockHeader.Inlines.Add((Inline) new Run()
      {
        Text = users[0].Name
      });
      if (users.Count >= 2 && totalUsersCount == 2)
      {
        Run run1 = new Run();
        FontFamily fontFamily = new FontFamily("Segoe WP SemiLight");
        run1.FontFamily = fontFamily;
        Brush brush = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        run1.Foreground = brush;
        Run run2 = run1;
        run2.Text = " " + CommonResources.And + " ";
        this.textBlockHeader.Inlines.Add((Inline) run2);
        this.textBlockHeader.Inlines.Add((Inline) new Run()
        {
          Text = users[1].Name
        });
      }
      else if (totalUsersCount > 2)
      {
        int number = totalUsersCount - 1;
        string[] strArray = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.AndOneOtherFrm, CommonResources.AndTwoFourOthersFrm, CommonResources.AndFiveOthersFrm, true, null, false).Split(new string[1]
        {
          number.ToString()
        }, StringSplitOptions.None);
        Run run1 = new Run();
        FontFamily fontFamily1 = new FontFamily("Segoe WP SemiLight");
        run1.FontFamily = fontFamily1;
        Brush brush1 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        run1.Foreground = brush1;
        Run run2 = run1;
        run2.Text = " " + strArray[0].Trim() + " ";
        Run run3 = new Run();
        FontFamily fontFamily2 = new FontFamily("Segoe WP Semibold");
        run3.FontFamily = fontFamily2;
        Brush brush2 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        run3.Foreground = brush2;
        Run run4 = run3;
        run4.Text = number.ToString();
        Run run5 = new Run();
        FontFamily fontFamily3 = new FontFamily("Segoe WP SemiLight");
        run5.FontFamily = fontFamily3;
        Brush brush3 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
        run5.Foreground = brush3;
        Run run6 = run5;
        run6.Text = " " + strArray[1].Trim();
        this.textBlockHeader.Inlines.Add((Inline) run2);
        this.textBlockHeader.Inlines.Add((Inline) run4);
        this.textBlockHeader.Inlines.Add((Inline) run6);
      }
      Run run7 = new Run();
      FontFamily fontFamily4 = new FontFamily("Segoe WP SemiLight");
      run7.FontFamily = fontFamily4;
      Brush brush4 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
      run7.Foreground = brush4;
      Run run8 = run7;
      run8.Text = " " + actionText;
      this.textBlockHeader.Inlines.Add((Inline) run8);
      if (string.IsNullOrEmpty(hightlightedText))
        return;
      this.textBlockHeader.Inlines.Add((Inline) new Run()
      {
        Text = (" " + hightlightedText)
      });
    }

    private double CalculateTextBlockHeaderWidth()
    {
      double width1 = this.gridInner.Width;
      Thickness margin1 = this.textBlockHeader.Margin;
      double left1 = margin1.Left;
      margin1 = this.textBlockHeader.Margin;
      double right1 = margin1.Right;
      double num1 = left1 - right1;
      double num2 = width1 - num1;
      if (this.imageThumb.Visibility == Visibility.Visible)
      {
        double num3 = num2;
        double width2 = this.imageThumb.Width;
        Thickness margin2 = this.imageThumb.Margin;
        double left2 = margin2.Left;
        double num4 = width2 + left2;
        margin2 = this.imageThumb.Margin;
        double right2 = margin2.Right;
        double num5 = num4 + right2;
        num2 = num3 - num5;
      }
      return num2;
    }

    private void ConfigureLeftSideImage(NotificationType type)
    {
      if (this._users.Count > 0)
      {
        this.mainUserImage.Tag = (object) this._users[0];
        ImageLoader.SetSourceForImage(this.mainUserImage, this._users[0].photo_max, false);
      }
      string str = "";
      switch (type)
      {
        case NotificationType.follow:
          str = "/Resources/FeedbackIconsFollower.png";
          break;
        case NotificationType.friend_accepted:
          str = "/Resources/FeedbackIconsRequest.png";
          break;
        case NotificationType.like_post:
        case NotificationType.like_comment:
        case NotificationType.like_photo:
        case NotificationType.like_video:
        case NotificationType.like_comment_photo:
        case NotificationType.like_comment_video:
        case NotificationType.like_comment_topic:
          str = "/Resources/FeedbackIconsLike.png";
          break;
        case NotificationType.copy_post:
        case NotificationType.copy_photo:
        case NotificationType.copy_video:
          str = "/Resources/FeedbackIconsRepost.png";
          break;
      }
      MultiResImageLoader.SetUriSource(this.imageIcon, str);
    }

    private string PrepareText(string text, bool haveThumb)
    {
      int num = haveThumb ? 22 : 30;
      string[] strArray = text.Split(' ');
      string str = "";
      bool flag = false;
      for (int index = 0; index < strArray.Length; ++index)
      {
        if (!flag && str.Length + strArray[index].Length > num)
        {
          str += Environment.NewLine;
          flag = true;
        }
        str = str + strArray[index] + " ";
      }
      if (!flag)
        str += Environment.NewLine;
      return str;
    }

    private void UserNotificationTemplateUC_Tap(object sender, GestureEventArgs e)
    {
      Image image = sender as Image;
      if (image == null)
        return;
      User user = image.Tag as User;
      if (user == null)
        return;
      Navigator.Current.NavigateToUserProfile(user.uid, user.Name, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UserNotificationTemplateUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.gridInner = (Grid) this.FindName("gridInner");
      this.mainUserImage = (Image) this.FindName("mainUserImage");
      this.imageIcon = (Image) this.FindName("imageIcon");
      this.textBlockHeader = (TextBlock) this.FindName("textBlockHeader");
      this.canvasAdditionalUserImages = (Canvas) this.FindName("canvasAdditionalUserImages");
      this.rect1 = (Ellipse) this.FindName("rect1");
      this.image1 = (Image) this.FindName("image1");
      this.rect2 = (Ellipse) this.FindName("rect2");
      this.image2 = (Image) this.FindName("image2");
      this.rect3 = (Ellipse) this.FindName("rect3");
      this.image3 = (Image) this.FindName("image3");
      this.rect4 = (Ellipse) this.FindName("rect4");
      this.image4 = (Image) this.FindName("image4");
      this.rect5 = (Ellipse) this.FindName("rect5");
      this.image5 = (Image) this.FindName("image5");
      this.textBlockDate = (TextBlock) this.FindName("textBlockDate");
      this.imageThumb = (Image) this.FindName("imageThumb");
      this.ucEarlierReplies = (TextSeparatorUC) this.FindName("ucEarlierReplies");
    }
  }
}
