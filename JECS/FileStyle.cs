﻿using System;

namespace JECS
{
    /// <summary>
    /// Rules for various aspects of how generated JECS data is formatted.
    /// </summary>
    /// <remarks>
    /// This only affects new data. If JECS is modifying existing data, it will do its best to keep the formatting intact.
    /// </remarks>
    public class FileStyle
    {
        /// <summary>
        /// If you do not specify a FileStyle for your DataFile, this will be used.
        /// </summary>
        public static FileStyle Default { get; set; } = new FileStyle();

        /// <summary>
        /// Creates a new FileStyle.
        /// </summary>
        public FileStyle()
        {
        }



        /// <summary>
        /// JECS strings can optionally be surrounded by "quotes". If this is true, they will be quoted even when not necessary.
        /// </summary>
        public bool AlwaysQuoteStrings { get; set; } = false;

        /// <summary>
        /// JECS can store dictionaries as KeyValuePair arrays if the key type is complex. If this is true, dictionaries will always be stored like that.
        /// </summary>
        public bool AlwaysArrayDictionaries { get; set; } = false;

        /// <summary>
        /// JECS can read booleans in several different ways. The BoolStyle specifies which of those ways to save them in.
        /// </summary>
        public BoolStyle BoolStyle { get; set; } = BoolStyle.true_false;

        /// <summary>
        /// JECS can read enums in a couple different ways. The EnumStyle specifies which of those ways to save them in.
        /// </summary>
        public EnumStyle EnumStyle { get; set; } = EnumStyle.name;


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
        int _IndentationInterval = 4;

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
        int _SpacesAfterColon = 1;

        /// <summary>
        /// The number of spaces between the dash and the value in a list node. Must be at least 0.
        /// </summary>
        public int SpacesAfterDash
        {
            get => _SpacesAfterDash;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"SpacesAfterDash cannot be less than 0. You tried to set it to {value}");
                else
                    _SpacesAfterDash = value;
            }
        }
        int _SpacesAfterDash = 1;
    }

    /// <summary>
    /// Different options for how to save boolean values
    /// </summary>
    public enum BoolStyle
    {
        /// <summary> Save true as "true" and false as "false" </summary>
        true_false,
        /// <summary> Save true as "on" and false as "off" </summary>
        on_off,
        /// <summary> Save true as "yes" and false as "no" </summary>
        yes_no,
        /// <summary> Save true as "y" and false as "n" </summary>
        y_n,
    }

    /// <summary>
    /// Different options for how to save enum values
    /// </summary>
    public enum EnumStyle
    {
        /// <summary> save the enum as its name if it has one, otherwise save its number </summary>
        name,
        /// <summary> always save the enum as its number </summary>
        number,
    }
}