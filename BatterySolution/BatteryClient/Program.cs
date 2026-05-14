using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Common.Models;
using System.ServiceModel;
using Common.Faults;

namespace BatteryClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new BatteryProxy();
            try
            {

                string file = FindFirstCsv();

                var meta = new EisMeta
                {
                    TotalRows = 28,
                    BatteryId = "B01",
                    TestId = "Test_1",
                    SoC = 50,
                    FileName = "50"
                };

                proxy.StartSession(meta);
                var lines = File.ReadAllLines(file);
                int index = 0;

                foreach (var line in lines.Skip(1))
                {
                    if(string.IsNullOrWhiteSpace(line)) 
                        continue;
                    Console.WriteLine(line);

                    var p = line.Split(',');

                    var sample = new EisSample
                    {
                        RowIndex = index++,
                        FrequencyHz = double.Parse(p[0], CultureInfo.InvariantCulture),
                        R_ohm = double.Parse(p[1], CultureInfo.InvariantCulture),
                        X_ohm = double.Parse(p[2], CultureInfo.InvariantCulture),
                        V = double.Parse(p[3], CultureInfo.InvariantCulture),
                        T_degC = double.Parse(p[4], CultureInfo.InvariantCulture),
                        Range_ohm = double.Parse(p[5], CultureInfo.InvariantCulture)
                    };

                    proxy.PushSample(sample);
                    Console.WriteLine($"Sent {sample.RowIndex}");
                }

                proxy.EndSession();
            }
            catch(FaultException<ValidationFault> ex)
            {
                Console.WriteLine("VALIDATION ERROR:");
                Console.WriteLine(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                Console.WriteLine("WCF FAULT:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GENERAL ERROR:");
                Console.WriteLine(ex.Message);
            }
        
            //proxy.Close();

            Console.WriteLine("DONE");
            Console.ReadLine();
        }
        /*static string FindFirstCsv()
        {
            string root = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\BatteryDataset");

            return Directory
                .GetFiles(root, "*.csv", SearchOption.AllDirectories)
                .First(f => Path.GetFileName(f).Contains("SOC"));
        }*/
        /*static string FindFirstCsv()
        {
            string root = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\BatteryDataset");

            var files = Directory.GetFiles(root, "*.csv", SearchOption.AllDirectories);

            foreach (var f in files)
            {
                Console.WriteLine(f);
            }

            return files[0];
        }*/
        static string FindFirstCsv()
        {
            string root = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\BatteryDataset");

            return Directory
                .GetFiles(root, "*.csv", SearchOption.AllDirectories)
                .First(f => f.Contains("EIS measurements") && f.Contains("Hioki"));
        }

    }
}
