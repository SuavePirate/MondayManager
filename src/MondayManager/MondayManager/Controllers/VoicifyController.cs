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

        [HttpPost("HandleBoards")]
        public async Task<IActionResult> GetBoardsResponse([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetBoards(request);
            return Ok(result);
        }
        [HttpPost("HandleCurrentBoard")]
        public async Task<IActionResult> GetCurrentBoard([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetCurrentBoard(request);
            return Ok(result);
        }
        [HttpPost("HandleNextBoard")]
        public async Task<IActionResult> GetNextBoard([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetNextBoard(request);
            return Ok(result);
        }
        [HttpPost("HandlePreviousBoard")]
        public async Task<IActionResult> GetPreviousBoard([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetPreviousBoard(request);
            return Ok(result);
        }
        [HttpPost("HandleItems")]
        public async Task<IActionResult> GetItemsResponse([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetItems(request);
            return Ok(result);
        }

        [HttpPost("HandleGenericQuery")]
        public async Task<IActionResult> HandleGenericQuery([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.HandleGenericMondayRequest(request);
            return Ok(result);
        }
    }
}
