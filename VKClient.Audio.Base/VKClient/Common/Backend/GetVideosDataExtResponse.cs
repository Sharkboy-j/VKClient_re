using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class GetVideosDataExtResponse
  {
    public VKList<VKClient.Common.Backend.DataObjects.Video> AddedVideos { get; set; }

    public VKList<VKClient.Common.Backend.DataObjects.Video> UploadedVideos { get; set; }

    public VKList<VideoAlbum> Albums { get; set; }

    public User User { get; set; }

    public Group Group { get; set; }
  }
}
