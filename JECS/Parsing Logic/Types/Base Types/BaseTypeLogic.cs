using System;
using JECS.ParsingLogic;

namespace JECS.ParsingLogic
{
    public abstract class BaseTypeLogic
    {
        public abstract Type ApplicableType { get; }

        public abstract string SerializeObject(object value, FileStyle style);
        public abstract object ParseObject(string text);
    }
}

namespace JECS
{
    public abstract class StyledBaseTypeLogic<T> : BaseTypeLogic
    {
        public override Type ApplicableType => typeof(T);

        public override string SerializeObject(object value, FileStyle style)
        {
            if (value is T item)
                return SerializeItem(item, style);

            throw new ArgumentException("Wrong type!!!!");
        }
        public abstract string SerializeItem(T value, FileStyle style);

        public override object ParseObject(string text)
        {
            return ParseItem(text);
        }
        public abstract T ParseItem(string text);
    }

    public abstract class BaseTypeLogic<T> : BaseTypeLogic
    {
        public override Type ApplicableType => typeof(T);

        public override string SerializeObject(object value, FileStyle _)
        {
            if (value is T item)
                return SerializeItem(item);

            throw new ArgumentException("Wrong type!!!!");
        }
        public abstract string SerializeItem(T value);

        public override object ParseObject(string text)
        {
            return ParseItem(text);
        }
        public abstract T ParseItem(string text);
    }
}
