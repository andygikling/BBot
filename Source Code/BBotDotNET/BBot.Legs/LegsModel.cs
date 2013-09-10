using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace BBot
{
    public class LegsModel : IRobotWidget, INotifyPropertyChanged
    {
        int throttleLeft, throttleRight, throttleLeftMixed, throttleRightMixed;
        int throttleInterval;
        bool controlSignalSourceSelect;
        
        //LeapMotion related vaiables
        bool leapMotionEnable;
        Thread LeapListenThread;
        const int X_TRAVEL = 200;
        const int Y_TRAVEL = 250;
        const int Z_TRAVEL = 300;


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
                
                if (LeapListenThread == null && value)
                {
                    SendMessage("C06 \r"); //Disable Logger
                    
                    leapWrapper = new BBotLeapMotionWrapper();
                    LeapListenThread = new Thread(LeampMotionListener);
                    LeapMotionThread_StopRead = false;
                    LeapListenThread.Name = "LeapListenThread";
                    LeapListenThread.Start();
                }
                else
                {
                    try
                    {
                        leapWrapper.StopRead = true;
                        leapWrapper.ReadThreadDispose();
                        LeapMotionThread_StopRead = true;
                        LeapListenThread.Join();
                        leapWrapper = null;
                        LeapListenThread = null;

                        SendMessage("C05 \r"); //Enable Logger
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine("Leap Motion thread doesn't exist - can't Join()");
                    }
                }
            }
        }

        public bool LeapMotionThread_StopRead { get; set; }

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

        private void LeampMotionListener()
        {
            int loopCount = 0;
            bool transmit = false;

            while (!LeapMotionThread_StopRead)
            {
                
                System.Threading.Thread.Sleep(200);

                System.Diagnostics.Trace.WriteLine("BBotLeapRead - X=" + (leapWrapper.Hand_X_Position.ToString() +
                                                    " Y=" + leapWrapper.Hand_Y_Position.ToString() +
                                                    " Z=" + leapWrapper.Hand_Z_Position.ToString() +
                                                    " Roll=" + leapWrapper.Hand_Roll.ToString() +
                                                    " Pitch=" + leapWrapper.Hand_Pitch.ToString() +
                                                    " Yaw=" + leapWrapper.Hand_Yaw.ToString() +
                                                    " Present=" + leapWrapper.Hand_Y_Position.ToString()));

                CalculateMotorPowersFromLeapData(leapWrapper.Hand_X_Position,
                                                    leapWrapper.Hand_Y_Position,
                                                    leapWrapper.Hand_Z_Position,
                                                    leapWrapper.Hand_Roll,
                                                    leapWrapper.Hand_Pitch,
                                                    leapWrapper.Hand_Yaw,
                                                    leapWrapper.Hand_Present,
                                                    transmit);

                if (loopCount == 2)
                {
                    transmit = true;
                    loopCount = 0;
                }
                else
                {
                    transmit = false;
                    loopCount++;
                }
                

            }


        }

        private void CalculateMotorPowersFromLeapData(double X, double Y, double Z, double Roll, double Pitch, double Yaw, bool HandPresent, bool Trasmit)
        {
            double motorL, motorR;
            double x, y, z, roll, pitch, yaw;

            if (HandPresent)
            {
                //Leap coords is, +Y is up, +X is to the right and +Z is toward you
                //We'll change it so the controls are only labeled XY and that represents Leap's XZ plane
                x = X;
                y = -Z;
                z = Y;
                roll = Roll;
                pitch = Pitch;
                yaw = Yaw;

                //Get total forward backward throttle
                motorL = ((y / Y_TRAVEL) * 100) + 100;
                motorR = ((y / Y_TRAVEL) * 100) + 100;

                //Trim by left right
                if (motorL >= 100 && motorR >= 100)
                {
                    //going forward
                    if (x >= 0)
                    {
                        //Need to turn right (left motor stays put and we subtract power from right motor
                        motorR = motorR - (x / X_TRAVEL) * 100;
                        //Pin travel for usability
                        if (motorR < 100)
                        {
                            motorR = 100;
                        }
                    }
                    else
                    {
                        //Need to turn left...
                        motorL = motorL - -((x / X_TRAVEL) * 100);
                        if (motorL < 100)
                        {
                            motorL = 100;
                        }
                    }
                }
                else
                {
                    //we're going backward
                    if (x >= 0)
                    {
                        motorR = (x / X_TRAVEL) * 100 + motorR;
                        if (motorR > 100)
                        {
                            motorR = 100;
                        }
                    }
                    else
                    {
                        //Need to turn left...
                        motorL = -((x / X_TRAVEL) * 100) + motorL;
                        if (motorL > 100)
                        {
                            motorL = 100;
                        }
                    }
                }

            }
            else
            {
                motorL = 100;
                motorR = 100;
            }

            this.ThrottleLeft = (int)motorL;
            this.ThrottleRight = (int)motorR;
            if (Trasmit)
            {
                UpdateMixedLeftAndRightThrottle();
                SendThrottleFormatted();
            }
        }


    }

}
