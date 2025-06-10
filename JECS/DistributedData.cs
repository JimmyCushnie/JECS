using JECS.Abstractions;
using JECS.ParsingLogic;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace JECS
{
    /// <summary>
    /// Represents JECS data spread over multiple files.
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
        /// Creates a new <see cref="DistributedData"/> by searching a folder for matching JECS files.
        /// </summary>
        /// <param name="path">The path of the directory to search.</param>
        /// <param name="searchPattern">The search string to match against the names of files. You do not need to add the ".jecs" extension.</param>
        /// <param name="searchOption">How to search the directory."/></param>
        /// <returns></returns>
        public static DistributedData CreateBySearching(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            path = Utilities.AbsolutePath(path);
            return CreateBySearching(new DirectoryInfo(path), searchPattern, searchOption);
        }

        /// <summary>
        /// Creates a new <see cref="DistributedData"/> by searching a folder for matching JECS files.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="searchPattern">The search string to match against the names of files. You do not need to add the ".jecs" extension.</param>
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
            => TryGet<T>(key, out var output) ? output : defaultValue;

        /// <summary> Non-generic version of <see cref="Get{T}(string, T)"/>. You probably want to use <see cref="Get{T}(string, T)"/>. </summary>
        /// <param name="type"> The type to get the data as. </param>
        /// <param name="key"> What the data is labeled as within the file. </param>
        public object GetNonGeneric(Type type, string key)
            => TryGetNonGeneric(type, key, out var output) ? output : type.GetDefaultValue();

        /// <summary> Non-generic version of <see cref="Get{T}(string, T)"/>. You probably want to use <see cref="Get{T}(string, T)"/>. </summary>
        /// <param name="type"> The type to get the data as. </param>
        /// <param name="key"> What the data is labeled as within the file. </param>
        /// <param name="defaultValue"> If the key does not exist in the file, this value is returned instead. </param>
        public object GetNonGeneric(Type type, string key, object defaultValue)
        {
            if (defaultValue != null && !type.IsAssignableFrom(defaultValue.GetType()))
                throw new InvalidCastException($"{nameof(type)} must be assignable from the type of {nameof(defaultValue)}");

            return TryGetNonGeneric(type, key, out var output) ? output : defaultValue;
        }


        /// <summary> Like <see cref="Get{T}(string, T)"/>, but works for nested paths instead of just the top level of the files. </summary>
        public T GetAtPath<T>(params string[] path)
            => GetAtPath((T)typeof(T).GetDefaultValue(), path);

        /// <summary> Like <see cref="Get{T}(string, T)"/>, but works for nested paths instead of just the top level of the files. </summary>
        public T GetAtPath<T>(T defaultValue, params string[] path)
            => TryGetAtPath<T>(out var output, path) ? output : defaultValue;

        /// <summary> Like <see cref="GetNonGeneric(Type, string)"/>, but works for nested paths instead of just the top level of the files. </summary>
        public object GetAtPathNonGeneric(Type type, params string[] path)
            => TryGetAtPathNonGeneric(type, out var value, path) ? value : type.GetDefaultValue();

        /// <summary> Like <see cref="GetNonGeneric(Type, string, object)"/>, but works for nested paths instead of just the top level of the files. </summary>
        public object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (defaultValue != null && !type.IsAssignableFrom(defaultValue.GetType()))
                throw new InvalidCastException($"{nameof(type)} must be assignable from the type of {nameof(defaultValue)}");

            return TryGetAtPathNonGeneric(type, out var value, path) ? value : defaultValue;
        }


        /// <summary> Tries to get some data from our files. </summary>
        /// <param name="key"> They key for the data. </param>
        /// <param name="value"> Output data, if found value of type <typeparamref name="T"/> else <see langword="null"/>. </param>
        /// <typeparam name="T"> The type the data is expected to be. </typeparam>
        /// <returns> <see langword="true"/>, if the key for data exists, <see langword="false"/> otherwise. </returns>
        public bool TryGet<T>(string key, out T value)
        {
            foreach (var file in DataSources)
            {
                if (file.TryGet(key, out value))
                    return true;
            }

            value = default;
            return false;
        }

        /// <summary> Tries to get some data from our files. </summary>
        /// <param name="type"> The type the data is expected to be. </param>
        /// <param name="key"> They key for the data. </param>
        /// <param name="value"> Output data, if found value of type <paramref name="type"/> else <see langword="null"/>.</param>
        /// <returns> <see langword="true"/>, if the key for data exists, <see langword="false"/> otherwise. </returns>
        public bool TryGetNonGeneric(Type type, string key, out object value)
        {
            foreach (var file in DataSources)
            {
                if (file.TryGetNonGeneric(type, key, out value))
                    return true;
            }

            value = null;
            return false;
        }

        /// <summary> Tries to get some data from our files. </summary>
        /// <param name="value"> Output data, if found value of type <typeparamref name="T"/> else <see langword="null"/>. </param>
        /// <param name="path"> They path for the data. </param>
        /// <typeparam name="T"> The type the data is expected to be. </typeparam>
        /// <returns> <see langword="true"/>, if the key for data exists, <see langword="false"/> otherwise. </returns>
        public bool TryGetAtPath<T>(out T value, params string[] path)
        {
            foreach (var file in DataSources)
            {
                if (file.TryGetAtPath(out value, path))
                    return true;
            }

            value = default;
            return false;
        }

        /// <summary> Tries to get some data from our files. </summary>
        /// <param name="type"> The type the data is expected to be. </param>
        /// <param name="value"> Output data, if found value of type <paramref name="type"/> else <see langword="null"/>.</param>
        /// <param name="path"> They path for the data. </param>
        /// <returns> <see langword="true"/>, if the key for data exists, <see langword="false"/> otherwise. </returns>
        public bool TryGetAtPathNonGeneric(Type type, out object value, params string[] path)
        {
            foreach (var file in DataSources)
            {
                if (file.TryGetAtPathNonGeneric(type, out value, path))
                    return true;
            }

            value = null;
            return false;
        }
    }
}