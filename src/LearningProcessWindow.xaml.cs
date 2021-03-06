﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HtmlAgilityPack;
using NAudio.Wave;

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

        private ConcurrentDictionary<string, MediaFoundationReader> _audioUrls = new ConcurrentDictionary<string, MediaFoundationReader>();
        private Word _currentWord;

        private string _questionWord;
        private List<string> _answerWords = new List<string>();
        private Correctness _correctness;
        private bool _engToRus;

        private bool _isAnswered;
        private bool _isNext;

        private readonly ObservableCollection<ComboBoxListItem> _answerBoxListItems = new ObservableCollection<ComboBoxListItem>();

        public LearningProcessWindow()
        {
            InitializeComponent();

            Top = SystemParameters.WorkArea.Height - Height;
            Left = SystemParameters.WorkArea.Width - Width;

            comboBox.ItemsSource = _answerBoxListItems;
        }

        public async Task<Dictionary<Word, bool>> StartProcessing(List<Word> words)
        {
            var result = words.ToDictionary(x => x, x => false);

            if (words.Count == 0)
                return result;

            Task.Run(() => PrepareAudio(words));

            foreach (var word in words)
            {
                _currentWord = word;
                var hint = word.Hint;

                _engToRus = rand.NextDouble() >= 0.5;
                if (_engToRus)
                {
                    _questionWord = word.Eng;
                    _answerWords = new List<string>(word.Rus.UnifyInfinitive());
                }
                else
                {
                    _questionWord = word.Rus.UnifyInfinitive().OrderBy(x => Guid.NewGuid()).First();
                    _answerWords = new List<string>(new []{word.Eng});
                }

                AudioBtn.Visibility = Visibility.Hidden;
                QuestionWordLbl.Content = _questionWord;
                AnswerWordTextBox.Text = string.Empty;
                AnswerWordTextBox.Focus();
                _answerBoxListItems.Clear();
                DataContext = null;
                comboBox.Visibility = Visibility.Hidden;
                HintPopupTextBlock.Text = hint;
                HintBtn.Visibility = hint.IsNullOrEmpty() ? Visibility.Hidden : Visibility.Visible;
                _isAnswered = false;

                while (!_isAnswered)
                {
                    // Wait for user to answer
                    while (!_isAnswered)
                    {
                        await Task.Delay(10);
                    }

                    comboBox.Visibility = Visibility.Visible;

                    if (_correctness == Correctness.Partly)
                    {
                        DataContext = new ComboBoxListItem
                        {
                            SelectedComboBox = "It's correct partly"
                        };
                        comboBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        AnswerWordTextBox.Text = "";
                        _isAnswered = false;
                    }
                }

                AudioBtn.Visibility = Visibility.Visible;
                AudioBtn_MouseLeftButtonUp(null, null);
                _isNext = false;
                DataContext = new ComboBoxListItem
                {
                    SelectedComboBox = _answerWords.First()
                };
                foreach (var w in _answerWords.Skip(1))
                {
                    _answerBoxListItems.Add(new ComboBoxListItem
                    {
                        Translation = w
                    });
                }
                if (_correctness == Correctness.Correct)
                {
                    comboBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    result[word] = true;
                }
                else if (_correctness == Correctness.Incorrect)
                {
                    comboBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
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

        private void PrepareAudio(IEnumerable<Word> words)
        {
            words.AsParallel().ForAll(x =>
            {
                using var client = new WebClient();

                var url = $"https://www.vocabulary.com/dictionary/{x.Eng}";
                var html = client.DownloadString(url);
                var pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(html);

                var audioNode = pageDocument.DocumentNode.SelectSingleNode("(//a[@class='audio'])");
                var data = audioNode.GetAttributeValue("data-audio", null);

                var mp3Url = $"https://audio.vocab.com/1.0/us/{data}.mp3";
                var mf = new MediaFoundationReader(mp3Url);
                _audioUrls[x.Eng] = mf;
            });
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

            var answer = AnswerWordTextBox.Text.UnifyInfinitive();
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
            var psi = new ProcessStartInfo
            {
                FileName = $"https://www.vocabulary.com/dictionary/{_currentWord.Eng}",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        
        private void AnswerWordTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                HintPopup.IsOpen = !HintPopup.IsOpen;
            }

            if (e.Key == Key.Enter)
            {
                CheckWordBtn_Click(null, null);
            }

            if (e.Key == Key.LeftShift && AudioBtn.Visibility != Visibility.Hidden)
            {
                AudioBtn_MouseLeftButtonUp(null, null);
            }
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HintPopup.IsOpen = !HintPopup.IsOpen;
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 1;
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 0.7;
        }

        private void comboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (_answerWords.Count <= 1)
            {
                ((ComboBox) sender).IsDropDownOpen = false;
            }
        }

        private async void AudioBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            while (!_audioUrls.ContainsKey(_currentWord.Eng))
            {
                await Task.Delay(10);
            }
            await using var mf = _audioUrls[_currentWord.Eng];
            using var wo = new WaveOutEvent();
            wo.Init(mf);
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }
        }
    }
}
