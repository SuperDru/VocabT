using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VocabT
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str) =>
            string.IsNullOrWhiteSpace(str);

        public static string UnifyInfinitive(this string str) =>
            str.Replace("ться", "ть");

        public static string[] UnifyInfinitive(this IEnumerable<string> strings) =>
            strings.Select(x => x.UnifyInfinitive()).ToArray();
    }
}
