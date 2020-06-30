using System;
using System.Collections.Generic;
using System.Text;

namespace VocabT
{
    public static class Sql
    {
        public const string SelectWords = "SELECT * FROM words";
        public const string SelectWordsWithStatus = "SELECT * FROM words where status=@status";
        public const string SelectWordWithEng = "SELECT * FROM words WHERE eng=@eng";
        public const string SelectWordsWithTranslations = "SELECT * FROM words WHERE rus && @translations::varchar[]";

        public const string InsertNewWord = "INSERT INTO words (eng, rus, hint, status) VALUES (@eng, @rus, @hint, 1)";

        public const string UpdateWord = @"UPDATE words SET
                                                    rus=@Rus,
                                                    count=@Count
                                                    mistakesCount=@MistakesCount,
                                                    seq=@Seq,
                                                    score=@Score,
                                                    hint=@Hint,
                                                    status=@Status,
                                                    updatedAt=@UpdatedAt
                                           WHERE eng=@Eng";

        public const string UpdatePositiveSeq = "UPDATE words SET seq=seq-1 WHERE eng NOT IN @words and seq>0";
    }
}
