using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using VocabT.src.NewWord;

namespace VocabT.src
{
    public partial class StartWindow : Window
    {
        private readonly DatabaseContext _db;
        private readonly ObservableCollection<WordsPageListItem> _wordsSource = new ObservableCollection<WordsPageListItem>();

        public StartWindow()
        {
            _db = new DatabaseContext("Server=127.0.0.1;Port=5432;Database=vocab_db;User Id=postgres;Password=mydb;");
            _db.GetWords().ContinueWith(InitWordsPage);

            InitializeComponent();

            wordsPage.ItemsSource = _wordsSource;
        }

        private void LoadWordsWithStatus(LearningStatus status)
        {
            _db.GetWordsWithStatus(status).ContinueWith(InitWordsPage);
        }

        private void InitWordsPage(Task<List<Word>> task)
        {
            var words = task.GetAwaiter().GetResult();

            var number = 0;
            var listWords = words.Select(x => new WordsPageListItem
            {
                Number = ++number,
                Original = x.Eng,
                Translation = string.Join('\n', x.Rus),
                Status = x.Status.ToString(),
                Count = x.Count,
                MistakesCount = x.MistakesCount
            });

            Application.Current.Dispatcher.Invoke(() => 
            {
                foreach (var word in listWords)
                {
                    _wordsSource.Add(word);
                }
            });

        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            new NewWordWindow().Show();
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;
            //Hide();
        }
    }
}
