using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace VocabT
{
    public enum Correctness
    {
        Correct = 1,
        Partly = 2,
        Incorrect = 3
    }

    public partial class LearningProcessWindow : Window
    {
        private static readonly Random rand = new Random();

        private readonly Brush _defaultCorrectnessLblColor;

        private string _questionWord;
        private List<string> _answerWords = new List<string>();
        private Correctness _correctness;
        private bool _engToRus;

        private bool _isAnswered;
        private bool _isNext;

        public LearningProcessWindow()
        {
            InitializeComponent();
            _defaultCorrectnessLblColor = CorrectnessLbl.Foreground;

            Top = SystemParameters.WorkArea.Height - Height;
            Left = SystemParameters.WorkArea.Width - Width;
        }

        public async Task<Dictionary<Word, bool>> StartProcessing(List<Word> words)
        {
            var result = words.ToDictionary(x => x, x => false);

            if (words.Count == 0)
                return result;

            foreach (var word in words)
            {
                var hint = word.Hint;

                _engToRus = rand.NextDouble() >= 0.5;
                if (_engToRus)
                {
                    _questionWord = word.Eng;
                    _answerWords = new List<string>(word.Rus);
                }
                else
                {
                    _questionWord = word.Rus.OrderBy(x => Guid.NewGuid()).First();
                    _answerWords = new List<string>(new []{word.Eng});
                }

                QuestionWordLbl.Content = _questionWord;
                AnswerWordTextBox.Text = string.Empty;
                CorrectnessLbl.Content = string.Empty;
                CorrectnessLbl.Foreground = _defaultCorrectnessLblColor;
                HintPopupTextBlock.Text = hint;
                image.Visibility = hint.IsNullOrEmpty() ? Visibility.Hidden : Visibility.Visible;
                _isAnswered = false;

                while (!_isAnswered)
                {
                    // Wait for user to answer
                    while (!_isAnswered)
                    {
                        await Task.Delay(10);
                    }

                    if (_correctness == Correctness.Partly)
                    {
                        CorrectnessLbl.Content = "It's correct word, but please, type another word";
                        CorrectnessLbl.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                    }
                }

                _isNext = false;
                CorrectnessLbl.Content = _answerWords.First();
                if (_correctness == Correctness.Correct)
                {
                    CorrectnessLbl.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    result[word] = true;
                }
                else if (_correctness == Correctness.Incorrect)
                {
                    CorrectnessLbl.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    result[word] = false;
                }

                CheckWordBtn.Content = "Next";

                // Wait for user to press NextBtn
                while (!_isNext)
                {
                    await Task.Delay(10);
                }
            }

            return result;
        }

        private async void CheckWordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckWordBtn.Content.ToString() == "Next")
            {
                CheckWordBtn.Content = "Check";
                _isNext = true;
                return;
            }

            if (_answerWords.Count == 0)
                return;

            var answer = AnswerWordTextBox.Text;
            _correctness = _answerWords.Contains(answer) ? Correctness.Correct : Correctness.Incorrect;

            if (!_engToRus && _correctness == Correctness.Incorrect)
            {
                var correctWords = await DatabaseContext.Instance.GetWordsWithTranslations(new[] {_questionWord});
                if (correctWords.Select(x => x.Eng).Any(x => x == answer))
                {
                    _correctness = Correctness.Partly;
                }
            }

            _isAnswered = true;
        }

        private void OpenWebBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AnswerWordTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                HintPopup.IsOpen = !HintPopup.IsOpen;
            }
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HintPopup.IsOpen = !HintPopup.IsOpen;
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            image.Opacity = 1;
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            image.Opacity = 0.7;
        }
    }
}
