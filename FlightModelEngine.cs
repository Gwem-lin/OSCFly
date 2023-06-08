using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSCFly
{
    public class FlightModelEngine
    {
        const float LATERAL_ACCELERATION_RATE = 0.2f;
        const float LONGITUDINAL_ACCELERATION_RATE = 1.0f;
        const float ROTATIONAL_ACCELERATION_RATE = 0.2f;
        const float VERTICAL_ACCELERATION_RATE = 1.0f;
        const float DRAG = 1.0f;
        public bool active;
        public Vector4 velocity;
        public float rotationDirection;
        public Vector4 input;
        readonly bool _verbose;
        long lastTick;
        Thread _flightModelWorkerThread;
        public FlightModelEngine(bool verbose)
        {
            _verbose = verbose;
            active = false;
            input = new Vector4();
            velocity = new Vector4();
            lastTick = DateTime.Now.Ticks;
        }
        public void Initialize()
        {
            _flightModelWorkerThread = new Thread(flightModel);
            _flightModelWorkerThread.Start();
        }
        public void End()
        {
            active = false;
            _flightModelWorkerThread.Abort();
            input = new Vector4();
            velocity = new Vector4();
        }
        void flightModel()
        {
            while (true)
            {
                if (active)
                {
                    float deltaTime = (float)(DateTime.Now.Ticks - lastTick) / TimeSpan.TicksPerSecond;
                    velocity = (velocity + new Vector4(input.X * deltaTime * LATERAL_ACCELERATION_RATE, input.Y * deltaTime * VERTICAL_ACCELERATION_RATE, input.Z * deltaTime * LONGITUDINAL_ACCELERATION_RATE, input.W * deltaTime * ROTATIONAL_ACCELERATION_RATE)) * new Vector4(1 - (DRAG * deltaTime));
                    lastTick = DateTime.Now.Ticks;
                    if (_verbose)
                    {
                        Console.WriteLine($"Input: {input}");
                        Console.WriteLine($"Velocity: {velocity}");
                    }
                    Thread.Sleep(1);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
