namespace VKClient.Audio.Base.DataObjects
{
  public class StorePurchaseResult
  {
    public int state { get; set; }

    public int error_fatal { get; set; }

    public string error_message { get; set; }

    public override string ToString()
    {
      return string.Format("State: {0}\nError fatal: {1}\nError message: {2}", (object) this.state, (object) this.error_fatal, (object) this.error_message);
    }

    public static StorePurchaseResult GetForFailedPurchaseState()
    {
      return new StorePurchaseResult()
      {
        state = -2
      };
    }
  }
}
