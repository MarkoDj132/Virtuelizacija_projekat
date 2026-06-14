using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class SampleReceivedEventArgs : EventArgs
    {
        public EisSample Sample { get; set; }
    }
}
