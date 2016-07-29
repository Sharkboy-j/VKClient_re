namespace VKClient.Audio.Base.Events
{
  public class MarketContactEvent : StatEventBase
  {
      public string ItemId { get; set; }

      public MarketContactAction Action { get; set; }

    public MarketContactEvent(string itemId, MarketContactAction action)
    {
      this.ItemId = itemId;
      this.Action = action;
    }
  }
}
