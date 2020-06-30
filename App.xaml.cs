using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace VocabT
{
    public partial class App
    {
        private NotifyIcon _icon;

        private LearningContextWindow _learningContextWindow;
        private NewWordWindow _newWordWindow;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _icon = new NotifyIcon();

            InitIcon();

            _newWordWindow = new NewWordWindow(OnUpdate);
            _mainWindow = new MainWindow(OnStart);

            _mainWindow.Show();
        }

        private void InitIcon()
        {
            _icon.Icon = new Icon("main.ico");
            var menuContext = new ContextMenuStrip();

            menuContext.Items.Add(new ToolStripMenuItem("New Word", null, OnTrayItemClick));
            menuContext.Items.Add(new ToolStripMenuItem("Words and Options", null, OnTrayItemClick));
            menuContext.Items.Add(new ToolStripMenuItem("Exit", null, OnTrayItemClick));

            _icon.ContextMenuStrip = menuContext;
            _icon.Visible = true;
        }

        private void OnTrayItemClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;

            switch (item.Text)
            {
                case "New Word":
                    _newWordWindow.Show();
                    break;
                case "Words and Options":
                    _mainWindow.Show();
                    break;
                case "Exit":
                    Current.Shutdown();
                    break;
            }
        }

        private void OnUpdate(Word word)
        {
            _mainWindow.UpdateWord(word);
        }

        private void OnStart(int toLearnCount, int toRepeatCount, int minMinutes, int maxMinutes)
        {
            _learningContextWindow?.Close();

            var options = new Options
            {
                ToLearnCount = toLearnCount,
                ToRepeatCount = toRepeatCount,
                MinMinutes = minMinutes,
                MaxMinutes = maxMinutes
            };
            _learningContextWindow = new LearningContextWindow(options);
            _learningContextWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _icon.Dispose();
            base.OnExit(e);
        }
    }
}
