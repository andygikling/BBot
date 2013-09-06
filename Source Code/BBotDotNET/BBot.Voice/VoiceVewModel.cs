using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BBot
{
    public class VoiceViewModel
    {
        VoiceModel model;

        ICommand speakCommand;
        ICommand historyUpArrow;
        ICommand historyDownArrow;

        int sentenceHistoryIndex = 0;

        //Need a private ConnectionViewModel here

        public VoiceViewModel(IRobotConnection Connection)
        {
            //Here we link up the send and receive logic with ConnectionViewModel
            model = new VoiceModel(Connection);

            //For keyboard bindings
            speakCommand = new VoiceDelegateCommand((x) => SendCurrentSentence(model.CurrentSentence));
            historyUpArrow = new VoiceDelegateCommand((x) => LoadHistorySentenceUp());
            historyDownArrow = new VoiceDelegateCommand((x) => LoadHistorySentenceDown());

        }

        #region Properties

        public VoiceModel VoiceModel
        {
            get
            {
                return model;
            }
        }

        public ICommand SpeakNowCommand
        {
            get
            {
                return speakCommand;
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

        public void SendCurrentSentence(string sentence)
        {
            model.SendCurrentSentence(sentence);
            sentenceHistoryIndex = 0;
        }

        public void StopTalking()
        {
            model.StopTalking();
        }

        public void SendSettings()
        {
            model.SendSettings();
        }

        public void LoadHistorySentenceUp()
        {
            if (model.SentenceHistory.Count > 0 && sentenceHistoryIndex >= 0 && sentenceHistoryIndex < model.SentenceHistory.Count)
            {
                sentenceHistoryIndex++;
                string s = model.SentenceHistory[(model.SentenceHistory.Count - sentenceHistoryIndex)];
                model.CurrentSentence = s;
            }
        }

        public void LoadHistorySentenceDown()
        {
            if (model.SentenceHistory.Count > 0 && sentenceHistoryIndex > 1 && sentenceHistoryIndex <= model.SentenceHistory.Count)
            {
                sentenceHistoryIndex--;
                string s = model.SentenceHistory[(model.SentenceHistory.Count - sentenceHistoryIndex)];
                model.CurrentSentence = s;                
            }
        }

        #endregion
        
    }

    //Note - if we were using Microsoft's "Prism" Framework we wouldn't need this class.
    //We're explicitly not using Prism here as a learning exercise
    public class VoiceDelegateCommand : ICommand
    {
        Action<object> _executeDelegate;

        public VoiceDelegateCommand( Action<object> executeDelegate )
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute( object parameter )
        {
            _executeDelegate( parameter );
        }

        public bool CanExecute( object parameter )
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
    }

}
