using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocabT
{
    public class WordsService
    {
        private static readonly Random Rand = new Random();

        private readonly DatabaseContext _db = DatabaseContext.Instance;

        public async Task<List<Word>> GetWords(int count, LearningStatus status)
        {
            var words = new List<Word>();

            var allWords = await _db.GetWordsWithStatus(status);
            var wordsEstimates = allWords.ToDictionary(x => x, x => x.CalculateEstimation());


            for (var i = 0; i < count; i++)
            {
                var wordsProbabilities = wordsEstimates.GetProbabilities();

                var randomValue = Rand.NextDouble();
                var value = 0d;
                foreach (var (word, p) in wordsProbabilities)
                {
                    value += p;
                    if (randomValue <= value)
                    {
                        words.Add(word);
                        wordsEstimates.Remove(word);
                        break;
                    }
                }
            }

            return words;
        }

        public async Task ProcessResult(Dictionary<Word, bool> result)
        {
            foreach (var (word,isCorrect) in result)
            {
                word.Seq++;
                word.Count++;
                word.MistakesCount += isCorrect ? 0 : 1;
                word.Score += (short) (isCorrect ? 1 : -1);
            }

            await _db.UpdateWords(result.Keys.ToList());
            await _db.UpdateSeq(result.Keys.ToList());
        }
    }
}
