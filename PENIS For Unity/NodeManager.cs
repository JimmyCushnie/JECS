using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;

namespace PENIS
{
    public class DontSaveAttribute : Attribute { }

    public static class NodeManager
    {
        public static void SetNodeData(Node node, object data, Type type)
        {
            if(type == typeof(string))
            {
                string dataAsString = (string)data;
                if (dataAsString.Contains(Environment.NewLine))
                {
                    node.Value = "\"\"\"";

                    node.ChildLines.Clear();
                    node.ChildNodes.Clear();

                    int indentation = node.IndentationLevel + Utilities.IndentationCount;
                    using (StringReader sr = new StringReader(dataAsString))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null) // this is effectively a ForEachLine, but it is platform agnostic (since new lines are encoded differently on different OSs)
                        {
                            string text = new string(' ', indentation) + SerializeString(line);
                            Line newline = new Line() { RawText = text };
                            node.ChildLines.Add(newline);
                        }
                    }

                    string endtext = new string(' ', indentation) + "\"\"\"";
                    Line endline = new Line() { RawText = endtext };
                    node.ChildLines.Add(endline);
                    return;
                }
                else
                {
                    node.ChildLines.Clear();
                    node.ChildNodes.Clear();
                } 
            }

            if (IsBaseType(type))
            {
                node.Value = SerializeBaseType(data, type);
                return;
            }
            if (IsIEnumerableType(type))
            {
                var dataEnumerable = (IEnumerable)data;
                IEnumerator numerator = dataEnumerable.GetEnumerator();
                Type elementType = GetAnyElementType(type);
                int count = 0;
                while (numerator.MoveNext())
                {
                    var child = node.GetChildAddressedByListNumber(count);
                    SetNodeData(child, numerator.Current, elementType);
                    count++;
                }

                return;
            }
            else if (node.ChildNodes.Count > 0 && node.ChildNodeType != NodeChildrenType.key)
            {
                throw new FormatException("ERROR ERROR BEEP BEEP BOOP");
            }

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) { continue; }
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) { continue; }

                var child = node.GetChildAddressedByName(f.Name);
                SetNodeData(child, f.GetValue(data), f.FieldType);
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) { continue; }
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) { continue; }

                var child = node.GetChildAddressedByName(p.Name);
                SetNodeData(child, p.GetValue(data), p.PropertyType);
            }
        }


        public static object GetNodeData(Node node, Type type)
        {
            if (type == typeof(string) && node.Value == "\"\"\"" && node.ChildLines.Count > 0)
            {
                string text = string.Empty;

                foreach(var line in node.ChildLines)
                {
                    var lineText = line.RawText;

                    // remove everything after the comment indicator
                    int PoundSignIndex = lineText.IndexOf('#');
                    if (PoundSignIndex > 0)
                        lineText = lineText.Substring(0, PoundSignIndex - 1);

                    lineText = lineText.Trim();

                    if(lineText == "\"\"\"")
                    {
                        break;
                    }

                    lineText = (string)ParseString(lineText); // to remove quotations

                    text += lineText;
                    text += Environment.NewLine;
                }

                return text.TrimEnd(Environment.NewLine.ToCharArray()); // remove all newlines at the end of the string
            }

            if (IsBaseType(type))
            {
                return ParseBaseType(node.Value, type);
            }

            if (type.IsArray)
            {
                Type elementType = GetAnyElementType(type);
                var ArrayBoi = Array.CreateInstance(elementType, node.ChildNodes.Count);

                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    ListNode child = node.GetChildAddressedByListNumber(i);
                    object element = GetNodeData(child, elementType);
                    ArrayBoi.SetValue(element, i);
                }

                return ArrayBoi;
            }
            if (IsIEnumerableType(type))
            {
                object CollectionBoi = Activator.CreateInstance(type, node.ChildNodes.Count);
                Type elementType = GetAnyElementType(type);
                var add = type.GetMethod("Add");
                if (add == null) { throw new Exception("aw shit, looks like the type " + type.ToString() + " doesn't have an 'Add' method 😭"); }

                for(int i = 0; i < node.ChildNodes.Count; i++)
                {
                    ListNode child = node.GetChildAddressedByListNumber(i);
                    object element = GetNodeData(child, elementType);
                    add.Invoke(CollectionBoi, new object[] { element });
                }

                return CollectionBoi;
            }
            else if (node.ChildNodeType == NodeChildrenType.list)
            {
                throw new Exception("You can't do that, you bitch!!!!!!!");
            }

            object returnThis = Activator.CreateInstance(type);

            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.IsInitOnly || f.IsLiteral || f.IsPrivate || f.IsStatic) { continue; }
                if (Attribute.IsDefined(f, typeof(DontSaveAttribute))) { continue; }

                var child = node.GetChildAddressedByName(f.Name);
                object data = GetNodeData(child, f.FieldType);
                f.SetValue(returnThis, data);
            }

            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) { continue; }
                if (Attribute.IsDefined(p, typeof(DontSaveAttribute))) { continue; }

                var child = node.GetChildAddressedByName(p.Name);
                object data = GetNodeData(child, p.PropertyType);
                p.SetValue(returnThis, data);
            }

            return returnThis;
        }


        private static bool IsIEnumerableType(Type type)
        {
            Type ElementType = GetAnyElementType(type);
            return ElementType != type;
        }
        private static Type GetAnyElementType(Type type)
        {
            // Type is Array
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            // type implements or extends IEnumerable<T>;
            var enumType = type.GetInterfaces()
                                    .Where(t => t.IsGenericType &&
                                           t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                    .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return enumType ?? type;
        }



        private static bool IsBaseType(Type type)
        {
            if (type.IsEnum) { return true; }
            if (BaseSerializeMethods.ContainsKey(type)) { return true; }
            return false;
        }

        private static string SerializeBaseType(object thing, Type type)
        {
            SerializeMethod method;
            if (BaseSerializeMethods.TryGetValue(type, out method))
                return method(thing);

            if (type.IsEnum)
                return SerializeEnum(thing);

            throw new Exception("Cannot serialize base type '" + type.ToString() + "'");
        }

        private static object ParseBaseType(string text, Type type)
        {
            ParseMethod method;
            if (BaseParseMethods.TryGetValue(type, out method))
                return method(text);

            if (type.IsEnum)
                return ParseEnum(text, type);

            throw new Exception("Cannot to serialize base type '" + type.ToString() + "'");
        }


        private delegate string SerializeMethod(object thing);
        private delegate object ParseMethod(string text);

        private static Dictionary<Type, SerializeMethod> BaseSerializeMethods = new Dictionary<Type, SerializeMethod>()
        {
            [typeof(string)]    = SerializeString,

            // integer types
            [typeof(int)]       = SerializeInt,
            [typeof(decimal)]   = SerializeInt,
            [typeof(long)]      = SerializeInt,
            [typeof(short)]     = SerializeInt,
            [typeof(uint)]      = SerializeInt,
            [typeof(ulong)]     = SerializeInt,
            [typeof(ushort)]    = SerializeInt,

            // floating point types
            [typeof(float)]     = SerializeFloat,
            [typeof(double)]    = SerializeFloat,

            [typeof(byte)]      = SerializeInt,
            [typeof(sbyte)]     = SerializeInt,

            [typeof(bool)]      = SerializeBool,
            [typeof(DateTime)]  = SerializeDateTime,
            [typeof(char)]      = SerializeChar,
        };

        private static Dictionary<Type, ParseMethod> BaseParseMethods = new Dictionary<Type, ParseMethod>()
        {
            [typeof(string)]    = ParseString,

            // integer types
            [typeof(int)]       = ParseInt,
            [typeof(decimal)]   = ParseDecimal,
            [typeof(long)]      = ParseLong,
            [typeof(short)]     = ParseShort,
            [typeof(uint)]      = ParseUint,
            [typeof(ulong)]     = ParseUlong,
            [typeof(ushort)]    = ParseUshort,

            // floating point types
            [typeof(float)]     = ParseFloat,
            [typeof(double)]    = ParseDouble,

            [typeof(byte)]      = ParseByte,
            [typeof(sbyte)]     = ParseSbyte,

            [typeof(bool)]      = ParseBool,
            [typeof(DateTime)]  = ParseDateTime,
            [typeof(char)]      = ParseChar,
        };


        #region base serialize/parse methods

        private static string SerializeString(object value)
        {
            string text = (string)value;
            if ((text[0] == ' ' || text[text.Length - 1] == ' ') ||
                (text[0] == '"' && text[text.Length - 1] == '"'))
                text = '"' + text + '"';

            if (text.Contains('#'))
            {
                text = text.Replace("#", "🍆");
                Debug.LogWarning("PENIS cannot serialize the character '#'. It is being replaced by the character '🍆'. Sorry :(");
            }

            return text;
        }
        private static object ParseString(string text)
        {
            if (text.Length > 1 && text[0] == '"' && text[text.Length - 1] == '"')
                text = text.Substring(1, text.Length - 2);

            return text;
        }

        private static readonly object[] DoublePrecision = new object[] { "0.#####################################################################################################################################################################################################################################################################################################################################" };
        private static string SerializeFloat(object value) // this method makes sure that maximum precision is used without going to scientific notation
        {
            var tostring = value.GetType().GetMethod("ToString", new Type[] { typeof(string) });
            string s = (string)tostring.Invoke(value, DoublePrecision);
            return s.ToLower();
        }
        private static string SerializeInt(object value)
        {
            return value.ToString().ToLower();
        }

        // all the annoying variations of the "number" object...
        private static object ParseInt      (string text) { return int.Parse(text); }
        private static object ParseDecimal  (string text) { return decimal.Parse(text); }
        private static object ParseLong     (string text) { return long.Parse(text); }
        private static object ParseShort    (string text) { return short.Parse(text); }
        private static object ParseUint     (string text) { return uint.Parse(text); }
        private static object ParseUlong    (string text) { return ulong.Parse(text); }
        private static object ParseUshort   (string text) { return ushort.Parse(text); }

        private static readonly NumberFormatInfo LowercaseParser = new NumberFormatInfo()
        {
            PositiveInfinitySymbol = "infinity",
            NegativeInfinitySymbol = "-infinity",
            NaNSymbol = "nan"
        };
        private static object ParseFloat    (string text) { return float.Parse(text.ToLower(), LowercaseParser); }
        private static object ParseDouble   (string text) { return double.Parse(text.ToLower(), LowercaseParser); }

        private static object ParseByte     (string text) { return byte.Parse(text); }
        private static object ParseSbyte    (string text) { return sbyte.Parse(text); }

        private static string SerializeBool(object value)
        {
            return value.ToString().ToLower();
        }
        private static object ParseBool(string text)
        {
            text = text.ToLower();
            if (text == "true") { return true; }
            if (text == "false") { return false; }
            throw new Exception("oh fuuuuuuuuuuuck");
        }

        private static string SerializeDateTime(object value)
        {
            DateTime dateTime = (DateTime)value;
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private static object ParseDateTime(string text)
        {
            DateTime result;
            DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
            return result;
        }

        private static string SerializeEnum(object value)
        {
            return value.ToString();
        }
        private static object ParseEnum(string text, Type type)
        {
            return Enum.Parse(type, text);
        }

        private static string SerializeChar(object value)
        {
            return value.ToString();
        }
        private static object ParseChar(string text)
        {
            return text[0];
        }

        #endregion
    }
}
