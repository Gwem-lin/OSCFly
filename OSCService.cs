using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading;
using SharpOSC;

namespace OSCFly
{
    public class OSCService
    {
        readonly OscMessage jumpOnMessage = new OscMessage("/input/Jump", 1);
        readonly OscMessage jumpOffMessage = new OscMessage("/input/Jump", 0);
        Thread _jumpThread;
        Thread _boosterThread;
        Thread _turnThread;
        readonly UDPSender _sender;
        readonly UDPListener _listener;
        readonly InputMapper _inputMapper;
        readonly FlightModelEngine _flightModelEngine;
        private bool _verbose;
        public OSCService(bool verbose, InputMapper inputMapper, FlightModelEngine flightModelEngine)
        {
            _inputMapper = inputMapper;
            _flightModelEngine = flightModelEngine;
            _verbose = verbose;
            _sender = new UDPSender("127.0.0.1", 9000);
            _listener = new UDPListener(9001, new HandleBytePacket(HandlePacket));
        }
        public void Initialize()
        {
            _jumpThread = new Thread(SendJump);
            _jumpThread.Start();
            _boosterThread = new Thread(SendBooster);
            _boosterThread.Start();
            _turnThread = new Thread(SendTurn);
            _turnThread.Start();
            _flightModelEngine.Initialize();
        }
        public void End()
        {
            _flightModelEngine.End();
            _jumpThread.Abort();
            _boosterThread.Abort();
            _sender.Send(new OscMessage("/input/LookHorizontal", 0));
            _sender.Send(jumpOffMessage);
            _sender.Send(new OscMessage("/avatar/parameters/Booster/X", 0));
            _sender.Send(new OscMessage("/avatar/parameters/Booster/Y", 0));
            _sender.Send(new OscMessage("/avatar/parameters/Booster/Z", 0));
            _listener.Close();
            _sender.Close();
        }
        void SendTurn()
        {
            while (true)
            {
                if (_flightModelEngine.active)
                {
                    _sender.Send(new OscMessage("/input/LookHorizontal", _flightModelEngine.velocity.W));
                    Thread.Sleep(1);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        void SendBooster()
        {
            while (true)
            {
                if (_flightModelEngine.active)
                {
                    _sender.Send(new OscMessage("/avatar/parameters/Booster/X", _flightModelEngine.velocity.X));
                    _sender.Send(new OscMessage("/avatar/parameters/Booster/Y", _flightModelEngine.velocity.Y));
                    _sender.Send(new OscMessage("/avatar/parameters/Booster/Z", _flightModelEngine.velocity.Z));
                    Thread.Sleep(1);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        void SendJump()
        {
            while (true)
            {
                if (_flightModelEngine.active && _flightModelEngine.velocity.Y > 0)
                {
                    _sender.Send(jumpOnMessage);
                    Thread.Sleep(1);
                    _sender.Send(jumpOffMessage);
                    Thread.Sleep(1);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
        
        void HandlePacket(byte[] bytes)
        {
            var boolResponse = checkPacketIsBool(bytes);
            if (boolResponse != null)
            {
                _inputMapper.ProcessMessage(new OscMessage(boolResponse.Item1, new object[]{ boolResponse.Item2 }));
                return;
            }
            var packet = OscPacket.GetPacket(bytes);
            if (packet is OscMessage message)
            {
                _inputMapper.ProcessMessage(message);
            }
            else if (packet is OscBundle bundle)
            {
                foreach (OscMessage part in bundle.Messages)
                {
                    _inputMapper.ProcessMessage(part);
                }
            }
            else if (packet is null)
            {
                if (_verbose)
                {
                    Console.WriteLine("Null packet! " + bytes);
                }
            }
            else
            {
                if (_verbose)
                {
                    Console.WriteLine("Unmatched packet! " + packet.ToString() + " bytes: " + packet.GetBytes());
                }
            }
        }
        Tuple<string, bool> checkPacketIsBool(byte[] packet)
        {
            string address = "";
            bool isAddress = true;
            foreach (byte bite in packet)
            {
                if (isAddress == false)
                {
                    switch ((char)bite)
                    {
                        case 'T':
                            return Tuple.Create(address.Trim('\0'), true);
                        case 'F':
                            return Tuple.Create(address.Trim('\0'), false);
                        default:
                            return null;
                    }
                }
                else if (bite == ',')
                {
                    isAddress = false;
                }
                else if (bite != '\0')
                {
                    address += (char)bite;
                }
            }
            return null;
        }
    }
}
