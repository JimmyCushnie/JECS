using System;

namespace SUCC
{
    /// <summary>
    /// Rules for various aspects of how generated SUCC data is formatted.
    /// </summary>
    /// <remarks>
    /// This only affects new data. If SUCC is modifying existing data, it will do its best to keep the formatting intact.
    /// </remarks>
    public class FileStyle
    {
        /// <summary>
        /// If you do not specify a FileStyle when creating a DataFile, this will be used.
        /// </summary>
        public static FileStyle Default = new FileStyle();


        /// <summary>
        /// Creatse a new Style.
        /// </summary>
        /// <param name="indentationInterval">The number of spaces used to indent a child line under its parent. Must be at least 1.</param>
        /// <param name="spacesAfterColon">The number of spaces between the colon and the value in a key node. Must be at least 0.</param>
        /// <param name="spacesAfterDash">The number of spaces between the dash and the value in a list node. Must be at least 0.</param>
        /// <param name="alwaysQuoteStrings">SUCC strings can optionally be surrounded by "quotes". If this is true, they will be quoted even when not necessary.</param>
        /// <param name="alwaysArrayDictionaries">SUCC can store dictionaries as KeyValuePair arrays if the key type is complex. If this is true, dictionaries will always be stored like that.</param>
        public FileStyle(
            int indentationInterval = 4, int spacesAfterColon = 1, int spacesAfterDash = 1, 
            bool alwaysQuoteStrings = true, bool alwaysArrayDictionaries = false)
        {
            IndentationInterval = indentationInterval;
            SpacesAfterColon = spacesAfterColon;
            SpacesAfterDash = spacesAfterDash;
            AlwaysQuoteStrings = alwaysQuoteStrings;
            AlwaysArrayDictionaries = alwaysArrayDictionaries;
        }



        /// <summary>
        /// SUCC strings can optionally be surrounded by "quotes". If this is true, they will be quoted even when not necessary.
        /// </summary>
        public bool AlwaysQuoteStrings { get; set; }

        /// <summary>
        /// SUCC can store dictionaries as KeyValuePair arrays if the key type is complex. If this is true, dictionaries will always be stored like that.
        /// </summary>
        public bool AlwaysArrayDictionaries { get; set; }

        int _IndentationInterval;
        int _SpacesAfterColon;
        int _SpacesAfterDash;

        /// <summary>
        /// The number of spaces used to indent a child line under its parent. Must be at least 1.
        /// </summary>
        public int IndentationInterval
        {
            get => _IndentationInterval;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException($"IndentationInterval must be at least 1. You tried to set it to {value}");
                else
                    _IndentationInterval = value;
            }
        }

        /// <summary>
        /// The number of spaces between the colon and the value in a key node. Must be at least 0.
        /// </summary>
        public int SpacesAfterColon
        {
            get => _SpacesAfterColon;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"SpacesAfterColon cannot be less than 0. You tried to set it to {value}");
                else
                    _SpacesAfterColon = value;
            }
        }

        /// <summary>
        /// The number of spaces between the dash and the value in a list node. Must be at least 0.
        /// </summary>
        public int SpacesAfterDash
        {
            get => _SpacesAfterDash;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"SpacesAfterColon cannot be less than 0. You tried to set it to {value}");
                else
                    _SpacesAfterDash = value;
            }
        }
    }
}