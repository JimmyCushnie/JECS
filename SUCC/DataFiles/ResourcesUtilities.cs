using SUCC.Abstractions;
using System;
using System.IO;
using UnityEngine;

namespace SUCC
{
    internal static class ResourcesUtilities
    {
        public static string ReadTextFromFile(string filePath)
        {
            var textAsset = Resources.Load<TextAsset>(filePath);
            if (textAsset == null)
                throw new Exception("The default file you specified doesn't exist in Resources :(");

            var text = textAsset.text;
            Resources.UnloadAsset(textAsset);

            return text;
        }
    }
}