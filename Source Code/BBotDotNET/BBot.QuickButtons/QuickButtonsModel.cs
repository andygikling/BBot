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

        private const string op_ClearDebugOverlay = "C00";
        private const string op_SetDebugOverlay_ListenThread = "C01";
        private const string op_SetDebugOverlay_VoiceThread = "C02";
        private const string op_SetDebugOverlay_LegsThread = "C03";
        private const string op_SetDebugOverlay_Mark1FPGAThread = "C04";
        private const string op_DisableLogger = "C05";
        private const string op_EnableLogger = "C06";
        private const string op_DisableAlarm = "C07";
        private const string op_EnableAlarm = "C08";
        private const string op_Termination = "\r";

        private bool enableDebugScreenOverlay;
        private int debugOverlayNumber = 0;
        private bool enableAlarm = false;
        private bool enableLogger = true;

        public QuickButtonsModel(IRobotConnection Connection)
        {
            connection = Connection;
            RouteMessages(connection);
        }

        public bool EnableDebugScreenOverlay
        {
            get
            {
                return enableDebugScreenOverlay;
            }
            set
            {
                enableDebugScreenOverlay = value;
                if (value)
                {
                    SetDebugOverlayOnScreen();
                }
                else
                {
                    ClearDebugOverlayOnScreen();
                }
            }
        }
        public int DebugOverlayNumber
        {
            get
            {
                return debugOverlayNumber;
            }
            set
            {
                debugOverlayNumber = value;
            }
        }

        public bool EnableAlarm
        {
            get
            {
                return enableAlarm;
            }
            set
            {
                enableAlarm = value;
                SendAlarm(value);
            }
        }

        public bool EnableLogger
        {
            get
            {
                return enableLogger;
            }
            set
            {
                enableLogger = value;
                SendEnableLogger(value);
            }
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
            switch (this.DebugOverlayNumber)
            {
                case 0 :
                    SendMessage(op_SetDebugOverlay_ListenThread + "  " + op_Termination);
                    break;
                case 1:
                    SendMessage(op_SetDebugOverlay_VoiceThread + "  " + op_Termination);
                    break;
                case 2:
                    SendMessage(op_SetDebugOverlay_LegsThread + "  " + op_Termination);
                    break;
                case 3:
                    SendMessage(op_SetDebugOverlay_Mark1FPGAThread + "  " + op_Termination);
                    break;
            }
            
        }

        public void ClearDebugOverlayOnScreen()
        {
            SendMessage(op_ClearDebugOverlay + "  " + op_Termination);
        }

        public void SendAlarm( bool Enable )
        {
            if (Enable)
            {
                SendMessage(op_EnableAlarm + " " + op_Termination);
            }
            else
            {
                SendMessage(op_DisableAlarm + " " + op_Termination);
            }
        }

        public void SendEnableLogger(bool Enable)
        {
            if (Enable)
            {
                SendMessage(op_EnableLogger + " " + op_Termination);
            }
            else
            {
                SendMessage(op_DisableLogger + " " + op_Termination);
            }
        }

    }


}
