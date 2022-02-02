using System;
using System.Collections.Generic;
using System.Reflection;

namespace SUCC.ParsingLogic
{
    // To save and load generic collection types, SUCC must be able to call generic methods where
    // the generic parameters are not known at compile time. To that end, reflection is used to
    // create bound generic methods at runtime. The classes here cache these runtime-created methods
    // to minimize GC.

    internal class GenericMethodHelper
    {
        readonly MethodInfo UnboundGenericMethod;

        public GenericMethodHelper(MethodInfo genericMethod)
        {
            if (genericMethod.GetGenericArguments().Length != 1)
                throw new ArgumentException($"{nameof(genericMethod)} must have exactly 1 generic parameter");

            UnboundGenericMethod = genericMethod;
        }


        private readonly Dictionary<Type, MethodInfo> CachedMethods = new Dictionary<Type, MethodInfo>();
        public MethodInfo GetBoundGenericMethod(Type genericArgument)
        {
            if (CachedMethods.TryGetValue(genericArgument, out var method))
                return method;

            method = UnboundGenericMethod.MakeGenericMethod(genericArgument);
            CachedMethods.Add(genericArgument, method);
            return method;
        }
    }

    internal class GenericMethodHelper_2
    {
        readonly MethodInfo UnboundGenericMethod;

        public GenericMethodHelper_2(MethodInfo genericMethod)
        {
            if (genericMethod.GetGenericArguments().Length != 2)
                throw new ArgumentException($"{nameof(genericMethod)} must have exactly 2 generic parameters");

            UnboundGenericMethod = genericMethod;
        }


        private readonly Dictionary<Type, Dictionary<Type, MethodInfo>> CachedMethods = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
        public MethodInfo GetBoundGenericMethod(Type genericArgument1, Type genericArgument2)
        {
            if (CachedMethods.TryGetValue(genericArgument1, out var cachedMethodsLevel2))
            {
                if (cachedMethodsLevel2.TryGetValue(genericArgument2, out var method))
                    return method;

                return CreateAndCacheMethod();
            }

            cachedMethodsLevel2 = new Dictionary<Type, MethodInfo>();
            CachedMethods.Add(genericArgument1, cachedMethodsLevel2);

            return CreateAndCacheMethod();


            MethodInfo CreateAndCacheMethod()
            {
                var method = UnboundGenericMethod.MakeGenericMethod(genericArgument1, genericArgument2);
                cachedMethodsLevel2.Add(genericArgument2, method);
                return method;
            }
        }
    }
}
