using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;

namespace BBot
{
    public class ComPortShell : ICommunicationShell
    {
        #region Vars

        private SerialPort comPort = new SerialPort();
        private int _baudRate;
        private Parity _parity;
        private int _dataBits;
        private StopBits _stopBits;
        private string _delimiter;
        private string _portName;

        private static bool _connected;
        private Thread _readThread;
        public event EventHandler<StringEventArgs> ShellDataReceived;

        private List<string> _availalbeComPorts;

        #endregion

        #region Construct

        public ComPortShell()
        {
            setDefaults("COM" + 3.ToString());

            _availalbeComPorts = GetComPortNames();
        }

        public ComPortShell(string ComPortName)
        {
            setDefaults(ComPortName);
            this.Connected = this.Connect();

            _availalbeComPorts = GetComPortNames();
        }

        public ComPortShell(int ComPortNumber)
        {
            setDefaults("COM" + ComPortNumber.ToString());
            //this.Connected = this.Connect();

            _availalbeComPorts = GetComPortNames();
        }

        void setDefaults(string name)
        {
            //Defaults
            this.BaudRate = 115200;
            this.Parity = Parity.None;
            this.DataBits = 8;
            this.PortName = name;
            this.StopBits = System.IO.Ports.StopBits.One;
            comPort.ReadTimeout = 800;
            comPort.WriteTimeout = 500;
            //comPort.NewLine = "";
        }

        #endregion

        #region Properties


        public int BaudRate
        {
            get
            {
                return _baudRate;
            }
            set
            {
                _baudRate = value;
                comPort.BaudRate = value;
            }
        }

        public Parity Parity
        {
            get
            {
                return _parity;
            }
            set
            {
                _parity = value;
                comPort.Parity = value;
            }
        }

        public int DataBits
        {
            get
            {
                return _dataBits;
            }
            set
            {
                _dataBits = value;
                comPort.DataBits = value;
            }
        }

        public StopBits StopBits
        {
            get
            {
                return _stopBits;
            }
            set
            {
                _stopBits = value;
                comPort.StopBits = value;
            }
        }

        public string Delimiter
        {
            get
            {
                return _delimiter;
            }
            set
            {
                _delimiter = value;
                comPort.NewLine = value;
            }
        }

        public string PortName
        {
            get
            {
                return _portName;
            }
            set
            {
                _portName = value;
                comPort.PortName = value;
            }
        }

        #endregion

        #region ICommunicationShell Members

        public bool Connected
        {
            get
            {
                return _connected;
            }
            set
            {
                _connected = value;
            }
        }

        public string ReceiveDataDelimiter
        {
            get
            {
                return this.Delimiter;
            }
            set
            {
                this.Delimiter = value;
            }
        }

        public bool Connect()
        {
            try
            {
                if (!comPort.IsOpen)
                {
                    //If it's not in use let's use it.
                    this.Connected = true;
                    comPort.Open();
                    _readThread = new Thread(Read);
                    _readThread.Start();
                    return true;
                }
            }
            catch
            {
                System.Diagnostics.Trace.WriteLine("-- COM Connect Error --");
            }
            return false;
        }

        public bool Disconnect()
        {
            try
            {
                ReadThreadDispose();
                comPort.Close();
                this.Connected = false;
                return true;
            }
            catch
            {
            }
            return false;
        }

        public void SendString(string command)
        {
            if (this.Connected)
                comPort.Write(command);
        }

        public void ParseResponse(string response)
        {
        }



        #endregion

        #region Methods

        private void Read()
        {
            while (_connected)
            {
                try
                {
                    string message = comPort.ReadLine();
                    this.PublishShellResponse(message);
                }
                catch (TimeoutException)
                {
                }
                catch (IOException)
                {
                    //This throws when you dispose the thread
                }
            }
        }

        public void PublishShellResponse(string message)
        {
            ShellDataReceived(this, new StringEventArgs(message));
        }

        public List<string> GetComPortNames()
        {
            //This can be usefull - don't need to ref the System.IO.Ports all over the place 
            //in projects using this class
            List<string> result = new List<string>();

            string[] comNames = SerialPort.GetPortNames();

            foreach (string s in comNames)
            {
                result.Add(s);
            }

            return result;
        }


        #endregion

        public void Dispose()
        {
            _connected = false;
            comPort.Dispose();
            ReadThreadDispose();
        }

        public void ReadThreadDispose()
        {
            _connected = false;
            _readThread.Join();
        }

    }



}
