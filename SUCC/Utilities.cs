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

        static int _indentationCount = 4;
        public static int IndentationCount
        {
            get => _indentationCount;
            set
            {
                if (value < 1)
                    _indentationCount = 1;
                else
                    _indentationCount = value;
            }
        }

        /// <summary> detects whether a file path is relative or absolute, and returns the absolute path </summary>
        public static string AbsolutePath(string RelativeOrAbsolutePath)
        {
            if (Path.IsPathRooted(RelativeOrAbsolutePath)) return RelativeOrAbsolutePath;
            return Path.Combine(DefaultPath, RelativeOrAbsolutePath);
        }

        /// <summary> Does a SUCC file exist at the path? </summary>
        public static bool SuccFileExists(string relativeOrAbsolutePath)
        {
            var path = Path.ChangeExtension(relativeOrAbsolutePath, FileExtension);
            path = AbsolutePath(path);
            return File.Exists(path);
        }



        internal static int LineIndentationLevel(string line)
            => line.TakeWhile(c => c == ' ').Count(); // the number of spaces in line that precede the first non-space character
    }
}