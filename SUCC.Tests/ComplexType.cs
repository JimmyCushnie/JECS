using System;

namespace SUCC.Tests
{
    public class ComplexType
    {
        public int Integer;
        [DoSave] private string String;
        [DoSave] public bool Boolean { get; private set; }

        // parameterless constructor required for reflection
        public ComplexType() { }
        public ComplexType(int integer, string text, bool boolean)
        {
            Integer = integer;
            String = text;
            Boolean = boolean;
        }

        public override bool Equals(object obj)
        {
            var other = (ComplexType)obj;
            if (other == null) 
                return false;

            return this.Integer == other.Integer 
                && this.String == other.String 
                && this.Boolean == other.Boolean;
        }

        public override int GetHashCode()
            => Integer;

        public override string ToString()
            => $"{Integer} | {String} | {Boolean}";






        public static ComplexType PropertyShortcut 
            => new ComplexType(0, "string", false);

        public static ComplexType MethodShortcut(int integer, string text, bool boolean)
            => new ComplexType(integer, text, boolean);

        public static ComplexType Shortcut(string shortcut)
        {
            if (shortcut == "shortcut1")
                return new ComplexType(0, "you sly dog, you've got me monologuing!", false);

            throw new Exception("invalid shortcut");
        }
    }
}