using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VocabT
{
    public static class WordExtensions
    {
        public static readonly float DefaultEstimation = 0.2f;

        public static float CalculateEstimation(this Word word)
        {
            var c = word.Count;
            var m = word.MistakesCount;
            var seq = word.Seq;

            if (c == 0)
            {
                return DefaultEstimation;
            }

            var mc = (float) m / (m + c);
            var pivot = MathF.Max(mc, DefaultEstimation);
            var range = seq >= 0 ? pivot : 1 - pivot;
            var shift = range * (1 - 1 / MathF.Pow(2f, MathF.Abs(seq)));

            return pivot + (seq >= 0 ? -1 : 1) * shift;
        }

        public static Dictionary<Word, float> GetProbabilities(this Dictionary<Word, float> estimates)
        {
            var estimationSum = estimates.Values.Sum();
            return estimates
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value / estimationSum);
        }
    }
}
