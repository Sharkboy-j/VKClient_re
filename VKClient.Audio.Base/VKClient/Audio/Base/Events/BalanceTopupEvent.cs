namespace VKClient.Audio.Base.Events
{
  public class BalanceTopupEvent : StatEventBase
  {
      public BalanceTopupSource Source { get; set; }

      public BalanceTopupAction Action { get; set; }

    public BalanceTopupEvent(BalanceTopupSource source, BalanceTopupAction action)
    {
      this.Source = source;
      this.Action = action;
    }
  }
}
