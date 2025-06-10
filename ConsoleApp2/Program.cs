// See https://aka.ms/new-console-template for more information



using System.Reflection;
using JECS;

class Program
{
    static void Main()
    {
        Console.WriteLine(string.Join(", ", typeof(TypeWithInheritedPrivateProperty).GetValidProperties().Select(p => $"{p.DeclaringType} | {p}")));
        
        
        Console.WriteLine(
            typeof(RegularTypeWithPublicProperty)
                .GetProperty(nameof(RegularTypeWithPublicProperty.PublicProperty))
                .GetImplementationPropertyInfo()
                .DeclaringType
            );
        
        Console.WriteLine(
            typeof(RegularTypeWithPrivateProperty)
                .GetProperty("PrivateProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .GetImplementationPropertyInfo()
                .DeclaringType
            );
        
        Console.WriteLine(
            typeof(TypeWithInheritedPublicProperty)
                .GetProperty(nameof(RegularTypeWithPublicProperty.PublicProperty))
                .GetImplementationPropertyInfo()
                .DeclaringType
            );
        
        Console.WriteLine(
            typeof(TypeWithInheritedPrivateProperty)
                .GetProperty("PrivateProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .GetImplementationPropertyInfo()
                .DeclaringType
            );
        
        Console.WriteLine();
        Console.WriteLine(string.Join(", ", typeof(TypeWithInheritedPrivateProperty).GetValidProperties().Select(p => $"{p.DeclaringType} | {p}")));
    }
}

class RegularTypeWithPublicProperty
{
    public int PublicProperty { get; set; }
}
class RegularTypeWithPrivateProperty
{
    public int PrivateProperty { get; private set; }
}

class TypeWithInheritedPublicProperty : RegularTypeWithPublicProperty
{
    
}
class TypeWithInheritedPrivateProperty : RegularTypeWithPrivateProperty
{
    
}





static class Ext
{
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
    
    internal static IEnumerable<PropertyInfo> GetValidProperties(this Type type)
    {
        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!p.CanRead) continue; // Skip unreadable properties
            if (p.GetIndexParameters().Length > 0) continue; // Skip indexer properties
                
            // Require a set method
            if (p.GetSetMethod(true) == null) continue;

            yield return p;
        }
    }
}



// var file = new DataFile("test");
//
//
// //file.SetAtPath("test", "A", "B", "C", "D", "E", "F", "G");
// //file.DeleteKeyAtPath("A", "B", "C", "D");
//
//
// file.Set("Test key", Random.Shared.NextDouble());
//
// // for (int i = 0; i < 1000; i++)
// // {
// //     file.Set(i.ToString(), Random.Shared.NextDouble());
// // }
//
//
// Console.WriteLine(file.FilePath);