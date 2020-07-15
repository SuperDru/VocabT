namespace VocabT
{
    public class Options
    {
        public static readonly string Name = "Options";

        public int ToLearnCount { get; set; }
        public int ToRepeatCount { get; set; }
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
    }
}
