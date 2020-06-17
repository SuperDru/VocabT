using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace VocabT
{
    class TrayCommands
    {
        public ICommand ExitApplicationCommand => new CommandDelegate { CommandAction = () => Application.Current.Shutdown() };
    }
}
