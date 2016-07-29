using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace XamlAnimatedGif
{
    public class UriLoader
    {
        private const string ROOT_FOLDER_NAME = "GifCache";

        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        private static async Task<Stream> GetStreamFromUriCoreAsync(Uri uri)
        {
            string scheme = uri.Scheme;
            if (!(scheme == "ms-appx") && !(scheme == "ms-appdata"))
            {
                if (!(scheme == "file"))
                    throw new NotSupportedException("Only ms-appx:, ms-appdata:, http:, https: and file: URIs are supported");
                return await (await StorageFile.GetFileFromPathAsync(uri.LocalPath)).OpenStreamForReadAsync();
            }
            return await (await StorageFile.GetFileFromApplicationUriAsync(uri)).OpenStreamForReadAsync();
        }

        private static async Task<Stream> OpenTempFileStreamAsync(string fileName)
        {
            IStorageFile file;
            try
            {
                file = (IStorageFile)await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", CreationCollisionOption.OpenIfExists)).GetFileAsync(fileName);
            }
            catch
            {
                return null;
            }
            return await file.OpenStreamForReadAsync();
        }

        private static async Task<Stream> CreateTempFileStreamAsync(string fileName)
        {
            return await ((IStorageFile)await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", (CreationCollisionOption)3)).CreateFileAsync(fileName, (CreationCollisionOption)1)).OpenStreamForWriteAsync();
        }

        private static async Task DeleteTempFileAsync(string fileName)
        {
            try
            {
                await (await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", CreationCollisionOption.OpenIfExists)).GetFileAsync(fileName)).DeleteAsync();
            }
            catch
            {
            }
        }

        private static async Task ClearCacheFolderAsync()
        {
            try
            {
                IEnumerator<StorageFile> enumerator = ((IEnumerable<StorageFile>)await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("GifCache", (CreationCollisionOption)3)).GetFilesAsync()).GetEnumerator();
                try
                {
                    while (((IEnumerator)enumerator).MoveNext())
                        await enumerator.Current.DeleteAsync();
                }
                finally
                {
                    if (enumerator != null)
                        ((IDisposable)enumerator).Dispose();
                }
                enumerator = null;
            }
            catch
            {
            }
        }

        private static string GetCacheFileName(Uri uri)
        {
            return uri.GetHashCode().ToString();
        }

        public Task<Stream> GetStreamFromUriAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (uri.Scheme == "http" || uri.Scheme == "https")
                return this.GetNetworkStreamAsync(uri, cancellationToken);
            return UriLoader.GetStreamFromUriCoreAsync(uri);
        }

        public async Task ClearCache()
        {
            await UriLoader.ClearCacheFolderAsync();
        }

        private async Task<Stream> GetNetworkStreamAsync(Uri uri, CancellationToken cancellationToken)
        {
            string cacheFileName = UriLoader.GetCacheFileName(uri);
            if (await UriLoader.OpenTempFileStreamAsync(cacheFileName) == null)
                await this.DownloadToCacheFileAsync(uri, cacheFileName, cancellationToken);
            return await UriLoader.OpenTempFileStreamAsync(cacheFileName);
        }

        private async Task DownloadToCacheFileAsync(Uri uri, string fileName, CancellationToken cancellationToken)
        {
            int num = 0;
            Exception obj = null;
            try
            {
                HttpClient client = new HttpClient();
                try
                {
                    Stream responseStream = (await client.GetBufferAsync(uri).AsTask<IBuffer, HttpProgress>(cancellationToken, (IProgress<HttpProgress>)new Progress<HttpProgress>((Action<HttpProgress>)(progress =>
                    {
                        ulong? nullable = (ulong?)progress.TotalBytesToReceive;
                        if (!nullable.HasValue)
                            return;
                        double percentage = Math.Round((double)progress.BytesReceived * 100.0 / (double)nullable.Value, 2);
                        EventHandler<DownloadProgressChangedArgs> eventHandler = this.DownloadProgressChanged;
                        if (eventHandler == null)
                            return;
                        DownloadProgressChangedArgs e = new DownloadProgressChangedArgs(uri, percentage);
                        eventHandler((object)this, e);
                    })))).AsStream();
                    try
                    {
                        Stream fileStream = await UriLoader.CreateTempFileStreamAsync(fileName);
                        try
                        {
                            await responseStream.CopyToAsync(fileStream);
                        }
                        finally
                        {
                            if (fileStream != null)
                                fileStream.Dispose();
                        }
                        fileStream = null;
                    }
                    finally
                    {
                        if (responseStream != null)
                            responseStream.Dispose();
                    }
                    responseStream = null;
                }
                finally
                {
                    if (client != null)
                        ((IDisposable)client).Dispose();
                }
                client = null;
            }
            catch (Exception ex)
            {
                obj = ex;
                num = 1;
            }
            if (num == 1)
            {
                await UriLoader.DeleteTempFileAsync(fileName);
                if (obj == null)//TODO: What????
                    throw obj;
                ExceptionDispatchInfo.Capture(obj).Throw();
            }
            //obj = null;
        }

        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter((Stream)memoryStream);
            string str = s;
            streamWriter.Write(str);
            streamWriter.Flush();
            long num = 0;
            memoryStream.Position = num;
            return (Stream)memoryStream;
        }
    }
}
