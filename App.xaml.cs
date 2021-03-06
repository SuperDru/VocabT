﻿using System;
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
            menuContext.Items.Add(new ToolStripMenuItem("Words", null, OnTrayItemClick));
            menuContext.Items.Add(new ToolStripMenuItem("Exit", null, OnTrayItemClick));

            _icon.ContextMenuStrip = menuContext;
            _icon.Visible = true;

            _icon.Click += (_, __) =>
            {
                _newWordWindow.Show();
                _newWordWindow.Focus();
            };
        }

        private void OnTrayItemClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;

            switch (item.Text)
            {
                case "New Word":
                    _newWordWindow.Show();
                    _newWordWindow.Focus();
                    break;
                case "Words":
                    _mainWindow.Show();
                    _mainWindow.Focus();
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

        private void OnStart()
        {
            _learningContextWindow?.Close();

            var options = Options.Name.ToObject<Options>();
            _learningContextWindow = new LearningContextWindow(options, OnUpdate);
            _learningContextWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _icon.Dispose();
            base.OnExit(e);
        }
    }
}
