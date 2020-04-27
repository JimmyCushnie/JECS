using System;
using System.Collections.Generic;
using System.Reflection;

namespace SUCC.ParsingLogic
{
    internal static class ComplexTypes
    {
        internal static void SetComplexNode(Node node, object item, Type type, FileStyle style)
        {
            // clear the shortcut if there is any
            if (!string.IsNullOrEmpty(node.Value))
                node.Value = "";

            foreach (var m in type.GetValidMembers())
            {
                var child = node.GetChildAddressedByName(m.Name);
                NodeManager.SetNodeData(child, m.GetValue(item), m.MemberType, style);
            }
        }

        internal static object RetrieveComplexType(Node node, Type type)
        {
            object returnThis = Activator.CreateInstance(type);

            foreach (var m in type.GetValidMembers())
            {
                if (!node.ContainsChildNode(m.Name)) continue;

                var child = node.GetChildAddressedByName(m.Name);
                object data = NodeManager.GetNodeData(child, m.MemberType);
                m.SetValue(returnThis, data);
            }

            return returnThis;
        }

        internal static IEnumerable<ClassMember> GetValidMembers(this Type type)
        {
            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (f.IsInitOnly || f.IsLiteral) continue;
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) continue;
                if (ComplexTypeOverrides.IsNeverSaved(f)) continue;
                if (f.IsPrivate && !Attribute.IsDefined(f, typeof(DoSaveAttribute)) && !ComplexTypeOverrides.IsAlwaysSaved(f)) continue;

                yield return f;
            }

            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) continue;
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) continue;
                if (ComplexTypeOverrides.IsNeverSaved(p)) continue;
                if (p.GetOrSetIsPrivate() && !Attribute.IsDefined(p, typeof(DoSaveAttribute)) && !ComplexTypeOverrides.IsAlwaysSaved(p)) continue;

                yield return p;
            }
        }
    }
}
