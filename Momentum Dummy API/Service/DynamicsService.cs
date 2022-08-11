using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Momentum_Dummy_API.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Momentum_Dummy_API.Service
{
    public class DynamicsService : IDynamicsService
    {
        private string serviceUrl = "https://myriad.crm4.dynamics.com";
        private string serviceUrlTest = "https://myriad.crm4.dynamics.com";
        private string clientId = "ca1478cb-2f6c-416a-a8e2-e5a5b7c34899";
        private string secret = "kF.7Q~HwYOA7Oa_9IN6.GyWHlOqZPI0PXPjZR";
        private string OAuthTenantID = "366b8d18-99ea-436a-824b-1ca89c369476";
        private string OAuthBaseURL = "https://login.microsoftonline.com/";
        private string DynamicsBaseURL = "https://myriad.crm4.dynamics.com/";
        private string DynamicsBaseURLTest = "https://myriad.crm4.dynamics.com/";
        private string DynamicsPhoneEndpoint = "api/data/v9.1/phonecalls";
        private string DynamicsQualifyHT = "/api/data/v9.1/leads(";
        private string DynamicsDisqualifyLead = "/api/data/v9.1/lead(";
        private string DynamicsLeadEndpoint = "api/data/v9.1/leads(";
        private string DynamicsOppEndpoint = "api/data/v9.1/opportunities(";
        private readonly ILogger<DynamicsService> _logger;
        private readonly IDataService _data;

        public DynamicsService(ILogger<DynamicsService> logger, IDataService data)
        {
            _data = data;
            _logger = logger;
        }
        public async Task<string> GetOAuthToken()
        {
            AuthenticationContext authContext = new AuthenticationContext(OAuthBaseURL + OAuthTenantID);
            ClientCredential credential = new ClientCredential(clientId, secret);

            AuthenticationResult result = await authContext.AcquireTokenAsync(serviceUrl, credential);

            return result.AccessToken;
        }

        public async Task<string> GetOAuthTokenProd()
        {
            AuthenticationContext authContext = new AuthenticationContext(OAuthBaseURL + OAuthTenantID);
            ClientCredential credential = new ClientCredential(clientId, secret);

            AuthenticationResult result = await authContext.AcquireTokenAsync(serviceUrlTest, credential);

            return result.AccessToken;
        }

        public async Task<bool> SendPhoneCall(CreatePhoneCall inPhone)
        {
            try
            {
                #region [ Create Content ]
                var entity = new Entity() { subject = inPhone.subject, dc_bit_capturing = true, dc_cho_leadcalloutcome = inPhone.dc_cho_leadcalloutcome, phonenumber = inPhone.phonenumber };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({inPhone.LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({inPhone.LeadOwner})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCall(string LeadGuid, string OwningUser, int CallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                #region [ Create Content ]
                var entity = new Entity() { subject = Subject, dc_bit_capturing = true, dc_cho_leadcalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                if(check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    var message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                }
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallProd(string LeadGuid, string OwningUser, int CallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                #region [ Create Content ]
                var entity = new Entity() { subject = Subject, dc_bit_capturing = true, dc_cho_leadcalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClientTest();
                var check = await client.PostAsync(DynamicsBaseURLTest + DynamicsPhoneEndpoint, data);
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    var message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                }
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallSchedule(string LeadGuid, string OwningUser, int CallOutcome, string PhoneNumber, string Subject, DateTime ScheduleDateTime, bool captured, int RecordingID)
        {
            try
            {
                bool outcome = false;
                #region [ Create Content ]
                var entity = new EntitySchedule() { subject = Subject, dc_bit_capturing = captured, dc_cho_leadcalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_date_selectcallbackdatetime = ScheduleDateTime.AddHours(2).ToString("o"), dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                if (check.IsSuccessStatusCode)
                {
                    outcome = true;
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    outobject.Remove("regardingobjectid_lead_phonecall@odata.bind");
                    outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                    outobject.Remove("dc_cho_leadcalloutcome");
                    outobject.Add("dc_cho_opportunitycalloutcome", CallOutcome);
                    data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                    var checknew = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                    if (checknew.IsSuccessStatusCode)
                    {
                        outcome = true;
                        _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                    }
                    else
                    {
                        var message = await checknew.Content.ReadAsStringAsync();
                        var code = ((int)checknew.StatusCode);
                        _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                    }
                }
                return outcome;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallNoApp(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)        {
            try
            {
                #region [ Create Content ]
                var entity = new EntitySubCall() { subject = Subject, dc_bit_capturing = true, dc_cho_leadcalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_cho_leadnoapplicationsubstatus = SubCallOutcome, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    var message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                }
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallNoReview(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                bool returnBool = false;
                #region [ Create Content ]
                var entity = new EntityNoReview() { subject = Subject, dc_bit_capturing = true, phonenumber = PhoneNumber, dc_callregistrationid = RecordingID.ToString(), dc_cho_leadcalloutcome = CallOutcome, dc_leadnoreviewsuboutcome = SubCallOutcome }; //dc_cho_opportunity
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                returnBool = check.IsSuccessStatusCode;
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    outobject.Remove("regardingobjectid_lead_phonecall@odata.bind");
                    outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                    outobject.Remove("dc_cho_leadcalloutcome");
                    outobject.Add("dc_cho_opportunitycalloutcome", CallOutcome);
                    var data2 = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                    var check2 = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data2);
                    returnBool = check2.IsSuccessStatusCode;
                    if (check2.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                    }
                    else
                    {
                        var message = await check2.Content.ReadAsStringAsync();
                        var code = ((int)check2.StatusCode);
                        _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                    }
                }
                return returnBool;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallNoReviewOpp(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                bool returnBool = false;
                #region [ Create Content ]
                var entity = new EntityNoReviewOpp() { subject = Subject, dc_bit_capturing = true, phonenumber = PhoneNumber, dc_callregistrationid = RecordingID.ToString(), dc_cho_leadcalloutcome = CallOutcome, dc_noreviewsuboutcome = SubCallOutcome };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                returnBool = check.IsSuccessStatusCode;
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    outobject.Remove("regardingobjectid_lead_phonecall@odata.bind");
                    outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                    outobject.Remove("dc_cho_leadcalloutcome");
                    outobject.Add("dc_cho_opportunitycalloutcome", CallOutcome);
                    var data2 = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                    var check2 = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data2);
                    returnBool = check2.IsSuccessStatusCode;
                    if (check2.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                    }
                    else
                    {
                        var message = await check2.Content.ReadAsStringAsync();
                        var code = ((int)check2.StatusCode);
                        _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                    }
                }
                return returnBool;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallOther(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                bool returnBool = false;
                #region [ Create Content ]
                var entity = new EntityOther() { subject = Subject, dc_bit_capturing = true, phonenumber = PhoneNumber, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                returnBool = check.IsSuccessStatusCode;
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    outobject.Remove("regardingobjectid_lead_phonecall@odata.bind");
                    outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                    outobject.Remove("dc_cho_leadcalloutcome");
                    outobject.Add("dc_cho_opportunitycalloutcome", CallOutcome);
                    var data2 = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                    var check2 = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data2);
                    returnBool = check2.IsSuccessStatusCode;
                    if (check2.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                    }
                    else
                    {
                        var message = await check2.Content.ReadAsStringAsync();
                        var code = ((int)check2.StatusCode);
                        _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                    }
                }
                return returnBool;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallWon(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                bool returnBool = false;

                #region [ Create Content ]
                var entity = new EntityWon() { subject = Subject, dc_bit_capturing = true, dc_cho_opportunitycalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_cho_opportunitywonoutcome = SubCallOutcome, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                returnBool = check.IsSuccessStatusCode;
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    outobject.Remove("regardingobjectid_lead_phonecall@odata.bind");
                    outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                    var data2 = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                    var check2 = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data2);
                    returnBool = check2.IsSuccessStatusCode;
                    if (check2.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                    }
                    else
                    {
                        var message = await check2.Content.ReadAsStringAsync();
                        var code = ((int)check2.StatusCode);
                        _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                    }
                }
                return returnBool;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallLost(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                #region [ Create Content ]
                var entity = new EntityLost() { subject = Subject, dc_bit_capturing = true, dc_cho_opportunitycalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_cho_opportunitylostoutcome = SubCallOutcome, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion  

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    var message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                }
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SendPhoneCallNoContact(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                bool returnBool = false;
                #region [ Create Content ]
                var entity = new EntityNoContactCall() { subject = Subject, dc_bit_capturing = true, dc_cho_leadcalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_nocontactsubstatus = SubCallOutcome };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                returnBool = check.IsSuccessStatusCode;
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    outobject.Remove("regardingobjectid_lead_phonecall@odata.bind");
                    outobject.Add("regardingobjectid_opportunity_phonecall@odata.bind", $"/opportunities({LeadGuid})");
                    outobject.Remove("dc_cho_leadcalloutcome");
                    outobject.Add("dc_cho_opportunitycalloutcome", CallOutcome);
                    var data2 = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                    var check2 = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data2);
                    returnBool = check2.IsSuccessStatusCode;
                    if (check2.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                    }
                    else
                    {
                        var message = await check2.Content.ReadAsStringAsync();
                        var code = ((int)check2.StatusCode);
                        _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                    }
                }
                return returnBool;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> QualifyHotTransfer(string LeadGuid, int Login)
        {
            try
            {
                using var client = await GetHttpClient();
                var paramInput = new QualifyParameters() { CreateAccount = false, CreateContact = false, CreateOpportunity = true, Status = 3 };
                Newtonsoft.Json.Linq.JObject outobject2 = Newtonsoft.Json.Linq.JObject.FromObject(paramInput);
                var data2 = new StringContent(outobject2.ToString(), Encoding.UTF8, "application/json");

                var check2 = await client.PostAsync(DynamicsBaseURL + DynamicsQualifyHT + LeadGuid + ")/Microsoft.Dynamics.CRM.QualifyLead", data2);
                var response = await check2.Content.ReadAsStringAsync();
                var dynamicObject = JObject.Parse(response);
                var tempOppGUID = (string)dynamicObject["value"][0]["opportunityid"];
                await _data.UpdateSingle<dynamic, dynamic>("EXEC [dbo].[spHotTransferLookup] @LeadGUIDIn = @LEADGUIDINPUT, @OpportunityGUIDIn = @OPPGUID", new { LEADGUIDINPUT = LeadGuid, OPPGUID = tempOppGUID });
                return tempOppGUID;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<bool> DisqualifyLead(string LeadGuid)
        {
            try
            {
                using var client = await GetHttpClient();
                var paramInput = new DisqualifyParameters() { statecode = 2 , dc_cho_leadoutcome = 948170000, dc_opt_noapplocationsubstatus = 948170018 };
                Newtonsoft.Json.Linq.JObject outobject2 = Newtonsoft.Json.Linq.JObject.FromObject(paramInput);
                //outobject2.Add("StatusCode@odata.bind", 5);
                var data2 = new StringContent(outobject2.ToString(), Encoding.UTF8, "application/json");

                var check2 = await client.PatchAsync(DynamicsBaseURL + DynamicsQualifyHT + LeadGuid + ")", data2);
                var response = await check2.Content.ReadAsStringAsync();
                
                return check2.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;  
            }
        }

        public async Task<bool> SendPhoneCallHotTransfer(string LeadGuid, string OwningUser, int CallOutcome, int SubCallOutcome, string PhoneNumber, string Subject, int RecordingID)
        {
            try
            {
                #region [ Create Content ]
                var entity = new Entity() { subject = Subject, dc_bit_capturing = true, dc_cho_leadcalloutcome = CallOutcome, phonenumber = PhoneNumber, dc_callregistrationid = RecordingID.ToString() };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("regardingobjectid_lead_phonecall@odata.bind", $"/leads({LeadGuid})");
                outobject.Add("ownerid@odata.bind", $"/systemusers({OwningUser})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PostAsync(DynamicsBaseURL + DynamicsPhoneEndpoint, data);
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {LeadGuid} phone call logged successfully");
                }
                else
                {
                    var message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {LeadGuid} phone call logging failed - code: {code}, message: {message}");
                }
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateLeadOwner(OwnerUpdate inOwner)
        {
            try
            {
                #region [ Create Content ]
                var entity = new EmptyEntity() {};
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("ownerid@odata.bind", $"/systemusers({inOwner.LeadOwner})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PatchAsync(DynamicsBaseURL + DynamicsLeadEndpoint + inOwner.LeadGuid + ")", data);
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {inOwner.LeadGuid} lead owner updated to {inOwner.LeadOwner} successfully");
                }
                else
                {
                    var message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {inOwner.LeadGuid} owner update failed - code: {code}, message: {message}");
                }
                return check.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Result> UpdateOppOwner(OwnerUpdate inOwner)
        {
            try
            {
                #region [ Create Content ]
                var entity = new EmptyEntity() { };
                Newtonsoft.Json.Linq.JObject outobject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                outobject.Add("ownerid@odata.bind", $"/systemusers({inOwner.LeadOwner})");
                var data = new StringContent(outobject.ToString(), Encoding.UTF8, "application/json");
                #endregion

                using var client = await GetHttpClient();
                var check = await client.PatchAsync(DynamicsBaseURL + DynamicsOppEndpoint + inOwner.LeadGuid + ")", data);
                var resultOut = new Result();
                if (check.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Lead GUID {inOwner.LeadGuid} lead owner updated to {inOwner.LeadOwner} successfully");
                    resultOut.Message = "Success";
                }
                else
                {
                    resultOut.Message = await check.Content.ReadAsStringAsync();
                    var code = ((int)check.StatusCode);
                    _logger.LogError($"Lead GUID {inOwner.LeadGuid} owner update failed - code: {code}, message: {resultOut.Message}");
                }
                resultOut.Success = check.IsSuccessStatusCode;
                return resultOut;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<HttpClient> GetHttpClient()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                client.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=\"*\"");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetOAuthToken());
                return client;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<HttpClient> GetHttpClientTest()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                client.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=\"*\"");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetOAuthTokenProd());
                return client;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public class EmptyEntity
        {

        }

        private class Entity
        {
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntitySchedule
        {
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_date_selectcallbackdatetime { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntityNoReview
        {
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public int dc_leadnoreviewsuboutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntityNoReviewOpp
        {
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public int dc_noreviewsuboutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntitySubCall
        {
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public int dc_cho_leadnoapplicationsubstatus { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntityOther
        {
            public string subject { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntityNoContactCall
        {
            public string subject { get; set; }
            public int dc_cho_leadcalloutcome { get; set; }
            public int dc_nocontactsubstatus { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }

        }

        private class EntityWon
        {
            public string subject { get; set; }
            public int dc_cho_opportunitycalloutcome { get; set; }
            public int dc_cho_opportunitywonoutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class EntityLost
        {
            public string subject { get; set; }
            public int dc_cho_opportunitycalloutcome { get; set; }
            public int dc_cho_opportunitylostoutcome { get; set; }
            public bool dc_bit_capturing { get; set; }
            public string phonenumber { get; set; }
            public string dc_callregistrationid { get; set; }
        }

        private class QualifyParameters 
        {
            public bool CreateAccount { get; set; }
            public bool CreateContact { get; set; }
            public bool CreateOpportunity { get; set; }
            public int Status { get; set; }
        }

        private class DisqualifyParameters
        {
            public int statecode { get; set; }
            public int dc_cho_leadoutcome { get; set; }
            public int dc_opt_noapplocationsubstatus { get; set; }
        }

    }
}