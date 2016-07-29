namespace VKClient.Audio.Base.DataObjects
{
  public class PostSource
  {
    private const string TYPE_API = "api";
    private const string TYPE_MOBILE = "mvk";
    private const string PLATFORM_IOS = "ios";
    private const string PLATFORM_IPHONE = "iphone";
    private const string PLATFORM_IPAD = "ipad";
    private const string PLATFORM_ANDROID = "android";
    private const string PLATFORM_WINDOWS = "windows";
    private const string PLATFORM_WINPHONE = "wphone";
    private const string PLATFORM_SNAPSTER = "chronicle";
    private const string PLATFORM_INSTAGRAM = "instagram";

    public string type { get; set; }

    public string data { get; set; }

    public string platform { get; set; }

    public PostSourcePlatform GetPlatform()
    {
      if (this.type == "api")
      {
        if (this.platform == "ios" || this.platform == "iphone" || this.platform == "ipad")
          return PostSourcePlatform.IOS;
        if (this.platform == "android")
          return PostSourcePlatform.Android;
        if (this.platform == "windows" || this.platform == "wphone")
          return PostSourcePlatform.Windows;
        if (this.platform == "chronicle")
          return PostSourcePlatform.Snapster;
        return this.platform == "instagram" ? PostSourcePlatform.Instagram : PostSourcePlatform.ThirdParty;
      }
      return this.type == "mvk" ? PostSourcePlatform.Mobile : PostSourcePlatform.None;
    }
  }
}
