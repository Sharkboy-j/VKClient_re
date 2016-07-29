namespace VKMessenger.Backend
{
  public class ChatUser
  {
    private string _photo_max;

    public int uid
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public int id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string first_name_acc { get; set; }

    public string last_name_acc { get; set; }

    public int online { get; set; }

    public int online_mobile { get; set; }

    public string photo_rec { get; set; }

    public string photo_max
    {
      get
      {
        if (this.id < -2000000000)
          return "/VKClient.Common;component/Resources/EmailUser.png";
        return this._photo_max ?? this.photo_200;
      }
      set
      {
        this._photo_max = value;
      }
    }

    public string photo_200 { get; set; }

    public string Name
    {
      get
      {
        return "" + this.first_name + " " + this.last_name;
      }
    }

    public string type { get; set; }

    public string name { get; set; }

    public int invited_by { get; set; }
  }
}
