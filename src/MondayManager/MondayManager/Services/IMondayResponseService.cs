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
        Task<GeneralFulfillmentResponse> GetBoardCount(GeneralWebhookFulfillmentRequest request);
        Task<GeneralFulfillmentResponse> HandleGenericMondayRequest(GeneralWebhookFulfillmentRequest request);
    }
}
