using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkButton : IBinarySerializable
  {
    private string _title;

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

    public string url { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this.title);
      writer.WriteString(this.url);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.title = reader.ReadString();
      this.url = reader.ReadString();
    }
  }
}
