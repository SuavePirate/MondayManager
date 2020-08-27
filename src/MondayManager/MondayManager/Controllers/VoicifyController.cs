using Microsoft.AspNetCore.Mvc;
using MondayManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voicify.Sdk.Core.Models.Webhooks.Requests;
using Voicify.Sdk.Core.Models.Webhooks.Responses;

namespace MondayManager.Controllers
{
    [Route("api/[controller]")]
    public class VoicifyController : ControllerBase
    {
        private readonly IMondayResponseService _mondayResponseService;

        public VoicifyController(IMondayResponseService mondayResponseService)
        {
            _mondayResponseService = mondayResponseService;
        }

        //[HttpPost("HandleVoicifyWebhook")]
        //public async Task<IActionResult> HandleVoicifyWebhook([FromBody]GeneralWebhookFulfillmentRequest request)
        //{
        //    var accessToken = request.OriginalRequest.AccessToken;

        //    // temp: get the current user
        //    var client = new HttpClient();
        //    client.DefaultRequestHeaders.Add("Authorization", accessToken);
        //    client.DefaultRequestHeaders.Add("ContentType", "application/json");
        //    var response = await client.PostAsync("https://api.monday.com/v2", new StringContent("{\"query\":\"{ me{ name } }\"}", Encoding.UTF8, "application/json"));
        //    var outputJson = await response.Content.ReadAsStringAsync();



        //    return Ok(new GeneralFulfillmentResponse
        //    {
        //        Data = new ContentFulfillmentWebhookData
        //        {
        //            Content = $"You are trying to request data as {outputJson}"
        //        }
        //    });
        //}

        [HttpPost("GetBoardCount")]
        public async Task<IActionResult> GetBoardCountResponse([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetBoardCount(request);
            return Ok(result);
        }
    }
}
