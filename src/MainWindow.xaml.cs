using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VocabT
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseContext _db;
        private readonly ObservableCollection<WordsPageListItem> _wordsSource = new ObservableCollection<WordsPageListItem>();

        private readonly Action<int, int, int, int> _start;

        private int _tableSize;

        public MainWindow(Action<int, int, int, int> start)
        {
            _db = DatabaseContext.Instance;
            _start = start;
            _db.GetWords().ContinueWith(InitWordsPage);

            InitializeComponent();

            wordsPage.ItemsSource = _wordsSource;
        }

        public void UpdateWord(Word word)
        {
            var oldWord = _wordsSource.FirstOrDefault(x => x.Original == word.Eng);
            if (oldWord != null)
            {
                oldWord.Translation = string.Join('\n', word.Rus);
                oldWord.Count = word.Count;
                oldWord.MistakesCount = word.MistakesCount;
                oldWord.Status = word.Status.ToString();
            }
            else
            {
                var newWord = ToListItem(word);
                _wordsSource.Add(newWord);
            }
        }

        private void LoadWordsWithStatus(LearningStatus status)
        {
            _db.GetWordsWithStatus(status).ContinueWith(InitWordsPage);
        }

        private void InitWordsPage(Task<List<Word>> task)
        {
            var words = task.GetAwaiter().GetResult();

            var listWords = words.Select(ToListItem);

            _tableSize = 0;
            Application.Current.Dispatcher.Invoke(() => 
            {
                _wordsSource.Clear();
                foreach (var word in listWords)
                {
                    _wordsSource.Add(word);
                }
            });

        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            var toLearnCountText = ToLearnCountTextBox.Text;
            var toRepeatCountText = ToRepeatCountTextBox.Text;
            var minMinutesText = MinTimeTextBox.Text;
            var maxMinutesText = MaxTimeTextBox.Text;

            if (!int.TryParse(toLearnCountText, out var toLearnCount) ||
                !int.TryParse(toRepeatCountText, out var toRepeatCount) ||
                !int.TryParse(minMinutesText, out var minMinutes) ||
                !int.TryParse(maxMinutesText, out var maxMinutes)
                || toLearnCount <= 0 || toRepeatCount <= 0 || minMinutes <= 0 || maxMinutes <= 0)
            {
                MessageBox.Show("The fields must have only positive integer values");
            }
            else if (minMinutes >= maxMinutes)
            {
                MessageBox.Show("Min time must be less than Max time");
            }
            else
            {
                _start(toLearnCount, toRepeatCount, minMinutes, maxMinutes);
                Hide();
            }
        }

        private void showInProgressBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadWordsWithStatus(LearningStatus.InProgress);
        }

        private void showRepeatingBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadWordsWithStatus(LearningStatus.Repeating);
        }

        private void showLearnedBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadWordsWithStatus(LearningStatus.Learned);
        }

        private WordsPageListItem ToListItem(Word word) =>
            new WordsPageListItem
            {
                Number = ++_tableSize,
                Original = word.Eng,
                Translation = string.Join('\n', word.Rus),
                Status = word.Status.ToString(),
                Count = word.Count,
                MistakesCount = word.MistakesCount
            };

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}
