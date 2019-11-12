using System;
using System.Collections.Generic;
using System.Reflection;

namespace SUCC.InternalParsingLogic
{
    internal static class ComplexTypes
    {
        internal static void SetComplexNode(Node node, object item, Type type, FileStyle style)
        {
            // clear the shortcut if there is any
            if (!string.IsNullOrEmpty(node.Value))
                node.Value = "";

            foreach (var f in GetValidFields(type))
            {
                var child = node.GetChildAddressedByName(f.Name);
                NodeManager.SetNodeData(child, f.GetValue(item), f.FieldType, style);
            }

            foreach (var p in GetValidProperties(type))
            {
                var child = node.GetChildAddressedByName(p.Name);
                NodeManager.SetNodeData(child, p.GetValue(item), p.PropertyType, style);
            }
        }

        internal static object RetrieveComplexType(Node node, Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            foreach (var f in GetValidFields(type))
            {
                if (!node.ContainsChildNode(f.Name)) continue;

                var child = node.GetChildAddressedByName(f.Name);
                object data = NodeManager.GetNodeData(child, f.FieldType);
                f.SetValue(returnThis, data);
            }

            foreach (var p in GetValidProperties(type))
            {
                if (!node.ContainsChildNode(p.Name)) continue;

                var child = node.GetChildAddressedByName(p.Name);
                object data = NodeManager.GetNodeData(child, p.PropertyType);
                p.SetValue(returnThis, data);
            }

            return returnThis;
        }


        internal static List<FieldInfo> GetValidFields(this Type type)
        {
            var validFields = new List<FieldInfo>();

            var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in allFields)
            {
                if (f.IsInitOnly || f.IsLiteral) continue;
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) continue;
                if (f.IsPrivate && !Attribute.IsDefined(f, typeof(DoSaveAttribute))) continue;

                validFields.Add(f);
            }

            return validFields;
        }

        internal static List<PropertyInfo> GetValidProperties(this Type type)
        {
            var validProperties = new List<PropertyInfo>();

            var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var p in allProperties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) continue;
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) continue;
                if (p.GetOrSetIsPrivate() && !Attribute.IsDefined(p, typeof(DoSaveAttribute))) continue;

                validProperties.Add(p);
            }

            return validProperties;
        }
    }
}
