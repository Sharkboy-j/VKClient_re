using VKClient.Audio.Base.Library;

namespace VKClient.Audio.Base.Events
{
  public class StickersPurchaseFunnelEvent : StatEventBase
  {
      public StickersPurchaseFunnelSource Source { get; set; }

      public StickersPurchaseFunnelAction Action { get; set; }

    public StickersPurchaseFunnelEvent(StickersPurchaseFunnelSource source, StickersPurchaseFunnelAction action)
    {
      this.Source = source;
      this.Action = action;
    }

    public StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction action)
      : this(CurrentStickersPurchaseFunnelSource.Source, action)
    {
    }
  }
}
