using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Momentum_Dummy_API.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Momentum_Dummy_API.Service;

namespace Momentum_Dummy_API.Controllers
{
    //[Authorize]
    [Route("api")]
    [ApiController]
    public class LeadController : Controller
    {
        private readonly ILogger<LeadController> _logger;
        private IUserService _userService;
        private IDynamicsService _dynamics;
        private IDataService _data;
        public LeadController(IUserService userService, ILogger<LeadController> logger, IDynamicsService dynamics, IDataService data)
        {
            _dynamics = dynamics;
            _userService = userService;
            _logger = logger;
            _data = data;
        }

        [HttpPost]
        [Route("lead")]
        public async Task<IActionResult> ProcessLead([FromBody] Lead InputLead)
        {
            if(!String.IsNullOrEmpty(InputLead.ScheduleDate) && InputLead.ScheduleDate != "\"\"")
            {
                DateTime tempDateTime = Convert.ToDateTime(InputLead.ScheduleDate).AddHours(-2);
                if(tempDateTime.Year == DateTime.Now.Year)
                {
                    InputLead.ScheduleDateParsed = tempDateTime;
                }
                else
                {
                    InputLead.ScheduleDateParsed = null;
                }
            }
            _logger.LogInformation("Received Lead: " + JsonConvert.SerializeObject(InputLead));
            var valid = InputLead.ValidateLead();
            if(valid.isValid)
            {
                _logger.LogInformation("Lead valid");
                try
                {
                    if(InputLead.HasCapturingAgent())
                    {
                        string AgentQuery = @"SELECT LA.LOGIN FROM [PREP].[PCO_AGENT] A
                                            INNER JOIN [PREP].[PCO_LOGINAGENT] LA
                                            ON A.ID = LA.AGENTID
                                            WHERE A.ADDRESS = @InputID";
                        var tempLogin = await _data.SelectSinglePresence<dynamic, dynamic>(AgentQuery, new { InputID = InputLead.ownerID });
                        if(tempLogin != null)
                        {
                            InputLead.CapturingAgent = (int)tempLogin.LOGIN;
                            InputLead.ServiceID = (int)tempLogin.LOGIN;
                        }
                    }
                    else
                    {
                        InputLead.CapturingAgent = (InputLead.CapturingAgent == null) ? 0 : InputLead.CapturingAgent;
                    }
                    var result = await _data.SelectSingle<dynamic, dynamic>("EXEC [dbo].[HandleLeadGUID] @LeadGUIDin = @INPUTGUID", new { @INPUTGUID = InputLead.LeadGUID });
                    InputLead.SourceID = Convert.ToInt32(result.SourceID);
                    await _data.InsertSingle<Lead, Lead>(@"INSERT INTO [dbo].[PhoneQueue]
                                            ([Command]
                                            ,[Input]
                                            ,[InputName]
                                            ,[Status]
                                            ,[Received]
                                            ,[NextExecute]
                                            ,[Actioned]
                                            ,[RetryCount]
                                            ,[RetryDate]
                                            ,[PersonID]
                                            ,[ExternalID]
                                            ,[SourceID]
                                            ,[ServiceID]
                                            ,[LoadID]
                                            ,[Name]
                                            ,[LeadGUID]
                                            ,[Phone]
                                            ,[ScheduleDate]
                                            ,[Priority]
                                            ,[CapturingAgent]
                                            ,[Phone1]
                                            ,[Phone2]
                                            ,[Phone3]
                                            ,[Phone4]
                                            ,[Phone5]
                                            ,[Phone6]
                                            ,[Phone7]
                                            ,[Phone8]
                                            ,[Phone9]
                                            ,[Phone10]
                                            ,[PhoneDesc1]
                                            ,[PhoneDesc2]
                                            ,[PhoneDesc3]
                                            ,[PhoneDesc4]
                                            ,[PhoneDesc5]
                                            ,[PhoneDesc6]
                                            ,[PhoneDesc7]
                                            ,[PhoneDesc8]
                                            ,[PhoneDesc9]
                                            ,[PhoneDesc10]
                                            ,[PhoneStatus1]
                                            ,[PhoneStatus2]
                                            ,[PhoneStatus3]
                                            ,[PhoneStatus4]
                                            ,[PhoneStatus5]
                                            ,[PhoneStatus6]
                                            ,[PhoneStatus7]
                                            ,[PhoneStatus8]
                                            ,[PhoneStatus9]
                                            ,[PhoneStatus10]
                                            ,[Comments]
                                            ,[CustomData1]
                                            ,[CustomData2]
                                            ,[CustomData3]
                                            ,[CallerID]
                                            ,[CallerName])
                                        VALUES
                                            (@Command
                                            ,'InovoCIM'
                                            ,'API'
                                            ,'Received'
                                            ,GETDATE()
                                            ,GETDATE()
                                            ,NULL
                                            ,0
                                            ,NULL
                                            ,0
                                            ,@LeadGUID
                                            ,@SourceID
                                            ,@ServiceID
                                            ,@LoadID
                                            ,@Name
                                            ,@LeadGUID
                                            ,@Phone1
                                            ,@ScheduleDateParsed
                                            ,@Priority
                                            ,@CapturingAgent
                                            ,@Phone1
                                            ,@Phone2
                                            ,@Phone3
                                            ,@Phone4
                                            ,@Phone5
                                            ,@Phone6
                                            ,@Phone7
                                            ,@Phone8
                                            ,@Phone9
                                            ,@Phone10
                                            ,@PhoneDesc1
                                            ,@PhoneDesc2
                                            ,@PhoneDesc3
                                            ,@PhoneDesc4
                                            ,@PhoneDesc5
                                            ,@PhoneDesc6
                                            ,@PhoneDesc7
                                            ,@PhoneDesc8
                                            ,@PhoneDesc9
                                            ,@PhoneDesc10
                                            ,@PhoneStatus1
                                            ,@PhoneStatus2
                                            ,@PhoneStatus3
                                            ,@PhoneStatus4
                                            ,@PhoneStatus5
                                            ,@PhoneStatus6
                                            ,@PhoneStatus7
                                            ,@PhoneStatus8
                                            ,@PhoneStatus9
                                            ,@PhoneStatus10
                                            ,@Comments
                                            ,@LeadGUID
                                            ,@CustomData2
                                            ,@CustomData3
                                            ,NULL
                                            ,NULL)", InputLead);
                }
                catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return Ok();
            }
            else
            {
                _logger.LogInformation("Lead Not Valid");
                return BadRequest(valid);
            }
        }
        
        [HttpPost]
        [Route("callbacklead")]
        public async Task<IActionResult> ProcessCallbackLead([FromBody] CallbackLead InputCallback)
        {
            _logger.LogInformation("Received Callback Lead: " + JsonConvert.SerializeObject(InputCallback));
            var valid = InputCallback.ValidateInput();
            if (valid.isValid)
            {
                _logger.LogInformation("Lead valid");
                try
                {

                    await _data.UpdateSingle<dynamic, dynamic>("EXEC [dbo].[ProcessCallback] @LeadGUIDin = @LEADGUID, @CapturingAgent = @OWNINGUSER", new { LEADGUID = InputCallback.LeadGUID, OWNINGUSER = InputCallback.CapturingAgent });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return Ok();
            }
            else
            {
                _logger.LogInformation("Lead Not Valid");
                return BadRequest(valid);
            }
        }

        [HttpGet]
        [Route("checkendpoint")]
        public async Task<IActionResult> CheckConnection()
        {
            _logger.LogInformation("Connection OK");
            return Ok();
        }

        [HttpPost]
        [Route("updateleadowner")]
        public async Task<IActionResult> UpdateLeadOwner([FromBody] OwnerUpdate inOwner)
        {
            try
            {
                return (await _dynamics.UpdateLeadOwner(inOwner)) ? Ok() : BadRequest("Request Failed");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecall")]
        public async Task<IActionResult> CreatePhoneCall([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCall(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.phonenumber, inPhone.subject,inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallnoapp")]
        public async Task<IActionResult> CreatePhoneCallNoApp([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallNoApp(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallnoreview")]
        public async Task<IActionResult> CreatePhoneCallNoReview([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallNoReview(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallnoreviewopp")]
        public async Task<IActionResult> CreatePhoneCallNoReviewOpp([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallNoReviewOpp(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallschedule")]
        public async Task<IActionResult> CreatePhoneCallSchedule([FromBody] CreatePhoneCallSchedule inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = (inPhone.SubOutcome == 1) ? false : true;
                return (await _dynamics.SendPhoneCallSchedule(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.phonenumber, inPhone.subject, inPhone.ScheduleDateTime, inPhone.dc_bit_capturing, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallwon")]
        public async Task<IActionResult> CreatePhoneCallWon([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallWon(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallother")]
        public async Task<IActionResult> CreatePhoneCallOther([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallOther(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecalllost")]
        public async Task<IActionResult> CreatePhoneCallLost([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallLost(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallnocontact")]
        public async Task<IActionResult> CreatePhoneCallNoContact([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallNoContact(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("createphonecallhottransfer")]
        public async Task<IActionResult> CreatePhoneCallHotTransfer([FromBody] CreatePhoneCallSub inPhone)
        {
            try
            {
                inPhone.dc_bit_capturing = true;
                return (await _dynamics.SendPhoneCallHotTransfer(inPhone.LeadGuid, inPhone.LeadOwner, inPhone.dc_cho_leadcalloutcome, inPhone.SubOutcome, inPhone.phonenumber, inPhone.subject, inPhone.RecordingID)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("disqualifylead")]
        public async Task<IActionResult> DisqualifyLead([FromBody] DisqualifyLead inLead)
        {
            try
            {
                return (await _dynamics.DisqualifyLead(inLead.LeadGuid)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("trycrm")]
        public async Task<IActionResult> TryCRM()
        {
            try
            {
                string LeadGuid = "f27b95b9-ed4d-ec11-8c62-000d3ab52954"; //f27b95b9-ed4d-ec11-8c62-000d3ab52954
                string LeadOwner = "6d5077e3-5638-eb11-bf68-000d3ab9b3f6"; // 9661e4e9-5638-eb11-bf68-000d3ab9b3f6
                int CallOutcome = 948170003;
                string PhoneNumber = "011 123 1234";
                string Subject = "Inovo - Lead Create Route to Digital Adv";
                return (await _dynamics.SendPhoneCallProd(LeadGuid, LeadOwner, CallOutcome, PhoneNumber, Subject,1)) ? Ok() : BadRequest("Request Failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
