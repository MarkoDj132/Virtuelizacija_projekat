using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BatteryService.Helpers
{
    public class SessionFileWriter
    {
        private StreamWriter writer;
        private bool disposed = false;

        public SessionFileWriter(string path)
        {
            writer = new StreamWriter(path, true);
        }

        public void WriteLine(string line) 
        { 
            writer.WriteLine(line);
        }
        public void Dispose() 
        {
            if (!disposed)
            {
                writer?.Flush();
                writer?.Close();
                writer?.Dispose();

                disposed = true;
            }
        }
    }
}
