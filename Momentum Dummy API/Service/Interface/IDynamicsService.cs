using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Momentum_Dummy_API.Models;

namespace Momentum_Dummy_API.Service
{
    public interface IDynamicsService
    {
        Task<string> GetOAuthToken();
        Task<string> GetOAuthTokenProd();
        Task<bool> SendPhoneCall(string LeadGuid, string OwningUser, int CallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallNoApp(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallNoReview(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallNoReviewOpp(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallProd(string LeadGuid, string OwningUser, int CallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallOther(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallSchedule(string LeadGuid, string OwningUser, int CallOutcome, string PhoneNumber, string Subject, DateTime ScheduleDateTime, bool captured, int RecordingID);
        Task<bool> SendPhoneCallWon(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallLost(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallNoContact(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCallHotTransfer(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID);
        Task<bool> SendPhoneCall(CreatePhoneCall inPhone);
        Task<bool> UpdateLeadOwner(OwnerUpdate inOwner);
        Task<Result> UpdateOppOwner(OwnerUpdate inOwner);
        Task<string> QualifyHotTransfer(string LeadGuid, int Login);
        Task<bool> DisqualifyLead(string LeadGuid);
    }
}
