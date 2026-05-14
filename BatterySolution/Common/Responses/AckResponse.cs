using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common.Responses
{
    [DataContract]
    public class AckResponse
    {
        [DataMember]
        public bool Sucess {  get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Status { get; set; }
    }
}
