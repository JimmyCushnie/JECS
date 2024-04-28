using System;

namespace JECS
{
    /// <summary>
    /// Public fields and properties with this attribute will NOT be saved and loaded by JECS.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DontSaveThisAttribute : Attribute 
    { 
    }
}
