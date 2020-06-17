using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VocabT.src.Data;

namespace VocabT.src.NewWord
{
    /// <summary>
    /// Interaction logic for NewWordWindow.xaml
    /// </summary>
    public partial class NewWordWindow : Window
    {
        private readonly ObservableCollection<TextBoxListItem> _secondaryTranslationItems;

        public NewWordWindow()
        {
            _secondaryTranslationItems = new ObservableCollection<TextBoxListItem>();

            InitializeComponent();

            translationWords.ItemsSource = _secondaryTranslationItems;

            Init();

            var label = label_Copy.Copy();
            var old = label.Margin;
            var ha = old.Top + 40;
            label.Margin = new Thickness(old.Left, ha, old.Right, old.Bottom);

            grid.Children.Add(label);
        }

        public void Init()
        {
            _secondaryTranslationItems.Add(new TextBoxListItem
            {
                Value = "Hero"
            });

            _secondaryTranslationItems.Add(new TextBoxListItem
            {
                Value = "Evil"
            });
        }
    }
}
