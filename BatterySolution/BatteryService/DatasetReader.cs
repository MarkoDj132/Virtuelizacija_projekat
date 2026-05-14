using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Common.Models;

namespace BatteryService
{
    public class DatasetReader
    {
        public IEnumerable<(EisMeta meta, List<EisSample> samples)> Read(string root)
        {
            var fies = Directory.GetFiles(root, "*.csv", SearchOption.AllDirectories);
            foreach (var file in fies) 
            {
                yield return (ParseMeta(file), ParseSample(file));
            }
        }

        private EisMeta ParseMeta(string file)
        {
            var parts = file.Split(Path.DirectorySeparatorChar);

            string batteryId = parts[parts.Length-4];
            string testId = parts[parts.Length-3];

            string fileName = Path.GetFileNameWithoutExtension(file);
            int soc = int.Parse(fileName);

            return new EisMeta
            {
                BatteryId = batteryId,
                TestId = testId,
                SoC = soc,
                FileName = fileName,
            };
        }
        private List<EisSample> ParseSample(string file)
        {
            var list = new List<EisSample>();
            var lines = File.ReadAllLines(file);

            int i = 0;

            foreach (var line in lines)
            {
                var p = line.Split(',');
                if (p.Length < 6) continue;
                list.Add(new EisSample
                {
                    RowIndex = i++,
                    FrequencyHz = double.Parse(p[0]),
                    R_ohm = double.Parse(p[1]),
                    X_ohm = double.Parse(p[2]),
                    V = double.Parse(p[3]),
                    T_degC = double.Parse(p[4]),
                    Range_ohm = double.Parse(p[5]),
                });
            }
            return list;
        }
    }
}
