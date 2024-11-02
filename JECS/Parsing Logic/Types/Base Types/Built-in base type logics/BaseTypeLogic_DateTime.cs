using System;
using System.Globalization;

namespace JECS.BuiltInBaseTypeLogics
{
    internal class BaseTypeLogic_DateTime : BaseTypeLogic<DateTime>
    {
        const string JecsDateTimeFormat = "yyyy-MM-dd HH:mm:ss";


        public override string SerializeItem(DateTime value)
        {
            return value.ToString(JecsDateTimeFormat);
        }

        public override DateTime ParseItem(string text)
        {
            if (DateTime.TryParseExact(text, JecsDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;

            throw new Exception("Invalid DateTime format!!");
        }
    }
}
