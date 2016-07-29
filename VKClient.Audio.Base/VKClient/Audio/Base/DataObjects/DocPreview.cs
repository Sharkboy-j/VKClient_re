using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class DocPreview : IBinarySerializable
  {
    public DocPreviewPhoto photo { get; set; }

    public DocPreviewVideo video { get; set; }

    public DocPreviewGraffiti graffiti { get; set; }// NEW: 4.8.0

    public void Write(BinaryWriter writer)
    {
        writer.Write(3);
        writer.Write<DocPreviewPhoto>(this.photo, false);
        writer.Write<DocPreviewVideo>(this.video, false);
        writer.Write<DocPreviewGraffiti>(this.graffiti, false);
    }

    public void Read(BinaryReader reader)
    {
        int num1 = reader.ReadInt32();
        this.photo = reader.ReadGeneric<DocPreviewPhoto>();
        int num2 = 2;
        if (num1 >= num2)
            this.video = reader.ReadGeneric<DocPreviewVideo>();
        int num3 = 3;
        if (num1 < num3)
            return;
        this.graffiti = reader.ReadGeneric<DocPreviewGraffiti>();
    }
  }
}
