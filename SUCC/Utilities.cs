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
            var path = Path.ChangeExtension(relativeOrAbsolutePath, FileExtension);
            path = AbsolutePath(path);
            return File.Exists(path);
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