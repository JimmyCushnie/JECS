using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;
using System.Linq;

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
            if (BaseStyledSerializeMethods.ContainsKey(type)) return true;
            return false;
        }

        /// <summary> Turns an object into text </summary>
        public delegate string SerializeMethod(object thing);

        private delegate string StyledSerializeMethod(object thing, FileStyle style);

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

        internal static string SerializeBaseType<T>(T thing, FileStyle style) => SerializeBaseType(thing, typeof(T), style);
        internal static string SerializeBaseType(object thing, Type type, FileStyle style)
        {
            if (BaseSerializeMethods.TryGetValue(type, out var method))
                return method(thing);

            if (BaseStyledSerializeMethods.TryGetValue(type, out var stylemethod))
                return stylemethod(thing, style);

            if (type.IsEnum)
                return SerializeEnum(thing, style);

            throw new Exception($"Cannot serialize base type {type} - are you sure it is a base type?");
        }

        internal static T ParseBaseType<T>(string text) => (T)ParseBaseType(text, typeof(T));
        internal static object ParseBaseType(string text, Type type)
        {
            try
            {
                if (BaseParseMethods.TryGetValue(type, out var method))
                    return method(text);

                if (type.IsEnum)
                    return ParseEnum(text, type);
            }
            catch { throw new Exception($"Error parsing text {text} as type {type}"); }

            throw new Exception($"Cannot parse base type {type} - are you sure it is a base type?");
        }


        private static Dictionary<Type, SerializeMethod> BaseSerializeMethods = new Dictionary<Type, SerializeMethod>()
        {
            // integer types
            [typeof(int)] = SerializeInt,
            [typeof(decimal)] = SerializeInt, // decimals aren't actually ints, but they use the same serialize method as them
            [typeof(long)] = SerializeInt,
            [typeof(short)] = SerializeInt,
            [typeof(uint)] = SerializeInt,
            [typeof(ulong)] = SerializeInt,
            [typeof(ushort)] = SerializeInt,
            [typeof(byte)] = SerializeInt,
            [typeof(sbyte)] = SerializeInt,

            // floating point types
            [typeof(float)] = SerializeFloat,
            [typeof(double)] = SerializeFloat,

            [typeof(DateTime)] = SerializeDateTime,
            [typeof(char)] = SerializeChar,
            [typeof(Type)] = SerializeType,
        };

        private static Dictionary<Type, StyledSerializeMethod> BaseStyledSerializeMethods = new Dictionary<Type, StyledSerializeMethod>()
        {
            [typeof(string)] = SerializeString,
            [typeof(bool)] = SerializeBool,
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
            [typeof(byte)] = ParseByte,
            [typeof(sbyte)] = ParseSbyte,

            // floating point types
            [typeof(float)] = ParseFloat,
            [typeof(double)] = ParseDouble,

            [typeof(bool)] = ParseBool,
            [typeof(DateTime)] = ParseDateTime,
            [typeof(char)] = ParseChar,
            [typeof(Type)] = ParseType,
        };


        #region base serialize/parse methods

        private static string SerializeString(object value, FileStyle style)
        {
            string text = (string)value;
            if (String.IsNullOrEmpty(text)) return String.Empty;

            if (
                style.AlwaysQuoteStrings ||
                text[0] == ' ' || text[text.Length - 1] == ' ' ||
                text.IsQuoted()
                )
                text = text.Quote();

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
        internal static void SerializeSpecialStringCase(string value, Node node, FileStyle style)
        {
            if (value != null && value.ContainsNewLine())
            {
                node.Value = MultiLineStringNode.Terminator;
                var lines = value.SplitIntoLines();

                node.CapChildCount(lines.Length + 1);

                for (int i = 0; i < lines.Length; i++)
                {
                    var newnode = node.GetChildAddresedByStringLineNumber(i);
                    newnode.Value = BaseTypes.SerializeString(lines[i], style);
                }

                node.GetChildAddresedByStringLineNumber(lines.Length).MakeTerminator();
                return;
            }
            else
            {
                node.ClearChildren();
                node.Value = BaseTypes.SerializeString(value, style);
            }
        }
        internal static string ParseSpecialStringCase(Node node)
        {
            string text = string.Empty;

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var line = node.ChildNodes[i] as MultiLineStringNode;

                if (i == node.ChildNodes.Count - 1)
                {
                    if (line.IsTerminator) break;
                    else throw new FormatException($"error parsing multi line string: the final child was not a terminator. Line so far was '{text}'");
                }

                text += (string)ParseString(line.Value);
                if (i != node.ChildNodes.Count - 2)
                    text += Utilities.NewLine;
            }

            return text;
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



        private static readonly string[] TrueStrings = new string[] { "true", "yes", "y", };
        private static readonly string[] FalseStrings = new string[] { "false", "no", "n", };

        private static string SerializeBool(object value, FileStyle style)
        {
            bool b = (bool)value;
            switch (style.BoolStyle)
            {
                case BoolStyle.true_false: default:
                    return b ? "true" : "false";
                case BoolStyle.yes_no:
                    return b ? "yes" : "no";
                case BoolStyle.y_n:
                    return b ? "y" : "n";
            }
        }
        private static object ParseBool(string text)
        {
            text = text.ToLower();
            if (TrueStrings.Contains(text)) return true;
            if (FalseStrings.Contains(text)) return false;
            throw new FormatException($"cannot parse text {text} as boolean");
        }

        private static string SerializeDateTime(object value)
        {
            DateTime dateTime = (DateTime)value;
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private static object ParseDateTime(string text)
        {
            DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
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
            if (TypeCache.TryGetValue(typeName, out Type type))
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

        private static string SerializeEnum(object value, FileStyle style)
        {
            switch (style.EnumStyle)
            {
                case EnumStyle.name: default:
                    return value.ToString();
                case EnumStyle.number:
                    return ((Enum)value).ToString("d");
            }
        }
        private static object ParseEnum(string text, Type type)
        {
            return Enum.Parse(type, text, ignoreCase: true);
        }

        #endregion
    }
}
