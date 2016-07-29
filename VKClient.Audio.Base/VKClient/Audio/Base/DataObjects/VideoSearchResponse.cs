using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class VideoSearchResponse
  {
    public List<VKClient.Common.Backend.DataObjects.Video> MyVideos { get; set; }

    public List<VKClient.Common.Backend.DataObjects.Video> GlobalVideos { get; set; }

    public int TotalCount { get; set; }
  }
}
