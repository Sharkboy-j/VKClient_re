using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Album : IBinarySerializable
  {
    private string _title = "";
    private string _description = "";

    public string aid
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public string id { get; set; }

    public string thumb_id { get; set; }

    public string owner_id { get; set; }

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
      }
    }

    public string description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = (value ?? "").ForUI();
      }
    }

    public string created { get; set; }

    public string updated { get; set; }

    public int size { get; set; }

    public string thumb_src { get; set; }

    public string thumb_src_small { get; set; }

    public Photo thumb { get; set; }

    public PrivacyInfo PrivacyViewInfo
    {
      get
      {
        return new PrivacyInfo(this.privacy_view);
      }
    }

    public List<string> privacy_view { get; set; }

    public int comment_privacy { get; set; }

    public Album()
    {
      this.privacy_view = new List<string>();
    }

    public Album Copy()
    {
      return new Album()
      {
        aid = this.aid,
        thumb_id = this.thumb_id,
        owner_id = this.owner_id,
        title = this.title,
        description = this.description,
        created = this.created,
        updated = this.updated,
        size = this.size,
        thumb_src = this.thumb_src,
        privacy_view = new List<string>((IEnumerable<string>) this.privacy_view),
        comment_privacy = this.comment_privacy
      };
    }

    public override string ToString()
    {
      return string.Format("album{0}_{1}", (object) this.owner_id, (object) this.id);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.aid);
      writer.WriteString(this.thumb_id);
      writer.WriteString(this.owner_id);
      writer.WriteString(this.title);
      writer.WriteString(this.description);
      writer.WriteString(this.created);
      writer.WriteString(this.updated);
      writer.Write(this.size);
      writer.WriteString(this.thumb_src);
      writer.WriteString(this.thumb_src_small);
      writer.Write<Photo>(this.thumb, false);
      writer.WriteList(this.privacy_view);
      writer.Write(this.comment_privacy);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.aid = reader.ReadString();
      this.thumb_id = reader.ReadString();
      this.owner_id = reader.ReadString();
      this.title = reader.ReadString();
      this.description = reader.ReadString();
      this.created = reader.ReadString();
      this.updated = reader.ReadString();
      this.size = reader.ReadInt32();
      this.thumb_src = reader.ReadString();
      this.thumb_src_small = reader.ReadString();
      this.thumb = reader.ReadGeneric<Photo>();
      this.privacy_view = reader.ReadList();
      this.comment_privacy = reader.ReadInt32();
    }
  }
}
