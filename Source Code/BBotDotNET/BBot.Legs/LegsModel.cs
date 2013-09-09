using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BBot
{
    public class LegsModel : IRobotWidget, INotifyPropertyChanged
    {
        int throttleLeft, throttleRight, throttleLeftMixed, throttleRightMixed;
        int throttleInterval;
        bool controlSignalSourceSelect;
        bool leapMotionEnable;

        BBotLeapMotionWrapper leapWrapper;

        public event PropertyChangedEventHandler PropertyChanged;

        private const string op_LegsWalk = "L00";
        private const string op_Control_Select_DX6i = "L01";
        private const string op_Control_Select_Software = "L02";
        private const string op_Termination = "\r";

        int motorMixedLeft, motorMixedRight;

        IRobotConnection connection;

        public LegsModel( IRobotConnection Connection )
        {
            this.connection = Connection;
            RouteMessages( this.connection );
            this.ThrottleInterval = 10;
            controlSignalSourceSelect = false;
            
            //100 is middle of motor throw (not rotating)
            //It's converted to 1.5ms pulse widths in the FPGA
            ThrottleLeft = 100;
            ThrottleRight = 100;

        }

        public int ThrottleLeft
        {
            get
            {
                return throttleLeft;
            }
            set
            {
                throttleLeft = value;
                NotifyPropertyChanged("ThrottleLeft");
                NotifyPropertyChanged("ThrottleLeft_Inverted");
                NotifyPropertyChanged("ThrottleLeft_Percent");
            }
        }

        public int ThrottleRight
        {
            get
            {
                return throttleRight;
            }
            set
            {
                throttleRight = value;
                NotifyPropertyChanged("ThrottleRight");
                NotifyPropertyChanged("ThrottleRight_Inverted");
                NotifyPropertyChanged("ThrottleRight_Percent");
            }
        }

        public int ThrottleLeft_Inverted
        {
            get
            {
                return 100 - throttleLeft ;
            }
            set { }
        }

        public int ThrottleRight_Inverted
        {
            get
            {
                return 100 - throttleRight;
            }
            set { }
        }

        public int ThrottleLeft_Percent
        {
            get
            {
                return throttleLeft - 100;
            }
            set { }
        }

        public int ThrottleRight_Percent
        {
            get
            {
                return throttleRight - 100;
            }
            set { }
        }

        public int ThrottleInterval
        {
            get
            {
                return throttleInterval;
            }
            set
            {
                throttleInterval = value;
                NotifyPropertyChanged( "ThrottleInterval" );
            }
        }

        public bool ControlSignalSourceSelect
        {
            get 
            { 
                return controlSignalSourceSelect; 
            }
            set 
            { 
                controlSignalSourceSelect = value;
                Throttle_Stop();
                SendControlFormatted(value);
            }
        }

        public bool LeapMotionEnable
        {
            get
            {
                return leapMotionEnable;
            }
            set
            {
                leapMotionEnable = value;
                leapWrapper = new BBotLeapMotionWrapper();
            }
        }

        #region IRobotWidget Members

        public void SendMessage( string Message )
        {
            connection.SendMessage( Message );
        }
        public void ReceiveMessage( string Message )
        {
            //Not needed yet
        }
        public void RouteMessages( IRobotConnection BotConnection )
        {
            //Not needed yet
        }
        public void InFocus( bool IsFocused )
        {
            //Not needed yet
        }


        #endregion

        public void Throttle_Up()
        {
            int temp = LowestThrottle();
            this.ThrottleLeft = temp + this.ThrottleInterval;
            this.ThrottleRight = temp + this.ThrottleInterval;
            SendThrottleFormatted();
        }

        public void Throttle_Down()
        {
            int temp = LowestThrottle();
            this.ThrottleLeft = temp - this.ThrottleInterval;
            this.ThrottleRight = temp - this.ThrottleInterval;
            SendThrottleFormatted();
        }

        public void Throttle_Left()
        {
            this.ThrottleRight += this.ThrottleInterval;
            SendThrottleFormatted();
        }

        public void Throttle_Right()
        {
            this.ThrottleLeft += this.ThrottleInterval;
            SendThrottleFormatted();
        }

        public void Throttle_Stop()
        {
            this.ThrottleLeft = 100;
            this.ThrottleRight = 100;
            SendThrottleFormatted();
        }

        int LowestThrottle()
        {
            return (this.ThrottleLeft >= this.ThrottleRight) ? this.ThrottleRight : this.ThrottleLeft;
        }

        void UpdateMixedLeftAndRightThrottle()
        {
            //Because the motor drives are running in a "mix mode"
            //left and right motor speeds need to be converted into 
            //the correct left and right "mixed" equivelet.
            //We will do that here, then simply send down the desired mixed outputs
            //to the embedded.

            //In general the right motor's pulse train is used as a throttle forward or backward
            //and the lef motor pulse train controls the "trim" or reletive speed between the wheels.

            //First determine the average power.  Forward or backward.
            double averagePower = (this.ThrottleLeft + this.ThrottleRight) / 2;
            motorMixedRight = (int)averagePower;

            double trim = Math.Abs((this.ThrottleLeft - this.ThrottleRight)) / 2;
            if (this.ThrottleLeft >= this.throttleRight)
            {
                motorMixedLeft = 100 - (int)trim;
            }
            else
            {
                motorMixedLeft = (int)trim + 100;
            }
        }
        
        void SendThrottleFormatted()
        {
            UpdateMixedLeftAndRightThrottle();

            string msg = op_LegsWalk;
            msg += " ";
            msg += motorMixedLeft.ToString();
            msg += " ";
            msg += motorMixedRight.ToString();
            msg += op_Termination;
            SendMessage( msg );
        }

        void SendControlFormatted(bool Source)
        {
            string msg = Source ? op_Control_Select_Software : op_Control_Select_DX6i;
            msg += "  ";
            msg += op_Termination;
            SendMessage(msg);
        }

        private void NotifyPropertyChanged( String propertyName = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }


    }
}
