using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VocabT
{
    public class WordsService
    {
        private const double ToRepeatingThreshold = 5;
        private const double ToLearnedThreshold = 5;
        private static readonly Random Rand = new Random();

        private readonly DatabaseContext _db = DatabaseContext.Instance;

        public async Task<List<Word>> GetWords(int count, LearningStatus status)
        {
            var words = new List<Word>();

            var allWords = await _db.GetWordsWithStatus(status);
            if (allWords.Count == 0)
            {
                return null;
            }

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
                word.Count++;
                word.MistakesCount += (short)(isCorrect ? 0 : 1);
                word.Seq += (short)(isCorrect ? 1 : -1);
                word.Score += (short)(isCorrect ? 1 : -1);

                if (word.Status == LearningStatus.InProgress && word.Score >= ToRepeatingThreshold)
                {
                    word.Status = LearningStatus.Repeating;
                    word.Score = 0;
                    word.Seq = 0;
                    word.MistakesCount = 0;
                    word.Count = 0;
                }

                if (word.Status == LearningStatus.Repeating && word.Score >= ToLearnedThreshold)
                {
                    word.Status = LearningStatus.Learned;
                }
            }

            await _db.UpdateWords(result.Keys.ToList());
            await _db.UpdateSeq(result.Keys.ToList());
        }
    }
}
