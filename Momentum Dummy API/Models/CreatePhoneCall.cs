using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Momentum_Dummy_API.Models
{
    public class CreatePhoneCall : OwnerUpdate
    { 
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
    }

    public class CreatePhoneCallSub : OwnerUpdate
    {
        public string subject { get; set; }
        public int dc_cho_leadcalloutcome { get; set; }
        public int SubOutcome { get; set; }
        public string OutcomeType { get; set; }
        public bool dc_bit_capturing { get; set; }
        public string phonenumber { get; set; }
        public int dc_callregistrationid { get; set; }
        public int RecordingID { get; set; }
    }

    public class CreatePhoneCallSchedule : OwnerUpdate
    {
        public string subject { get; set; }
        public int dc_cho_leadcalloutcome { get; set; }
        public int SubOutcome { get; set; }
        public string OutcomeType { get; set; }
        public bool dc_bit_capturing { get; set; }
        public string phonenumber { get; set; }
        public int dc_callregistrationid { get; set; }
        public int RecordingID { get; set; }
        public DateTime ScheduleDateTime { get; set; }
    }
}
