using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace SUCC
{
    public static class Utilities
    {
        /// <summary>
        /// For the built game, this refers to the same folder that the executable is in. In the editor, it refers to [project folder]/Game. You can change it if you like.
        /// </summary>
        private static string _DefaultPath = GetDefaultDefaultPath();
        public static string DefaultPath
        {
            get => _DefaultPath;
            set
            {
                if (@Path.IsPathRooted(value))
                    throw new Exception($"When setting a custom default path, you must set an absolute path. The path {value} is not absolute.");
                _DefaultPath = value;
            }
        }

        private static string GetDefaultDefaultPath()
        {
#if UNITY_EDITOR
            string ProjectFolder = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(ProjectFolder, "Game");
#elif UNITY_WEBGL
            return "GameData";
#else
            return Directory.GetParent(Application.dataPath).FullName;
#endif
        }

        public static readonly string FileExtension = ".succ";

        /// <summary> detects whether a file path is relative or absolute, and returns the absolute path </summary>
        public static string AbsolutePath(string relativeOrAbsolutePath)
        {
            if (Path.IsPathRooted(relativeOrAbsolutePath)) return relativeOrAbsolutePath;
            return Path.Combine(DefaultPath, relativeOrAbsolutePath);
        }

        /// <summary> Does a SUCC file exist at the path? </summary>
        public static bool SuccFileExists(string relativeOrAbsolutePath)
        {
            var path = AbsolutePath(relativeOrAbsolutePath);
            path = Path.ChangeExtension(path, FileExtension);

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return PlayerPrefs.GetString(path, "") == "";

            return File.Exists(path);
        }

        internal static bool IsValidKey(string potentialKey, out string whyNot)
        {
            whyNot = null;

            if (string.IsNullOrEmpty(potentialKey))
                whyNot = "SUCC keys must contain at least one character";
            else if (potentialKey[0] == '-')
                whyNot = "SUCC keys may not begin with the character '-'";
            else if (potentialKey.Contains(':'))
                whyNot = "SUCC keys may not contain the character ':'";
            else if (potentialKey.Contains('#'))
                whyNot = "SUCC keys may not contain the character '#'";
            else if (potentialKey.ContainsNewLine())
                whyNot = "SUCC keys cannot contain a newline";
            else if (potentialKey[0] == ' ' || potentialKey[potentialKey.Length - 1] == ' ')
                whyNot = "SUCC keys may not start or end with a space";

            return whyNot == null;
        }

        public static LineEndingStyle LineEndingStyle { get; set; } = LineEndingStyle.PlatformDefault;
        internal static string NewLine
        {
            get
            {
                switch (LineEndingStyle)
                {
                    case LineEndingStyle.Unix:
                        return "\n";
                    case LineEndingStyle.Windows:
                        return "\r\n";
                    case LineEndingStyle.PlatformDefault:
                    default:
                        return Environment.NewLine;
                }
            }
        }
    }

    /// <summary>
    /// Different ways of saving line endings.
    /// </summary>
    public enum LineEndingStyle
    {
        /// <summary> Line endings are Windows style (CR LF) if on Windows and Unix style (LF) everywhere else. </summary>
        PlatformDefault = 0,

        /// <summary> Line endings are Unix style (LF) </summary>
        Unix,

        /// <summary> Line endings are Windows style (CR LF) </summary>
        Windows,
    }
}