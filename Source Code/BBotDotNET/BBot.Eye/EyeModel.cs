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
        bool panTiltSoftwareControlEnable;

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

        public bool PanTiltSoftwareControlEnable
        {
            get
            {
                return panTiltSoftwareControlEnable;
            }
            set
            {
                panTiltSoftwareControlEnable = value;
                SendPanTiltModeSelect(value);
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

        void SendPanTiltModeSelect(bool SoftwarePanTiltEnable)
        {
            string msg = "";
            if (SoftwarePanTiltEnable)
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
            //Direction of sliders increasing value doesn't match increasing pulse width
            //movement of servo...  we could invert it anywhere... Here is just as fine a place 
            //as any (could invert it in the C++ on the BeagleBone Black or on the FPGA)
            //"Invert the direction by subtrating the position from our max range value of 200
            int pan = 200 - PanPosition;
            int tilt = 200 - TiltPosition;
            msg += " " +  pan.ToString() + " " + tilt.ToString() + op_Termination;
            SendMessage(msg);
        }
        
        #endregion

    }
}
