namespace JECS
{
    /// <summary>
    /// The easiest way to save and load data in JECS.
    /// </summary>
    public static class Easy
    {
        /// <summary>
        /// Saves <see cref="data"/> to a JECS file at <see cref="filePath"/>.
        /// </summary>
        public static void Save<T>(string filePath, T data)
        {
            var dataFile = new DataFile(filePath);
            dataFile.SaveAsObject(data);
        }

        /// <summary>
        /// Loads data of type <see cref="T"/> from a JECS file at <see cref="filePath"/>.
        /// If the file doesn't exist or only contains partial data, this method will return <see cref="defaultValue"/> and store <see cref="defaultValue"/> in the file.
        /// </summary>
        public static T Get<T>(string filePath, T defaultValue)
        {
            if (Utilities.JecsFileExists(filePath))
            {
                var dataFile = new DataFile(filePath);
                return dataFile.GetAsObject<T>(defaultValue);
            }
            else
            {
                var dataFile = new DataFile(filePath);
                dataFile.SaveAsObject(defaultValue);
                return defaultValue;
            }
        }
    }
}