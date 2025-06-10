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
        
        /// <summary>
        /// If this is a regular property, just returns the PropertyInfo.
        /// If this property is inherited from a base class, it returns the PropertyInfo from the base class where it was declared.
        /// If this property is overridden from a virtual or abstract property, it returns the PropertyInfo where the override happens.
        /// Basically, this allows you to get the PropertyInfo where the code actually lives that gets executed when you get/set the property.
        /// </summary>
        internal static PropertyInfo GetImplementationPropertyInfo(this PropertyInfo property)
        {
            // Start with the declaring type of the property
            Type type = property.DeclaringType;

            while (type != null)
            {
                // Attempt to get the property from the current type
                PropertyInfo localProperty = type.GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (localProperty != null)
                {
                    // If the property is overridden, the MethodInfo for GetMethod will differ from the base's MethodInfo
                    if (property.GetMethod != null && localProperty.GetMethod != null && property.GetMethod.GetBaseDefinition() == localProperty.GetMethod.GetBaseDefinition())
                    {
                        return localProperty;
                    }
                }

                // Move to the base class
                type = type.BaseType;
            }

            // In case no implementation is found in the hierarchy, return the original property
            return property;
        }
    }
}
