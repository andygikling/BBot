using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBot
{
    public class ConnectionCOMModel : IRobotConnection
    {

        ComPortShell comShell = new ComPortShell();

        public event EventHandler<StringEventArgs> MessageSent;
        public event EventHandler<StringEventArgs> MessageReceived;

        bool isConnected = false;
        bool comPortConnected = false;
        
        #region Constructor
        public ConnectionCOMModel()
        {

        }

        public ConnectionCOMModel(int COMPortNum)
        {
            this.ComPortNumber = COMPortNum;
        }

        #endregion

        #region Properties

        public ComPortShell ComPortShell
        {
            get { return comShell; }
        }

        public bool ComPortConnected
        {
            get
            {
                return isConnected;
            }
            set
            {
                isConnected = Connect(value);  
            }
        }

        public int ComPortNumber { get; set; }

        #endregion //Properties

        #region IRobotConnection Members
        public void SendMessage(string Message)
        {
            if (IsConnected())
            {
                comShell.SendString(Message);
                MessageSent(this, new StringEventArgs(this, Message));
                System.Diagnostics.Trace.WriteLine(Message);
            }
            else
            {
                MessageSent(this, new StringEventArgs(this, "Connection To Bot Not Open!"));
            }
        }

        public bool IsConnected()
        {
            return isConnected;
        }
        #endregion

        #region Methods


        public bool Connect(bool ConnectDisconnect)
        {
            //Make sure we're not already connected
            if (!comPortConnected && ConnectDisconnect)
            {
                //Open COM port if it is there
                foreach (string s in this.comShell.GetComPortNames())
                {
                    if (s == "COM" + this.ComPortNumber.ToString())
                    {
                        //Good this machine has the requested COM port
                        //Now Open it if it's not in use
                        this.comShell.PortName = s;
                        if (comShell.Connect())
                        {
                            comShell.ShellDataReceived += new EventHandler<StringEventArgs>(botResponse);
                            comPortConnected = ConnectDisconnect;
                            System.Diagnostics.Trace.WriteLine("ConnectionModel - COM port Opened");
                            return true;
                        }
                        else
                        {
                            comPortConnected = false;
                            System.Diagnostics.Trace.WriteLine("ConnectionModel - faild to connect to COM port...");
                            return false;
                        }
                    }
                }
                //Didn't find the com port number
                comPortConnected = false;
                comShell.ShellDataReceived -= new EventHandler<StringEventArgs>(botResponse);
                comShell.Disconnect();
                return false;
            }
            else
            {
                comPortConnected = false;
                comShell.ShellDataReceived -= new EventHandler<StringEventArgs>(botResponse);
                comShell.Disconnect();
                return false;
            }
        }
        
        public void botResponse(object sender, StringEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Bot Message Back: " + e.Message);
            MessageReceived(this, new StringEventArgs(this, e.Message));
        }

    
        #endregion

    }

    public class ConnectionIPModel : IRobotConnection
    {
        //IP Variables
        bool isConnected = false;
        string ip = "";
        int port = 22;
        string userName = "";
        string password = "";

        public event EventHandler<StringEventArgs> MessageSent;
        public event EventHandler<StringEventArgs> MessageReceived;
        
        public ConnectionIPModel()
        {
        }

        public ConnectionIPModel(string Ip, int Port, string UserName, string Password)
        {
            ip = Ip;
            port = Port;
            userName = UserName;
            password = Password;
        }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public void SendMessage(string Message)
        {
        }

        public bool Connect(bool ConnectDisconnect)
        {
            return false;
        }

        public bool IsConnected()
        {
            return isConnected;
        }
    }

}
