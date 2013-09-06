using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BBot
{
    public class BotTerminalViewModel
    {
        BotTerminalModel model;

        ICommand sendTerminalCommand;
        ICommand historyUpArrow;
        ICommand historyDownArrow;

        int sentenceHistoryIndex = 0;

        public BotTerminalViewModel(IRobotConnection Connection)
        {
            model = new BotTerminalModel(Connection);

            //For keyboard bindings
            sendTerminalCommand = new TerminalDelegateCommand((x) => SendTerminalCommand(model.CurrentCommand));
            historyUpArrow = new TerminalDelegateCommand((x) => LoadHistorySentenceUp());
            historyDownArrow = new TerminalDelegateCommand((x) => LoadHistorySentenceDown());
        }

        #region Properties

        public BotTerminalModel BotTerminalModel
        {
            get
            {
                return model;
            }
        }

        public ICommand SendTerminalCommandNow
        {
            get
            {
                return sendTerminalCommand;
            }
        }

        public ICommand HistoryUpArrowCommand
        {
            get
            {
                return historyUpArrow;
            }
        }

        public ICommand HistoryDownArrowCommand
        {
            get
            {
                return historyDownArrow;
            }
        }
        #endregion

        #region Methods

        public void LoadHistorySentenceUp()
        {
            if (model.TerminalHistory.Count > 0 && sentenceHistoryIndex >= 0 && sentenceHistoryIndex < model.TerminalHistory.Count)
            {
                sentenceHistoryIndex++;
                string s = model.TerminalHistory[(model.TerminalHistory.Count - sentenceHistoryIndex)];
                model.CurrentCommand = s;
            }
        }

        public void LoadHistorySentenceDown()
        {
            if (model.TerminalHistory.Count > 0 && sentenceHistoryIndex > 1 && sentenceHistoryIndex <= model.TerminalHistory.Count)
            {
                sentenceHistoryIndex--;
                string s = model.TerminalHistory[(model.TerminalHistory.Count - sentenceHistoryIndex)];
                model.CurrentCommand = s;
            }
        }

        public void SendTerminalCommand(string Message)
        {
            model.SendCustomBotCommand(Message);
            model.CurrentCommand = "";
            sentenceHistoryIndex = 0;
        }

        #endregion

    }

    //Note - if we were using Microsoft's "Prism" Framework we wouldn't need this class.
    //We're explicitly not using Prism here as a learning exercise
    public class TerminalDelegateCommand : ICommand
    {
        Action<object> _executeDelegate;

        public TerminalDelegateCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
    }

}
