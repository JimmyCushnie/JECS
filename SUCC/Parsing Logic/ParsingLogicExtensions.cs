using System;
using System.Linq;
using System.Reflection;

namespace SUCC.ParsingLogic
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


        // this is just to enable params
        internal static object Invoke(this MethodInfo method, object obj, params object[] parameters)
            => method.Invoke(obj, parameters);


        internal static bool GetOrSetIsPrivate(this PropertyInfo p)
            => p.GetMethod.IsPrivate || p.SetMethod.IsPrivate;


        /// <summary>
        /// Like C#'s `default` keyword, but it works on `Type` variables.
        /// </summary>
        internal static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            else
                return null;
        }
    }
}
