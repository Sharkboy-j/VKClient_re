using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.UC.InplaceGifViewer;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using XamlAnimatedGif;

namespace VKClient.Common.UC
{
  public class DocImageAttachmentUC : UserControlVirtualizable
  {
    public const double FIXED_HEIGHT = 152.0;
    private List<Attachment> _attachments;
    private DocumentHeader _docHeader1;
    private DocumentHeader _docHeader2;
    private Uri _docImageUri1;
    private Uri _docImageUri2;
    private bool _tapHandled;
    internal Image image1;
    internal TextBlock textBlockDescription1;
    internal Grid gridDoc2;
    internal Image image2;
    internal TextBlock textBlockDescription2;
    private bool _contentLoaded;

    public DocImageAttachmentUC()
    {
      this.InitializeComponent();
      this.textBlockDescription1.Text = "";
      this.textBlockDescription2.Text = "";
      this.gridDoc2.Visibility = Visibility.Collapsed;
    }

    private void Doc1_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      this.HandleTap(this._docHeader1);
    }

    private void Doc2_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      this.HandleTap(this._docHeader2);
    }

    private async void HandleTap(DocumentHeader docHeader)
    {
      if (docHeader.IsGif)
      {
        if (this._tapHandled)
          return;
        this._tapHandled = true;
        List<PhotoOrDocument> list = this._attachments.OrderBy<Attachment, bool>((Func<Attachment, bool>) (at =>
        {
          if (at.doc == null)
            return false;
          DocPreview preview = at.doc.preview;
          return (preview != null ? preview.video : (DocPreviewVideo) null) == null;
        })).ToList<Attachment>().Where<Attachment>((Func<Attachment, bool>) (at =>
        {
          if (at.photo != null)
            return true;
          if (at.doc != null)
            return at.doc.IsGif;
          return false;
        })).Select<Attachment, PhotoOrDocument>((Func<Attachment, PhotoOrDocument>) (a => new PhotoOrDocument()
        {
          document = a.doc,
          photo = a.photo
        })).ToList<PhotoOrDocument>();
        int index = list.IndexOf(list.FirstOrDefault<PhotoOrDocument>((Func<PhotoOrDocument, bool>) (at =>
        {
          if (at.document != null && at.document.owner_id == docHeader.Document.owner_id)
            return at.document.id == docHeader.Document.id;
          return false;
        })));
        InplaceGifViewerUC gifViewer = new InplaceGifViewerUC();
        await AnimationBehavior.ClearGifCacheAsync();
        Navigator.Current.NavigateToImageViewerPhotosOrGifs(index, list, false, false, (Func<int, Image>) null, (PageBase) null, false, (FrameworkElement) gifViewer, (Action<int>) (ind =>
        {
          Doc document = list[ind].document;
          if (document != null)
          {
            InplaceGifViewerViewModel gifViewerViewModel = new InplaceGifViewerViewModel(document, true, false, false);
            gifViewerViewModel.Play(GifPlayStartType.manual);
            gifViewer.VM = gifViewerViewModel;
            gifViewer.Visibility = Visibility.Visible;
          }
          else
          {
            InplaceGifViewerViewModel vm = gifViewer.VM;
            if (vm != null)
              vm.Stop();
            gifViewer.Visibility = Visibility.Collapsed;
          }
        }), (Action<int, bool>) ((i, b) => {}), false);
        this._tapHandled = false;
      }
      else
        Navigator.Current.NavigateToWebUri(docHeader.Document.url, true, false);
    }

    public void Initialize(List<Attachment> attachments, Doc doc1, Doc doc2)
    {
      this._attachments = attachments;
      this._docHeader1 = new DocumentHeader(doc1, 0, false);
      this._docImageUri1 = this._docHeader1.ThumbnailUri.ConvertToUri();
      this.textBlockDescription1.Text = this._docHeader1.IsGif ? DocImageAttachmentUC.GetGifDescription(this._docHeader1) : this._docHeader1.Description;
      if (doc2 == null)
        return;
      this.gridDoc2.Visibility = Visibility.Visible;
      this._docHeader2 = new DocumentHeader(doc2, 0, false);
      this._docImageUri2 = this._docHeader2.ThumbnailUri.ConvertToUri();
      this.textBlockDescription2.Text = this._docHeader2.IsGif ? DocImageAttachmentUC.GetGifDescription(this._docHeader2) : this._docHeader2.Description;
    }

    private static string GetGifDescription(DocumentHeader documentHeader)
    {
      if (GifsPlayerUtils.ShouldShowSize())
        return documentHeader.Description;
      return "GIF";
    }

    public override void LoadFullyNonVirtualizableItems()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image1, this._docImageUri1);
      if (this._docHeader2 == null)
        return;
      VeryLowProfileImageLoader.SetUriSource(this.image2, this._docImageUri2);
    }

    public override void ReleaseResources()
    {
      VeryLowProfileImageLoader.SetUriSource(this.image1, (Uri) null);
      if (this._docHeader2 == null)
        return;
      VeryLowProfileImageLoader.SetUriSource(this.image2, (Uri) null);
    }

    public override void ShownOnScreen()
    {
      DateTime now;
      if (this._docImageUri1 != (Uri) null && this._docImageUri1.IsAbsoluteUri)
      {
        string originalString = this._docImageUri1.OriginalString;
        now = DateTime.Now;
        long ticks = now.Ticks;
        VeryLowProfileImageLoader.SetPriority(originalString, ticks);
      }
      if (!(this._docImageUri2 != (Uri) null) || !this._docImageUri2.IsAbsoluteUri)
        return;
      string originalString1 = this._docImageUri2.OriginalString;
      now = DateTime.Now;
      long ticks1 = now.Ticks;
      VeryLowProfileImageLoader.SetPriority(originalString1, ticks1);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/DocImageAttachmentUC.xaml", UriKind.Relative));
      this.image1 = (Image) this.FindName("image1");
      this.textBlockDescription1 = (TextBlock) this.FindName("textBlockDescription1");
      this.gridDoc2 = (Grid) this.FindName("gridDoc2");
      this.image2 = (Image) this.FindName("image2");
      this.textBlockDescription2 = (TextBlock) this.FindName("textBlockDescription2");
    }
  }
}
