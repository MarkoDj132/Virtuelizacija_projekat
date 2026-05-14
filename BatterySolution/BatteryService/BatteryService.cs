using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Common.Contracts;
using Common.Models;
using Common.Responses;
using System.ServiceModel;
using Common.Faults;


namespace BatteryService
{
    public class BatteryService : IBatteryService
    {
        private int lastRowIndex = -1;
        public AckResponse StartSession(EisMeta meta)
        {
            ValidateMeta(meta);
            Console.WriteLine("Session started");

            return new AckResponse { Sucess = true, 
                Message =  "Session started successfuly",
                Status = "IN_PROGRESS"};
        }

        public AckResponse PushSample(EisSample sample)
        {
            ValidateSample(sample);
            Console.WriteLine($"Recived sample: {sample.RowIndex}");

            return new AckResponse
            {
                Sucess = true,
                Message = "Sample recived",
                Status = "IN_PROGRES"
            };
        }

        public AckResponse EndSession()
        {
            Console.WriteLine("Session completed");
            return new AckResponse
            {
                Sucess = true,
                Message = "Session completed",
                Status = "COMPLETED"
            };
        }

        public void ValidateSample(EisSample sample) 
        {
            if(sample == null)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "Sample is null."
                    });
            }
            if(sample.FrequencyHz <= 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "FrequencyHz must be greater than 0."
                    });
            }
            if(sample.RowIndex <= lastRowIndex)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault
                    {
                        Message = "RowIndex must increase monotonically."
                    });
            }
            lastRowIndex = sample.RowIndex;
        }

        private void ValidateMeta(EisMeta meta)
        {
            if(meta == null)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "Meta is null." });
            }
            if (string.IsNullOrWhiteSpace(meta.BatteryId))
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "BatterryId is required." });
            }
            if (string.IsNullOrWhiteSpace(meta.TestId))
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "TestId is required." });
            }
            if (meta.SoC < 0 || meta.SoC > 100)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "Soc must be 0-100." });
            }
            if (meta.TotalRows <=0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "TotalRows must be > 0." });
            }
        }
    }
}
