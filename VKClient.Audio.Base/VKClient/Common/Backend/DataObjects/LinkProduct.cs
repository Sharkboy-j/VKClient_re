using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkProduct : IBinarySerializable
  {
    public Price price { get; set; }

    public LinkProduct(Product product)
    {
      this.price = product.price;
    }

    public LinkProduct()
    {
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write<Price>(this.price, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.price = reader.ReadGeneric<Price>();
    }
  }
}
