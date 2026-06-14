using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class WarningEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
