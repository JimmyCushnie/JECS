using System;
using System.Collections.Generic;
using SUCC;

namespace SUCC.Tests
{
    public static class TestUtilities
    {
        /// <summary> Whether or not to save the files used in testing to disk </summary>
        public static bool SaveFiles = true;

        public static string[] GenerateSUCCKeys(int count)
        {
            var array = new string[count];

            for (int i = 0; i < count; i++)
                array[i] = i.ToString();

            return array;
        }
    }
}
