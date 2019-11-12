using System;

namespace SUCC.Abstractions
{
    internal interface IDataFileOnDisk
    {
        /// <summary> The absolute path of the file this object corresponds to. </summary>
        string FilePath { get; }

        /// <summary> The name of this file on disk (without the file path or extension) </summary>
        string FileName { get; }

        /// <summary> Size of this file on disk in bytes. If there is unsaved data in memory it will not be counted. </summary>
        long SizeOnDisk { get; }

        /// <summary> If true, the DataFile will automatically reload when the file changes on disk. If false, you can still call ReloadAllData() manually. </summary>
        bool AutoReload { get; set; }

        /// <summary> Invoked every time the file is auto-reloaded. This only happens when AutoReload is set to true. </summary>
        event Action OnAutoReload;
    }
}