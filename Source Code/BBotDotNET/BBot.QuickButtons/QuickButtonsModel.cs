using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;


namespace BBot
{
    public class QuickButtonsModel : IRobotWidget
    {
        IRobotConnection connection;

        private const string op_SetDebugOverlay = "C00";
        private const string op_ClearDebugOverlay = "C01";
        private const string op_Termination = "\r";


        public QuickButtonsModel(IRobotConnection Connection)
        {
            connection = Connection;
            RouteMessages(connection);
        }




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

        public void SetDebugOverlayOnScreen()
        {
            SendMessage(op_SetDebugOverlay + " " + op_Termination);
        }

        public void ClearDebugOverlayOnScreen()
        {
            SendMessage(op_ClearDebugOverlay + " " + op_Termination);
        }
    }


}
