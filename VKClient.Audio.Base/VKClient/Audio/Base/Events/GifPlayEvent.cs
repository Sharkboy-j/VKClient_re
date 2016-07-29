namespace VKClient.Audio.Base.Events
{
  public class GifPlayEvent : StatEventBase
  {
      public string GifId { get; set; }

      public GifPlayStartType StartType { get; set; }

      public StatisticsActionSource Source { get; set; }

    public GifPlayEvent(string gifId, GifPlayStartType startType, StatisticsActionSource source)
    {
      this.GifId = gifId;
      this.StartType = startType;
      this.Source = source;
    }
  }
}
