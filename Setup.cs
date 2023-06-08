using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OSCFly
{
    internal class Setup
    {
        public static void Main(string[] args)
        {
            #if DEBUG
                var verbose = true;
                Console.WriteLine("Verbose mode active!");
            #else
                var verbose = false;
            #endif
            var flightModelEngine = new FlightModelEngine(verbose);
            var inputMapper = new InputMapper(verbose, flightModelEngine);
            var service = new OSCService(verbose, inputMapper, flightModelEngine);
            service.Initialize();
            Console.WriteLine("Started! Press enter to exit.");
            Console.ReadLine();
            service.End();
        }
    }   
}
