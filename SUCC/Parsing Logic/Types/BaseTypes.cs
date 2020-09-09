using SUCC.ParsingLogic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SUCC
{
    /// <summary>
    /// Manages SUCC's database of Base Types. https://github.com/JimmyCushnie/SUCC/wiki/Base-Types
    /// </summary>
    public static class BaseTypes
    {
        internal static string SerializeBaseType<T>(T data, FileStyle style) => SerializeBaseType(data, typeof(T), style);
        internal static string SerializeBaseType(object data, Type type, FileStyle style)
        {
            if (BaseSerializeMethods.TryGetValue(type, out var method))
                return method(data);

            if (BaseStyledSerializeMethods.TryGetValue(type, out var stylemethod))
                return stylemethod(data, style);

            if (type.IsEnum)
                return SerializeEnum(data, style);

            throw new Exception($"Cannot serialize base type {type} - are you sure it is a base type?");
        }


        /// <summary> Turn some text into data, if that data is of a base type. </summary>
        public static T ParseBaseType<T>(string text) => (T)ParseBaseType(text, typeof(T));

        /// <summary> Non-generic version of <see cref="ParseBaseType{T}(string)"/> </summary>
        public static object ParseBaseType(string text, Type type)
        {
            if (TryParseBaseType(text, type, out var result))
                return result;

            throw new Exception($"Failed to parse text '{text}' as base type {type}");
        }

        /// <summary> Attempt to turn some text into data, if that data is of a base type. </summary>
        public static bool TryParseBaseType<T>(string text, out T result)
        {
            bool success = TryParseBaseType(text, typeof(T), out object _result);
            result = (T)_result;
            return success;
        }

        /// <summary> Non-generic version of <see cref="TryParseBaseType{T}(string, out T)"/> </summary>
        public static bool TryParseBaseType(string text, Type type, out object result)
        {
            if (!IsBaseType(type))
                throw new Exception($"Cannot parse: {type} is not a base type");


            try
            {
                if (type.IsEnum)
                {
                    result = ParseEnum(text, type);
                    return true;
                }

                result = BaseParseMethods[type].Invoke(text);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }


        /// <summary> Returns true if the type is a base type. </summary>
        public static bool IsBaseType(Type type)
        {
            if (type.IsEnum) return true;
            if (BaseSerializeMethods.ContainsKey(type)) return true;
            if (BaseStyledSerializeMethods.ContainsKey(type)) return true;
            return false;
        }

        /// <summary> Turns an object into text </summary>
        public delegate string SerializeMethod<T>(T data);
        /// <summary> Turns text into an object </summary>
        public delegate T ParseMethod<T>(string text);

        private delegate string SerializeMethod(object data);
        private delegate object ParseMethod(string text);

        /// <summary> Add a base type to SUCC serialization. It is recommended that you call this method in a static constructor. </summary>
        public static void AddBaseType<T>(SerializeMethod<T> serializeMethod, ParseMethod<T> parseMethod)
        {
            AddBaseType
                (
                typeof(T), 
                (object obj) => serializeMethod((T)obj),
                (string text) => parseMethod.Invoke(text)
                );
        }

        private static void AddBaseType(Type type, SerializeMethod serializeMethod, ParseMethod parseMethod)
        {
            if (IsBaseType(type))
                throw new Exception($"Type {type} is already a supported base type. You cannot re-add it.");

            BaseSerializeMethods.Add(type, serializeMethod);
            BaseParseMethods.Add(type, parseMethod);
        }





        private static Dictionary<Type, SerializeMethod> BaseSerializeMethods { get; } = new Dictionary<Type, SerializeMethod>()
        {
            // Integer types
            [typeof(int)] = SerializeInt,
            [typeof(long)] = SerializeLong,
            [typeof(short)] = SerializeShort,
            [typeof(uint)] = SerializeUint,
            [typeof(ulong)] = SerializeUlong,
            [typeof(ushort)] = SerializeUshort,
            [typeof(byte)] = SerializeByte,
            [typeof(sbyte)] = SerializeSbyte,

            // Floating point types
            [typeof(float)] = SerializeFloat,
            [typeof(double)] = SerializeDouble,
            [typeof(decimal)] = SerializeDecimal,

            // Misc
            [typeof(char)] = SerializeChar,
            [typeof(Type)] = SerializeType,

            [typeof(DateTime)] = SerializeDateTime,
            [typeof(Version)] = SerializeVersion,
        };

        private delegate string StyledSerializeMethod(object data, FileStyle style);
        private static Dictionary<Type, StyledSerializeMethod> BaseStyledSerializeMethods { get; } = new Dictionary<Type, StyledSerializeMethod>()
        {
            [typeof(string)] = SerializeString,
            [typeof(bool)] = SerializeBool,
        };

        private static Dictionary<Type, ParseMethod> BaseParseMethods { get; } = new Dictionary<Type, ParseMethod>()
        {
            [typeof(string)] = ParseString,

            // integer types
            [typeof(int)] = ParseInt,
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
            [typeof(decimal)] = ParseDecimal,

            // Misc
            [typeof(bool)] = ParseBool,
            [typeof(char)] = ParseChar,
            [typeof(Type)] = ParseType,

            [typeof(DateTime)] = ParseDateTime,
            [typeof(Version)] = ParseVersion,
        };


        #region base serialize/parse methods

        private static string SerializeString(object value, FileStyle style)
        {
            string text = (string)value;

            if (String.IsNullOrEmpty(text))
                return String.Empty;

            text = text.Replace("\t", "    "); // SUCC files cannot contain tabs. Prevent saving strings with tabs in them.

            if (
                style.AlwaysQuoteStrings 
                || text[0] == ' '
                || text[text.Length - 1] == ' '
                || text.IsQuoted()
                || text == Utilities.NullIndicator
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

        // Support for multi-line strings
        internal static void SetStringSpecialCase(Node node, string value, FileStyle style)
        {
            if (value != null && value.ContainsNewLine())
            {
                node.Value = MultiLineStringNode.Terminator;
                var lines = value.SplitIntoLines();

                node.CapChildCount(lines.Length + 1);

                for (int i = 0; i < lines.Length; i++)
                {
                    var newnode = node.GetChildAddresedByStringLineNumber(i);
                    string lineValue = BaseTypes.SerializeString(lines[i], style);

                    if (lineValue.EndsWith(MultiLineStringNode.NoLineBreakIndicator))
                        lineValue = lineValue.Quote();

                    newnode.Value = lineValue;
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
        internal static string ParseSpecialStringCase(Node parentNode)
        {
            string text = string.Empty;

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                var lineNode = parentNode.ChildNodes[i] as MultiLineStringNode;

                if (i == parentNode.ChildNodes.Count - 1)
                {
                    if (lineNode.IsTerminator) 
                        break;
                    else 
                        throw new FormatException($"Error parsing multi line string: the final child was not a terminator. Line so far was '{text}'");
                }

                text += (string)ParseString(lineNode.Value);

                if (AddLineBreak())
                    text += Utilities.NewLine;


                bool AddLineBreak()
                {
                    if (i == parentNode.ChildNodes.Count - 2) // Is the line before the terminator
                        return false;

                    return !lineNode.Value.EndsWith(MultiLineStringNode.NoLineBreakIndicator);       
                }
            }

            return text;
        }

        private static string SerializeInt(object value)    => ((int)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeLong(object value)   => ((long)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeShort(object value)  => ((short)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeUint(object value)   => ((uint)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeUlong(object value)  => ((ulong)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeUshort(object value) => ((ushort)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeByte(object value)   => ((byte)value).ToString(NumberFormatInfo.InvariantInfo);
        private static string SerializeSbyte(object value)  => ((sbyte)value).ToString(NumberFormatInfo.InvariantInfo);

        // This lets us use decimal places instead of scientific notation. Yes, it's horrible.
        // See https://docs.microsoft.com/en-us/dotnet/api/system.single.tostring?view=netframework-4.7.2#System_Single_ToString_System_String_
        private const string DoublePrecision = "0.#####################################################################################################################################################################################################################################################################################################################################";

        private static string SerializeFloat(object value)
        {
            var f = (float)value;

            if (float.IsPositiveInfinity(f)) 
                return "infinity";

            if (float.IsNegativeInfinity(f)) 
                return "-infinity";

            if (float.IsNaN(f)) 
                return "nan";

            return f.ToString(DoublePrecision, NumberFormatInfo.InvariantInfo);
        }
        private static string SerializeDouble(object value)
        {
            var d = (double)value;

            if (double.IsPositiveInfinity(d)) return "infinity";
            if (double.IsNegativeInfinity(d)) return "-infinity";
            if (double.IsNaN(d)) return "nan";

            return d.ToString(DoublePrecision, NumberFormatInfo.InvariantInfo);
        }
        private static string SerializeDecimal(object value)
        {
            var d = (decimal)value;
            return d.ToString(NumberFormatInfo.InvariantInfo);
        }

        private static object ParseInt(string text)     => int.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseLong(string text)    => long.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseShort(string text)   => short.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseUint(string text)    => uint.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseUlong(string text)   => ulong.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseUshort(string text)  => ushort.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseByte(string text)    => byte.Parse(text, NumberFormatInfo.InvariantInfo);
        private static object ParseSbyte(string text)   => sbyte.Parse(text, NumberFormatInfo.InvariantInfo);

        private static object ParseFloat(string text)   => ParseFloatWithRationalSupport(text, float.Parse,       (float a, float b) => a / b);
        private static object ParseDouble(string text)  => ParseFloatWithRationalSupport(text, double.Parse,    (double a, double b) => a / b);
        private static object ParseDecimal(string text) => ParseFloatWithRationalSupport(text, decimal.Parse, (decimal a, decimal b) => a / b);

        private static T ParseFloatWithRationalSupport<T>(string text, Func<string, IFormatProvider, T> parseMethod, Func<T, T, T> divideMethod)
        {
            // We only really needed to support one /, but honestly supporting infinity of them is easier.
            if (text.Contains('/'))
            {
                var numbers = text.Split('/').Select(parse).ToArray();
                T result = numbers[0];

                for (int i = 1; i < numbers.Length; i++)
                    result = divideMethod.Invoke(result, numbers[i]);

                return result;
            }

            return parse(text);

            T parse(string floatText)
                => parseMethod.Invoke(floatText.ToLower().Trim(), LowercaseParser);
        }

        private static readonly NumberFormatInfo LowercaseParser = new NumberFormatInfo()
        {
            PositiveInfinitySymbol = "infinity",
            NegativeInfinitySymbol = "-infinity",
            NaNSymbol = "nan"
        };



        private static readonly string[] TrueStrings = new string[] { "true", "on", "yes", "y", };
        private static readonly string[] FalseStrings = new string[] { "false", "off", "no", "n", };

        private static string SerializeBool(object value, FileStyle style)
        {
            bool b = (bool)value;
            switch (style.BoolStyle)
            {
                case BoolStyle.true_false: default:
                    return b ? "true" : "false";
                case BoolStyle.on_off:
                    return b ? "on" : "off";
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

        private static Dictionary<string, Type> TypeCache { get; } = new Dictionary<string, Type>();
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


        private static string SerializeDateTime(object value)
        {
            var dateTime = (DateTime)value;
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private static object ParseDateTime(string text)
        {
            DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
            return result;
        }

        private static string SerializeVersion(object value)
        {
            var version = (Version)value;
            return version.ToString(4);
        }
        private static object ParseVersion(string text)
        {
            return Version.Parse(text);
        }

        #endregion
    }
}
