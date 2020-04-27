using System;
using System.Reflection;

namespace SUCC
{
    internal class ClassMember
    {
        public string Name => Member.Name;
        public Type MemberType { get; }
        public MemberInfo Member { get; }

        private ClassMember(MemberInfo member, Type memberType)
        {
            this.Member = member;
            this.MemberType = memberType;
        }

        public object GetValue(object obj)
        {
            return Member is FieldInfo field ? field.GetValue(obj)
                 : Member is PropertyInfo prop ? prop.GetValue(obj)
                 : throw new Exception("??");
        }

        public void SetValue(object obj, object value)
        {
            if (Member is FieldInfo field)
                field.SetValue(obj, value);
            else if (Member is PropertyInfo prop)
                prop.SetValue(obj, value);
        }

        public static implicit operator ClassMember(FieldInfo field) => new ClassMember(field, field.FieldType);
        public static implicit operator ClassMember(PropertyInfo prop) => new ClassMember(prop, prop.PropertyType);
    }
}
