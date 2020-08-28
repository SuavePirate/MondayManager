﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MondayManager.Models.Constants;
using MondayManager.Models.Monday;
using MondayManager.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceResult;
using Voicify.Sdk.Core.Models.Model;
using Voicify.Sdk.Core.Models.Webhooks.Requests;
using Voicify.Sdk.Core.Models.Webhooks.Responses;
using Voicify.Sdk.Webhooks.Services.Definitions;

namespace MondayManager.Services
{
    public class MondayResponseService : IMondayResponseService
    {
        private readonly IMondayDataProvider _mondayDataProvider;
        private readonly IDataTraversalService _dataTraversalService;

        public MondayResponseService(IMondayDataProvider mondayDataProvider, IDataTraversalService dataTraversalService)
        {
            _mondayDataProvider = mondayDataProvider;
            _dataTraversalService = dataTraversalService;
        }


        public async Task<GeneralFulfillmentResponse> GetBoards(GeneralWebhookFulfillmentRequest request)
        {
            try
            {

                if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                    return Unauthorized();

                var boards = await _mondayDataProvider.GetAllBoards(request.OriginalRequest.AccessToken);

                if (boards.ResultType != ResultType.Ok)
                    return new GeneralFulfillmentResponse
                    {
                        Data = new ContentFulfillmentWebhookData
                        {
                            Content = $"Something went wrong getting your boards: {boards.Errors.FirstOrDefault()}"
                        }
                    };

                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = request.Response.Content.Replace(ResponseVariables.BoardCount, boards.Data.Length.ToString()),
                        AdditionalSessionAttributes = new Dictionary<string, object>
                        {
                            { SessionAttributes.BoardsSessionAttribute, boards.Data },
                            { SessionAttributes.CurrentBoardSessionAttribute, boards.Data.FirstOrDefault() }
                        }
                    }
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Error();
            }
        }
        public async Task<GeneralFulfillmentResponse> GetCurrentBoard(GeneralWebhookFulfillmentRequest request)
        {
            try
            {

                if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                    return Unauthorized();

                var currentBoard = await GetCurrentBoardFromRequest(request);
                if (currentBoard == null)
                    return Error();

                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = BuildBoardResponse(request.Response.Content, currentBoard),
                        AdditionalSessionAttributes = new Dictionary<string, object>
                        {
                            { SessionAttributes.CurrentBoardSessionAttribute, currentBoard }
                        }
                    }
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Error();
            }
        }

        public async Task<GeneralFulfillmentResponse> GetNextBoard(GeneralWebhookFulfillmentRequest request)
        {
            try
            {

                if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                    return Unauthorized();

                var boards = await GetBoardsFromRequest(request);
                if (boards == null)
                    return Error();

                var currentBoard = await GetCurrentBoardFromRequest(request);
                if (currentBoard == null)
                    return Error();

                var currentIndex = Array.FindIndex(boards, b => b.Id == currentBoard.Id);
                if (currentIndex < boards.Length - 1)
                    currentBoard = boards[currentIndex + 1];

                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = BuildBoardResponse(request.Response.Content, currentBoard),
                        AdditionalSessionAttributes = new Dictionary<string, object>
                        {
                            { SessionAttributes.CurrentBoardSessionAttribute, currentBoard }
                        }
                    }
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Error();
            }
        }

        public async Task<GeneralFulfillmentResponse> GetPreviousBoard(GeneralWebhookFulfillmentRequest request)
        {
            try
            {

                if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                    return Unauthorized();

                var boards = await GetBoardsFromRequest(request);
                if (boards == null)
                    return Error();

                var currentBoard = await GetCurrentBoardFromRequest(request);
                if (currentBoard == null)
                    return Error();

                var currentIndex = Array.FindIndex(boards, b => b.Id == currentBoard.Id);
                if (currentIndex > 0)
                    currentBoard = boards[currentIndex - 1];

                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = BuildBoardResponse(request.Response.Content, currentBoard),
                        AdditionalSessionAttributes = new Dictionary<string, object>
                        {
                            { SessionAttributes.CurrentBoardSessionAttribute, currentBoard }
                        }
                    }
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Error();
            }
        }

        public async Task<GeneralFulfillmentResponse> GetItems(GeneralWebhookFulfillmentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                    return Unauthorized();

                // TODO: item stuff
                    return new GeneralFulfillmentResponse
                    {
                        Data = new ContentFulfillmentWebhookData
                        {
                            Content = $"Not implemented"
                        }
                    };


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Error();
            }
        }

        private async Task<Board[]> GetBoardsFromRequest(GeneralWebhookFulfillmentRequest request)
        {
            var hasSessionAttribute = request.OriginalRequest.SessionAttributes.TryGetValue(SessionAttributes.BoardsSessionAttribute, out var boardsObj);
            if (hasSessionAttribute)
                return JsonConvert.DeserializeObject<Board[]>(JsonConvert.SerializeObject(boardsObj));

            var boardsResult = await _mondayDataProvider.GetAllBoards(request.OriginalRequest.AccessToken);
            return boardsResult?.Data;
        }

        private async Task<Board> GetCurrentBoardFromRequest(GeneralWebhookFulfillmentRequest request)
        {
            var hasSessionAttribute = request.OriginalRequest.SessionAttributes.TryGetValue(SessionAttributes.CurrentBoardSessionAttribute, out var boardObj);
            if (hasSessionAttribute)
                return JsonConvert.DeserializeObject<Board>(JsonConvert.SerializeObject(boardObj));

            var boards = await GetBoardsFromRequest(request);
            return boards?.FirstOrDefault();
        }

        private string BuildBoardResponse(string template, Board board)
        {
            return template.Replace(ResponseVariables.BoardItemCount, (board?.Items?.Length ?? 0).ToString())
                            .Replace(ResponseVariables.BoardName, board.Name)
                            .Replace(ResponseVariables.BoardGroupCount, (board.Groups?.Length ?? 0).ToString());
        }


        private GeneralFulfillmentResponse Error()
        {
            return new GeneralFulfillmentResponse
            {
                Data = new ContentFulfillmentWebhookData
                {
                    Content = "Something went wrong"
                }
            };
        }

        private GeneralFulfillmentResponse Unauthorized()
        {
            return new GeneralFulfillmentResponse
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
            };
        }

        public async Task<GeneralFulfillmentResponse> HandleGenericMondayRequest(GeneralWebhookFulfillmentRequest request)
        {
            try
            {
                // so basically this endpoint takes the parameters of a graphql query, and then variables with traversal values just like the remote content fulfillment integration in voicify
                // ex:
                // "query": "{boards { id, name }}"
                // "{first_board_name}": "boards[0]->name"
                // then the conversation item can have something like "Your first board is {first_board_name}", and a webhook with those params and the response output would be "Your first board is blah blah"
                if (string.IsNullOrEmpty(request.OriginalRequest.AccessToken))
                    return Unauthorized();
                if (request.Parameters is null)
                    return Error();

                request.Parameters.TryGetValue("query", out var query);
                if (string.IsNullOrEmpty(query))
                    return Error();

                var tokens = request.Parameters?.Where(k => k.Key.Trim().StartsWith("{")).Select(k => new KeyValuePair<string, string>(k.Key, k.Value));

                var response = await _mondayDataProvider.MakeRawQueryRequest(request.OriginalRequest.AccessToken, query);
                if (response.ResultType != ResultType.Ok)
                    return Error();

                var jObject = JObject.Parse(response.Data);
                var evaluatedTokens = tokens.Select(t => new KeyValuePair<string, string>(t.Key, _dataTraversalService.Traverse(jObject, t.Value)));
                var responseContent = request.Response.Content;
                foreach (var token in evaluatedTokens)
                {
                    responseContent = responseContent.Replace(token.Key, token.Value);
                }

                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = responseContent
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = "Something went wrong"
                    }
                };
            }
        }
    }
}
