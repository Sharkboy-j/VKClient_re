using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Threading.Tasks;
using VKClient.Common.Utils;
using Windows.Storage;

namespace VKClient.Common.Framework
{
  public static class CacheManager
  {
    private static string _cacheFolderName = "CachedDataV4";
    private static string _stateFolderName = "CachedData";

    public static void EnsureCacheFolderExists()
    {
      using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
      {
        if (!storeForApplication.DirectoryExists(CacheManager._cacheFolderName))
          storeForApplication.CreateDirectory(CacheManager._cacheFolderName);
        if (storeForApplication.DirectoryExists(CacheManager._stateFolderName))
          return;
        storeForApplication.CreateDirectory(CacheManager._stateFolderName);
      }
    }

    public static void EraseAll()
    {
      IsolatedStorageFile.GetUserStoreForApplication().Remove();
    }

    public static string GetFilePath(string fileId, CacheManager.DataType dataType)
    {
      return CacheManager.GetFolderNameForDataType(dataType) + "/" + fileId;
    }

    public static string TrySerializeToString(IBinarySerializable obj)
    {
      try
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          BinaryWriter writer = new BinaryWriter((Stream) memoryStream);
          obj.Write(writer);
          memoryStream.Position = 0L;
          return CacheManager.AsciiToString(new BinaryReader((Stream) memoryStream).ReadBytes((int) memoryStream.Length));
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("TrySerializeToString.TryDeserialize failed.", ex);
      }
      return "";
    }

    public static void TryDeserializeFromString(IBinarySerializable obj, string serStr)
    {
      try
      {
        using (MemoryStream memoryStream = new MemoryStream(CacheManager.StringToAscii(serStr)))
        {
          BinaryReader reader = new BinaryReader((Stream) memoryStream);
          obj.Read(reader);
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("TrySerializeToString.TryDeserialize failed.", ex);
      }
    }

    public static byte[] StringToAscii(string s)
    {
      byte[] numArray = new byte[s.Length];
      for (int index = 0; index < s.Length; ++index)
      {
        char ch = s[index];
        numArray[index] = (int) ch > (int) sbyte.MaxValue ? (byte) 63 : (byte) ch;
      }
      return numArray;
    }

    public static string AsciiToString(byte[] bytes)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (byte @byte in bytes)
        stringBuilder = stringBuilder.Append((char) @byte);
      return stringBuilder.ToString();
    }

    public static bool TryDeserialize(IBinarySerializable obj, string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
    {
      try
      {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          string filePath = CacheManager.GetFilePath(fileId, dataType);
          if (!storeForApplication.FileExists(filePath))
            return false;
          using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Open, FileAccess.Read))
          {
            BinaryReader reader = new BinaryReader((Stream) storageFileStream);
            obj.Read(reader);
          }
        }
        stopwatch.Stop();
        Logger.Instance.Info("CacheManager.TryDeserialize succeeded for fileId = {0}, in {1} ms.", (object) fileId, (object) stopwatch.ElapsedMilliseconds);
        return true;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TryDeserialize failed.", ex);
      }
      return false;
    }

    public static async Task<bool> TryDeserializeAsync(IBinarySerializable obj, string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
    {
      try
      {
        StorageFolder folderAsync = await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(dataType));
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          string filePath = CacheManager.GetFilePath(fileId, dataType);
          if (!storeForApplication.FileExists(filePath))
            return false;
        }
        Stream input = await ((IStorageFolder) folderAsync).OpenStreamForReadAsync(fileId);
        obj.Read(new BinaryReader(input));
        input.Close();
        return true;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TryDeserializeAsync failed.", ex);
        return false;
      }
    }

    public static async Task<bool> TryDeleteAsync(string fileId)
    {
      try
      {
        await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(CacheManager.DataType.CachedData))).GetFileAsync(fileId)).DeleteAsync();
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TryDeleteAsync failed. File Id = " + fileId, ex);
        return false;
      }
      return true;
    }

    public static string GetFolderNameForDataType(CacheManager.DataType dataType)
    {
      if (dataType == CacheManager.DataType.CachedData)
        return CacheManager._cacheFolderName;
      if (dataType == CacheManager.DataType.StateData)
        return CacheManager._stateFolderName;
      throw new Exception("Unknown data type");
    }

    public static async Task<bool> TrySerializeAsync(IBinarySerializable obj, string fileId, bool trim = false, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
    {
      try
      {
        Stream output = await ((IStorageFolder) await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(dataType))).OpenStreamForWriteAsync(fileId, (CreationCollisionOption) 1);
        BinaryWriter writer = new BinaryWriter(output);
        if (trim && obj is IBinarySerializableWithTrimSupport)
          (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
        else
          obj.Write(writer);
        output.Close();
        return true;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TrySerializeAsync failed.", ex);
        return false;
      }
    }

    public static bool TrySaveRawCachedData(byte[] bytes, string fileId, FileMode fileMode)
    {
      try
      {
        new Stopwatch().Start();
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData), fileMode))
            storageFileStream.Write(bytes, 0, bytes.Length);
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TrySaveRawCachedData failed.", ex);
      }
      return false;
    }

    public static bool TrySerialize(IBinarySerializable obj, string fileId, bool trim = false, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
    {
      try
      {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, dataType), FileMode.Create))
          {
            BinaryWriter writer = new BinaryWriter((Stream) storageFileStream);
            if (trim && obj is IBinarySerializableWithTrimSupport)
              (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
            else
              obj.Write(writer);
          }
        }
        stopwatch.Stop();
        Logger.Instance.Info("CacheManager.TrySerialize succeeded for fileId = {0}, in {1} ms.", (object) fileId, (object) stopwatch.ElapsedMilliseconds);
        return true;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TrySerialize failed.", ex);
      }
      return false;
    }

    public static Stream GetStreamForWrite(string fileId)
    {
      using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        return (Stream) storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData), FileMode.Create);
    }

    public static bool TryDelete(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
    {
      try
      {
        new Stopwatch().Start();
        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
        {
          string filePath = CacheManager.GetFilePath(fileId, dataType);
          if (!storeForApplication.FileExists(filePath))
            return false;
          storeForApplication.DeleteFile(filePath);
          return true;
        }
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("CacheManager.TryDelete failed.", ex);
      }
      return false;
    }

    public enum DataType
    {
      CachedData,
      StateData,
    }
  }
}
