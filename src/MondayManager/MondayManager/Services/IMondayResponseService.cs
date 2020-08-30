using ServiceResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voicify.Sdk.Core.Models.Webhooks.Requests;
using Voicify.Sdk.Core.Models.Webhooks.Responses;

namespace MondayManager.Services
{
    public interface IMondayResponseService
    {
        Task<GeneralFulfillmentResponse> GetBoards(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> GetCurrentBoard(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> GetPreviousBoard(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> GetNextBoard(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> GetItems(GeneralWebhookFulfillmentRequest request);

        Task<GeneralFulfillmentResponse> GetCurrentItem(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> GetPreviousItem(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> GetNextItem(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> SearchForItem(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> CreateItem(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> HandleGenericMondayRequest(GeneralWebhookFulfillmentRequest request);
    }
}
