using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Core;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
    public class JsonWebRequest
    {
        private static DelayedExecutorWithQueue _lowPriorityQueue = new DelayedExecutorWithQueue(200, new Func<DelayedExecutorWithQueue.ExecutionInfo, bool>(ei => { return JsonWebRequest._currentNumberOfRequests <= 1 || (DateTime.Now - ei.TimestampAdded).TotalMilliseconds > 2000.0; }));
        private static DelayedExecutorWithQueue _pageDataDelayedQueue = new DelayedExecutorWithQueue(1000, new Func<DelayedExecutorWithQueue.ExecutionInfo, bool>(ei => { return true; }));
        public static Func<IPageDataRequesteeInfo> GetCurrentPageDataRequestee = new Func<IPageDataRequesteeInfo>(() => { return null; });
        private const int BUFFER_SIZE = 5000;
        private static int _currentNumberOfRequests;

        public static void GetHttpStatusCode(string url, Action<HttpStatusCode> callback)
        {
            HttpStatusCode result = (HttpStatusCode)0;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.BeginGetResponse((AsyncCallback)(asyncRes =>
            {
                try
                {
                    result = ((HttpWebResponse)request.EndGetResponse(asyncRes)).StatusCode;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                        result = (ex.Response as HttpWebResponse).StatusCode;
                }
                catch
                {
                }
                callback(result);
            }), null);
        }

        public static void Download(string uri, Stream destinationStream, Action<bool> resultCallback, Action<double> progressCallback, Cancellation c)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                resultCallback(false);
            }
            else
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowReadStreamBuffering = false;
                request.BeginGetResponse((AsyncCallback)(asyncRes =>
                {
                    bool flag = true;
                    try
                    {
                        HttpWebResponse httpWebResponse = (HttpWebResponse)request.EndGetResponse(asyncRes);
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                            StreamUtils.CopyStream(responseStream, destinationStream, progressCallback, c, httpWebResponse.ContentLength);
                    }
                    catch
                    {
                        flag = false;
                    }
                    resultCallback(flag);
                }), null);
            }
        }

        public static void Download(string uri, long fromByte, long toByte, Action<HttpStatusCode, long, byte[]> resultCallback)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            if (fromByte > 0L || toByte > 0L)
                request.Headers["range"] = "bytes=" + (object)fromByte + "-" + (object)toByte;
            request.BeginGetResponse((AsyncCallback)(asyncRes =>
            {
                byte[] numArray = (byte[])null;
                HttpStatusCode httpStatusCode = HttpStatusCode.ServiceUnavailable;
                long num = 0;
                try
                {
                    HttpWebResponse httpWebResponse = (HttpWebResponse)request.EndGetResponse(asyncRes);
                    httpStatusCode = httpWebResponse.StatusCode;
                    if (((IEnumerable<string>)httpWebResponse.Headers.AllKeys).Contains<string>("Content-Range"))
                        num = JsonWebRequest.ReadContentLengthFromHeaderValue(httpWebResponse.Headers["Content-Range"]);
                    using (Stream responseStream = httpWebResponse.GetResponseStream())
                    {
                        if (num == 0L)
                            num = responseStream.Length;
                        numArray = StreamUtils.ReadFullyToByteArray(responseStream);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                        httpStatusCode = (ex.Response as HttpWebResponse).StatusCode;
                }
                catch
                {
                }
                resultCallback(httpStatusCode, num, numArray);
            }), null);
        }

        public static long ReadContentLengthFromHeaderValue(string headerContentRange)
        {
            return long.Parse(headerContentRange.Split('/')[1]);
        }

        public static void SendHTTPRequestAsync(string baseUri, Dictionary<string, string> parameters, Action<JsonResponseData> resultCallback, bool usePost = true, bool lowPriority = false, bool pageDataRequest = true)
        {
            if (lowPriority)
            {
                Action<Action> action = (Action<Action>)(a => JsonWebRequest.SendHTTPRequestAsync(baseUri, parameters, (Action<JsonResponseData>)(res =>
                {
                    a();
                    resultCallback(res);
                }), usePost, false, true));
                JsonWebRequest._lowPriorityQueue.AddToDelayedExecutionQueue(action, baseUri);
            }
            else
            {
                Logger.Instance.Info(">>>>>>>>>>>>>>>Starting GETAsync concurrentRequestsNo = {0} ; baseUri = {1}; parameters = {2}", (object)JsonWebRequest._currentNumberOfRequests, (object)baseUri, (object)JsonWebRequest.GetAsLogString(parameters));
                Interlocked.Increment(ref JsonWebRequest._currentNumberOfRequests);
                string queryString = JsonWebRequest.ConvertDictionaryToQueryString(parameters, true);
                JsonWebRequest.RequestState myRequestState = new JsonWebRequest.RequestState();
                try
                {
                    myRequestState.resultCallback = resultCallback;
                    string requestUriString = baseUri;
                    if (!usePost && queryString.Length > 0)
                        requestUriString = requestUriString + "?" + queryString;
                    //
                    //

                    //Logger.Instance.Error("WTF " + baseUri + " " + queryString);

                    //
                    //
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                    myHttpWebRequest.UserAgent = AppInfo.AppVersionForUserAgent;
                    myRequestState.request = myHttpWebRequest;
                    if (usePost)
                    {
                        myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                        myHttpWebRequest.Method = "POST";
                        myHttpWebRequest.BeginGetRequestStream((AsyncCallback)(ar =>
                        {
                            using (StreamWriter streamWriter = new StreamWriter(myHttpWebRequest.EndGetRequestStream(ar)))
                                streamWriter.Write(queryString);
                            myHttpWebRequest.BeginGetCompressedResponse(new AsyncCallback(JsonWebRequest.RespCallback), (object)myRequestState);
                        }), null);
                    }
                    else
                        myHttpWebRequest.BeginGetCompressedResponse(new AsyncCallback(JsonWebRequest.RespCallback), (object)myRequestState);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("GetJsonAsync failed.", ex);
                    JsonWebRequest.SafeClose(myRequestState);
                    JsonWebRequest.SafeInvokeCallback(myRequestState.resultCallback, false, null);
                }
            }
        }

        public static void Upload(string uri, Stream data, string paramName, string uploadContentType, Action<JsonResponseData> resultCallback, string fileName = null, Action<double> progressCallback = null, Cancellation c = null)
        {
            JsonWebRequest.RequestState rState = new JsonWebRequest.RequestState();
            rState.resultCallback = resultCallback;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowWriteStreamBuffering = false;
                request.UserAgent = AppInfo.AppVersionForUserAgent;
                rState.request = request;
                request.Method = "POST";
                string str1 = string.Format("----------{0:N}", (object)Guid.NewGuid());
                string str2 = "multipart/form-data; boundary=" + str1;
                request.ContentType = str2;
                request.CookieContainer = new CookieContainer();
                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", (object)str1, (object)paramName, (object)(fileName ?? "myDataFile"), (object)uploadContentType);
                string footer = "\r\n--" + str1 + "--\r\n";
                request.ContentLength = (long)Encoding.UTF8.GetByteCount(header) + data.Length + (long)Encoding.UTF8.GetByteCount(footer);
                request.BeginGetRequestStream((AsyncCallback)(ar =>
                {
                    try
                    {
                        Stream requestStream = request.EndGetRequestStream(ar);
                        requestStream.Write(Encoding.UTF8.GetBytes(header), 0, Encoding.UTF8.GetByteCount(header));
                        StreamUtils.CopyStream(data, requestStream, progressCallback, c, 0L);
                        requestStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));
                        requestStream.Close();
                        request.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), (object)rState);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error("Upload failed to write data to request stream.", ex);
                        JsonWebRequest.SafeClose(rState);
                        JsonWebRequest.SafeInvokeCallback(rState.resultCallback, false, null);
                    }
                }), null);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Upload failed.", ex);
                JsonWebRequest.SafeClose(rState);
                JsonWebRequest.SafeInvokeCallback(rState.resultCallback, false, null);
            }
        }

        public static void SendHTTPRequestAsync(string uri, Action<JsonResponseData> resultCallback, Dictionary<string, object> postData = null)
        {
            Logger.Instance.Info("Starting GetJsonAsync for uri = {0}", (object)uri);
            Interlocked.Increment(ref JsonWebRequest._currentNumberOfRequests);
            JsonWebRequest.RequestState myRequestState = new JsonWebRequest.RequestState();
            try
            {
                myRequestState.resultCallback = resultCallback;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                myHttpWebRequest.UserAgent = AppInfo.AppVersionForUserAgent;
                myRequestState.request = myHttpWebRequest;
                if (postData != null)
                {
                    string boundary = string.Format("----------{0:N}", (object)Guid.NewGuid());
                    string str = "multipart/form-data; boundary=" + boundary;
                    byte[] formData = JsonWebRequest.GetMultipartFormData(postData, boundary);
                    myHttpWebRequest.Method = "POST";
                    myHttpWebRequest.ContentType = str;
                    myHttpWebRequest.CookieContainer = new CookieContainer();
                    myHttpWebRequest.BeginGetRequestStream((AsyncCallback)(ar =>
                    {
                        Stream requestStream = myHttpWebRequest.EndGetRequestStream(ar);
                        byte[] buffer = formData;
                        int offset = 0;
                        int length = formData.Length;
                        requestStream.Write(buffer, offset, length);
                        requestStream.Close();
                        myHttpWebRequest.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), (object)myRequestState);
                    }), null);
                }
                else
                    myHttpWebRequest.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), (object)myRequestState);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("GetJsonAsync failed.", ex);
                JsonWebRequest.SafeClose(myRequestState);
                JsonWebRequest.SafeInvokeCallback(myRequestState.resultCallback, false, null);
            }
        }

        public static string ConvertDictionaryToQueryString(Dictionary<string, string> parameters, bool escapeString)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                if (parameter.Key != null && parameter.Value != null)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder = stringBuilder.Append("&");
                    string str = escapeString ? Uri.EscapeDataString(parameter.Value) : parameter.Value;
                    stringBuilder = stringBuilder.AppendFormat("{0}={1}", parameter.Key, str);
                }
            }
            return stringBuilder.ToString();
        }

        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            JsonWebRequest.RequestState state = (JsonWebRequest.RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest httpWebRequest = state.request;
                state.response = (HttpWebResponse)httpWebRequest.EndGetResponse(asynchronousResult);
                Stream compressedResponseStream = state.response.GetCompressedResponseStream();
                state.streamResponse = compressedResponseStream;
                compressedResponseStream.BeginRead(state.BufferRead, 0, 5000, new AsyncCallback(JsonWebRequest.ReadCallBack), (object)state);
            }
            catch (WebException ex1)
            {
                Logger.Instance.Error(string.Format("RespCallback failed. Got httpWebResponse = {0} , uri = {1}", (object)(ex1.Response is HttpWebResponse), (object)state.request.RequestUri), (Exception)ex1);
                if (ex1.Response is HttpWebResponse && ex1.Response.ContentLength > 0L)
                {
                    WebResponse response = ex1.Response;
                    state.response = ex1.Response as HttpWebResponse;
                    try
                    {
                        Stream compressedResponseStream = state.response.GetCompressedResponseStream();
                        state.streamResponse = compressedResponseStream;
                        compressedResponseStream.BeginRead(state.BufferRead, 0, 5000, new AsyncCallback(JsonWebRequest.ReadCallBack), (object)state);
                    }
                    catch (Exception ex2)
                    {
                        Logger.Instance.Error(string.Format("RespCallback failed. Uri ={0}", (object)state.request.RequestUri), ex2);
                        JsonWebRequest.SafeClose(state);
                        JsonWebRequest.SafeInvokeCallback(state.resultCallback, false, null);
                    }
                }
                else
                {
                    JsonWebRequest.SafeClose(state);
                    JsonWebRequest.SafeInvokeCallback(state.resultCallback, false, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(string.Format("RespCallback failed. Uri ={0}", (object)state.request.RequestUri), ex);
                JsonWebRequest.SafeClose(state);
                JsonWebRequest.SafeInvokeCallback(state.resultCallback, false, null);
            }
        }

        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            JsonWebRequest.RequestState state = (JsonWebRequest.RequestState)asyncResult.AsyncState;
            try
            {
                Stream stream = state.streamResponse;
                int count = stream.EndRead(asyncResult);
                if (count > 0)
                {
                    state.readBytes.AddRange(((IEnumerable<byte>)state.BufferRead).Take<byte>(count));
                    stream.BeginRead(state.BufferRead, 0, 5000, new AsyncCallback(JsonWebRequest.ReadCallBack), (object)state);
                }
                else
                {
                    string @string = Encoding.UTF8.GetString(state.readBytes.ToArray(), 0, state.readBytes.Count);
                    JsonWebRequest.SafeClose(state);
                    Logger.Instance.Info("<<<<<<<<<<<<JSONWebRequest duration {0} ms. URI {1} ---->>>>> {2}", (object)(DateTime.Now - state.startTime).TotalMilliseconds, (object)state.request.RequestUri.OriginalString, (object)@string);
                    JsonWebRequest.SafeInvokeCallback(state.resultCallback, true, @string);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("ReadCallBack failed.", ex);
                JsonWebRequest.SafeClose(state);
                JsonWebRequest.SafeInvokeCallback(state.resultCallback, false, null);
            }
        }

        private static void SafeClose(JsonWebRequest.RequestState state)
        {
            try
            {
                if (state.streamResponse != null)
                    state.streamResponse.Close();
                if (state.response == null)
                    return;
                state.response.Close();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SafeClose failed.", ex);
            }
        }

        private static void SafeInvokeCallback(Action<JsonResponseData> action, bool p, string stringContent)
        {
            Interlocked.Decrement(ref JsonWebRequest._currentNumberOfRequests);
            Logger.Instance.Info("JsonWebRequest currentNumberOfRequests " + (object)JsonWebRequest._currentNumberOfRequests);
            try
            {
                action(new JsonResponseData(p, stringContent));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SafeInvokeCallback failed.", ex);
            }
        }

        private static string GetAsLogString(Dictionary<string, string> parameters)
        {
            string str = "";
            foreach (KeyValuePair<string, string> parameter in parameters)
                str = str + parameter.Key + " = " + parameter.Value + Environment.NewLine;
            return str;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream stream = (Stream)new MemoryStream();
            bool flag = false;
            Encoding utF8 = Encoding.UTF8;
            foreach (KeyValuePair<string, object> postParameter in postParameters)
            {
                if (flag)
                    stream.Write(utF8.GetBytes("\r\n"), 0, utF8.GetByteCount("\r\n"));
                flag = true;
                if (postParameter.Value is JsonWebRequest.FileParameter)
                {
                    JsonWebRequest.FileParameter fileParameter = (JsonWebRequest.FileParameter)postParameter.Value;
                    string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", (object)boundary, (object)postParameter.Key, (object)(fileParameter.FileName ?? postParameter.Key), (object)(fileParameter.ContentType ?? "application/octet-stream"));
                    stream.Write(utF8.GetBytes(s), 0, utF8.GetByteCount(s));
                    stream.Write(fileParameter.File, 0, fileParameter.File.Length);
                }
                else
                {
                    string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", (object)boundary, (object)postParameter.Key, postParameter.Value);
                    stream.Write(utF8.GetBytes(s), 0, utF8.GetByteCount(s));
                }
            }
            string s1 = "\r\n--" + boundary + "--\r\n";
            stream.Write(utF8.GetBytes(s1), 0, utF8.GetByteCount(s1));
            stream.Position = 0L;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return buffer;
        }

        private class RequestState
        {
            public StringBuilder requestData;
            public List<byte> readBytes;
            public byte[] BufferRead;
            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public Action<JsonResponseData> resultCallback;
            public DateTime startTime;

            public RequestState()
            {
                this.BufferRead = new byte[5000];
                this.requestData = new StringBuilder("");
                this.readBytes = new List<byte>(1024);
                this.request = (HttpWebRequest)null;
                this.streamResponse = null;
                this.startTime = DateTime.Now;
            }
        }

        public class FileParameter
        {
            public byte[] File { get; set; }

            public string FileName { get; set; }

            public string ContentType { get; set; }

            public FileParameter(byte[] file)
                : this(file, null)
            {
            }

            public FileParameter(byte[] file, string filename)
                : this(file, filename, null)
            {
            }

            public FileParameter(byte[] file, string filename, string contenttype)
            {
                this.File = file;
                this.FileName = filename;
                this.ContentType = contenttype;
            }
        }
    }
}
