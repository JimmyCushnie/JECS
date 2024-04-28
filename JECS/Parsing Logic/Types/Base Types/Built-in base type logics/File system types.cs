using System.IO;

namespace JECS.BuiltInBaseTypeRules
{
    internal class BaseTypeLogic_DirectoryInfo : BaseTypeLogic<DirectoryInfo>
    {
        // Backslashes in file paths are annoying, and also JECS uses backslashes as an escape character for some things. We therefore
        // use forward slashes only. Screw you Windows
        public override string SerializeItem(DirectoryInfo value) => value.FullName.Replace('\\', '/');

        public override DirectoryInfo ParseItem(string text) => new DirectoryInfo(text);
    }

    internal class BaseTypeLogic_FileInfo : BaseTypeLogic<FileInfo>
    {
        public override string SerializeItem(FileInfo value) => value.FullName.Replace('\\', '/');

        public override FileInfo ParseItem(string text) => new FileInfo(text);
    }
}
