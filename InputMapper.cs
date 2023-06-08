using SharpOSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCFly
{
    public class InputMapper
    {
        bool _verbose;
        readonly FlightModelEngine _flightModelEngine;
        static readonly Dictionary<string, Action<object, FlightModelEngine>> addressMapping = new Dictionary<string, Action<object, FlightModelEngine>>
        {
            { "/avatar/parameters/ShouldJump", (argument, flightModelWorker) =>
            { flightModelWorker.active = (bool) argument; } },
            { "/avatar/parameters/Joystick/Held", (argument, flightModelWorker) =>
            { if (!(bool)argument) flightModelWorker.input.X = 0; flightModelWorker.input.Z = 0; } },
            { "/avatar/parameters/Joystick/X", (argument, flightModelWorker) =>
            { flightModelWorker.input.X = (float) argument; } },
            { "/avatar/parameters/Joystick/Y", (argument, flightModelWorker) =>
            { flightModelWorker.input.Z = (float) argument; } },
            { "/avatar/parameters/Throttle/Held", (argument, flightModelWorker) =>
            { if (!(bool)argument) flightModelWorker.input.W = 0; flightModelWorker.input.Y = 0; } },
            { "/avatar/parameters/Throttle/Rudder", (argument, flightModelWorker) =>
            { flightModelWorker.input.W = (float) argument; } },
            { "/avatar/parameters/Throttle/Power", (argument, flightModelWorker) =>
            { flightModelWorker.input.Y = (float) argument; } },
            { "/avatar/change", (argument, flightModelWorker) =>
            { flightModelWorker.active = false; } },
        };
        public InputMapper(bool verbose, FlightModelEngine flightModelEngine)
        {
            _verbose = verbose;
            _flightModelEngine = flightModelEngine;
        }
        public void ProcessMessage(OscMessage message)
        {
            if (addressMapping.ContainsKey(message.Address))
            {
                addressMapping[message.Address].Invoke(message.Arguments[0], _flightModelEngine);
                if (_verbose)
                {
                    Console.WriteLine($"Packet Processed: {message.Address}, {message.Arguments[0]}");
                }
            }
            else if (_verbose)
            {
//                Console.WriteLine($"Packet Ignored: {message.Address}, {message.Arguments[0]}");
            }
        }
    }
}
