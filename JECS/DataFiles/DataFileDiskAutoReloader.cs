using JECS.Abstractions;
using System;
using System.Timers;

namespace JECS
{
    /// <summary>
    /// Utility class that watches for a DataFile has been changed on disk, i.e. by a user editing the file manually.
    /// Subscribe to <see cref="OnAutoReload"/> then call <see cref="StartWatchingForDiskChanges"/>.
    /// Make sure to call <see cref="Dispose"/> when you're done with the instance.
    /// </summary>
    public class DataFileDiskAutoReloader : IDisposable
    {
        public IDataFileOnDisk DataFileOnDisk { get; }

        /// <summary>
        /// This will fire when <see cref="DataFileOnDisk"/> has been changed by an external program, after its data has been reloaded from disk.
        /// </summary>
        public event Action OnAutoReload;

        public DataFileDiskAutoReloader(IDataFileOnDisk dataFileOnDisk)
        {
            DataFileOnDisk = dataFileOnDisk;
        }

        public void StartWatchingForDiskChanges()
        {
            if (AutoReloadTimer != null)
                throw new Exception($"You can only call {nameof(StartWatchingForDiskChanges)} once!");
            
            const float AutoReloadTimerIntervalSeconds = 1f;
            AutoReloadTimer = new Timer(TimeSpan.FromSeconds(AutoReloadTimerIntervalSeconds).TotalMilliseconds);
            AutoReloadTimer.AutoReset = false;
            AutoReloadTimer.Elapsed += AutoReloadTimerElapsed;
            
            AutoReloadTimer.Start();
        }


        // To make AutoReload work, we regularly check the last write time on the filesystem.
        // We used to use FileSystemWatcher, but that class is a nasty bastard that loves to randomly not work, especially on Linux, and especially on Mono.
        // It was also a problem because it's very hard to determine whether FileSystemWatcher is firing legitimately or just because this code has saved a
        // new value to disk.

        private Timer AutoReloadTimer;
        private readonly object TimerLock = new object();

        private void AutoReloadTimerElapsed(object _, ElapsedEventArgs __)
        {
            // Restart the timer manually, only after we finish ReloadIfChanged.
            // Use the lock to ensure we can properly stop the timer when Disposing.
            lock (TimerLock)
            {
                ReloadIfChanged();
                AutoReloadTimer.Start();
            }
        }
        private void ReloadIfChanged()
        {
            lock (DataFileOnDisk.FileSystemReadWriteLock)
            {
                if (DataFileOnDisk.GetCurrentLastWriteTimeUTC() != DataFileOnDisk.LastKnownWriteTimeUTC)
                {
                    DataFileOnDisk.ReloadAllData();
                    OnAutoReload?.Invoke();
                }
            }
        }

        public void Dispose()
        {
            lock (TimerLock)
            {
                AutoReloadTimer.Stop();
                AutoReloadTimer.Dispose();
            }
        }
    }
}
