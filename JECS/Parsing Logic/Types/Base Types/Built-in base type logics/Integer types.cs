using System.Globalization;
using System.Numerics;

namespace JECS.BuiltInBaseTypeLogics
{
    internal class BaseTypeLogic_Sbyte : BaseTypeLogic<sbyte>
    {
        public override sbyte ParseItem(string text) => sbyte.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(sbyte value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Byte : BaseTypeLogic<byte>
    {
        public override byte ParseItem(string text) => byte.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(byte value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Short : BaseTypeLogic<short>
    {
        public override short ParseItem(string text) => short.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(short value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Ushort : BaseTypeLogic<ushort>
    {
        public override ushort ParseItem(string text) => ushort.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(ushort value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Int : BaseTypeLogic<int>
    {
        public override int ParseItem(string text) => int.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(int value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Uint : BaseTypeLogic<uint>
    {
        public override uint ParseItem(string text) => uint.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(uint value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Long : BaseTypeLogic<long>
    {
        public override long ParseItem(string text) => long.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(long value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }

    internal class BaseTypeLogic_Ulong : BaseTypeLogic<ulong>
    {
        public override ulong ParseItem(string text) => ulong.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(ulong value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }


    internal class BaseTypeLogic_BigInt : BaseTypeLogic<BigInteger>
    {
        public override BigInteger ParseItem(string text) => BigInteger.Parse(text, NumberFormatInfo.InvariantInfo);
        public override string SerializeItem(BigInteger value) => value.ToString(NumberFormatInfo.InvariantInfo);
    }
}
