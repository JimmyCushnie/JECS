using System;
using System.Reflection;

namespace SUCC.InternalParsingLogic
{
    internal static class ComplexTypeShortcuts
    {
        internal static object GetFromShortcut(string shortcut, Type type)
        {
            object result;
            if (TryPropertyShortcut(shortcut, type, out result)) return result;
            if (TryConstructorShortcut(shortcut, type, out result)) return result;
            if (TryMethodShortcut(shortcut, type, out result)) return result;
            if (TryCustomShortCut(shortcut, type, out result)) return result;

            throw new FormatException($"{shortcut} is not a valid shortcut for type {type}");
        }

        private static bool TryPropertyShortcut(string shortcut, Type type, out object result)
        {
            result = null;
            var p = type.GetProperty(name: shortcut, BindingFlags.Public | BindingFlags.Static);

            if (p == null) return false;
            if (p.CanWrite || p.PropertyType != type) return false;

            result = p.GetValue(null);
            return true;
        }

        private static bool TryConstructorShortcut(string shortcut, Type type, out object result)
        {
            try {
            if (shortcut.StartsWith("(") && shortcut.EndsWith(")"))
            {
                string text = shortcut.Substring(1, shortcut.Length - 2); // remove the ( and )
                var paramStrings = text.Split(',');

                var constructors = type.GetConstructors();

                ConstructorInfo constructor = constructors[0];

                if (constructors.Length > 1)
                {
                    foreach (var c in constructors)
                    {
                        // todo: it would be nice to check constructor parameter types for compatibility with paramStrings.
                        // say a type had one constructor that took a string, and one constructor that took an int. We should be able to pick the right one.
                        if (c.GetParameters().Length == paramStrings.Length)
                        {
                            constructor = c;
                            break;
                        }
                    }
                }

                var parameters = constructor.GetParameters();
                var constructorParams = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i < paramStrings.Length)
                    {
                        constructorParams[i] = BaseTypes.ParseBaseType(paramStrings[i].Trim(), parameters[i].ParameterType);
                    }
                    else // optional parameter support
                    {
                        constructorParams[i] = parameters[i].DefaultValue;
                    }
                }

                result = constructor.Invoke(constructorParams);
                return true;
            } } catch { } // I am a good programmer

            result = null;
            return false;
        }

        private static bool TryMethodShortcut(string shortcut, Type type, out object result)
        {
            try { 
            if (shortcut.Contains("(") && shortcut.Contains(")"))
            {
                string methodname = shortcut.Substring(0, shortcut.IndexOf('('));
                var method = type.GetMethod(methodname, BindingFlags.Public | BindingFlags.Static);

                if (method != null && method.ReturnType == type)
                {
                    var parameters = method.GetParameters();

                    string s = shortcut.Substring(shortcut.IndexOf('(') + 1, shortcut.Length - shortcut.IndexOf('(') - 2);
                    var paramStrings = s.Split(',');

                    var methodParams = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i < paramStrings.Length)
                        {
                            methodParams[i] = BaseTypes.ParseBaseType(paramStrings[i].Trim(), parameters[i].ParameterType);
                        }
                        else // optional parameter support
                        {
                            methodParams[i] = parameters[i].DefaultValue;
                        }
                    }

                    result = method.Invoke(null, methodParams);
                    return true;
                }
            }} catch { } // Who am I kidding. I am a bad programmer :(

            result = null;
            return false;
        }

        private static bool TryCustomShortCut(string shortcut, Type type, out object result)
        {
            try { 
            var m = type.GetMethod("Shortcut", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(String) }, null);
            if (m != null && m.ReturnType == type)
            {
                result = m.Invoke(null, new object[] { shortcut });
                return true;
            }} catch { }

            result = null;
            return false;
        }
    }
}
