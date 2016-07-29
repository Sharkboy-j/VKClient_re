using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Attachment : IBinarySerializable
  {
    public string type { get; set; }

    public AudioObj audio { get; set; }

    public VKClient.Common.Backend.DataObjects.Video video { get; set; }

    public Photo photo { get; set; }

    public Doc doc { get; set; }

    public WallPost wall { get; set; }

    public Note note { get; set; }

    public Poll poll { get; set; }

    public Sticker sticker { get; set; }

    public Gift gift { get; set; }

    public Link link { get; set; }

    public Comment wall_reply { get; set; }

    public Product market { get; set; }

    public MarketAlbum market_album { get; set; }

    public Album album { get; set; }

    public Wiki Page { get; set; }

    public void Write(BinaryWriter writer)
    {
      this.SerializeForVersion2(writer);
    }

    private void SerializeForVersion2(BinaryWriter writer)
    {
      writer.Write(6);
      writer.WriteString(this.type);
      writer.Write<AudioObj>(this.audio, false);
      writer.Write<VKClient.Common.Backend.DataObjects.Video>(this.video, false);
      writer.Write<Photo>(this.photo, false);
      writer.Write<Doc>(this.doc, false);
      writer.Write<WallPost>(this.wall, false);
      writer.Write<Note>(this.note, false);
      writer.Write<Poll>(this.poll, false);
      writer.Write<Sticker>(this.sticker, false);
      writer.Write<Gift>(this.gift, false);
      writer.Write<Link>(this.link, false);
      writer.Write<Comment>(this.wall_reply, false);
      writer.Write<Product>(this.market, false);
      writer.Write<MarketAlbum>(this.market_album, false);
      writer.Write<Album>(this.album, false);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.type = reader.ReadString();
      this.audio = reader.ReadGeneric<AudioObj>();
      this.video = reader.ReadGeneric<VKClient.Common.Backend.DataObjects.Video>();
      this.photo = reader.ReadGeneric<Photo>();
      this.doc = reader.ReadGeneric<Doc>();
      this.wall = reader.ReadGeneric<WallPost>();
      this.note = reader.ReadGeneric<Note>();
      this.poll = reader.ReadGeneric<Poll>();
      int num2 = 2;
      if (num1 >= num2)
      {
        this.sticker = reader.ReadGeneric<Sticker>();
        this.gift = reader.ReadGeneric<Gift>();
      }
      int num3 = 3;
      if (num1 >= num3)
        this.link = reader.ReadGeneric<Link>();
      int num4 = 4;
      if (num1 >= num4)
        this.wall_reply = reader.ReadGeneric<Comment>();
      int num5 = 6;
      if (num1 < num5)
        return;
      this.market = reader.ReadGeneric<Product>();
      this.market_album = reader.ReadGeneric<MarketAlbum>();
      this.album = reader.ReadGeneric<Album>();
    }
  }
}
