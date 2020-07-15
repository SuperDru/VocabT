using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace VocabT
{
    public partial class NewWordWindow : Window
    {
        private readonly DatabaseContext _db;
        private readonly Action<Word> _update;

        private bool _isWarningOpened;

        public NewWordWindow(Action<Word> update)
        {
            _db = DatabaseContext.Instance;
            _update = update;

            InitializeComponent();

            Top = SystemParameters.WorkArea.Height - Height;
            Left = SystemParameters.WorkArea.Width - Width;
        }

        private async void newWordBtn_Click(object sender, RoutedEventArgs e)
        {
            var originalWord = originalWordTextBox.Text;
            var translationWordsText = translationWordsTextBox.Text;
            var translationWords = translationWordsText.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (!ValidateFields(originalWord, translationWords)) 
                return;
            NormalizeFields(ref originalWord, ref translationWords);

            if (await _db.GetWord(originalWord) != null)
            {
                ShowWarning("An english word with that name already exists");
                return;
            }

            var existingWordsWithTranslations = await _db.GetWordsWithTranslations(translationWords);
            if (existingWordsWithTranslations.Count > 0 && HintTextBox.Text.IsNullOrEmpty())
            {
                ShowWarning("Please, type hint to avoid ambiguous cases with similar translations");
                ContentControl_MouseLeftButtonUp(null, null);
                return;
            }

            await _db.AddWord(originalWord, translationWords.UnifyInfinitive(),HintTextBox.Text.IsNullOrEmpty() ? null : HintTextBox.Text);

            var word = await _db.GetWord(originalWord);
            _update(word);

            originalWordTextBox.Text = "";
            translationWordsTextBox.Text = "";
            HintTextBox.Text = "";
            Hide();
        }

        private bool ValidateFields(string originalWord, string[] translationWords)
        {
            if (string.IsNullOrWhiteSpace(originalWord))
            {
                ShowWarning("The 'English word' field can't be empty");
                return false;
            }
            if (!Regex.IsMatch(originalWord, "^[a-zA-Z- ]*$"))
            {
                ShowWarning("The 'English word' field must have only english characters");
                return false;
            }

            if (translationWords.Length < 1)
            {
                ShowWarning("The 'Translation words' field must have one or more non empty words");
                return false;
            }
            if (!translationWords.All(x => Regex.IsMatch(x, "^[а-яА-Я- ё]*$")))
            {
                ShowWarning("The 'Translation words' field must have only russian characters");
                return false;
            }

            return true;
        }

        private void NormalizeFields(ref string originalWord, ref string[] translationWords)
        {
            originalWord = originalWord.ToLower();
            translationWords = translationWords.Select(x => x.ToLower()).ToArray();
        }

        private void originalWordTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                translationWordsTextBox.Focus();
            }
        }

        private void translationWordsTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                newWordBtn_Click(sender, null);
                newWordBtn.Focus();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void ContentControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HintTextBox.IsEnabled = true;
            HintTextBox.Focus();
        }

        private void ContentControl_LostFocus(object sender, RoutedEventArgs e)
        {
            HintTextBox.IsEnabled = false;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            originalWordTextBox.Focus();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!_isWarningOpened)
            {
                Hide();
            }
        }

        private void ShowWarning(string msg)
        {
            _isWarningOpened = true;
            MessageBox.Show(msg);
            _isWarningOpened = false;
        }
    }
}
