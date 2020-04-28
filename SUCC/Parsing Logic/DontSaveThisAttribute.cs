using System;

namespace SUCC
{
    /// <summary>
    /// Public fields and properties with this attribute will NOT be saved and loaded by SUCC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DontSaveThisAttribute : Attribute 
    { 
    }
}
