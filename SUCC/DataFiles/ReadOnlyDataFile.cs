using System;

namespace SUCC
{
    /// <summary>
    /// A read-only version of DataFile. Data can be read from disk, but not saved to disk.
    /// </summary>
    public class ReadOnlyDataFile : DataFileBase
    {
        /// <summary>
        /// Creates a new ReadOnlyDataFile object corresponding to a SUCC file in system storage.
        /// </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        /// <param name="defaultFile"> optionally, if there isn't a file at the path, one can be created from a file in the Resources folder. </param>
        /// <param name="autoReload"> if true, the DataFile will automatically reload when the file changes on disk. </param>
        public ReadOnlyDataFile(string path, string defaultFile = null, bool autoReload = false) : base(path, defaultFile, autoReload) { }

        /// <summary> Get some data from the file, or return a default value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="defaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public override T Get<T>(string key, T defaultValue = default) => base.Get(key, defaultValue);

        /// <summary> Like Get but works for nested paths instead of just the top level of the file </summary>
        public override T GetAtPath<T>(T defaultValue, params string[] path) => base.GetAtPath(defaultValue, path);
        /// <summary> Like Get but works for nested paths instead of just the top level of the file </summary>
        public override object GetAtPathNonGeneric(Type type, object defaultValue, params string[] path)
        {
            if (!KeyExistsAtPath(path)) // throws exception for us when path.length < 1
                throw new Exception($"The specified path doesn't exist. Check whether {nameof(KeyExistsAtPath)} first.");

            var topNode = TopLevelNodes[path[0]];
            for (int i = 1; i < path.Length; i++)
            {
                topNode = topNode.GetChildAddressedByName(path[i]);
            }

            return NodeManager.GetNodeData(topNode, type);
        }

        /// <summary> Non-generic version of Get. You probably want to use Get. </summary>
        /// <param name="type"/> the type to get the data as </param>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="DefaultValue"> if the key does not exist in the file, this value is returned instead </param>
        public override object GetNonGeneric(Type type, string key, object defaultValue)
        {
            if (!KeyExists(key)) return defaultValue;

            var node = TopLevelNodes[key];
            return NodeManager.GetNodeData(node, type);
        }
    }
}