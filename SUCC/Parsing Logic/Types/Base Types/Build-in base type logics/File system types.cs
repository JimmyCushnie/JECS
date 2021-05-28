using System.IO;

namespace SUCC.BuiltInBaseTypeRules
{
    internal class BaseTypeLogic_DirectoryInfo : BaseTypeLogic<DirectoryInfo>
    {
        public override string SerializeItem(DirectoryInfo value) => value.FullName;

        public override DirectoryInfo ParseItem(string text) => new DirectoryInfo(text);
    }

    internal class BaseTypeLogic_FileInfo : BaseTypeLogic<FileInfo>
    {
        public override string SerializeItem(FileInfo value) => value.FullName;

        public override FileInfo ParseItem(string text) => new FileInfo(text);
    }
}
