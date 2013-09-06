using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Data;


namespace BBot
{

    [ValueConversion( typeof( List<string> ), typeof( string ) )]
    public class ListToStringConverter : IValueConverter
    {

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( targetType != typeof( string ) )
                throw new InvalidOperationException( "The target must be a String" );

            return String.Join("\r", ( ( List<string> ) value ).ToArray() );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }



    public interface IRobotWidget
    {
        void SendMessage(string Message);
        void ReceiveMessage(string Message);
        void RouteMessages(IRobotConnection BotConnection);
        void InFocus(bool IsFocused);
    }

    public interface IRobotConnection
    {
        event EventHandler<StringEventArgs> MessageSent;
        event EventHandler<StringEventArgs> MessageReceived;
        void SendMessage(string Message);
        bool IsConnected();
    }


    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(string message)
        {
            this.Sender = null;
            this.Message = message;
        }

        public StringEventArgs( object sender, string message )
        {
            this.Sender = sender;
            this.Message = message;
        }

        public string Message
        {
            get;
            private set;
        }

        public object Sender
        {
            get;
            private set;
        }
    }


}
