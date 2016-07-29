using System;
using System.Globalization;
using System.IO;
using System.Windows;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
    public class AttachmentViewModel : ViewModelBase, IBinarySerializable
    {
        private string _resourceDescription = string.Empty;
        //private string _accessKey = "";
        private AttachmentType _attachmentType;
        private double _latitude;
        private double _longitude;
        private int _mediaDurationSeconds;
        private AudioObj _audio;
        private Photo _photo;
        private WallPost _wallPost;
        private double _stickerDimension;
        private HorizontalAlignment _stickerAlignment;
        private string _documentImageUri;
        private Attachment _attachment;
        private Geo _geo;
        private Comment _comment;

        public long MediaId { get; private set; }

        public long MediaOwnerId { get; private set; }

        public string AccessKey { get; set; }

        public string ResourceUri { get; set; }

        public string VideoUri { get; set; }

        public bool IsExternalVideo { get; set; }

        public AttachmentType AttachmentType
        {
            get
            {
                return this._attachmentType;
            }
            private set
            {
                this._attachmentType = value;
            }
        }

        public string ResourceDescription
        {
            get
            {
                return this._resourceDescription;
            }
            set
            {
                this._resourceDescription = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ResourceDescription));
            }
        }

        public string Artist { get; private set; }

        public string UniqueId { get; private set; }

        public double Latitude
        {
            get
            {
                return this._latitude;
            }
        }

        public double Longitude
        {
            get
            {
                return this._longitude;
            }
        }

        public string MediaDuration
        {
            get
            {
                if (this._mediaDurationSeconds < 3600)
                    return TimeSpan.FromSeconds((double)this._mediaDurationSeconds).ToString("m\\:ss");
                return TimeSpan.FromSeconds((double)this._mediaDurationSeconds).ToString("h\\:mm\\:ss");
            }
        }

        public AudioObj Audio
        {
            get
            {
                return this._audio;
            }
            set
            {
                AudioObj audioObj = this._audio;
                this._audio = value;
            }
        }

        public Photo Photo
        {
            get
            {
                return this._photo;
            }
            set
            {
                this._photo = value;
            }
        }

        public WallPost WallPost
        {
            get
            {
                return this._wallPost;
            }
        }

        public double StickerDimension
        {
            get
            {
                return this._stickerDimension;
            }
        }

        public HorizontalAlignment StickerAlignment
        {
            get
            {
                return this._stickerAlignment;
            }
        }

        public string DocumentImageUri
        {
            get
            {
                return this._documentImageUri;
            }
        }

        public Attachment Attachment
        {
            get
            {
                return this._attachment;
            }
        }

        public Geo Geo
        {
            get
            {
                return this._geo;
            }
        }

        public Comment Comment
        {
            get
            {
                return this._comment;
            }
        }

        public bool IsDocumentImageAttachement
        {
            get
            {
                bool flag = false;
                if (this.AttachmentType == AttachmentType.Document && !string.IsNullOrEmpty(this.ResourceDescription))
                {
                    string upperInvariant = this.ResourceDescription.ToUpperInvariant();
                    foreach (string supportedImageExtension in VKConstants.SupportedImageExtensions)
                    {
                        if (upperInvariant.EndsWith(supportedImageExtension))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                return flag;
            }
        }

        // NEW: 4.8.0
        public bool IsDocumentGraffitiAttachment
        {
            get
            {
                if (this.AttachmentType == AttachmentType.Document)
                {
                    Attachment attachment = this.Attachment;
                    bool? nullable1;
                    if (attachment == null)
                    {
                        nullable1 = new bool?();
                    }
                    else
                    {
                        Doc doc = attachment.doc;
                        nullable1 = doc != null ? new bool?(doc.IsGraffiti) : new bool?();
                    }
                    bool? nullable2 = nullable1;
                    if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        return true;
                }
                return false;
            }
        }

        public AttachmentViewModel(Attachment attachment, Message message)
            : this(attachment)
        {
            if (message == null)
                return;
            if (message.@out == 1)
                this._stickerAlignment = HorizontalAlignment.Right;
            else
                this._stickerAlignment = HorizontalAlignment.Left;
        }

        public AttachmentViewModel(Attachment attachment)
        {
            this._attachment = attachment;
            if (attachment.type == "photo")
            {
                this.AttachmentType = AttachmentType.Photo;
                this.ResourceUri = attachment.photo.src_big;
                this.AccessKey = attachment.photo.access_key ?? "";
                this.Photo = attachment.photo;
            }
            if (attachment.type == "video")
            {
                this.AttachmentType = AttachmentType.Video;
                this.ResourceUri = attachment.video.image_big;
                if (string.IsNullOrEmpty(this.ResourceUri))
                    this.ResourceUri = attachment.video.image_medium;
                this._mediaDurationSeconds = attachment.video.duration;
                this.MediaId = attachment.video.vid;
                this.MediaOwnerId = attachment.video.owner_id;
                this.AccessKey = attachment.video.access_key ?? "";
            }
            if (attachment.type == "doc")
            {
                this.AttachmentType = AttachmentType.Document;
                if (attachment.doc != null)
                {
                    this.ResourceDescription = attachment.doc.title;
                    this.ResourceUri = attachment.doc.url;
                    this._documentImageUri = !attachment.doc.IsGraffiti ? attachment.doc.PreviewUri : attachment.doc.GraffitiPreviewUri;// UPDATE: 4.8.0
                }
            }
            if (attachment.type == "audio")
            {
                this.AttachmentType = AttachmentType.Audio;
                if (attachment.audio != null)
                {
                    this.ResourceUri = attachment.audio.url;
                    this.ResourceDescription = attachment.audio.title ?? "";
                    this.ResourceDescription = this.ResourceDescription.Trim();
                    this.Artist = attachment.audio.artist ?? "";
                    this.Artist = this.Artist.Trim();
                    this.MediaId = attachment.audio.aid;
                    this.MediaOwnerId = attachment.audio.owner_id;
                    this._audio = attachment.audio;
                }
            }
            if (attachment.type == "wall")
            {
                this.AttachmentType = AttachmentType.WallPost;
                if (attachment.wall != null)
                {
                    this._wallPost = attachment.wall;
                    this.ResourceUri = "http://vk.com/wall" + (object)attachment.wall.to_id + "_" + (object)attachment.wall.id;
                }
            }
            if (attachment.type == "wall_reply")
            {
                this.AttachmentType = AttachmentType.WallReply;
                if (attachment.wall_reply != null)
                {
                    this._comment = attachment.wall_reply;
                    this.ResourceUri = "http://vk.com/wall" + (object)attachment.wall_reply.owner_id + "_" + (object)attachment.wall_reply.post_id;
                }
            }
            if (attachment.type == "sticker")
            {
                this.AttachmentType = AttachmentType.Sticker;
                if (attachment.sticker != null)
                {
                    this.TryReadStickerData(attachment.sticker.photo_64, 64);
                    this.TryReadStickerData(attachment.sticker.photo_128, 128);
                    this.TryReadStickerData(attachment.sticker.photo_256, 256);
                }
            }
            this.UniqueId = Guid.NewGuid().ToString();
        }

        public AttachmentViewModel(Geo geo)
        {
            this._geo = geo;
            this.AttachmentType = AttachmentType.Geo;
            string[] strArray = geo.coordinates.Split(' ');
            if (strArray.Length > 1)
            {
                double.TryParse(strArray[0], NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out this._latitude);
                double.TryParse(strArray[1], NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out this._longitude);
            }
            this.InitializeUIPropertiesForGeoAttachment();
        }

        public AttachmentViewModel()
        {
        }

        private void TryReadStickerData(string resourceUri, int dimension)
        {
            if (!string.IsNullOrWhiteSpace(resourceUri))
            {
                this.ResourceUri = resourceUri;
                this._stickerDimension = (double)(dimension * 100 / ScaleFactor.GetScaleFactor());
            }
            if (this._stickerDimension < 200.0)
                return;
            this._stickerDimension = this._stickerDimension / 1.5;
        }

        private void InitializeUIPropertiesForGeoAttachment()
        {
            this.ResourceUri = MapsService.Current.GetMapUri(this.Latitude, this.Longitude, 15, 310, 1.8).ToString();
            MapsService.Current.ReverseGeocodeToAddress(this.Latitude, this.Longitude, (Action<BackendResult<string, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.ResourceDescription = res.ResultData;
            }));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(5);
            writer.Write((int)this._attachmentType);
            writer.WriteString(this.ResourceUri);
            writer.WriteString(this._resourceDescription);
            writer.Write(this.Latitude);
            writer.Write(this.Longitude);
            writer.Write(this._mediaDurationSeconds);
            writer.Write(this.MediaId);
            writer.Write(this.MediaOwnerId);
            writer.WriteString(this.VideoUri);
            writer.Write(this.IsExternalVideo);
            writer.WriteString(this.Artist);
            writer.WriteString(this.UniqueId);
            writer.Write<AudioObj>(this.Audio, false);
            writer.WriteString(this.AccessKey);
            writer.Write<Photo>(this.Photo, false);
            writer.Write<WallPost>(this.WallPost, false);
            writer.Write(this._stickerDimension);
            writer.Write((int)this._stickerAlignment);
            writer.WriteString(this._documentImageUri);
            writer.Write<Attachment>(this._attachment, false);
            writer.Write<Geo>(this._geo, false);
            writer.Write<Comment>(this._comment, false);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            int num2 = 1;
            if (num1 >= num2)
            {
                this._attachmentType = (AttachmentType)reader.ReadInt32();
                this.ResourceUri = reader.ReadString();
                this._resourceDescription = reader.ReadString();
                this._latitude = reader.ReadDouble();
                this._longitude = reader.ReadDouble();
                this._mediaDurationSeconds = reader.ReadInt32();
                this.MediaId = reader.ReadInt64();
                this.MediaOwnerId = reader.ReadInt64();
                this.VideoUri = reader.ReadString();
                this.IsExternalVideo = reader.ReadBoolean();
                this.Artist = reader.ReadString();
                this.UniqueId = reader.ReadString();
                this.Audio = reader.ReadGeneric<AudioObj>();
                this.AccessKey = reader.ReadString();
                this.Photo = reader.ReadGeneric<Photo>();
                this._wallPost = reader.ReadGeneric<WallPost>();
                if (this._attachmentType == AttachmentType.Geo && string.IsNullOrEmpty(this._resourceDescription))
                    this.InitializeUIPropertiesForGeoAttachment();
            }
            int num3 = 2;
            if (num1 >= num3)
            {
                this._stickerDimension = reader.ReadDouble();
                this._stickerAlignment = (HorizontalAlignment)reader.ReadInt32();
            }
            int num4 = 3;
            if (num1 >= num4)
                this._documentImageUri = reader.ReadString();
            int num5 = 4;
            if (num1 >= num5)
            {
                this._attachment = reader.ReadGeneric<Attachment>();
                this._geo = reader.ReadGeneric<Geo>();
            }
            int num6 = 5;
            if (num1 < num6)
                return;
            this._comment = reader.ReadGeneric<Comment>();
        }

        public void NotifyResourceUriChanged()
        {
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ResourceUri));
        }
    }
}
