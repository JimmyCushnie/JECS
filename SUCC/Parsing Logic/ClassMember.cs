using System;
using System.Reflection;

namespace SUCC.ParsingLogic
{
    internal sealed class ClassMember
    {
        public string Name { get; }
        public Type MemberType { get; }
        public MemberInfo Member { get; }

        private ClassMember(MemberInfo member, Type memberType, string name = null)
        {
            this.Member = member;
            this.MemberType = memberType;
            this.Name = name ?? member.Name;
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

        public ClassMember WithName(string name) => new ClassMember(Member, MemberType, name);

        public static implicit operator ClassMember(FieldInfo field) => new ClassMember(field, field.FieldType);
        public static implicit operator ClassMember(PropertyInfo prop) => new ClassMember(prop, prop.PropertyType);
    }
}
