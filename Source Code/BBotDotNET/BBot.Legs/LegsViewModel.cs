using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BBot
{
    public class LegsViewModel
    {

        LegsModel model;

        ICommand keyUp;
        ICommand keyDown;
        ICommand keyLeft;
        ICommand keyRight;
        ICommand keyStop;

        public LegsViewModel( IRobotConnection Connection )
        {
            model = new LegsModel( Connection );

            keyUp = new VoiceDelegateCommand( ( x ) => model.Throttle_Up() );
            keyDown = new VoiceDelegateCommand( ( x ) => model.Throttle_Down() );
            keyLeft = new VoiceDelegateCommand( ( x ) => model.Throttle_Left() );
            keyRight = new VoiceDelegateCommand( ( x ) => model.Throttle_Right() );
            keyStop = new VoiceDelegateCommand( ( x ) => model.Throttle_Stop() );
        }

        public LegsModel LegsModel
        {
            get
            {
                return model;
            }
        }

        public ICommand KeyUp
        {
            get
            {
                return keyUp;
            }
        }

        public ICommand KeyDown
        {
            get
            {
                return keyDown;
            }
        }

        public ICommand KeyLeft
        {
            get
            {
                return keyLeft;
            }
        }

        public ICommand KeyRight
        {
            get
            {
                return keyRight;
            }
        }

        public ICommand KeyStop
        {
            get
            {
                return keyStop;
            }
        }

    }



    //Note - if we were using Microsoft's "Prism" Framework we wouldn't need this class.
    //We're explicitly not using Prism here as a learning exercise
    public class VoiceDelegateCommand : ICommand
    {
        Action<object> _executeDelegate;

        public VoiceDelegateCommand( Action<object> executeDelegate )
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute( object parameter )
        {
            _executeDelegate( parameter );
        }

        public bool CanExecute( object parameter )
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
    }
}
