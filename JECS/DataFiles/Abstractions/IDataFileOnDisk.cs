using System;
using System.IO;

namespace JECS.Abstractions
{
    public interface IDataFileOnDisk
    {
        /// <summary> The absolute path of the file this object corresponds to. </summary>
        string FilePath { get; }

        /// <summary> The name of this file on disk (without the file path or extension) </summary>
        string FileName { get; }

        /// <summary> Size of this file on disk in bytes. If there is unsaved data in memory it will not be counted. </summary>
        long SizeOnDisk { get; }

        void ReloadAllData();


        object FileSystemReadWriteLock { get; }
        DateTime LastKnownWriteTimeUTC { get; }
    }

    internal static class IDataFileOnDiskExtensions
    {
        public static DateTime GetCurrentLastWriteTimeUTC(this IDataFileOnDisk dataFileOnDisk)
            => File.GetLastWriteTimeUtc(dataFileOnDisk.FilePath);
    }
}