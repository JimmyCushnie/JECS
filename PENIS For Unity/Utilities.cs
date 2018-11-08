using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PENIS
{
    public static class Utilities
    {
        private static string _DefaultPath;

        /// <summary>
        /// For the built game, this refers to the same folder that the executable is in. In the editor, it refers to a "GameData" folder in the same directory as the Assets folder. You can change it if you like.
        /// </summary>
        public static string DefaultPath
        {
            get
            {
                if (Path.IsPathRooted(_DefaultPath)) { return _DefaultPath; }
#if UNITY_EDITOR
                string ProjectFolder = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(ProjectFolder, "Game");
# elif UNITY_WEBGL
                return "GameData";
#endif
                return Directory.GetParent(Application.dataPath).FullName;
            }
            set
            {
                _DefaultPath = value;
            }
        }

        public static readonly string FileExtension = ".PENIS";

        static int _indentationCount = 4;
        public static int IndentationCount
        {
            get { return _indentationCount; }
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
            if (Path.IsPathRooted(RelativeOrAbsolutePath)) { return RelativeOrAbsolutePath; }
            return Path.Combine(DefaultPath, RelativeOrAbsolutePath);
        }
    }
}