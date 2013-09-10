using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using Leap;

namespace BBot
{
    public class BBotLeapMotionWrapper
    {

        Thread leapReadThread;
        SampleListener listener;
        Controller controller;

        public BBotLeapMotionWrapper()
        {
            // Create a sample listener and controller
            listener = new SampleListener();
            controller = new Controller();

            StopRead = false;

            ThreadStart starter = () => Read(ref listener, ref controller);
            leapReadThread = new Thread(starter);
            leapReadThread.Name = "BBotLeapMotionWrapper:ReadThread";
            leapReadThread.Start();            
        }

        #region Properties
        public double Hand_X_Position
        {
            get
            {
                return listener.Hand_X_Position;
            }
            set
            {
            }
        }

        public double Hand_Y_Position
        {
            get
            {
                return listener.Hand_Y_Position;
            }
            set
            {
            }
        }
        public double Hand_Z_Position
        {
            get
            {
                return listener.Hand_Z_Position;
            }
            set
            {
            }
        }
        public double Hand_Roll
        {
            get
            {
                return listener.Hand_Roll;
            }
            set
            {
            }
        }
        public double Hand_Pitch
        {
            get
            {
                return listener.Hand_Pitch;
            }
            set
            {
            }
        }
        public double Hand_Yaw
        {
            get
            {
                return listener.Hand_Yaw;
            }
            set
            {
            }
        }
        public bool Hand_Present
        {
            get
            {
                return listener.Hand_Present;
            }
            set
            {
            }
        }

        public bool StopRead { get; set; }



        #endregion

        void Read( ref SampleListener Listener, ref Controller Controller)
        {
  
            // Have the sample listener receive events from the controller
            Controller.AddListener(Listener);

            while (!StopRead)
            {
                //Do thing here
                //The listener class is gathering our data
            }

            // Remove the sample listener when done
            Controller.RemoveListener(Listener);
            Controller.Dispose();
            Listener.Dispose();
        }

        public void ReadThreadDispose()
        {
            leapReadThread.Join();
        }
    }


    //The following code was derrived from the LeapMotion SDK -
    /******************************************************************************\
    * Copyright (C) 2012-2013 Leap Motion, Inc. All rights reserved.               *
    * Leap Motion proprietary and confidential. Not for distribution.              *
    * Use subject to the terms of the Leap Motion SDK Agreement available at       *
    * https://developer.leapmotion.com/sdk_agreement, or another agreement         *
    * between Leap Motion and you, your company or other organization.             *
    \******************************************************************************/
    class SampleListener : Listener
    {

        public double Hand_X_Position { get; set; }
        public double Hand_Y_Position { get; set; }
        public double Hand_Z_Position { get; set; }
        public double Hand_Roll { get; set; }
        public double Hand_Pitch { get; set; }
        public double Hand_Yaw { get; set; }
        public bool Hand_Present { get; set; }

        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        //Get's all the important hand information we need for BBot
        //and sets it into the properties of this class.
        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();

            /*
            SafeWriteLine("Frame id: " + frame.Id
                        + ", timestamp: " + frame.Timestamp
                        + ", hands: " + frame.Hands.Count
                        + ", fingers: " + frame.Fingers.Count
                        + ", tools: " + frame.Tools.Count
                        + ", gestures: " + frame.Gestures().Count);
            */
              
            if (!frame.Hands.IsEmpty)
            {
                Hand_Present = true;

                // Get the first hand
                Hand hand = frame.Hands[0];

                // Check if the hand has any fingers
                FingerList fingers = hand.Fingers;
                if (!fingers.IsEmpty)
                {
                    // Calculate the hand's average finger tip position
                    Vector avgPos = Vector.Zero;
                    foreach (Finger finger in fingers)
                    {
                        avgPos += finger.TipPosition;
                    }
                    avgPos /= fingers.Count;
                    //SafeWriteLine("Hand has " + fingers.Count
                    //            + " fingers, average finger tip position: " + avgPos);

                    this.Hand_X_Position = avgPos.x;
                    this.Hand_Y_Position = avgPos.y;
                    this.Hand_Z_Position = avgPos.z;
                }

                // Get the hand's sphere radius and palm position
                /*
                SafeWriteLine("Hand sphere radius: " + hand.SphereRadius.ToString("n2")
                            + " mm, palm position: " + hand.PalmPosition);
                 * */

                // Get the hand's normal vector and direction
                Vector normal = hand.PalmNormal;
                Vector direction = hand.Direction;

                // Calculate the hand's pitch, roll, and yaw angles
                /*
                SafeWriteLine("Hand pitch: " + direction.Pitch * 180.0f / (float)Math.PI + " degrees, "
                            + "roll: " + normal.Roll * 180.0f / (float)Math.PI + " degrees, "
                            + "yaw: " + direction.Yaw * 180.0f / (float)Math.PI + " degrees");
                */

                this.Hand_Roll = normal.Roll * 180.0f / (float)Math.PI;
                this.Hand_Pitch = direction.Pitch * 180.0f / (float)Math.PI;
                this.Hand_Yaw = direction.Yaw * 180.0f / (float)Math.PI;

            }
            else
            {
                Hand_Present = false;
            }

            // Get gestures
            /*
            GestureList gestures = frame.Gestures();
            for (int i = 0; i < gestures.Count; i++)
            {
                Gesture gesture = gestures[i];

                switch (gesture.Type)
                {
                    case Gesture.GestureType.TYPECIRCLE:
                        CircleGesture circle = new CircleGesture(gesture);

                        // Calculate clock direction using the angle between circle normal and pointable
                        String clockwiseness;
                        if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4)
                        {
                            //Clockwise if angle is less than 90 degrees
                            clockwiseness = "clockwise";
                        }
                        else
                        {
                            clockwiseness = "counterclockwise";
                        }

                        float sweptAngle = 0;

                        // Calculate angle swept since last frame
                        if (circle.State != Gesture.GestureState.STATESTART)
                        {
                            CircleGesture previousUpdate = new CircleGesture(controller.Frame(1).Gesture(circle.Id));
                            sweptAngle = (circle.Progress - previousUpdate.Progress) * 360;
                        }

                        SafeWriteLine("Circle id: " + circle.Id
                                       + ", " + circle.State
                                       + ", progress: " + circle.Progress
                                       + ", radius: " + circle.Radius
                                       + ", angle: " + sweptAngle
                                       + ", " + clockwiseness);
                        break;
                    case Gesture.GestureType.TYPESWIPE:
                        SwipeGesture swipe = new SwipeGesture(gesture);
                        SafeWriteLine("Swipe id: " + swipe.Id
                                       + ", " + swipe.State
                                       + ", position: " + swipe.Position
                                       + ", direction: " + swipe.Direction
                                       + ", speed: " + swipe.Speed);
                        break;
                    case Gesture.GestureType.TYPEKEYTAP:
                        KeyTapGesture keytap = new KeyTapGesture(gesture);
                        SafeWriteLine("Tap id: " + keytap.Id
                                       + ", " + keytap.State
                                       + ", position: " + keytap.Position
                                       + ", direction: " + keytap.Direction);
                        break;
                    case Gesture.GestureType.TYPESCREENTAP:
                        ScreenTapGesture screentap = new ScreenTapGesture(gesture);
                        SafeWriteLine("Tap id: " + screentap.Id
                                       + ", " + screentap.State
                                       + ", position: " + screentap.Position
                                       + ", direction: " + screentap.Direction);
                        break;
                    default:
                        SafeWriteLine("Unknown gesture type.");
                        break;
                }
            }
             * */

            //if (!frame.Hands.IsEmpty || !frame.Gestures().IsEmpty)
            //{
            //    SafeWriteLine("");
            //}
        }
    }

}