using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Momentum_Dummy_API.Models
{
    public class Lead
    {
        public int ServiceID { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int LoadID { get; set; }
        public string Phone1 { get; set; }
        public string LeadGUID { get; set; }
        public string Command { get; set; }
        public int? SourceID { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Phone4 { get; set; }
        public string Phone5 { get; set; }
        public string Phone6 { get; set; }
        public string Phone7 { get; set; }
        public string Phone8 { get; set; }
        public string Phone9 { get; set; }
        public string Phone10 { get; set; }
        public int? PhoneDesc1 { get; set; }
        public int? PhoneDesc2 { get; set; }
        public int? PhoneDesc3 { get; set; }
        public int? PhoneDesc4 { get; set; }
        public int? PhoneDesc5 { get; set; }
        public int? PhoneDesc6 { get; set; }
        public int? PhoneDesc7 { get; set; }
        public int? PhoneDesc8 { get; set; }
        public int? PhoneDesc9 { get; set; }
        public int? PhoneDesc10 { get; set; }
        public int? PhoneStatus1 { get; set; }
        public int? PhoneStatus2 { get; set; }
        public int? PhoneStatus3 { get; set; }
        public int? PhoneStatus4 { get; set; }
        public int? PhoneStatus5 { get; set; }
        public int? PhoneStatus6 { get; set; }
        public int? PhoneStatus7 { get; set; }
        public int? PhoneStatus8 { get; set; }
        public int? PhoneStatus9 { get; set; }
        public int? PhoneStatus10 { get; set; }
        public string Comments { get; set; }
        public string CustomData2 { get; set; }
        public string CustomData3 { get; set; }
        public int? CapturingAgent { get; set; }
        public string ScheduleDate { get; set; }
        public DateTime? ScheduleDateParsed { get; set; }
        public string ownerID { get; set; }


        public Validation ValidateLead()
        {
            Validation OutValidation = new() { isValid = true };
            if(this.Name.Length > 39)
            {
                this.Name = this.Name.Substring(0, 39);
            }
            if(String.IsNullOrWhiteSpace(this.Name))
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "Name is null or empty; ";
            }
            if (String.IsNullOrWhiteSpace(this.Command))
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "Command is null or empty; ";
            }
            if (String.IsNullOrWhiteSpace(this.Phone1))
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "Phone is null or empty; ";
            }
            if (String.IsNullOrWhiteSpace(this.LeadGUID))
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "LeadGUID is null or empty; ";
            }
            if (this.ServiceID == 0)
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "ServiceID invalid; ";
            }
            if (this.LoadID == 0)
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "LoadID invalid; ";
            }
            if (this.Priority == 0)
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "Priority invalid; ";
            }
            OutValidation.InvalidReason = (!String.IsNullOrWhiteSpace(OutValidation.InvalidReason)) ? OutValidation.InvalidReason.TrimEnd() : null;
            return OutValidation;
        }

        public bool HasCapturingAgent()
        {
            return !String.IsNullOrEmpty(this.ownerID);
        }
    }
}
