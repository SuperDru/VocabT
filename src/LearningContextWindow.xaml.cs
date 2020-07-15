using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
#pragma warning disable 4014

namespace VocabT
{
    public partial class LearningContextWindow : Window
    {
        private static readonly Random Rand = new Random();

        private const double LearningProcessProbability = 0.9;
        private const double MinWordsCount = 5;

        private readonly Options _options;
        private readonly SoundPlayer _pingSound;
        private readonly WordsService _wordsService;

        private readonly Action<Word> _update;

        public LearningContextWindow(Options options, Action<Word> update)
        {
            _options = options;
            _pingSound = new SoundPlayer("ping_sound.wav");
            _wordsService = new WordsService();
            _update = update;

            InitializeComponent();

            Top = SystemParameters.WorkArea.Height - Height;
            Left = SystemParameters.WorkArea.Width - Width;
        }

        private void btn_MouseEnter(object sender, MouseEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            ellipse.Stroke = new SolidColorBrush(Colors.DarkBlue);
            ellipse.StrokeThickness = 4;
        }

        private void btn_MouseLeave(object sender, MouseEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.StrokeThickness = 1;
        }

        private void btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = (Ellipse) sender;
            ellipse.Stroke = new SolidColorBrush(Colors.DarkBlue);
            ellipse.StrokeThickness = 5;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NextIteration();
        }

        private async Task NextIteration()
        {
            var db = DatabaseContext.Instance;
            var inProgressWords = await db.GetWordsWithStatus(LearningStatus.InProgress);
            var repeatingWords = await db.GetWordsWithStatus(LearningStatus.Repeating);

            var isLearningActive = inProgressWords.Count >= MinWordsCount;
            var isRepeatingActive = repeatingWords.Count >= MinWordsCount;
            if (!isLearningActive && !isRepeatingActive)
            {
                throw new Exception("Not enough words in the dictionary.");
            }

            var isLearningProcess = Rand.NextDouble() <= LearningProcessProbability;

            if (isLearningProcess && !isLearningActive || !isLearningProcess && !isRepeatingActive)
            {
                isLearningProcess = !isLearningProcess;
            }

            Show();
            if (isLearningProcess)
            {
                LearningBtn.Visibility = Visibility.Visible;
                RepeatingBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                LearningBtn.Visibility = Visibility.Hidden;
                RepeatingBtn.Visibility = Visibility.Visible;
            }

            _pingSound.Play();

            await Task.Delay(20 * 1000);
            if (LearningBtn.Visibility != Visibility.Visible && RepeatingBtn.Visibility != Visibility.Visible)
                return;

            Hide();
            LearningBtn.Visibility = Visibility.Hidden;
            RepeatingBtn.Visibility = Visibility.Hidden;

            await WaitForNextIteration();
            // ReSharper disable once TailRecursiveCall
            NextIteration();
        }

        private async Task WaitForNextIteration()
        {
            var isMinInterval = Rand.NextDouble() <= 0.8;

            var range = _options.MaxMinutes - _options.MinMinutes;
            var interval = 
                isMinInterval ?
                Rand.Next(_options.MinMinutes, _options.MinMinutes + range / 2) :
                Rand.Next(_options.MaxMinutes - range / 2, _options.MaxMinutes);

            await Task.Delay(TimeSpan.FromMinutes(interval));
        }

        private async void btnStudy_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var isLearningProcess = LearningBtn.Visibility == Visibility.Visible;

            Hide();
            LearningBtn.Visibility = Visibility.Hidden;
            RepeatingBtn.Visibility = Visibility.Hidden;

            var count = isLearningProcess ? _options.ToLearnCount : _options.ToRepeatCount;
            var status = isLearningProcess ? LearningStatus.InProgress : LearningStatus.Repeating;
            var words = await _wordsService.GetWords(count, status);

            var processWindow = new LearningProcessWindow();
            processWindow.Show();

            var result = await processWindow.StartProcessing(words);
            await _wordsService.ProcessResult(result);

            foreach (var (word, _) in result)
            {
                _update(word);
            }

            processWindow.Close();

            await WaitForNextIteration();
            NextIteration();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl)
            {
                btnStudy_MouseLeftButtonUp(null, null);
            }
        }
    }
}
