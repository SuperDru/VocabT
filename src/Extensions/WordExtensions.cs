using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VocabT
{
    public static class WordExtensions
    {
        public static readonly double DefaultEstimation = 0.8;

        public static double CalculateEstimation(this Word word)
        {
            var c = word.Count;
            var m = word.MistakesCount;
            var seq = word.Seq;

            if (c == 0)
            {
                return DefaultEstimation;
            }

            return seq switch
            {
                _ when seq >= 0 => m / (c * Math.Pow(2, seq)),
                _ => m / c + 1 - (c - m) * Math.Pow(2, seq) / c
            };
        }

        public static Dictionary<Word, double> GetProbabilities(this Dictionary<Word, double> estimates)
        {
            var estimationSum = estimates.Values.Sum();
            return estimates
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value / estimationSum);
        }
    }
}
