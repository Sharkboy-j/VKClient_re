using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class Game
  {
    public long id { get; set; }

    public string title { get; set; }

    public string screen_name { get; set; }

    public string description { get; set; }

    public string icon_200 { get; set; }

    public string icon_100 { get; set; }

    public string icon_75 { get; set; }

    public string icon_50 { get; set; }

    public string icon_25 { get; set; }

    public string icon_16 { get; set; }

    public string banner_186 { get; set; }

    public string banner_896 { get; set; }

    public string type { get; set; }

    public string section { get; set; }

    public long author_id { get; set; }

    public string author_url { get; set; }

    public long author_group { get; set; }

    public int members_count { get; set; }

    public long published_date { get; set; }

    public int is_new { get; set; }

    public string genre_id { get; set; }

    public string genre { get; set; }

    public int push_enabled { get; set; }

    public string platform_id { get; set; }

    public string install_url { get; set; }

    public List<long> friends { get; set; }

    public int catalog_position { get; set; }

    public int international { get; set; }

    public int leaderboard_type { get; set; }

    public bool installed { get; set; }

    public int is_in_catalog { get; set; }
  }
}
