using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBot
{
    public interface ICommunicationShell
    {
        bool Connected { get; set; }
        string ReceiveDataDelimiter { get; set; }
        bool Connect();
        bool Disconnect();
        void SendString(string command);
        void ParseResponse(string response);
    }
}
