using System;
using System.Collections.Generic;
using System.Text;

namespace VocabT
{
    public static class Sql
    {
        public const string SelectWords = "SELECT * FROM words";
        public const string SelectWordsWithStatus = "SELECT * FROM words where status=@status";
    }
}
