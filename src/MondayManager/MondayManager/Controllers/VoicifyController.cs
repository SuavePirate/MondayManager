using Alexa.NET.Response;
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

        [HttpPost("HandleInitialSignIn")]
        public IActionResult HandleInitialSignIn([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            if (!string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                return Ok(); // don't need to sign in

            // use payload override for alexa
            if (request.OriginalRequest?.Assistant?.ToLower() == "alexa")
                return Ok(new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        PayloadOverride = Alexa.NET.ResponseBuilder.TellWithLinkAccountCard("In order to let you interact with your monday.com resources, you need to link your Monday account. We've sent a card to your alexa mobile app to get started.")
                    }
                });
            else
                return Ok(new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = "You need to link your Monday account before requesting data.",
                        AccountLinking = new AccountLinkingModel
                        {
                            GoogleAccountLinkingPrompt = "To ask about your monday account",
                            AlexaAccountLinkingPrompt = "In order to ask about your monday account, you need to link your Amazon and Monday accounts. I've sent a card to your Alexa app to get started"
                        }
                    }
                });
        }

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
        [HttpPost("HandleCurrentItem")]
        public async Task<IActionResult> GetCurrentItem([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetCurrentItem(request);
            return Ok(result);
        }
        [HttpPost("HandleNextItem")]
        public async Task<IActionResult> GetNextItem([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetNextItem(request);
            return Ok(result);
        }
        [HttpPost("HandlePreviousItem")]
        public async Task<IActionResult> GetPreviousItem([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.GetPreviousItem(request);
            return Ok(result);
        }
        [HttpPost("SearchForItem")]
        public async Task<IActionResult> SearchForItem([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.SearchForItem(request);
            return Ok(result);
        }
        [HttpPost("CreateItem")]
        public async Task<IActionResult> CreateItem([FromBody]GeneralWebhookFulfillmentRequest request)
        {
            var result = await _mondayResponseService.CreateItem(request);
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
