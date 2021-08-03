using System.Globalization;

namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeRules_Int : BaseTypeLogic<int>
    {
        public override int ParseItem(string text) => int.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(int value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Long : BaseTypeLogic<long>
    {
        public override long ParseItem(string text) => long.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(long value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Short : BaseTypeLogic<short>
    {
        public override short ParseItem(string text) => short.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(short value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Uint : BaseTypeLogic<uint>
    {
        public override uint ParseItem(string text) => uint.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(uint value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Ulong : BaseTypeLogic<ulong>
    {
        public override ulong ParseItem(string text) => ulong.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(ulong value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Ushort : BaseTypeLogic<ushort>
    {
        public override ushort ParseItem(string text) => ushort.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(ushort value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Byte : BaseTypeLogic<byte>
    {
        public override byte ParseItem(string text) => byte.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(byte value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeRules_Sbyte : BaseTypeLogic<sbyte>
    {
        public override sbyte ParseItem(string text) => sbyte.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(sbyte value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }
}
