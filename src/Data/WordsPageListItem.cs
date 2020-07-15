using System.Collections.Generic;
using System.Windows.Controls;

namespace VocabT
{
    class WordsPageListItem
    {
        public int Number { get; set; }
        public string Original { get; set; }
        public List<ComboBoxListItem> Translation { get; set; }
        public int Count { get; set; }
        public int MistakesCount { get; set; }
        public string Status { get; set; }

        public TextBlock SelectedComboBox { get; set; }
        public bool Enabled { get; set; }
    }
}
