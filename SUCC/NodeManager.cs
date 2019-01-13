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
    public class DontSaveAttribute : Attribute { }

    internal static class NodeManager
    {
        internal static void SetNodeData(Node node, object data, Type type)
        {
            if (data == null)
                throw new Exception("you can't serialize null");

            if (type == typeof(string))
            {
                string dataAsString = (string)data;
                BaseTypes.SerializeSpecialStringCase(dataAsString, node);
            }

            else if (BaseTypes.IsBaseType(type))
                node.Value = BaseTypes.SerializeBaseType(data, type);

            else if (CollectionTypes.TrySetCollection(node, data, type))
                return;

            ComplexTypes.SetComplexNode(node, data, type);
        }


        internal static object GetNodeData(Node node, Type type)
        {
            if (type == typeof(string) && node.Value == "\"\"\"" && node.ChildLines.Count > 0)
                return BaseTypes.ParseSpecialStringCase(node);

            if (BaseTypes.IsBaseType(type))
                return BaseTypes.ParseBaseType(node.Value, type);

            var collection = CollectionTypes.TryGetCollection(node, type);
            if (collection != null) return collection;

            if (!String.IsNullOrEmpty(node.Value))
                ComplexTypeShortcuts.GetFromShortcut(node.Value, type);

            return ComplexTypes.RetrieveComplexType(node, type);
        }
    }
}