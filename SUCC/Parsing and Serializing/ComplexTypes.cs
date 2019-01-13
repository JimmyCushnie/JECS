using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;

namespace SUCC.Types
{
    internal static class ComplexTypes
    {
        internal static void SetComplexNode(Node node, object item, Type type)
        {
            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) { continue; }
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) { continue; }

                var child = node.GetChildAddressedByName(f.Name);
                NodeManager.SetNodeData(child, f.GetValue(item), f.FieldType);
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) { continue; }
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) { continue; }

                var child = node.GetChildAddressedByName(p.Name);
                NodeManager.SetNodeData(child, p.GetValue(item), p.PropertyType);
            }
        }

        internal static object RetrieveComplexType(Node node, Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) { continue; }
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) { continue; }

                if (!node.ContainsChildNode(f.Name)) { continue; }

                var child = node.GetChildAddressedByName(f.Name);
                object data = NodeManager.GetNodeData(child, f.FieldType);
                f.SetValue(returnThis, data);
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) { continue; }
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) { continue; }

                if (!node.ContainsChildNode(p.Name)) { continue; }

                var child = node.GetChildAddressedByName(p.Name);
                object data = NodeManager.GetNodeData(child, p.PropertyType);
                p.SetValue(returnThis, data);
            }

            return returnThis;
        }
    }
}
