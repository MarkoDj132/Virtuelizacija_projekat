using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace BatteryHost
{
    class Progarm
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(BatteryService.BatteryService));
            try
            {
                host.Open();

                Console.WriteLine("WCF service started...");
                Console.WriteLine("Press ENTER to stop service.");

                Console.ReadLine();
                host.Close();
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
                host.Abort();
            }
        }
    }
}
