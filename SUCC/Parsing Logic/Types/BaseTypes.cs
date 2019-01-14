using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;

namespace SUCC
{
    public static class BaseTypes
    {
        /// <summary>
        /// Returns true if the type is a base type.
        /// </summary>
        public static bool IsBaseType(Type type)
        {
            if (type.IsEnum) return true;
            if (BaseSerializeMethods.ContainsKey(type)) return true;
            return false;
        }

        /// <summary> Turns an object into text </summary>
        public delegate string SerializeMethod(object thing);

        /// <summary> Turns text into an object </summary>
        public delegate object ParseMethod(string text);

        /// <summary>
        /// Add a base type to SUCC serialization. It is recommended that you call this method in a static constructor.
        /// </summary>
        public static void AddBaseType(Type type, SerializeMethod serializeMethod, ParseMethod parseMethod)
        {
            if (IsBaseType(type))
                throw new Exception($"Type {type} is already a supported base type. You cannot re-add it.");

            BaseSerializeMethods.Add(type, serializeMethod);
            BaseParseMethods.Add(type, parseMethod);
        }

        internal static string SerializeBaseType(object thing, Type type)
        {
            SerializeMethod method;
            if (BaseSerializeMethods.TryGetValue(type, out method))
                return method(thing);

            if (type.IsEnum)
                return SerializeEnum(thing);

            throw new Exception($"Cannot serialize base type {type} - are you sure it is a base type?");
        }

        internal static object ParseBaseType(string text, Type type)
        {
            ParseMethod method;
            if (BaseParseMethods.TryGetValue(type, out method))
                return method(text);

            if (type.IsEnum)
                return ParseEnum(text, type);

            throw new Exception($"Cannot parse base type {type} - are you sure it is a base type?");
        }


        private static Dictionary<Type, SerializeMethod> BaseSerializeMethods = new Dictionary<Type, SerializeMethod>()
        {
            [typeof(string)] = SerializeString,

            // integer types
            [typeof(int)] = SerializeInt,
            [typeof(decimal)] = SerializeInt,
            [typeof(long)] = SerializeInt,
            [typeof(short)] = SerializeInt,
            [typeof(uint)] = SerializeInt,
            [typeof(ulong)] = SerializeInt,
            [typeof(ushort)] = SerializeInt,

            // floating point types
            [typeof(float)] = SerializeFloat,
            [typeof(double)] = SerializeFloat,

            [typeof(byte)] = SerializeInt,
            [typeof(sbyte)] = SerializeInt,

            [typeof(bool)] = SerializeBool,
            [typeof(DateTime)] = SerializeDateTime,
            [typeof(char)] = SerializeChar,
            [typeof(Type)] = SerializeType,
        };

        private static Dictionary<Type, ParseMethod> BaseParseMethods = new Dictionary<Type, ParseMethod>()
        {
            [typeof(string)] = ParseString,

            // integer types
            [typeof(int)] = ParseInt,
            [typeof(decimal)] = ParseDecimal,
            [typeof(long)] = ParseLong,
            [typeof(short)] = ParseShort,
            [typeof(uint)] = ParseUint,
            [typeof(ulong)] = ParseUlong,
            [typeof(ushort)] = ParseUshort,

            // floating point types
            [typeof(float)] = ParseFloat,
            [typeof(double)] = ParseDouble,

            [typeof(byte)] = ParseByte,
            [typeof(sbyte)] = ParseSbyte,

            [typeof(bool)] = ParseBool,
            [typeof(DateTime)] = ParseDateTime,
            [typeof(char)] = ParseChar,
            [typeof(Type)] = ParseType,
        };


        #region base serialize/parse methods

        private static string SerializeString(object value)
        {
            string text = (string)value;
            if (String.IsNullOrEmpty(text)) { return text; }

            if (
                text[0] == ' '
                || text[text.Length - 1] == ' '
                || (text[0] == '"' && text[text.Length - 1] == '"')
                )
                text = '"' + text + '"';

            return text;
        }
        private static object ParseString(string text)
        {
            if (
                text.Length > 1 
                && text[0] == '"' && text[text.Length - 1] == '"'
                )
                text = text.Substring(1, text.Length - 2);

            return text;
        }

        // support for multi-line strings
        internal static void SerializeSpecialStringCase(string value, Node node)
        {
            if (value != null && value.Contains(Environment.NewLine))
            {
                node.Value = "\"\"\"";

                node.ChildLines.Clear();
                node.ChildNodes.Clear();

                int indentation = node.IndentationLevel + Utilities.IndentationCount;
                using (StringReader sr = new StringReader(value))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null) // this is effectively a ForEachLine, but it is platform agnostic (since new lines are encoded differently on different OSs)
                    {
                        string text = new string(' ', indentation) + SerializeString(line);
                        text = text.Replace("#", "\\#"); // good god this code is a fucking mess
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
                node.Value = SerializeString(value);
            }
        }
        internal static string ParseSpecialStringCase(Node node)
        {
            string text = string.Empty;

            foreach (var line in node.ChildLines)
            {
                var lineText = line.RawText;

                // remove everything after the comment indicator, unless it's preceded by a \

                int PoundSignIndex = lineText.IndexOf('#');

                while (PoundSignIndex > 0 && text[PoundSignIndex - 1] == '\\')
                    PoundSignIndex = text.IndexOf('#', PoundSignIndex + 1);

                if (PoundSignIndex > 0)
                    lineText = lineText.Substring(0, PoundSignIndex - 1);

                lineText = lineText.Trim();

                if (lineText == "\"\"\"")
                {
                    break;
                }

                lineText = (string)ParseString(lineText); // to remove quotations

                text += lineText;
                text += Environment.NewLine;
            }

            return text.TrimEnd(Environment.NewLine.ToCharArray()); // remove all newlines at the end of the string
        }

        private static readonly object[] DoublePrecision = new object[] { "0.#####################################################################################################################################################################################################################################################################################################################################" };
        private static string SerializeFloat(object value) // this method makes sure that maximum precision is used without going to scientific notation
        {
            var tostring = value.GetType().GetMethod("ToString", new Type[] { typeof(string) });
            string s = (string)tostring.Invoke(obj: value, parameters: DoublePrecision);
            return s.ToLower();
        }
        private static string SerializeInt(object value)
        {
            return value.ToString().ToLower();
        }

        // all the annoying variations of the "number" object...
        private static object ParseInt(string text)     => int.Parse(text);
        private static object ParseDecimal(string text) => decimal.Parse(text);
        private static object ParseLong(string text)    => long.Parse(text);
        private static object ParseShort(string text)   => short.Parse(text);
        private static object ParseUint(string text)    => uint.Parse(text);
        private static object ParseUlong(string text)   => ulong.Parse(text);
        private static object ParseUshort(string text)  => ushort.Parse(text);
        private static object ParseByte(string text)    => byte.Parse(text);
        private static object ParseSbyte(string text)   => sbyte.Parse(text);

        private static readonly NumberFormatInfo LowercaseParser = new NumberFormatInfo()
        {
            PositiveInfinitySymbol = "infinity",
            NegativeInfinitySymbol = "-infinity",
            NaNSymbol = "nan"
        };
        private static object ParseFloat(string text) => float.Parse(text.ToLower(), LowercaseParser);
        private static object ParseDouble(string text) => double.Parse(text.ToLower(), LowercaseParser);



        private static string SerializeBool(object value)
        {
            return value.ToString().ToLower();
        }
        private static object ParseBool(string text)
        {
            text = text.ToLower();
            if (text == "true") return true;
            if (text == "false") return false;
            throw new FormatException($"cannot parse text {text} as boolean");
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

        private static string SerializeChar(object value)
        {
            return value.ToString();
        }
        private static object ParseChar(string text)
        {
            return text[0];
        }

        private static string SerializeType(object value)
        {
            Type t = (Type)value;
            return t.FullName;
        }

        private static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();
        private static object ParseType(string typeName)
        {
            Type type;
            if (TypeCache.TryGetValue(typeName, out type))
                return type;

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(typeName);

                if (type != null)
                {
                    TypeCache.Add(typeName, type);
                    return type;
                }
            }

            throw new FormatException($"cannot parse text {typeName} as System.Type");
        }

        private static string SerializeEnum(object value)
        {
            return value.ToString();
        }
        private static object ParseEnum(string text, Type type)
        {
            return Enum.Parse(type, text);
        }

        #endregion
    }
}
