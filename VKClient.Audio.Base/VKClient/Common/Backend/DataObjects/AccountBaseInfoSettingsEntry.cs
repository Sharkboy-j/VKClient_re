namespace VKClient.Common.Backend.DataObjects
{
  public class AccountBaseInfoSettingsEntry
  {
    public static readonly string GIF_AUTOPLAY_KEY = "gif_autoplay";
    public static readonly string PAYMENT_TYPE_KEY = "payment_type";

    public string name { get; set; }

    public bool available { get; set; }

    public bool forced { get; set; }

    public string value { get; set; }
  }
}
