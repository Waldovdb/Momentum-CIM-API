using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Momentum_Dummy_API.Models
{
    public class CallbackLead
    {
        public string LeadGUID { get; set; }
        public string CapturingAgent { get; set; }

        public Validation ValidateInput()
        {
            Validation OutValidation = new() { isValid = true };
            if (String.IsNullOrWhiteSpace(this.LeadGUID))
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "LeadGUID is null or empty; ";
            }
            if (String.IsNullOrWhiteSpace(this.CapturingAgent))
            {
                OutValidation.isValid = false;
                OutValidation.InvalidReason += "CapturingAgent invalid; ";
            }
            OutValidation.InvalidReason = (!String.IsNullOrWhiteSpace(OutValidation.InvalidReason)) ? OutValidation.InvalidReason.TrimEnd() : null;
            return OutValidation;
        }
    }
}
