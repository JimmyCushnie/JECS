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
            var members = new List<ClassMember>();

            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (f.IsInitOnly || f.IsLiteral) continue;
                if (Attribute.IsDefined(f, typeof(DontSaveThisAttribute))) continue;
                if (ComplexTypeOverrides.IsNeverSaved(f)) continue;
                if (f.IsPrivate && !Attribute.IsDefined(f, typeof(SaveThisAttribute)) && !ComplexTypeOverrides.IsAlwaysSaved(f)) continue;

                members.Add(f);
            }

            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) continue;
                if (Attribute.IsDefined(p, typeof(DontSaveThisAttribute))) continue;
                if (ComplexTypeOverrides.IsNeverSaved(p)) continue;
                if (p.GetOrSetIsPrivate() && !Attribute.IsDefined(p, typeof(SaveThisAttribute)) && !ComplexTypeOverrides.IsAlwaysSaved(p)) continue;

                members.Add(p);
            }

            for (int i = 0; i < members.Count; i++)
            {
                var m = members[i];

                var attr = m.Member.GetCustomAttribute<SaveThisAttribute>();
                if (attr?.SaveAs != null)
                {
                    members[i] = m.WithName(attr.SaveAs);
                }
            }

            return members;
        }
    }
}
