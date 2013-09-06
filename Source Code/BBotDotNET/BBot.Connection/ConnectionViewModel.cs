using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBot
{

    public class ConnectionViewModel
    {

        ConnectionIPModel modelIP;
        ConnectionCOMModel modelCOM;

        #region Constructor
        public ConnectionViewModel()
        {
            modelCOM = new ConnectionCOMModel(3);
            modelIP = new ConnectionIPModel();
        }

        public ConnectionViewModel(string IpAddress, int Port, string UserName, string Password)
        {
            modelIP = new ConnectionIPModel();
            modelIP.IpAddress = IpAddress;
            modelIP.Port = Port;
            modelIP.UserName = UserName;
            modelIP.Password = Password;
        }

        public ConnectionViewModel(int ComPortNum)
        {
            modelCOM = new ConnectionCOMModel();
            modelCOM.ComPortNumber = ComPortNum;
        }

        #endregion

        #region Properties

        public ConnectionIPModel ConnectionIPModel
        {
            get
            {
                return modelIP;
            }
        }

        public ConnectionCOMModel ConnectionCOMModel
        {
            get
            {
                return modelCOM;
            }
        }

        public bool IsConnectedIP
        {
            get { return modelIP.IsConnected(); }
        }

        public string IpAddress
        {
            get { return modelIP.IpAddress; }
            set { modelIP.IpAddress = value; }
        }

        public int Port
        {
            get { return modelIP.Port; }
            set { modelIP.Port = value; }
        }

        public string UserName
        {
            get { return modelIP.UserName; }
            set { modelIP.UserName = value; }
        }

        public string Password
        {
            get { return modelIP.Password; }
            set { modelIP.Password = value; }
        }

        public bool IsConnectedCOM
        {
            get { return modelCOM.IsConnected(); }
        }
        public int ComPortNumber
        {
            get { return modelCOM.ComPortNumber; }
            set { modelCOM.ComPortNumber = value; }
        }

        public bool ComPortConnected
        {
            get { return modelCOM.ComPortConnected; }
            set { modelCOM.ComPortConnected = value; }
        }



        #endregion

        public void CloseComPort()
        {
            modelCOM.ComPortShell.Disconnect();
        }


    }
}
