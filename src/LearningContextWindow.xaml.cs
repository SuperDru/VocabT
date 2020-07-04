using System;
using System.Collections.Generic;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VocabT
{
    public partial class LearningContextWindow : Window
    {
        private static readonly Random Rand = new Random();

        private const double LearningProcessProbability = 1;

        private readonly Options _options;
        private readonly SoundPlayer _pingSound;
        private readonly WordsService _wordsService;

        public LearningContextWindow(Options options)
        {
            _options = options;
            _pingSound = new SoundPlayer("ping_sound.wav");
            _wordsService = new WordsService();

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
            var isLearningProcess = Rand.NextDouble() <= LearningProcessProbability;
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

            LearningBtn.Visibility = Visibility.Hidden;
            RepeatingBtn.Visibility = Visibility.Hidden;

            var count = isLearningProcess ? _options.ToLearnCount : _options.ToRepeatCount;
            var status = isLearningProcess ? LearningStatus.InProgress : LearningStatus.Repeating;
            var words = await _wordsService.GetWords(count, status);

            var processWindow = new LearningProcessWindow();
            processWindow.Show();

            var result = await processWindow.StartProcessing(words);
            await _wordsService.ProcessResult(result);

            await WaitForNextIteration();
            NextIteration();
        }
    }
}
