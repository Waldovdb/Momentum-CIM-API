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
    [Route("proxy")]
    [ApiController]
    public class ProxyController : Controller
    {
        private readonly ILogger<ProxyController> _logger;
        private IUserService _userService;
        private IDynamicsService _dynamics;
        private IDataService _data;
        public ProxyController(IUserService userService, ILogger<ProxyController> logger, IDynamicsService dynamics, IDataService data)
        {
            _dynamics = dynamics;
            _userService = userService;
            _logger = logger;
            _data = data;
        }

        [HttpGet]
        [Route("updateleadowner")]
        public async Task<IActionResult> UpdateLeadOwner(string LeadGUID, int LeadOwner)
        {
            try
            {
                var result = await _data.SelectSingle<dynamic, dynamic>("EXEC [dbo].[spGetLeadOwner] @LOGIN = @LOGININ, @LEADGUID = @LEADGUIDIN", new { LOGININ = LeadOwner, LEADGUIDIN = LeadGUID });
                string tempOwner = result.LeadOwner;
                string tempLead = LeadGUID;
                OwnerUpdate inOwner = new OwnerUpdate() { LeadOwner = tempOwner, LeadGuid = tempLead };
                return (await _dynamics.UpdateLeadOwner(inOwner)) ? Redirect(
"https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=lead&id=" +
inOwner.LeadGuid) : BadRequest("Request Failed");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("updateoppowner")]
        public async Task<IActionResult> UpdateLeadOwnerOpp(string LeadGUID, int LeadOwner)
        {
            try
            {
                var result = await _data.SelectSingle<dynamic, dynamic>("EXEC [dbo].[spGetLeadOwner] @LOGIN = @LOGININ, @LEADGUID = @LEADGUIDIN", new { LOGININ = LeadOwner, LEADGUIDIN = LeadGUID });
                string tempOwner = result.LeadOwner;
                string tempLead = LeadGUID;
                OwnerUpdate inOwner = new OwnerUpdate() { LeadOwner = tempOwner, LeadGuid = tempLead };
                var ResultOut = await _dynamics.UpdateOppOwner(inOwner);
                return (ResultOut.Success) ? Redirect(
"https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=opportunity&id=" +
inOwner.LeadGuid) : BadRequest(ResultOut.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("reroute")]
        public async Task<IActionResult> UpdateLeadOwnerOpp(string LeadGUID, int LeadOwner, string Type)
        {
            try
            {
                var result = await _data.SelectSingle<dynamic, dynamic>("EXEC [dbo].[spGetLeadOwner] @LOGIN = @LOGININ, @LEADGUID = @LEADGUIDIN", new { LOGININ = LeadOwner, LEADGUIDIN = LeadGUID });
                string tempOwner = result.LeadOwner;
                string tempLead = LeadGUID;
                OwnerUpdate inOwner = new OwnerUpdate() { LeadOwner = tempOwner, LeadGuid = tempLead };

                if (Type == "Opportunity")
                {
                    var ResultOut = await _dynamics.UpdateOppOwner(inOwner);
                    return (ResultOut.Success) ? Redirect(
    "https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=opportunity&id=" +
    inOwner.LeadGuid) : BadRequest(ResultOut.Message);
                }
                else if (Type == "Lead")
                {
                    return (await _dynamics.UpdateLeadOwner(inOwner)) ? Redirect(
"https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=lead&id=" +
inOwner.LeadGuid) : BadRequest("Request Failed");
                }
                else
                {
                    return Ok("No information on lead/opp type.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("inboundopp")]
        public async Task<IActionResult> GetInboundOpp(string Phone)
        {
            try
            {
                var result = await _data.SelectSingle<dynamic, dynamic>($"EXEC [dbo].[spLookupPhone] @INPUTPHONE", new { INPUTPHONE = Phone });
                string tempGuid = result.GUID;
                return (tempGuid!="None") ? Redirect(
"https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=opportunity&id=" +
tempGuid) : Redirect("https://myriad.crm4.dynamics.com/");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("inboundlead")]
        public async Task<IActionResult> GetInboundLead(string Phone)
        {
            try
            {
                var result = await _data.SelectSingle<dynamic, dynamic>($"EXEC [dbo].[spLookupPhone] @INPUTPHONE", new { INPUTPHONE = Phone });
                string tempGuid = result.GUID;
                return (tempGuid != "None") ? Redirect(
"https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=lead&id=" +
tempGuid) : Redirect("https://myriad.crm4.dynamics.com/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("updatehtowner")]
        public async Task<IActionResult> UpdateLeadOwnerHotTransfer(string ANI, int LeadOwner)
        {
            try
            {
                int tempANI = Convert.ToInt32(ANI) - 5000;
                var result2 = await _data.SelectSingle<dynamic, dynamic>("EXEC [dbo].[GetLastLeadHT] @LOGIN = @LOGININ", new { LOGININ = tempANI });
                string tempLead = result2.Lead;
                var OpportunityGUID = await _dynamics.QualifyHotTransfer(tempLead, LeadOwner);
                var result = await _data.SelectSingle<dynamic, dynamic>("EXEC [dbo].[spGetLeadOwner] @LOGIN = @LOGININ, @LEADGUID = @LEADGUIDIN", new { LOGININ = LeadOwner, LEADGUIDIN = OpportunityGUID });
                string tempOwner = result.LeadOwner;

                OwnerUpdate inOwner = new OwnerUpdate() { LeadOwner = tempOwner, LeadGuid = OpportunityGUID };
                var ResultOut = await _dynamics.UpdateOppOwner(inOwner);
                return (ResultOut.Success) ? Redirect(
"https://myriad.crm4.dynamics.com/main.aspx?appid=2b916d32-ea39-eb11-a813-000d3ab9bd1c&pagetype=entityrecord&etn=opportunity&id=" +
inOwner.LeadGuid) : BadRequest(ResultOut.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
