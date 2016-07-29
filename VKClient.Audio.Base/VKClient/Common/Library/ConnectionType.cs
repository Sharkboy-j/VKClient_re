namespace VKClient.Common.Library
{
  public class ConnectionType
  {
      public string Type { get; set; }

      public string Subtype { get; set; }

    public ConnectionType(string type, string subtype)
    {
      this.Type = type;
      this.Subtype = subtype;
    }

    public override string ToString()
    {
      return string.Format("{0} {1}", (object) this.Type, (object) this.Subtype);
    }
  }
}
