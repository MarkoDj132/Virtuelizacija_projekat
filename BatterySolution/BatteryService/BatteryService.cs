using BatteryService.Helpers;
using Common.Contracts;
using Common.Events;
using Common.Faults;
using Common.Models;
using Common.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Common.Events;
using System.Configuration;

namespace BatteryService
{
    public class BatteryService : IBatteryService
    {
        private SessionFileWriter fileWriter;
        private string currentFillePath;
        private int lastRowIndex = -1;
        private SessionFileWriter rejectsWriter;
        private string rejectsFilePath;

        public event EventHandler OnTransferStarted;
        public event EventHandler OnTransferCompleted;
        public event EventHandler<SampleReceivedEventArgs> OnSampleReceived;
        public event EventHandler<WarningEventArgs> OnWarningRaised;
        private double? previousVoltage = null;
        private double V_threshold = double.Parse(ConfigurationManager.AppSettings["V_threshold"]);
        private double? previousZ = null;

        private double zSum = 0;
        private int zCount = 0;

        private double Z_threshold = 0.01;

        public BatteryService()
        {
            OnTransferStarted += BatteryService_OnTransferStarted;
            OnSampleReceived += BatteryService_OnSampleReceived;
            OnTransferCompleted += BatteryService_OnTransferCompleted;
            OnWarningRaised += BatteryService_OnWarningRaised;
        }

        public AckResponse StartSession(EisMeta meta)
        {
            ValidateMeta(meta);

            string folderPath = $@"Data\{meta.BatteryId}\{meta.TestId}\{meta.SoC}";
            Directory.CreateDirectory(folderPath);

            currentFillePath = Path.Combine(folderPath,"session.csv");
            fileWriter = new SessionFileWriter(currentFillePath);
            rejectsFilePath = Path.Combine(folderPath, "rejects.csv");
            rejectsWriter = new SessionFileWriter(rejectsFilePath);
            
            Console.WriteLine(Path.GetFullPath(currentFillePath));
            Console.WriteLine(Path.GetFullPath(rejectsFilePath));

            Console.WriteLine("Session started");
            OnTransferStarted?.Invoke(this, EventArgs.Empty);

            return new AckResponse { Sucess = true, 
                Message =  "Session started successfuly",
                Status = "IN_PROGRESS"
            };
        }

        public AckResponse PushSample(EisSample sample)
        {
            ValidateSample(sample);
            
            OnSampleReceived?.Invoke(
            this,
            new SampleReceivedEventArgs
            {
                Sample = sample
            });

            string line = $"{sample.RowIndex},{sample.FrequencyHz},{sample.R_ohm},{sample.X_ohm},{sample.V},{sample.T_degC},{sample.Range_ohm}";
            fileWriter.WriteLine(line);

            Console.WriteLine($"Recived sample: {sample.RowIndex}");

            if (previousVoltage != null)
            {
                double deltaV = sample.V - previousVoltage.Value;

                if (Math.Abs(deltaV) > V_threshold)
                {
                    OnWarningRaised?.Invoke(this, new WarningEventArgs
                    {
                        Message = $"Voltage spike detected: ΔV = {deltaV}"
                    });
                }
            }

            previousVoltage = sample.V;
            
            double z = Math.Sqrt(sample.R_ohm * sample.R_ohm + sample.X_ohm * sample.X_ohm);
            if (previousZ != null)
            {
                double deltaZ = z - previousZ.Value;

                if (Math.Abs(deltaZ) > Z_threshold)
                {
                    //Console.WriteLine($"[EVENT] Impedance jump ΔZ = {deltaZ}");
                    OnWarningRaised?.Invoke(this, new WarningEventArgs
                    {
                        Message = $"Impedance jump ΔZ = {deltaZ}"
                    });
                }
            }
            zSum += z;
            zCount++;

            double zAvg = zSum / zCount;
            if (z < 0.75 * zAvg || z > 1.25 * zAvg)
            {
                //Console.WriteLine($"[EVENT] OutOfBand Z = {z}, Avg = {zAvg}");
                OnWarningRaised?.Invoke(this, new WarningEventArgs
                {
                    Message = $"OutOfBand Z = {z}, Avg = {zAvg}"
                });
            }
            previousZ = z;

            return new AckResponse
            {
                Sucess = true,
                Message = "Sample recived",
                Status = "IN_PROGRES"
            };
        }

        public AckResponse EndSession()
        {
            fileWriter?.Dispose();
            fileWriter = null;

            rejectsWriter?.Dispose();
            rejectsWriter = null;

            OnTransferCompleted?.Invoke(this, EventArgs.Empty);
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
                    new ValidationFault { Message = "TotalRows must be > 0." },
                    new FaultReason("Invalid meta data: TotalRows is not valid."));
            }
        }
        public void TestDatase()
        {
            var files = System.IO.Directory.GetFiles("BatteryDataset", "*.csv", System.IO.SearchOption.AllDirectories);

            Console.WriteLine($"Found files: {files.Length}");
        }

        private void SaveRejectedSample(EisSample sample, string reason)
        {
            if (sample == null || rejectsWriter == null)
                return;

            string line =
                $"{sample.RowIndex}," +
                $"{sample.FrequencyHz}," +
                $"{sample.R_ohm}," +
                $"{sample.X_ohm}," +
                $"{sample.V}," +
                $"{sample.T_degC}," +
                $"{sample.Range_ohm}," +
                $"{reason}";

            rejectsWriter.WriteLine(line);
        }
        private void BatteryService_OnTransferStarted(object sender, EventArgs e)
        {
            Console.WriteLine("[EVENT] Transfer started.");
        }

        private void BatteryService_OnSampleReceived(object sender, SampleReceivedEventArgs e)
        {
            Console.WriteLine($"[EVENT] Sample {e.Sample.RowIndex} received.");
        }

        private void BatteryService_OnTransferCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("[EVENT] Transfer completed.");
        }

        private void BatteryService_OnWarningRaised(object sender, WarningEventArgs e)
        {
            Console.WriteLine($"[WARNING] {e.Message}");
        }
    }
}
