using JECS.Abstractions;
using JECS.MemoryFiles;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace JECS
{
    public static class DistributedDataExtensions
    {
        public static void AddDataSources(this DistributedData distributedData, params ReadableDataFile[] sources)
            => AddDataSources(distributedData, (IEnumerable<ReadableDataFile>)sources);

        public static void AddDataSources<T>(this DistributedData distributedData, IEnumerable<T> sources) where T : ReadableDataFile
        {
            foreach (var source in sources)
                distributedData.AddDataSource(source);
        }


        public static void AddFilesOnDisk(this DistributedData distributedData, params string[] paths)
            => AddFilesOnDisk(distributedData, (IEnumerable<string>)paths);

        public static void AddFilesOnDisk(this DistributedData distributedData, IEnumerable<string> paths)
        {
            foreach (var path in paths)
                distributedData.AddFileOnDisk(path);
        }

        public static void AddFileOnDisk(this DistributedData distributedData, string path)
            => distributedData.AddDataSource(new ReadOnlyDataFile(path));


        public static void AddRawJecsDatas(this DistributedData distributedData, params string[] rawJecsDatas)
            => AddRawJecsDatas(distributedData, (IEnumerable<string>)rawJecsDatas);

        public static void AddRawJecsDatas(this DistributedData distributedData, IEnumerable<string> rawJecsDatas)
        {
            foreach (var rawJecsData in rawJecsDatas)
                distributedData.AddRawJecsData(rawJecsData);
        }

        public static void AddRawJecsData(this DistributedData distributedData, string rawJecsData)
            => distributedData.AddDataSource(new MemoryReadOnlyDataFile(rawJecsData));
    }
}