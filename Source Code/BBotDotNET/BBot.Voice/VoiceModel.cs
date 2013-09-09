using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BBot
{
    public class VoiceModel : IRobotWidget, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> sentences = new List<string>();
        private List<string> sentencesWithTimestamp = new List<string>();
        private string currentSentence;
        private int speed = 77;
        private int voiceNumber;

        private const string op_VoiceOpCode = "V";
        private const string op_SendSettings = "00";
        private const string op_StopTalking = "05";
        private const string op_SpeakNow = "01";
        private const string op_Termination = "\r";

        IRobotConnection connection;

        public VoiceModel(IRobotConnection Connection)
        {
            connection = Connection;
            RouteMessages(connection);

            this.TalkSpeed = 150;
        }

        #region Properties

        public string CurrentSentence
        {
            get
            {
                return currentSentence;
            }
            set
            {
                currentSentence = value;
                NotifyPropertyChanged("CurrentSentence");
            }
        }

        public List<string> SentenceHistory
        {
            get
            {
                return sentences;
            }
            set
            {
                sentences = value;
            }
        }

        public List<string> SentenceHistoryWithTimestamp
        {
            get
            {
                return sentencesWithTimestamp;
            }
            set
            {
                sentencesWithTimestamp = value;
            }
        }

        public int TalkSpeed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                NotifyPropertyChanged("TalkSpeed");
            }
        }

        public int VoiceNumber
        {
            get
            {
                return voiceNumber;
            }
            set
            {
                voiceNumber = value;
            }
        }
        
        #endregion

        #region IRobotWidget Members

        public void SendMessage(string Message)
        {
            connection.SendMessage(Message);
        }

        public void ReceiveMessage(string Message)
        {
            //Not needed
        }
        
        public void RouteMessages(IRobotConnection BotConnection)
        {
            //Used to link up the connection's MessageSent and MessageReceived events
            //Not really needed here because the Voice only sends messages and doesn't really care what comes back... (for now)
        }

        public void InFocus(bool IsFocused)
        {
            //TBD
        }

        #endregion

        #region Methods

        public void SendCurrentSentence(string sentence)
        {
            SentenceHistory.Add(sentence);
            SentenceHistoryWithTimestamp.Add(DateTime.Now.ToString() + " : " + sentence);
            TalkNow(sentence);
            CurrentSentence = "";
            NotifyPropertyChanged("SentenceHistoryWithTimestamp");
        }

        public void TalkNow(string Message)
        {
            string message = formatMessage(Message, op_SpeakNow);
            SendMessage(message);
        }

        public string formatMessage(string Message, string SubFunction)
        {
            string result = "";
            result = result + op_VoiceOpCode + SubFunction + " " + Message + op_Termination;
            return result;
        }

        public void StopTalking()
        {
            string message = formatMessage("", op_StopTalking);
            SendMessage(message);
        }

        public void SendSettings()
        {
            string message;
            message = VoiceNumber.ToString() + " " + TalkSpeed.ToString();
            string send = formatMessage(message, op_SendSettings);
            SendMessage(send);
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        
       
        


    }
}
