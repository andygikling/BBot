using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BBot
{
    public class BotTerminalModel : IRobotWidget, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        string currentCommand;
        private List<string> terminalHistory = new List<string>();
        private List<string> terminalHistoryWithTimeStamp = new List<string>();

        IRobotConnection connection;

        public BotTerminalModel(IRobotConnection Connection)
        {
            RouteMessages(Connection);
        }

        #region Properties

        public string CurrentCommand
        {
            get
            {
                return currentCommand;
            }
            set
            {
                currentCommand = value;
                NotifyPropertyChanged("CurrentCommand");
            }
        }

        public List<string> TerminalHistory
        {
            get
            { 
                return terminalHistory;
            }
            set
            {
                terminalHistory = value;
            }
        }

        public List<string> TerminalHistoryWithTimeStamp
        {
            get
            {
                return terminalHistoryWithTimeStamp;
            }
            set
            {
                terminalHistoryWithTimeStamp = value;
            }
        }
        
        #endregion

        #region IRobotWidget Members

        public void SendMessage(string Message)
        {
            connection.SendMessage(Message + " \r"); //Embedded code is expecting a \r to delimit the end of a message
        }

        public void ReceiveMessage(string Message)
        {
        }

        public void RouteMessages(IRobotConnection Connection)
        {
            Connection.MessageSent += new EventHandler<StringEventArgs>(DisplayMessageSent);
            Connection.MessageReceived += new EventHandler<StringEventArgs>(DisplayMessageReceived);
            connection = Connection;
        }

        public void InFocus(bool IsFocused)
        {

        }

        #endregion

        #region Methods

        public void SendCustomBotCommand(string Command)
        {
            SendMessage(Command);
        }

        public void AddToTerminalHistory(string Message)
        {
            //Messages passing through here always have a \r at the end because the embedded code is expecting it
            //The \r makes extra spaces in our view's text block.
            //Here we remove the \r before adding the message to the view
            if (Message.Contains("\r"))
            {
                Message = Message.Remove(Message.Length - 1);
            }

            terminalHistoryWithTimeStamp.Add(DateTime.Now.ToString() + " : " + Message);
            NotifyPropertyChanged("TerminalHistoryWithTimeStamp");
            terminalHistory.Add(Message);
        }

        void DisplayMessageSent(object sender, StringEventArgs e)
        {
            AddToTerminalHistory(e.Message);
        }

        void DisplayMessageReceived(object sender, StringEventArgs e)
        {
            AddToTerminalHistory("Received : " + e.Message);
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
