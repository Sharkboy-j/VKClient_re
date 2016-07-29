using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
    public class AudioService
    {
        private static readonly Func<string, List<AudioObj>> _deserializeAudioList = new Func<string, List<AudioObj>>(jsonStr => { return JsonConvert.DeserializeObject<GenericRoot<VKList<AudioObj>>>(jsonStr).response.items; });
        private Dictionary<string, string> _cachedResults = new Dictionary<string, string>();
        private static AudioService _instance;

        public static AudioService Instance
        {
            get
            {
                return AudioService._instance ?? (AudioService._instance = new AudioService());
            }
        }

        private AudioService()
        {
        }

        public void GetLyrics(long lyrics_id, Action<BackendResult<Lyrics, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["lyrics_id"] = lyrics_id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<Lyrics>("audio.getLyrics", parameters, callback, (Func<string, Lyrics>)null, false, true, new CancellationToken?());
        }

        public void GetAllTracksAndAlbums(long userOrGroupId, bool isGroup, Action<BackendResult<AudioData, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string str = string.Format("var allTracks = API.audio.get({{{0}:\"{1}\", count:\"{2}\", offset:\"0\"}}).items;\r\n                                            var allAlbums = API.audio.getAlbums({{{0}:\"{1}\", count:\"{3}\", offset:\"0\"}}).items;\r\n                                            return {{\"allTracks\":allTracks, \"allAlbums\":allAlbums}};", (object)(isGroup ? "group_id" : "user_id"), (object)userOrGroupId, (object)VKConstants.TracksReadCount.ToString(), (object)VKConstants.AlbumsReadCount.ToString());
            parameters["code"] = str;
            VKRequestsDispatcher.DispatchRequestToVK<AudioData>("execute", parameters, callback, (Func<string, AudioData>)(jsonStr =>
            {
                AudioData audioData = new AudioData();
                int resultCount;
                jsonStr = VKRequestsDispatcher.GetArrayCountAndRemove(jsonStr, "allAlbums", out resultCount);
                AudioData response = JsonConvert.DeserializeObject<AudioDataRoot>(jsonStr).response;
                int num = resultCount;
                response.AllAlbumsCount = num;
                return response;
            }), false, true, new CancellationToken?());
        }

        public void MoveToAlbum(List<long> aids, long albumId, Action<BackendResult<object, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audio_ids"] = aids.GetCommaSeparated();
            parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("audio.moveToAlbum", parameters, callback, (Func<string, object>)null, false, true, new CancellationToken?());
        }

        public void EditAlbum(long albumId, string albumName, Action<BackendResult<object, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = albumName;
            parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("audio.editAlbum", parameters, callback, (Func<string, object>)null, false, true, new CancellationToken?());
        }

        public void DeleteAlbum(long albumId, Action<BackendResult<object, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("audio.deleteAlbum", parameters, callback, (Func<string, object>)null, false, true, new CancellationToken?());
        }

        public void CreateAlbum(string albumName, Action<BackendResult<AudioAlbum, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = albumName;
            VKRequestsDispatcher.DispatchRequestToVK<AudioAlbum>("audio.addAlbum", parameters, callback, (Func<string, AudioAlbum>)null, false, true, new CancellationToken?());
        }

        public void GetAllAudio(Action<BackendResult<List<AudioObj>, ResultCode>> callback, long? userOrGroupId = null, bool isGroup = false, long? albumId = null, int offset = 0, int count = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userOrGroupId.HasValue && !isGroup)
                parameters["user_id"] = userOrGroupId.Value.ToString();
            if (userOrGroupId.HasValue & isGroup)
                parameters["group_id"] = userOrGroupId.Value.ToString();
            if (albumId.HasValue)
                parameters["album_id"] = albumId.Value.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count == 0 ? VKConstants.TracksReadCount.ToString() : count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<AudioObj>>("audio.get", parameters, callback, AudioService._deserializeAudioList, false, true, new CancellationToken?());
        }

        public void GetRecommended(long uid, long offset, long count, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = uid.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<AudioObj>>("audio.getRecommendations", parameters, callback, AudioService._deserializeAudioList, false, true, new CancellationToken?());
        }

        public void GetPopular(int offset, int count, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<AudioObj>>("audio.getPopular", parameters, callback, (Func<string, List<AudioObj>>)null, false, true, new CancellationToken?());
        }

        public void GetUserAlbums(Action<BackendResult<ListResponse<AudioAlbum>, ResultCode>> callback, long? userOrGroupId = null, bool isGroup = false, int offset = 0, int count = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userOrGroupId.HasValue && !isGroup)
                parameters["user_id"] = userOrGroupId.Value.ToString();
            if (userOrGroupId.HasValue & isGroup)
                parameters["group_id"] = userOrGroupId.Value.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count == 0 ? VKConstants.AlbumsReadCount.ToString() : count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<ListResponse<AudioAlbum>>("audio.getAlbums", parameters, callback, (Func<string, ListResponse<AudioAlbum>>)(jsonStr =>
            {
                GenericRoot<VKList<AudioAlbum>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<VKList<AudioAlbum>>>(jsonStr);
                return new ListResponse<AudioAlbum>()
                {
                    Data = genericRoot.response.items,
                    TotalCount = genericRoot.response.count
                };
            }), false, true, new CancellationToken?());
        }

        public void GetAllAudioForUser(long uid, long guid, long albumId, List<long> aids, int count, int offset, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<List<AudioObj>>("audio.get", new Dictionary<string, string>(), callback, AudioService._deserializeAudioList, false, true, new CancellationToken?());
        }

        public void SearchTracks(string query, int offset, int count, Action<BackendResult<VKList<AudioObj>, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = query;
            parameters["count"] = count.ToString();
            parameters["offset"] = offset.ToString();
            parameters["search_own"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKList<AudioObj>>("audio.search", parameters, callback, (Func<string, VKList<AudioObj>>)null, false, true, new CancellationToken?());
        }

        public void GetAudio(List<string> ids, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audios"] = ids.GetCommaSeparated(",");
            VKRequestsDispatcher.DispatchRequestToVK<List<AudioObj>>("audio.getById", parameters, callback, (Func<string, List<AudioObj>>)null, false, true, new CancellationToken?());
        }

        public void AddAudio(long ownerId, long aid, Action<BackendResult<long, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["audio_id"] = aid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<long>("audio.add", parameters, callback, (Func<string, long>)null, false, true, new CancellationToken?());
        }

        public void DeleteAudios(List<long> list)
        {
            string format = "API.audio.delete({{ \"audio_id\":{0}, \"owner_id\":{1} }});";
            long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
            string str = "";
            foreach (long num in list)
                str = str + string.Format(format, (object)num, (object)loggedInUserId) + Environment.NewLine;
            string code = str;
            Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { });
            CancellationToken? cancellationToken = new CancellationToken?();
            VKRequestsDispatcher.Execute<ResponseWithId>(code, callback, (Func<string, ResponseWithId>)(jsonStr => new ResponseWithId()), false, true, cancellationToken);
        }

        public void ReorderAudio(long aid, long oid, long album_id, long after, long before, Action<BackendResult<long, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audio_id"] = aid.ToString();
            if (oid != 0L)
                parameters["owner_id"] = oid.ToString();
            if (album_id != 0L)
                parameters["album_id"] = album_id.ToString();
            parameters["after"] = after.ToString();
            parameters["before"] = before.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<long>("audio.reorder", parameters, callback, (Func<string, long>)null, false, true, new CancellationToken?());
        }

        public void StatusSet(string text, string audio, Action<BackendResult<long, ResultCode>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(audio))
            {
                string str = string.Format("API.audio.setBroadcast({{\"audio\":\"{0}\"}});", (object)audio);
                parameters["code"] = str;
                VKRequestsDispatcher.DispatchRequestToVK<long>("execute", parameters, callback, (Func<string, long>)(res => 1L), false, true, new CancellationToken?());
            }
            else
            {
                parameters["text"] = text;
                VKRequestsDispatcher.DispatchRequestToVK<long>("status.set", parameters, callback, (Func<string, long>)null, false, true, new CancellationToken?());
            }
        }

        public void ResetBroadcast(Action<BackendResult<long, ResultCode>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<long>("audio.setBroadcast", new Dictionary<string, string>(), callback, (Func<string, long>)(res => 1L), false, true, new CancellationToken?());
        }

        public void GetAlbumArtwork(string search, Action<BackendResult<AlbumArtwork, ResultCode>> callback)
        {
            string format = "https://itunes.apple.com/search?media=music&limit=1&version=2&term={0}";
            search = HttpUtility.UrlEncode(search);
            if (this._cachedResults.ContainsKey(search))
                callback(new BackendResult<AlbumArtwork, ResultCode>(ResultCode.Succeeded, new AlbumArtwork()
                {
                    ImageUri = this._cachedResults[search]
                }));
            else
                JsonWebRequest.SendHTTPRequestAsync(string.Format(format, (object)search), (Action<JsonResponseData>)(resp =>
                {
                    if (resp.IsSucceeded)
                    {
                        try
                        {
                            AudioService.ItunesList itunesList = JsonConvert.DeserializeObject<AudioService.ItunesList>(resp.JsonString);
                            if (!itunesList.results.IsNullOrEmpty())
                            {
                                AudioService.ItunesAlbumArt itunesAlbumArt = itunesList.results[0];
                                AlbumArtwork albArt = new AlbumArtwork()
                                {
                                    ImageUri = itunesAlbumArt.artworkUrl100.Replace("100x100", "600x600")
                                };
                                Execute.ExecuteOnUIThread((Action)(() =>
                                {
                                    this._cachedResults[search] = albArt.ImageUri;
                                    callback(new BackendResult<AlbumArtwork, ResultCode>(ResultCode.Succeeded, albArt));
                                }));
                            }
                            else
                                callback(new BackendResult<AlbumArtwork, ResultCode>(ResultCode.Succeeded, new AlbumArtwork()));
                        }
                        catch
                        {
                            callback(new BackendResult<AlbumArtwork, ResultCode>(ResultCode.UnknownError));
                        }
                    }
                    else
                        callback(new BackendResult<AlbumArtwork, ResultCode>(ResultCode.CommunicationFailed));
                }), (Dictionary<string, object>)null);
        }

        private class ItunesAlbumArt
        {
            public string artworkUrl100 { get; set; }
        }

        private class ItunesList
        {
            public int resultCount { get; set; }

            public List<AudioService.ItunesAlbumArt> results { get; set; }
        }
    }
}
