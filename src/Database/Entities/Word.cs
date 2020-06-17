using System;
using System.Collections.Generic;
using System.Text;

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
        public LearningStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
