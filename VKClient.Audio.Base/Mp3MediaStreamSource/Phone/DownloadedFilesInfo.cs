using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mp3MediaStreamSource.Phone
{
  public class DownloadedFilesInfo : IReportFileChunkDownloaded
  {
    private Dictionary<string, List<FileDescription>> _filesMap = new Dictionary<string, List<FileDescription>>();
    private ManualResetEvent _waitHandle = new ManualResetEvent(false);
    private object _lockObj = new object();
    private static DownloadedFilesInfo _instance;

    public static DownloadedFilesInfo Instance
    {
      get
      {
        if (DownloadedFilesInfo._instance == null)
          DownloadedFilesInfo._instance = new DownloadedFilesInfo();
        return DownloadedFilesInfo._instance;
      }
    }

    public void ReportDownloadedFileChunk(string fileId, string filePath, long byteRangeBeginning, long byteRangeEnd, bool isWholeFile)
    {
      lock (this._lockObj)
      {
        if (!this._filesMap.ContainsKey(fileId))
          this._filesMap[fileId] = new List<FileDescription>();
        this._filesMap[fileId].Add(new FileDescription()
        {
          FileId = fileId,
          FilePath = filePath,
          FromByte = byteRangeBeginning,
          ToByte = byteRangeEnd,
          IsWholeFile = isWholeFile
        });
      }
      this._waitHandle.Set();
      this._waitHandle.Reset();
    }

    public List<FileDescription> GetCoverage(string fileId)
    {
      long offset = 0;
      List<FileDescription> fileDescriptionList = new List<FileDescription>();
      while (true)
      {
        FileDescription fileInDistFor = this.FindFileInDistFor(fileId, offset);
        if (fileInDistFor != null)
        {
          fileDescriptionList.Add(fileInDistFor);
          offset = fileInDistFor.ToByte + 1L;
        }
        else
          break;
      }
      return fileDescriptionList;
    }

    public long GetNextNotDownloadedSpot(string fileId, long offset)
    {
      long offset1 = offset;
      while (true)
      {
        FileDescription fileFor = DownloadedFilesInfo.Instance.GetFileFor(fileId, offset1, 0);
        if (fileFor != null)
          offset1 = fileFor.ToByte + 1L;
        else
          break;
      }
      return offset1;
    }

    public FileDescription GetFileFor(string fileId, long offset, int maxWaitTimeSeconds = 15)
    {
      FileDescription fileDescription = (FileDescription) null;
      if (maxWaitTimeSeconds > 0)
      {
        int num = 0;
        while (fileDescription == null && num < maxWaitTimeSeconds)
        {
          fileDescription = this.FindFileInDistFor(fileId, offset);
          if (fileDescription == null)
          {
            this._waitHandle.WaitOne(1000);
            ++num;
          }
        }
      }
      else
        fileDescription = this.FindFileInDistFor(fileId, offset);
      return fileDescription;
    }

    private FileDescription FindFileInDistFor(string fileId, long offset)
    {
      lock (this._lockObj)
      {
        if (!this._filesMap.ContainsKey(fileId))
          return (FileDescription) null;
        List<FileDescription> temp_15 = this._filesMap[fileId];
        List<FileDescription> local_3 = new List<FileDescription>();
        foreach (FileDescription item_0 in temp_15)
        {
          if (item_0.FromByte <= offset && item_0.ToByte >= offset)
            local_3.Add(item_0);
        }
        return local_3.OrderBy<FileDescription, long>((Func<FileDescription, long>) (fd => fd.ToByte - offset + (fd.IsWholeFile ? 1L : 0L))).LastOrDefault<FileDescription>();
      }
    }
  }
}
