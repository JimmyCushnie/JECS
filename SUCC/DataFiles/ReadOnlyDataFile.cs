using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SUCC
{
    public class ReadOnlyDataFile : DataFileBase
    {
        public ReadOnlyDataFile(string path, string defaultFile = null) : base(path, defaultFile) { }

        public override T Get<T>(string key, T defaultValue = default) => base.Get(key, defaultValue);
        public override object Get(Type type, string key, object defaultValue)
        {
            if (!KeyExists(key)) return defaultValue;

            var node = TopLevelNodes[key];
            return NodeManager.GetNodeData(node, type);
        }
    }
}