using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using SUCC.Types;

namespace SUCC
{
    internal static class NodeManager
    {
        internal static void SetNodeData<T>(Node node, T data) => SetNodeData(node, data, typeof(T));
        internal static void SetNodeData(Node node, object data, Type type)
        {
            if (data == null)
                throw new Exception("you can't serialize null");

            string dataAsString = data as string;
            if (type == typeof(string) && dataAsString.Contains(Environment.NewLine))
                BaseTypes.SerializeSpecialStringCase(dataAsString, node);

            else if (BaseTypes.IsBaseType(type))
                node.Value = BaseTypes.SerializeBaseType(data, type);

            else if (CollectionTypes.TrySetCollection(node, data, type))
                return;

            ComplexTypes.SetComplexNode(node, data, type);
        }

        internal static T GetNodeData<T>(Node node) => (T)GetNodeData(node, typeof(T));
        internal static object GetNodeData(Node node, Type type)
        {
            try
            {
                if (type == typeof(string) && node.Value == "\"\"\"" && node.ChildLines.Count > 0)
                    return BaseTypes.ParseSpecialStringCase(node);

                if (BaseTypes.IsBaseType(type))
                    return BaseTypes.ParseBaseType(node.Value, type);

                var collection = CollectionTypes.TryGetCollection(node, type);
                if (collection != null) return collection;

                if (!String.IsNullOrEmpty(node.Value))
                    return ComplexTypeShortcuts.GetFromShortcut(node.Value, type);

                return ComplexTypes.RetrieveComplexType(node, type);
            }
            catch(Exception e)
            {
                throw new Exception($"Error getting data of type {type} from node: {e.Message}");
            }
        }
    }
}