using System;

namespace SUCC
{
    /// <summary>
    /// Allows you to customize various aspects of how generated SUCC data is formatted.
    /// </summary>
    /// <remarks>
    /// This only affects new data. If SUCC is modifying existing data, it will do its best to keep the formatting intact.
    /// </remarks>
    public class FileStyle
    {
        /// <summary>
        /// If you do not specify a FileStyle when creating a DataFile, this will be used.
        /// </summary>
        public static FileStyle Default { get; set; } = new FileStyle();

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

        int _IndentationInterval = 4;
        int _SpacesAfterColon = 1;
        int _SpacesAfterDash = 1;

        /// <summary>
        /// SUCC strings can optionally be surrounded by "quotes". If this is true, they will be quoted even when not necessary.
        /// </summary>
        public bool AlwaysQuoteStrings { get; set; } = true;

        /// <summary>
        /// SUCC can store dictionaries as KeyValuePair arrays if the key type is complex. If this is true, dictionaries will always be stored like that.
        /// </summary>
        public bool AlwaysArrayDictionaries { get; set; } = false;

        /// <summary>
        /// The number of spaces used to indent a child line under its parent.
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
        /// The number of spaces between the colon and the value in a key node.
        /// </summary>
        public int SpacesAfterColon
        {
            get => _SpacesAfterColon;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"SpacesAfterColon cannot be less than 0. You tried to set it to {value}");
                else
                    _IndentationInterval = value;
            }
        }

        /// <summary>
        /// The number of spaces between the dash and the value in a list node.
        /// </summary>
        public int SpacesAfterDash
        {
            get => _SpacesAfterDash;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"SpacesAfterColon cannot be less than 0. You tried to set it to {value}");
                else
                    _IndentationInterval = value;
            }
        }
    }
}