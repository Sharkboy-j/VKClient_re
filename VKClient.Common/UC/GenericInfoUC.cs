using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GenericInfoUC : UserControl
  {
    private DelayedExecutor _deHide;
    private const int DEFAULT_DELAY = 1000;
    internal Grid LayoutRoot;
    internal ScrollableTextBlock textBlockInfo;
    internal RichTextBox richTextBox;
    private bool _contentLoaded;

    public GenericInfoUC()
    {
      this.InitializeComponent();
      this._deHide = new DelayedExecutor(1000);
    }

    public GenericInfoUC(int delayToHide)
    {
      this.InitializeComponent();
      this._deHide = new DelayedExecutor(delayToHide > 0 ? delayToHide : 1000);
    }

    public void ShowAndHideLater(string text, FrameworkElement elementToFadeout)
    {
      DialogService ds = new DialogService();
      this.textBlockInfo.Text = text;
      ds.IsOverlayApplied = false;
      ds.Child = (FrameworkElement) this;
      ds.KeepAppBar = true;
      ds.Show((UIElement) elementToFadeout);
      this._deHide.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => ds.Hide()))));
    }

    public static void ShowBasedOnResult(int resultCode, string successString = "", VKRequestsDispatcher.Error error = null)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (resultCode == 0)
        {
          if (string.IsNullOrWhiteSpace(successString))
            return;
          new GenericInfoUC().ShowAndHideLater(successString, null);
        }
        else
        {
          int delayToHide = 0;
          string text = CommonResources.Error;
          if (Enum.IsDefined(typeof (ResultCode), (object) resultCode))
          {
            switch ((ResultCode) resultCode)
            {
              case ResultCode.InvalidCode:
                text = CommonResources.Registration_WrongCode;
                break;
              case ResultCode.Processing:
                text = CommonResources.Registration_TryAgainLater;
                break;
              case ResultCode.ProductNotFound:
                text = CommonResources.CannotLoadProduct;
                delayToHide = 2000;
                break;
              case ResultCode.CommunicationFailed:
                text = CommonResources.Error_Connection;
                delayToHide = 3000;
                break;
              case ResultCode.WrongPhoneNumberFormat:
                text = CommonResources.Registration_InvalidPhoneNumber;
                break;
              case ResultCode.PhoneAlreadyRegistered:
                text = CommonResources.Registration_PhoneNumberIsBusy;
                break;
            }
          }
          if (error != null && !string.IsNullOrWhiteSpace(error.error_text))
            text = error.error_text;
          new GenericInfoUC(delayToHide).ShowAndHideLater(text, null);
        }
      }));
    }

    public static void ShowPhotoIsSavedInSavedPhotos()
    {
      GenericInfoUC genericInfoUc = new GenericInfoUC(3000);
      genericInfoUc.richTextBox.Visibility = Visibility.Visible;
      genericInfoUc.textBlockInfo.Visibility = Visibility.Collapsed;
      RichTextBox richTextBox = genericInfoUc.richTextBox;
      Paragraph paragraph = new Paragraph();
      string text1 = CommonResources.PhotoIsSaved.Replace(".", "") + " ";
      paragraph.Inlines.Add((Inline) BrowserNavigationService.GetRunWithStyle(text1, richTextBox));
      Hyperlink hyperlink = HyperlinkHelper.GenerateHyperlink(CommonResources.InTheAlbum, "", (Action<Hyperlink, string>) ((hl, str) => Navigator.Current.NavigateToPhotoAlbum(AppGlobalStateManager.Current.LoggedInUserId, false, "SavedPhotos", "", CommonResources.AlbumSavedPictures, 1, "", "", false, 0)), richTextBox.Foreground);
      hyperlink.FontSize = richTextBox.FontSize;
      paragraph.Inlines.Add((Inline) hyperlink);
      richTextBox.Blocks.Add((Block) paragraph);
      string text2 = "";
      object local = null;
      genericInfoUc.ShowAndHideLater(text2, (FrameworkElement) local);
    }

    public static void ShowPublishResult(GenericInfoUC.PublishedObj publishedObj, long gid = 0, string groupName = "")
    {
      new GenericInfoUC(3000).ShowAndHideLater(GenericInfoUC.GetInfoStr(publishedObj, gid, groupName), null);
    }

    private static string GetInfoStr(GenericInfoUC.PublishedObj publishedObj, long gid, string groupName)
    {
      string format = "";
      string str;
      if (gid == 0L)
      {
        switch (publishedObj)
        {
          case GenericInfoUC.PublishedObj.WallPost:
            format = CommonResources.ThePostIsPublishedOnWallFrm;
            break;
          case GenericInfoUC.PublishedObj.Photo:
            format = CommonResources.ThePhotoIsPublishedOnWallFrm;
            break;
          case GenericInfoUC.PublishedObj.Video:
            format = CommonResources.TheVideoIsPublishedOnWallFrm;
            break;
          case GenericInfoUC.PublishedObj.Doc:
            format = CommonResources.TheDocumentIsPublishedOnWallFrm;
            break;
        }
        str = string.Format(format, (object) ("[id" + (object) AppGlobalStateManager.Current.LoggedInUserId + "|" + CommonResources.Yours + "]"));
      }
      else
      {
        switch (publishedObj)
        {
          case GenericInfoUC.PublishedObj.WallPost:
            format = CommonResources.ThePostIsPublishedInCommunityFrm;
            break;
          case GenericInfoUC.PublishedObj.Photo:
            format = CommonResources.ThePhotoIsPublishedInCommunityFrm;
            break;
          case GenericInfoUC.PublishedObj.Video:
            format = CommonResources.TheVideoIsPublishedInCommunityFrm;
            break;
          case GenericInfoUC.PublishedObj.Doc:
            format = CommonResources.TheDocumentIsPublishedInCommunityFrm;
            break;
        }
        str = string.Format(format, (object) ("[club" + (object) gid + "|" + groupName + "]"));
      }
      return str;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GenericInfoUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.textBlockInfo = (ScrollableTextBlock) this.FindName("textBlockInfo");
      this.richTextBox = (RichTextBox) this.FindName("richTextBox");
    }

    public enum PublishedObj
    {
      WallPost,
      Photo,
      Video,
      Doc,
    }
  }
}
