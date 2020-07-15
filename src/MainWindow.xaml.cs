using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VocabT
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseContext _db;
        private readonly ObservableCollection<WordsPageListItem> _wordsSource = new ObservableCollection<WordsPageListItem>();

        private readonly Action _start;

        private int _tableSize;

        public MainWindow(Action start)
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
                var index = _wordsSource.IndexOf(oldWord);
                var newWord = ToListItem(word, oldWord);
                _wordsSource.Add(newWord);
                _wordsSource.Remove(oldWord);
                _wordsSource.Move(_wordsSource.IndexOf(newWord), index);
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

        private async void InitWordsPage(Task<List<Word>> task)
        {
            var words = await task;

            var listWords = words.Select(x => ToListItem(x));

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
            _start();
            Hide();
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

        private WordsPageListItem ToListItem(Word word, WordsPageListItem oldItem = null)
        {
            if (oldItem != null)
            {
                oldItem.Count = word.Count;
                oldItem.MistakesCount = word.MistakesCount;
                oldItem.Status = word.Status.ToString();
                return oldItem;
            }

            var selected = word.Rus.First();
            return new WordsPageListItem
            {
                Number = ++_tableSize,
                Original = word.Eng,
                Translation = word.Rus.Skip(1).Select(x => new ComboBoxListItem
                {
                    Translation = x
                }).ToList(),
                Status = word.Status.ToString(),
                Count = word.Count,
                MistakesCount = word.MistakesCount,
                SelectedComboBox = new TextBlock
                {
                    Text = selected,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
                },
            };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var comboBox = (ComboBox) sender;
            if (!comboBox.ItemsSource.Cast<object>().Any())
            {
                comboBox.IsDropDownOpen = false;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            startBtn.Focus();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
