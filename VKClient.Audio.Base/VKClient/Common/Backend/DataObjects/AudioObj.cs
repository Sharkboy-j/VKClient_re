using System.IO;
using VKClient.Audio.Base;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class AudioObj : IBinarySerializable
  {
    private string _artist = "";
    private string _title = "";

    public long aid
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

    public long id { get; set; }

    public string Id
    {
      get
      {
        long num = this.owner_id;
        string string1 = num.ToString();
        string str = "_";
        num = this.aid;
        string string2 = num.ToString();
        return string1 + str + string2;
      }
    }

    public long owner_id { get; set; }

    public long lyrics_id { get; set; }

    public string artist
    {
      get
      {
        return this._artist;
      }
      set
      {
        this._artist = (value ?? "").ForUI();
        this._artist = StringUtils.MakeIntoOneLine(this._artist);
      }
    }

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
        this._title = StringUtils.MakeIntoOneLine(this._title);
      }
    }

    public string duration { get; set; }

    public string performer
    {
      get
      {
        return this.artist;
      }
      set
      {
        this.artist = value;
      }
    }

    public string url { get; set; }

    public long album_id { get; set; }

    public string UniqueId
    {
      get
      {
        return this.Id;
      }
    }

    public override string ToString()
    {
      return string.Format("audio{0}_{1}", (object) this.owner_id, (object) this.id);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write(this.aid);
      writer.Write(this.owner_id);
      writer.WriteString(this.artist);
      writer.WriteString(this.title);
      writer.WriteString(this.duration);
      writer.WriteString(this.url);
      writer.Write(this.album_id);
      writer.Write(this.lyrics_id);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.aid = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.artist = reader.ReadString();
      this.title = reader.ReadString();
      this.duration = reader.ReadString();
      this.url = reader.ReadString();
      this.album_id = reader.ReadInt64();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.lyrics_id = reader.ReadInt64();
    }
  }
}
