using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BBot
{
    public class EyeModel : IRobotWidget
    {
        IRobotConnection connection;
        int panPosition, tiltPosition;

        const string op_EyeRemotePanTiltEnable = "E00";
        const string op_EyeSoftwarePanTiltEnable = "E01";
        const string op_EyeSoftwareSetPanTiltPosition = "E02";
        const string op_Termination = "\r";

        public EyeModel(IRobotConnection Connection)
        {
            RouteMessages(Connection);

            PanPosition = 100;
            TiltPosition = 100;
        }

        #region Properties

        public int PanPosition
        {
            get
            {
                return panPosition;
            }
            set
            {
                panPosition = value;
                SendPanTiltPosition(panPosition, tiltPosition);
            }
        }

        public int TiltPosition
        {
            get
            { 
                return tiltPosition;
            }
            set
            {
                tiltPosition = value;
                SendPanTiltPosition(panPosition, tiltPosition);
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
        }

        public void RouteMessages(IRobotConnection Connection)
        {
            connection = Connection;
        }

        public void InFocus(bool IsFocused)
        {

        }

        #endregion

        #region Methods

        void SendPanTiltModeSelect(bool SoftwarePanTiltMode)
        {
            string msg = "";
            if (SoftwarePanTiltMode)
            {
                msg += op_EyeSoftwarePanTiltEnable;
            }
            else
            {
                msg += op_EyeRemotePanTiltEnable;
            }
            msg += " " + op_Termination;
            SendMessage(msg);
        }

        void SendPanTiltPosition(int PanPosition, int TiltPosition)
        {
            string msg = "";
            msg += op_EyeSoftwareSetPanTiltPosition;
            msg += " " + PanPosition.ToString() + " " + TiltPosition.ToString() + op_Termination;
            SendMessage(msg);
        }
        
        #endregion

    }
}
