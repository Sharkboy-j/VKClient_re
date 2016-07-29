using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class VideoListWithCount
  {
    public int VideosCount { get; set; }

    public List<VKClient.Common.Backend.DataObjects.Video> response { get; set; }
  }
}
