using SUCC.Abstractions;
using SUCC.ParsingLogic;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SUCC
{
    /// <summary>
    /// Represents SUCC data spread over multiple files.
    /// Using this class, you can search all those files at once for a piece of data.
    /// </summary>
    public class DistributedData
    {
        private readonly List<ReadableDataFile> _DataSources = new List<ReadableDataFile>();
        public IReadOnlyList<ReadableDataFile> DataSources => _DataSources;

        /// <summary>
        /// Adds a <see cref="ReadableDataFile"/> as a data source that this <see cref="DistributedData"/> can search through.
        /// </summary>
        public void AddDataSource(ReadableDataFile source)
        {
            _DataSources.Add(source);
        }




        /// <summary>
        /// Creates a new <see cref="DistributedData"/>.
        /// </summary>
        public DistributedData()
        {
        }


        /// <summary>
        /// Creates a new <see cref="DistributedData"/> by searching a folder for matching SUCC files.
        /// </summary>
        /// <param name="path">The path of the directory to search.</param>
        /// <param name="searchPattern">The search string to match against the names of files. You do not need to add the ".succ" extension.</param>
        /// <param name="searchOption">How to search the directory."/></param>
        /// <returns></returns>
        public static DistributedData CreateBySearching(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            path = Utilities.AbsolutePath(path);
            return CreateBySearching(new DirectoryInfo(path), searchPattern, searchOption);
        }

        /// <summary>
        /// Creates a new <see cref="DistributedData"/> by searching a folder for matching SUCC files.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="searchPattern">The search string to match against the names of files. You do not need to add the ".succ" extension.</param>
        /// <param name="searchOption">How to search the directory."/></param>
        /// <returns></returns>
        public static DistributedData CreateBySearching(DirectoryInfo directory, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            searchPattern = Path.ChangeExtension(searchPattern, Utilities.FileExtension);
            
            var paths = new List<string>();
            foreach (var fileInfo in directory.EnumerateFiles(searchPattern, searchOption))
                paths.Add(fileInfo.FullName);

            var data = new DistributedData();
            data.AddFilesOnDisk(paths);
            return data;
        }


        /// <summary>
        /// All of the top-level keys in all of the files within this <see cref="DistributedData"/>.
        /// </summary>
        public IReadOnlyCollection<string> GetTopLevelKeys()
        {
            var keys = new HashSet<string>();

            foreach (var source in DataSources)
                keys.UnionWith(source.TopLevelKeys);

            return keys;
        }

        /// <summary>
        /// The top level keys in this dataset, in the order they appear in the files, sorted by the file identifier.
        /// Unlike <see cref="GetTopLevelKeys"/>, this might contain duplicate keys if there are two files with the same key.
        /// </summary>
        public IReadOnlyList<string> GetTopLevelKeysInOrder()
        {
            var keys = new List<string>();

            foreach (var source in DataSources.OrderBy(source => source.Identifier))
                keys.AddRange(source.GetTopLevelKeysInOrder());

            return keys;
        }


        /// <summary> Does data exist in any of our files at this top-level key? </summary>
        public bool KeyExists(string key)
        {
            foreach (var file in DataSources)
            {
                if (file.KeyExists(key))
                    return true;
            }

            return false;
        }

        /// <summary> Does data exist in any of our files at this nested path? </summary>
        public bool KeyExistsAtPath(params string[] path)
        {
            foreach (var file in DataSources)
            {
                if (file.KeyExistsAtPath(path))
                    return true;
            }

            return false;
        }


        /// <summary> Get some data from our files, or return a default value if the data does not exist. </summary>
        /// <param name="key"> What the data is labeled as within the file. </param>
        /// <param name="defaultValue"> If the key does not exist in the file, this value is returned instead. </param>
        public T Get<T>(string key, T defaultValue = default) 
            => (T)GetNonGeneric(typeof(T), key, defaultValue);

        /// <summary> Non-generic version of <see cref="Get{T}(string, T)"/>. You probably want to use <see cref="Get{T}(string, T)"/>. </summary>
        /// <param name="type"> The type to get the data as. </param>
        /// <param name="key"> What the data is labeled as within the file. </param>
        public object GetNonGeneric(Type type, string key)
            => GetNonGeneric(type, key, type.GetDefaultValue());

        /// <summary> Non-generic version of <see cref="Get{T}(string, T)"/>. You probably want to use <see cref="Get{T}(string, T)"/>. </summary>
        /// <param name="type"> The type to get the data as. </param>
        /// <param name="key"> What the data is labeled as within the file. </param>
        /// <param name="defaultValue"> If the key does not exist in the file, this value is returned instead. </param>
        public object GetNonGeneric(Type type, string key, object defaultValue)
        {
            if (defaultValue != null && !type.IsAssignableFrom(defaultValue.GetType()))
                throw new InvalidCastException($"{nameof(type)} must be assignable from the type of {nameof(defaultValue)}");

            foreach (var file in DataSources)
            {
                if (file.KeyExists(key))
                    return file.GetNonGeneric(type, key, defaultValue);
            }

            return defaultValue;
        }


        /// <summary> Like <see cref="Get{T}(string, T)"/>, but but works for nested paths instead of just the top level of the files. </summary>
        public T GetAtPath<T>(T defaultValue, params string[] path)
            => (T)GetAtPathNonGeneric(typeof(T), defaultValue, path);

        /// <summary> Like <see cref="GetNonGeneric(Type, string)"/>, but but works for nested paths instead of just the top level of the files. </summary>
        public object GetAtPathNonGeneric(Type type, params string[] path)
            => GetAtPathNonGeneric(type, type.GetDefaultValue(), path);

        /// <summary> Like <see cref="GetNonGeneric(Type, string, object)"/>, but but works for nested paths instead of just the top level of the files. </summary>
        public object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (defaultValue != null && !type.IsAssignableFrom(defaultValue.GetType()))
                throw new InvalidCastException($"{nameof(type)} must be assignable from the type of {nameof(defaultValue)}");

            foreach (var file in DataSources)
            {
                if (file.KeyExistsAtPath(path))
                    return file.GetAtPathNonGeneric(type, defaultValue, path);
            }

            return defaultValue;
        }


        public bool TryGet<T>(string key, out T value)
        {
            if (!KeyExists(key))
            {
                value = default;
                return false;
            }

            value = Get<T>(key);
            return true;
        }

        public bool TryGetNonGeneric(Type type, string key, out object value)
        {
            if (!KeyExists(key))
            {
                value = null;
                return false;
            }

            value = GetNonGeneric(type, key);
            return true;
        }
    }
}