using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
  public static class DownloadUrlResolver
  {
    private const string RateBypassFlag = "ratebypass";
    private const int CorrectSignatureLength = 81;
    private const string SignatureQuery = "signature";

    public static void DecryptDownloadUrl(VideoInfo videoInfo)
    {
      IDictionary<string, string> queryString = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);
      if (!queryString.ContainsKey("signature"))
        return;
      string signature = queryString["signature"];
      string decipheredSignature;
      try
      {
        decipheredSignature = DownloadUrlResolver.GetDecipheredSignature(videoInfo.HtmlPlayerVersion, signature);
      }
      catch (Exception ex)
      {
        throw new YoutubeParseException("Could not decipher signature", ex);
      }
      videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, "signature", decipheredSignature);
      videoInfo.RequiresDecryption = false;
    }

    public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
    {
      if (videoUrl == null)
        throw new ArgumentNullException("videoUrl");
      if (!DownloadUrlResolver.TryNormalizeYoutubeUrl(videoUrl, out videoUrl))
        throw new ArgumentException("URL is not a valid youtube URL!");
      try
      {
        JObject json = DownloadUrlResolver.LoadJson(videoUrl);
        string videoTitle = DownloadUrlResolver.GetVideoTitle(json);
        IEnumerable<VideoInfo> videoInfos = (IEnumerable<VideoInfo>) DownloadUrlResolver.GetVideoInfos(DownloadUrlResolver.ExtractDownloadUrls(json), videoTitle).ToList<VideoInfo>();
        string html5PlayerVersion = DownloadUrlResolver.GetHtml5PlayerVersion(json);
        foreach (VideoInfo videoInfo in videoInfos)
        {
          videoInfo.HtmlPlayerVersion = html5PlayerVersion;
          if (decryptSignature && videoInfo.RequiresDecryption)
            DownloadUrlResolver.DecryptDownloadUrl(videoInfo);
        }
        return videoInfos;
      }
      catch (Exception ex)
      {
        if (ex is WebException || ex is VideoNotAvailableException)
          throw;
        else
          DownloadUrlResolver.ThrowYoutubeParseException(ex, videoUrl);
      }
      return (IEnumerable<VideoInfo>) null;
    }

    public static Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl, bool decryptSignature = true)
    {
      return Task.Run<IEnumerable<VideoInfo>>((Func<IEnumerable<VideoInfo>>) (() => DownloadUrlResolver.GetDownloadUrls(videoUrl, decryptSignature)));
    }

    public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
    {
      url = url.Trim();
      url = url.Replace("youtu.be/", "youtube.com/watch?v=");
      url = url.Replace("www.youtube", "youtube");
      url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");
      if (url.Contains("/v/"))
        url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
      url = url.Replace("/watch#", "/watch?");
      string str;
      if (!HttpHelper.ParseQueryString(url).TryGetValue("v", out str))
      {
        normalizedUrl = null;
        return false;
      }
      normalizedUrl = "http://youtube.com/watch?v=" + str;
      return true;
    }

    private static IEnumerable<DownloadUrlResolver.ExtractionInfo> ExtractDownloadUrls(JObject json)
    {
      string[] strArray = ((IEnumerable<string>) DownloadUrlResolver.GetStreamMap(json).Split(',')).Concat<string>((IEnumerable<string>) DownloadUrlResolver.GetAdaptiveStreamMap(json).Split(',')).ToArray<string>();
      for (int index = 0; index < strArray.Length; ++index)
      {
        IDictionary<string, string> queryString = HttpHelper.ParseQueryString(strArray[index]);
        bool flag = false;
        string url;
        if (queryString.ContainsKey("s") || queryString.ContainsKey("sig"))
        {
          flag = queryString.ContainsKey("s");
          string str = queryString.ContainsKey("s") ? queryString["s"] : queryString["sig"];
          url = string.Format("{0}&{1}={2}", (object) queryString["url"], (object) "signature", (object) str) + (queryString.ContainsKey("fallback_host") ? "&fallback_host=" + queryString["fallback_host"] : string.Empty);
        }
        else
          url = queryString["url"];
        string str1 = HttpHelper.UrlDecode(HttpHelper.UrlDecode(url));
        if (!HttpHelper.ParseQueryString(str1).ContainsKey("ratebypass"))
          str1 += string.Format("&{0}={1}", (object) "ratebypass", (object) "yes");
        DownloadUrlResolver.ExtractionInfo extractionInfo = new DownloadUrlResolver.ExtractionInfo();
        extractionInfo.RequiresDecryption = flag;
        Uri uri = new Uri(str1);
        extractionInfo.Uri = uri;
        yield return extractionInfo;
      }
      strArray = (string[]) null;
    }

    private static string GetAdaptiveStreamMap(JObject json)
    {
      return (json["args"][(object) "adaptive_fmts"] ?? json["args"][(object) "url_encoded_fmt_stream_map"]).ToString();
    }

    private static string GetDecipheredSignature(string htmlPlayerVersion, string signature)
    {
      if (signature.Length == 81)
        return signature;
      return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
    }

    private static string GetHtml5PlayerVersion(JObject json)
    {
      return new Regex("player-(.+?).js").Match(json["assets"][(object) "js"].ToString()).Result("$1");
    }

    private static string GetStreamMap(JObject json)
    {
      JToken jtoken = json["args"][(object) "url_encoded_fmt_stream_map"];
      string str = jtoken == null ? null : jtoken.ToString();
      if (str == null || str.Contains("been+removed"))
        throw new VideoNotAvailableException("Video is removed or has an age restriction.");
      return str;
    }

    private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<DownloadUrlResolver.ExtractionInfo> extractionInfos, string videoTitle)
    {
      List<VideoInfo> videoInfoList = new List<VideoInfo>();
      foreach (DownloadUrlResolver.ExtractionInfo extractionInfo in extractionInfos)
      {
        int formatCode = int.Parse(HttpHelper.ParseQueryString(extractionInfo.Uri.Query)["itag"]);
        VideoInfo info = VideoInfo.Defaults.SingleOrDefault<VideoInfo>((Func<VideoInfo, bool>) (videoInfo => videoInfo.FormatCode == formatCode));
        VideoInfo videoInfo1;
        if (info != null)
        {
          VideoInfo videoInfo2 = new VideoInfo(info);
          videoInfo2.DownloadUrl = extractionInfo.Uri.ToString();
          videoInfo2.Title = videoTitle;
          int num = extractionInfo.RequiresDecryption ? 1 : 0;
          videoInfo2.RequiresDecryption = num != 0;
          videoInfo1 = videoInfo2;
        }
        else
          videoInfo1 = new VideoInfo(formatCode)
          {
            DownloadUrl = extractionInfo.Uri.ToString()
          };
        videoInfoList.Add(videoInfo1);
      }
      return (IEnumerable<VideoInfo>) videoInfoList;
    }

    private static string GetVideoTitle(JObject json)
    {
      JToken jtoken = json["args"][(object) "title"];
      if (jtoken != null)
        return jtoken.ToString();
      return string.Empty;
    }

    private static bool IsVideoUnavailable(string pageSource)
    {
      return pageSource.Contains("<div id=\"watch-player-unavailable\">");
    }

    private static JObject LoadJson(string url)
    {
      string str = HttpHelper.DownloadString(url);
      if (DownloadUrlResolver.IsVideoUnavailable(str))
        throw new VideoNotAvailableException();
      return JObject.Parse(new Regex("ytplayer\\.config\\s*=\\s*(\\{.+?\\});", RegexOptions.Multiline).Match(str).Result("$1"));
    }

    private static void ThrowYoutubeParseException(Exception innerException, string videoUrl)
    {
      throw new YoutubeParseException("Could not parse the Youtube page for URL " + videoUrl + "\nThis may be due to a change of the Youtube page structure.\nPlease report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
    }

    private class ExtractionInfo
    {
      public bool RequiresDecryption { get; set; }

      public Uri Uri { get; set; }
    }
  }
}
