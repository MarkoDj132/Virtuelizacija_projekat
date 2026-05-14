using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common.Contracts;
using Common.Models;
using Common.Responses;

namespace BatteryClient
{
    public class BatteryProxy
    {
        private ChannelFactory<IBatteryService> factory;
        private IBatteryService chanel;

        public BatteryProxy()
        {
            factory = new ChannelFactory<IBatteryService>(
                new NetTcpBinding(),
                new EndpointAddress("net.tcp://localhost:9000/BatteryService"));
            
            chanel = factory.CreateChannel();
        }

        public AckResponse StartSession(EisMeta meta)
        {
            return chanel.StartSession(meta);
        }
        public AckResponse PushSample(EisSample sample)
        {
            return chanel.PushSample(sample);
        }
        public AckResponse EndSession()
        {
            return chanel.EndSession();
        }

        public void Close()
        {
            factory.Close();
        }
    }
}
