using System;

namespace VocabT
{
    public enum LearningStatus
    {
        InProgress = 1,
        Repeating = 2,
        Learned = 3
    }

    public class Word
    {
        public string Eng { get; set; }
        public string[] Rus { get; set; }
        public int Count { get; set; }
        public int MistakesCount { get; set; }
        /// <summary>
        /// Value that reflects count of consecutive correct and incorrect answers.
        /// Correct +1
        /// Incorrect -1
        /// Every iteration decreases the value by 1, if current value > 0 and the word didn't participate in the current iteration
        /// </summary>
        public short Seq { get; set; }
        /// <summary>
        /// Similar to <see cref="Seq"/>, but no decreasing on each iteration.
        /// It is used to move words from one status to another.
        /// </summary>
        public short Score { get; set; }
        /// <summary>
        /// Hint for ambiguous cases with similar values
        /// </summary>
        public string Hint { get; set; }
        public LearningStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
