using SUCC.Abstractions;
using SUCC.MemoryFiles;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SUCC
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


        public static void AddRawSuccDatas(this DistributedData distributedData, params string[] rawSuccDatas)
            => AddRawSuccDatas(distributedData, (IEnumerable<string>)rawSuccDatas);

        public static void AddRawSuccDatas(this DistributedData distributedData, IEnumerable<string> rawSuccDatas)
        {
            foreach (var rawSuccData in rawSuccDatas)
                distributedData.AddRawSuccData(rawSuccData);
        }

        public static void AddRawSuccData(this DistributedData distributedData, string rawSuccData)
            => distributedData.AddDataSource(new MemoryReadOnlyDataFile(rawSuccData));
    }
}