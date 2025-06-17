namespace JECS
{
    /// <summary>
    /// Use when you have some data (of type <typeparamref name="T"/>) that's represented by a file on disk.
    /// When the <see cref="WrappedObjectDataFile{T}"/> is created, <see cref="Data"/> is loaded from the file.
    /// If you change <see cref="Data"/>, call <see cref="Save"/> to save your changes to disk.
    /// </summary>
    public class WrappedObjectDataFile<T>
    {
        private readonly DataFile DiskFile;
        public T Data;

        public WrappedObjectDataFile(string filePath, T defaultValue)
        {
            DiskFile = new DataFile(filePath);
            DiskFile.AutoSave = false;

            Data = DiskFile.GetAsObject(defaultValue);
        }

        public WrappedObjectDataFile(string filePath, string defaultFileText = null)
        {
            DiskFile = new DataFile(filePath, defaultFileText);
            DiskFile.AutoSave = false;
            Data = DiskFile.GetAsObject<T>();
        }

        public void Save()
        {
            DiskFile.SaveAsObject(Data);
            DiskFile.SaveAllData();
        }
    }
}