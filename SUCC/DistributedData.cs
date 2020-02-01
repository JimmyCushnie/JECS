using System;
using System.IO;
using System.Collections.Generic;
using SUCC.InternalParsingLogic;

namespace SUCC
{
    public class DistributedData
    {
        private readonly List<ReadOnlyDataFile> _Files = new List<ReadOnlyDataFile>();
        public IReadOnlyList<ReadOnlyDataFile> Files => _Files;


        public DistributedData(params string[] paths) : this((IReadOnlyList<string>)paths)
        {
        }
        public DistributedData(IReadOnlyList<string> paths)
        {
            foreach (var path in paths)
                _Files.Add(new ReadOnlyDataFile(path));
        }

        public static DistributedData CreateBySearching(DirectoryInfo folder, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            searchPattern = Path.ChangeExtension(searchPattern, Utilities.FileExtension);

            var paths = new List<string>();
            foreach (var fileInfo in folder.EnumerateFiles(searchPattern, searchOption))
                paths.Add(fileInfo.FullName);

            return new DistributedData(paths);
        }



        public bool KeyExists(string key)
        {
            foreach (var file in Files)
            {
                if (file.KeyExists(key))
                    return true;
            }

            return false;
        }

        /// <summary> whether a key exists in the file at a nested path </summary>
        public bool KeyExistsAtPath(params string[] path)
        {
            foreach (var file in Files)
            {
                if (file.KeyExistsAtPath(path))
                    return true;
            }

            return false;
        }


        /// <summary> Get some data from the file, or return a default value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public T Get<T>(string key, T defaultValue = default) 
            => (T)GetNonGeneric(typeof(T), key, defaultValue);

        public object GetNonGeneric(Type type, string key)
            => GetNonGeneric(type, key, type.GetDefaultValue());

        /// <summary> Non-generic version of Get. You probably want to use Get. </summary>
        /// <param name="type"> the type to get the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public object GetNonGeneric(Type type, string key, object defaultValue)
        {
            if (defaultValue != null && defaultValue.GetType() != type)
                throw new Exception($"{nameof(type)} must match {nameof(defaultValue)}");

            foreach (var file in Files)
            {
                if (file.KeyExists(key))
                    return file.GetNonGeneric(type, key, defaultValue);
            }

            return defaultValue;
        }

        /// <summary> 
        /// Like Get but works for nested paths instead of just the top level of the file 
        /// </summary>
        public T GetAtPath<T>(T defaultValue, params string[] path)
            => (T)GetAtPathNonGeneric(typeof(T), defaultValue, path);

        public object GetAtPathNonGeneric(Type type, params string[] path)
            => GetAtPathNonGeneric(type, type.GetDefaultValue(), path);

        /// <summary>
        /// Non-generic version of GetAtPath. You probably want to use GetAtPath.
        /// </summary>
        public object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (defaultValue != null && defaultValue.GetType() != type)
                throw new Exception($"{nameof(type)} must match {nameof(defaultValue)}");

            foreach (var file in Files)
            {
                if (file.KeyExistsAtPath(path))
                    return file.GetAtPathNonGeneric(type, defaultValue, path);
            }

            return defaultValue;
        }
    }
}