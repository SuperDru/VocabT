using System;
using System.Collections.Generic;
using System.Text;

namespace VocabT
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str) =>
            string.IsNullOrWhiteSpace(str);
    }
}
