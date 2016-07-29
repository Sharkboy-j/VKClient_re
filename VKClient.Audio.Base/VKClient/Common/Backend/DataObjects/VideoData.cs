using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class VideoData
  {
    public List<VKClient.Common.Backend.DataObjects.Video> AllVideos { get; set; }

    public List<VideoAlbum> AllAlbums { get; set; }

    public int AlbumsCount { get; set; }

    public int VideosCount { get; set; }
  }
}
