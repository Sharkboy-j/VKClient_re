using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.AudioManager
{
    public class PlaylistManager
    {
        public static readonly string PlaylistFileName = "currentPlaylist";
        public static readonly string PlaybackSettingsFileName = "PlaybackSettings";
        public static readonly string PlaylistMetadataFileName = "playlistMetadata";
        public static readonly string SettingsMetadataFileName = "SettingsMetadata";
        public static readonly string BackupPlaylistFileName = "backupPlaylist";
        public static readonly string PlaylistMutexName = "playlistLockMutex";
        private static Mutex playlistAccessMutex = new Mutex(false, PlaylistManager.PlaylistMutexName);
        private static Mutex settingsAccessMutex = new Mutex(false, "SettingsMutex");
        private static Mutex metadataAccessMutex = new Mutex(false, "metadataMutex");
        private static object _lockObj = new object();
        private static PlaybackSettings _settingsCached;
        private static Playlist _playlistCached;

        private static DateTime GetLastChangedDate(string fileName)
        {
            try
            {
                PlaylistManager.metadataAccessMutex.WaitOne();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!storeForApplication.FileExists(fileName))
                        return DateTime.MinValue;
                    using (BinaryReader reader = new BinaryReader((Stream)storeForApplication.OpenFile(fileName, FileMode.Open, FileAccess.Read)))
                        return reader.ReadGeneric<Metadata>().LastUpdated;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("PlaylistManager.GetLastChangedDate failed", ex);
                return DateTime.MinValue;
            }
            finally
            {
                PlaylistManager.metadataAccessMutex.ReleaseMutex();
            }
        }

        public static void Initialize()
        {
        }

        public static bool HaveAssignedTrack()
        {
            try
            {
                return BGAudioPlayerWrapper.Instance.Track != null;
            }
            catch
            {
            }
            return false;
        }

        public static PlaybackSettings ReadPlaybackSettings(bool allowCached = false)
        {
            lock (PlaylistManager._lockObj)
            {
                if (allowCached && PlaylistManager._settingsCached != null)
                    return PlaylistManager._settingsCached;
                PlaybackSettings local_2 = new PlaybackSettings();
                try
                {
                    if (PlaylistManager._settingsCached != null && PlaylistManager._settingsCached.Metadata.LastUpdated == PlaylistManager.GetLastChangedDate(PlaylistManager.SettingsMetadataFileName))
                        return PlaylistManager._settingsCached;
                    using (IsolatedStorageFile resource_1 = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        PlaylistManager.settingsAccessMutex.WaitOne();
                        if (resource_1.FileExists(PlaylistManager.PlaybackSettingsFileName))
                        {
                            IsolatedStorageFileStream local_5 = resource_1.OpenFile(PlaylistManager.PlaybackSettingsFileName, FileMode.Open, FileAccess.Read);
                            if (local_5 != null)
                            {
                                using (BinaryReader resource_0 = new BinaryReader((Stream)local_5))
                                    local_2 = resource_0.ReadGeneric<PlaybackSettings>();
                                PlaylistManager._settingsCached = local_2;
                            }
                        }
                        PlaylistManager.settingsAccessMutex.ReleaseMutex();
                    }
                }
                catch (Exception exception_0)
                {
                    Logger.Instance.Error("ReadPlaybackSettings failed", exception_0);
                }
                return local_2;
            }
        }

        public static void WritePlaybackSettings(PlaybackSettings settings)
        {
            settings.Metadata = new Metadata()
            {
                LastUpdated = DateTime.Now
            };
            lock (PlaylistManager._lockObj)
            {
                try
                {
                    using (IsolatedStorageFile resource_2 = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        PlaylistManager.metadataAccessMutex.WaitOne();
                        using (BinaryWriter resource_0 = new BinaryWriter((Stream)resource_2.CreateFile(PlaylistManager.SettingsMetadataFileName)))
                            resource_0.Write<Metadata>(settings.Metadata, false);
                        PlaylistManager.metadataAccessMutex.ReleaseMutex();
                        PlaylistManager.settingsAccessMutex.WaitOne();
                        using (BinaryWriter resource_1 = new BinaryWriter((Stream)resource_2.CreateFile(PlaylistManager.PlaybackSettingsFileName)))
                            resource_1.Write<PlaybackSettings>(settings, false);
                        PlaylistManager.settingsAccessMutex.ReleaseMutex();
                        PlaylistManager._settingsCached = settings;
                    }
                }
                catch (Exception exception_0)
                {
                    Logger.Instance.Error("WritePlaybackSettings failed", exception_0);
                }
            }
        }

        public static Playlist LoadTracksFromIsolatedStorage(bool allowCached = false)
        {
            Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage, allowCached = {0}", (object)allowCached);
            lock (PlaylistManager._lockObj)
            {
                try
                {
                    if (allowCached && PlaylistManager._playlistCached != null)
                    {
                        Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: returning data from 1st level cache");
                        return PlaylistManager._playlistCached;
                    }
                    Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: _playlistCached is not null? = {0}" + (PlaylistManager._playlistCached != null).ToString());
                    if (PlaylistManager._playlistCached != null)
                    {
                        Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: _playlistCached.Metadata.LastUpdated = {0}", (object)PlaylistManager._playlistCached.Metadata.LastUpdated);
                        DateTime local_4 = PlaylistManager.GetLastChangedDate(PlaylistManager.PlaylistMetadataFileName);
                        Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: Storemetadata.LastUpdated = {0}", (object)local_4);
                        if (PlaylistManager._playlistCached.Metadata.LastUpdated == local_4)
                        {
                            Logger.Instance.Info("PlaylistManager.LoadTracksFromIsolatedStorage: returning data from 2nd level cache");
                            return PlaylistManager._playlistCached;
                        }
                    }
                    using (IsolatedStorageFile resource_1 = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        try
                        {
                            PlaylistManager.playlistAccessMutex.WaitOne();
                            if (!resource_1.FileExists(PlaylistManager.PlaylistFileName))
                                return new Playlist();
                            IsolatedStorageFileStream local_6 = resource_1.OpenFile(PlaylistManager.PlaylistFileName, FileMode.Open, FileAccess.Read);
                            if (local_6 == null)
                                return new Playlist();
                            using (BinaryReader resource_0 = new BinaryReader((Stream)local_6))
                                return PlaylistManager._playlistCached = resource_0.ReadGeneric<Playlist>();
                        }
                        finally
                        {
                            PlaylistManager.playlistAccessMutex.ReleaseMutex();
                        }
                    }
                }
                catch (Exception exception_0)
                {
                    Logger.Instance.Error("Failed to read playlist", exception_0);
                }
                return new Playlist();
            }
        }

        public static void SetAudioAgentPlaylist(List<AudioObj> tracks, StatisticsActionSource actionSource)
        {
            PlaylistManager.SetAudioAgentPlaylistImpl(tracks, actionSource);
        }

        private static void SetAudioAgentPlaylistImpl(List<AudioObj> tracks, StatisticsActionSource actionSource)
        {
            tracks = tracks.Distinct<AudioObj, string>((Func<AudioObj, string>)(a => a.UniqueId)).ToList<AudioObj>();
            Playlist playlist1 = new Playlist();
            playlist1.Metadata = new Metadata()
            {
                LastUpdated = DateTime.Now,
                ActionSource = actionSource
            };
            List<AudioObj> audioObjList = tracks;
            playlist1.Tracks = audioObjList;
            Playlist playlist2 = playlist1;
            lock (PlaylistManager._lockObj)
            {
                try
                {
                    using (IsolatedStorageFile resource_2 = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        PlaylistManager.metadataAccessMutex.WaitOne();
                        using (BinaryWriter resource_0 = new BinaryWriter((Stream)resource_2.CreateFile(PlaylistManager.PlaylistMetadataFileName)))
                            resource_0.Write<Metadata>(playlist2.Metadata, false);
                        PlaylistManager.metadataAccessMutex.ReleaseMutex();
                        PlaylistManager.playlistAccessMutex.WaitOne();
                        using (BinaryWriter resource_1 = new BinaryWriter((Stream)resource_2.CreateFile(PlaylistManager.PlaylistFileName)))
                            resource_1.Write<Playlist>(playlist2, false);
                        PlaylistManager.playlistAccessMutex.ReleaseMutex();
                        PlaylistManager._playlistCached = playlist2;
                    }
                }
                catch (Exception exception_0)
                {
                    Logger.Instance.Error("Failed to set playlist", exception_0);
                }
            }
        }
    }
}
