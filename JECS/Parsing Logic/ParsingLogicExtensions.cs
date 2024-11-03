using System;
using System.Linq;
using System.Reflection;

namespace JECS.ParsingLogic
{
    internal static class ParsingLogicExtensions
    {
        internal static string Quote(this string s)
            => '"' + s + '"';

        internal static bool IsQuoted(this string s)
            => s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"';

        internal static string UnQuote(this string s)
        {
            if (!s.IsQuoted()) return s;
            return s.Substring(1, s.Length - 2);
        }

        /// <summary> the number of spaces in the string that precede the first non-space character </summary>
        internal static int GetIndentationLevel(this string s)
            => s == null ? 0 : s.TakeWhile(c => c == ' ').Count();

        internal static string AddSpaces(this string s, int count)
            => s + new string(' ', count);

        internal static string[] SplitIntoLines(this string s)
            => s.Replace("\r\n", "\n").Split('\n'); // fuck windows line endings. WHY are they still used.

        internal static bool ContainsNewLine(this string s)
            => s.Contains('\n');

        internal static bool IsWhitespace(this string s)
            => s.Trim().Length == 0;

        internal static bool IsNullOrEmpty(this string s)
            => String.IsNullOrEmpty(s);


        // this extension is just to enable params
        internal static object Invoke(this MethodInfo method, object obj, params object[] parameters)
            => method.Invoke(obj, parameters);


        /// <summary>
        /// Like C#'s `default` keyword, but it works on `Type` variables.
        /// </summary>
        internal static object GetDefaultValue(this Type t)
        {
            if (t.IsNullableType())
                return null;

            return Activator.CreateInstance(t);
        }

        internal static bool IsNullableType(this Type type)
        {
            if (!type.IsValueType)
                return true;

            return Nullable.GetUnderlyingType(type) != null;
        }

        // internal static PropertyInfo GetDeclaredPropertyInfo(this PropertyInfo possiblyInheritedProperty)
        // {
        //     possiblyInheritedProperty.DeclaringType.BaseType
        // }
    }
}
