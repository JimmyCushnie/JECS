﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JECS.ParsingLogic
{
    public static class ComplexTypes
    {
        internal const string KEY_CONCRETE_TYPE = "{JECS_CONCRETE_TYPE}";

        internal static void SetComplexNode(Node node, object data, Type type, FileStyle style)
        {
            // Clear the shortcut if there is any
            if (node.HasValue)
                node.ClearValue();
            

            if (type.IsAbstract || type.IsInterface)
            {
                var concreteTypeNode = node.GetChildAddressedByName(KEY_CONCRETE_TYPE);
                var concreteType = data.GetType();
                NodeManager.SetNodeData(concreteTypeNode, concreteType, typeof(Type), style);

                type = concreteType;
            }
            
            foreach (var m in type.GetValidMembers())
            {
                var child = node.GetChildAddressedByName(m.Name);
                NodeManager.SetNodeData(child, m.GetValue(data), m.MemberType, style);
            }
        }

        internal static object RetrieveComplexType(Node node, Type type)
        {
            if (node.ChildNodes.Count > 0 && node.ChildNodeType != NodeChildrenType.Key)
                throw new FormatException("Non-shortcut complex type nodes must have key children");

            
            if (type.IsAbstract || type.IsInterface)
            {
                var concreteTypeNode = node.GetChildAddressedByName(KEY_CONCRETE_TYPE);
                var concreteType = NodeManager.GetNodeData<Type>(concreteTypeNode);

                type = concreteType;
            }
            
            object returnThis = Activator.CreateInstance(type);

            foreach (var m in type.GetValidMembers())
            {
                if (!node.ContainsChildNode(m.Name)) 
                    continue;

                var child = node.GetChildAddressedByName(m.Name);
                object data = NodeManager.GetNodeData(child, m.MemberType);
                m.SetValue(returnThis, data);
            }

            return returnThis;
        }

        public static IEnumerable<ClassMember> GetValidMembers(this Type type)
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
                if (p.GetMethod.IsPrivate && !Attribute.IsDefined(p, typeof(SaveThisAttribute)) && !ComplexTypeOverrides.IsAlwaysSaved(p)) continue;

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
