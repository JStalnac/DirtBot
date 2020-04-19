using System;
using System.Collections.Generic;

namespace DirtBot.Helpers
{
    public static class StringHelper
    {
        static readonly Random random = new Random();

        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ChooseRandomString(this string[] array)
        {
            return array[random.Next(0, array.Length)];
        }

        /// <summary>
        /// Chooses a random string from an array or a list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ChooseRandomString(this List<string> list)
        {
            return list[random.Next(0, list.Count)];
        }

        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
