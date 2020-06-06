using System;
using UnityEngine;

namespace SUCC.UnityStuff
{
    public static class ResourcesUtilities
    {
        public static string ReadTextFromFile(string filePath)
        {
            var textAsset = Resources.Load<TextAsset>(filePath);

            string text = string.Empty;

            if (textAsset == null)
            {
                Debug.LogWarning($"The specified file path ({filePath}) is invalid. Your DataFile will be created with empty default text.");
            }
            else
            {
                text = textAsset.text;
                Resources.UnloadAsset(textAsset);
            }

            return text;
        }
    }
}