using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Video.Library;

namespace VKMessenger.Library
{
  public class ConversationMaterialsViewModel : ViewModelBase, ICollectionDataProvider<VKList<Attachment>, AlbumPhotoHeaderFourInARow>, ICollectionDataProvider<VKList<Attachment>, VideoHeader>, ICollectionDataProvider<VKList<Attachment>, AudioHeader>, ICollectionDataProvider<VKList<Attachment>, DocumentHeader>, ICollectionDataProvider<VKList<Attachment>, LinkHeader>
  {
    private readonly long _peerId;
    private string _photosNextFrom;
    private string _videosNextFrom;
    private string _audiosNextFrom;
    private string _documentsNextFrom;
    private string _linksNextFrom;

    public GenericCollectionViewModel<VKList<Attachment>, AlbumPhotoHeaderFourInARow> PhotosVM { get; set; }//

    public GenericCollectionViewModel<VKList<Attachment>, VideoHeader> VideosVM { get; set; }//

    public GenericCollectionViewModel<VKList<Attachment>, AudioHeader> AudiosVM { get; set; }//

    public GenericCollectionViewModel<VKList<Attachment>, DocumentHeader> DocumentsVM { get; set; }//

    public GenericCollectionViewModel<VKList<Attachment>, LinkHeader> LinksVM { get; set; }//

    public string Title
    {
      get
      {
        return CommonResources.Messenger_Materials.ToUpperInvariant();
      }
    }

    Func<VKList<Attachment>, ListWithCount<AlbumPhotoHeaderFourInARow>> ICollectionDataProvider<VKList<Attachment>, AlbumPhotoHeaderFourInARow>.ConverterFunc
    {
      get
      {
        return (Func<VKList<Attachment>, ListWithCount<AlbumPhotoHeaderFourInARow>>) (list =>
        {
          ListWithCount<AlbumPhotoHeaderFourInARow> listWithCount = new ListWithCount<AlbumPhotoHeaderFourInARow>();
          foreach (IEnumerable<Attachment> source in list.items.Partition<Attachment>(4))
          {
            AlbumPhotoHeaderFourInARow headerFourInArow = new AlbumPhotoHeaderFourInARow(source.Select<Attachment, Photo>((Func<Attachment, Photo>) (e => e.photo)));
            listWithCount.List.Add(headerFourInArow);
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<Attachment>, ListWithCount<VideoHeader>> ICollectionDataProvider<VKList<Attachment>, VideoHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<Attachment>, ListWithCount<VideoHeader>>) (list =>
        {
          ListWithCount<VideoHeader> listWithCount = new ListWithCount<VideoHeader>();
          foreach (Attachment attachment in list.items)
          {
            VideoHeader videoHeader = new VideoHeader(attachment.video, (List<MenuItemData>) null, list.profiles, list.groups, StatisticsActionSource.messages, "", false, 0L);
            listWithCount.List.Add(videoHeader);
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<Attachment>, ListWithCount<AudioHeader>> ICollectionDataProvider<VKList<Attachment>, AudioHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<Attachment>, ListWithCount<AudioHeader>>) (list =>
        {
          ListWithCount<AudioHeader> listWithCount = new ListWithCount<AudioHeader>();
          foreach (Attachment attachment in list.items)
          {
            AudioHeader audioHeader = new AudioHeader(attachment.audio);
            listWithCount.List.Add(audioHeader);
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<Attachment>, ListWithCount<DocumentHeader>> ICollectionDataProvider<VKList<Attachment>, DocumentHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<Attachment>, ListWithCount<DocumentHeader>>) (list =>
        {
          ListWithCount<DocumentHeader> listWithCount = new ListWithCount<DocumentHeader>();
          foreach (Attachment attachment in list.items)
          {
            DocumentHeader documentHeader = new DocumentHeader(attachment.doc, 0, false);
            listWithCount.List.Add(documentHeader);
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<Attachment>, ListWithCount<LinkHeader>> ICollectionDataProvider<VKList<Attachment>, LinkHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<Attachment>, ListWithCount<LinkHeader>>) (list =>
        {
          ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
          foreach (Attachment attachment in list.items)
          {
            LinkHeader linkHeader = new LinkHeader(attachment.link);
            listWithCount.List.Add(linkHeader);
          }
          return listWithCount;
        });
      }
    }

    public ConversationMaterialsViewModel(long peerId)
    {
      this._peerId = peerId;
      this.PhotosVM = new GenericCollectionViewModel<VKList<Attachment>, AlbumPhotoHeaderFourInARow>((ICollectionDataProvider<VKList<Attachment>, AlbumPhotoHeaderFourInARow>) this);
      this.VideosVM = new GenericCollectionViewModel<VKList<Attachment>, VideoHeader>((ICollectionDataProvider<VKList<Attachment>, VideoHeader>) this);
      this.AudiosVM = new GenericCollectionViewModel<VKList<Attachment>, AudioHeader>((ICollectionDataProvider<VKList<Attachment>, AudioHeader>) this);
      this.DocumentsVM = new GenericCollectionViewModel<VKList<Attachment>, DocumentHeader>((ICollectionDataProvider<VKList<Attachment>, DocumentHeader>) this);
      this.LinksVM = new GenericCollectionViewModel<VKList<Attachment>, LinkHeader>((ICollectionDataProvider<VKList<Attachment>, LinkHeader>) this);
      this.PhotosVM.LoadCount = 40;
      this.PhotosVM.ReloadCount = 80;
    }

    public void GetData(GenericCollectionViewModel<VKList<Attachment>, AlbumPhotoHeaderFourInARow> caller, int offset, int count, Action<BackendResult<VKList<Attachment>, ResultCode>> callback)
    {
      if (offset > 0 && this._photosNextFrom == null)
      {
        callback(new BackendResult<VKList<Attachment>, ResultCode>(ResultCode.Succeeded, new VKList<Attachment>()));
      }
      else
      {
        if (offset == 0 && this._photosNextFrom != null)
          this._photosNextFrom = null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "photo", this._photosNextFrom, count, (Action<BackendResult<VKList<Attachment>, ResultCode>>) (result =>
        {
          this._photosNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Attachment>, AlbumPhotoHeaderFourInARow> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPhotos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePhotoFrm, CommonResources.TwoFourPhotosFrm, CommonResources.FivePhotosFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<Attachment>, VideoHeader> caller, int offset, int count, Action<BackendResult<VKList<Attachment>, ResultCode>> callback)
    {
      if (offset > 0 && this._videosNextFrom == null)
      {
        callback(new BackendResult<VKList<Attachment>, ResultCode>(ResultCode.Succeeded, new VKList<Attachment>()));
      }
      else
      {
        if (offset == 0 && this._videosNextFrom != null)
          this._videosNextFrom = null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "video", this._videosNextFrom, count, (Action<BackendResult<VKList<Attachment>, ResultCode>>) (result =>
        {
          this._videosNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Attachment>, VideoHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoVideos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<Attachment>, AudioHeader> caller, int offset, int count, Action<BackendResult<VKList<Attachment>, ResultCode>> callback)
    {
      if (offset > 0 && this._audiosNextFrom == null)
      {
        callback(new BackendResult<VKList<Attachment>, ResultCode>(ResultCode.Succeeded, new VKList<Attachment>()));
      }
      else
      {
        if (offset == 0 && this._audiosNextFrom != null)
          this._audiosNextFrom = null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "audio", this._audiosNextFrom, count, (Action<BackendResult<VKList<Attachment>, ResultCode>>) (result =>
        {
          this._audiosNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Attachment>, AudioHeader> caller, int count)
    {
      if (count <= 0)
        return AudioResources.NoTracks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneTrackFrm, AudioResources.TwoFourTracksFrm, AudioResources.FiveTracksFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<Attachment>, DocumentHeader> caller, int offset, int count, Action<BackendResult<VKList<Attachment>, ResultCode>> callback)
    {
      if (offset > 0 && this._documentsNextFrom == null)
      {
        callback(new BackendResult<VKList<Attachment>, ResultCode>(ResultCode.Succeeded, new VKList<Attachment>()));
      }
      else
      {
        if (offset == 0 && this._documentsNextFrom != null)
          this._documentsNextFrom = null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "doc", this._documentsNextFrom, count, (Action<BackendResult<VKList<Attachment>, ResultCode>>) (result =>
        {
          this._documentsNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Attachment>, DocumentHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.Documents_NoDocuments;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneDocFrm, CommonResources.TwoFourDocumentsFrm, CommonResources.FiveDocumentsFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<Attachment>, LinkHeader> caller, int offset, int count, Action<BackendResult<VKList<Attachment>, ResultCode>> callback)
    {
      if (offset > 0 && this._linksNextFrom == null)
      {
        callback(new BackendResult<VKList<Attachment>, ResultCode>(ResultCode.Succeeded, new VKList<Attachment>()));
      }
      else
      {
        if (offset == 0 && this._linksNextFrom != null)
          this._linksNextFrom = null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "link", this._linksNextFrom, count, (Action<BackendResult<VKList<Attachment>, ResultCode>>) (result =>
        {
          this._linksNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Attachment>, LinkHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.Messenger_NoLinks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneLinkFrm, CommonResources.TwoFourLinksFrm, CommonResources.FiveLinksFrm, true, null, false);
    }
  }
}
