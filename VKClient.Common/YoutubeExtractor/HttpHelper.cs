using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
  internal static class HttpHelper
  {
    public static string DownloadString(string url)
    {
      WebRequest request = WebRequest.Create(url);
      request.Headers["User-Agent"] = "Mozilla/4.0";
      request.Method = "GET";
      return Task.Factory.FromAsync<WebResponse>(new Func<AsyncCallback, object, IAsyncResult>(request.BeginGetResponse), (Func<IAsyncResult, WebResponse>) (asyncResult => request.EndGetResponse(asyncResult)), null).ContinueWith<string>((Func<Task<WebResponse>, string>) (t => HttpHelper.ReadStreamFromResponse(t.Result))).Result;
    }

    public static string HtmlDecode(string value)
    {
      return WebUtility.HtmlDecode(value);
    }

    public static IDictionary<string, string> ParseQueryString(string s)
    {
      if (s.Contains("?"))
        s = s.Substring(s.IndexOf('?') + 1);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      foreach (string input in Regex.Split(s, "&"))
      {
        string[] strArray = Regex.Split(input, "=");
        dictionary.Add(strArray[0], strArray.Length == 2 ? HttpHelper.UrlDecode(strArray[1]) : string.Empty);
      }
      return (IDictionary<string, string>) dictionary;
    }

    public static string ReplaceQueryStringParameter(string currentPageUrl, string paramToReplace, string newValue)
    {
      IDictionary<string, string> queryString = HttpHelper.ParseQueryString(currentPageUrl);
      string index = paramToReplace;
      string str = newValue;
      queryString[index] = str;
      StringBuilder stringBuilder = new StringBuilder();
      bool flag = true;
      foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>) queryString)
      {
        if (!flag)
          stringBuilder.Append("&");
        stringBuilder.Append(keyValuePair.Key);
        stringBuilder.Append("=");
        stringBuilder.Append(keyValuePair.Value);
        flag = false;
      }
      return new UriBuilder(currentPageUrl)
      {
        Query = stringBuilder.ToString()
      }.ToString();
    }

    public static string UrlDecode(string url)
    {
      return WebUtility.UrlDecode(url);
    }

    private static string ReadStreamFromResponse(WebResponse response)
    {
      using (Stream responseStream = response.GetResponseStream())
      {
        using (StreamReader streamReader = new StreamReader(responseStream))
          return streamReader.ReadToEnd();
      }
    }
  }
}
