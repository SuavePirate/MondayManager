using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voicify.Sdk.Core.Models.Webhooks.Requests;
using Voicify.Sdk.Core.Models.Webhooks.Responses;

namespace MondayManager.Controllers
{
    [Route("api/[controller]")]
    public class VoicifyController : ControllerBase
    {
        [HttpPost("HandleVoicifyWebhook")]
        public async Task<IActionResult> HandleVoicifyWebhook([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var accessToken = request.OriginalRequest.AccessToken;
            return Ok(new GeneralFulfillmentResponse
            {
                Data = new ContentFulfillmentWebhookData
                {
                    Content = $"You are trying to request data as {accessToken}"
                }
            });
        }
    }
}
