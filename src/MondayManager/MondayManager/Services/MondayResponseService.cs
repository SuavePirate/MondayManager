using System;
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
        private readonly IEnhancedLanguageService _languageService;

        public MondayResponseService(IMondayDataProvider mondayDataProvider, IDataTraversalService dataTraversalService, IEnhancedLanguageService languageService)
        {
            _mondayDataProvider = mondayDataProvider;
            _dataTraversalService = dataTraversalService;
            _languageService = languageService;
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

                var currentBoard = await GetCurrentBoardFromRequest(request);
                if (currentBoard == null)
                    return Error();

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


        public async Task<GeneralFulfillmentResponse> CreateItem(GeneralWebhookFulfillmentRequest request)
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

                request.OriginalRequest.Slots.TryGetValue("query", out string query);
                if(string.IsNullOrEmpty(query))
                    return Error();


                // get group and item name
                var processedLanguage = await ExtractGroupAndItem(query);

                processedLanguage.Slots.TryGetValue("Group", out var groupName);
                processedLanguage.Slots.TryGetValue("Item", out var itemName);
                if (string.IsNullOrEmpty(groupName))
                    return Error("You have to provide a group to add the item to. Try something like \"Add a new item to backlog called do work\"");
                if (string.IsNullOrEmpty(itemName))
                    return Error("You have to provide a item name in order to add it. Try something like \"Add a new item to backlog called do work\"");

                // search all groups, prioritize current board
                // TODO: consider grooming input before search to have a greater chance of a match. Ex: "the backlog" should match "backlog"
                var matchedGroup = currentBoard.Groups?.FirstOrDefault(g => g.Title.ToLower().Contains(groupName.ToLower()));
                var matchedBoard = currentBoard;
                if(matchedGroup == null)
                {
                    foreach(var board in boards)
                    {
                        matchedGroup = board.Groups.FirstOrDefault(g => g.Title.ToLower().Contains(groupName.ToLower()));
                        if (matchedGroup != null)
                        {
                            matchedBoard = board;
                            break;
                        }
                    }
                }
                if (matchedGroup == null)
                    return Error($"I couldn't find any group for {groupName}. Try adding to a different group.");





                // create the item given the text

                var itemResult = await _mondayDataProvider.CreateItem(request.OriginalRequest.AccessToken, matchedBoard.Id, matchedGroup.Id, itemName);

                return new GeneralFulfillmentResponse
                {
                    Data = new ContentFulfillmentWebhookData
                    {
                        Content = $"Added {itemResult.Data.Name} to {matchedGroup.Title}."
                    }
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Error();
            }
        }

        private async Task<ProcessedLanguage> ExtractGroupAndItem(string query)
        {
            var languageModel = new InteractionModel
            {
                Intents = new List<Intent>
                {
                    new Intent
                    {
                        Name = new Dictionary<string, string> {{"voicify", "GroupAndItem" } },
                        DisplayName = "GroupAndItem",
                        Utterances = new List<string>
                        {
                            "item to {Group} called {Item}",
                            "item to the {Group} called {Item}",
                            "item in {Group} called {Item}",
                            "item the in {Group} called {Item}",
                            "add an item to {Group} called {Item}",
                            "add an item in {Group} called {Item}",
                            "add an item to the {Group} called {Item}",
                            "add an item in the {Group} called {Item}",
                            "create an item in {Group} called {Item}",
                            "item to {Group} titled {Item}",
                            "item in {Group} titled {Item}",
                            "add an item to {Group} titled {Item}",
                            "add an item in {Group} titled {Item}",
                            "create an item in {Group} titled {Item}",
                            "item to {Group} called {Item}",
                            "item in {Group} called {Item}",
                            "add an item to {Group} called {Item}",
                            "add an item in {Group} called {Item}",
                            "create an item in {Group} called {Item}",

                            "new item to {Group} called {Item}",
                            "new item in {Group} called {Item}",
                            "new item to the {Group} called {Item}",
                            "new item in the {Group} called {Item}",
                            "add a new item to {Group} called {Item}",
                            "add a new item in {Group} called {Item}",
                            "add a new item to the {Group} called {Item}",
                            "add a new item in the {Group} called {Item}",
                            "create a new item in {Group} called {Item}",
                            "new item to {Group} titled {Item}",
                            "new item in {Group} titled {Item}",
                            "add a new item to {Group} titled {Item}",
                            "add a new item in {Group} titled {Item}",
                            "create an item in {Group} titled {Item}",
                            "new item to {Group} called {Item}",
                            "new item in {Group} called {Item}",
                            "add a new item to {Group} called {Item}",
                            "add a new item in {Group} called {Item}",
                            "create a new item in {Group} called {Item}",
                        }
                    }
                }
            };

            var output = await _languageService.Process(query, languageModel);
            return output?.Data;
        }


        private async Task<Board[]> GetBoardsFromRequest(GeneralWebhookFulfillmentRequest request)
        {
            (request.OriginalRequest.SessionAttributes ?? new Dictionary<string, object>()).TryGetValue(SessionAttributes.BoardsSessionAttribute, out var boardsObj);
            if (boardsObj != null)
                return JsonConvert.DeserializeObject<Board[]>(JsonConvert.SerializeObject(boardsObj));

            var boardsResult = await _mondayDataProvider.GetAllBoards(request.OriginalRequest.AccessToken);
            return boardsResult?.Data;
        }

        private async Task<Board> GetCurrentBoardFromRequest(GeneralWebhookFulfillmentRequest request)
        {
            (request.OriginalRequest.SessionAttributes ?? new Dictionary<string, object>()).TryGetValue(SessionAttributes.CurrentBoardSessionAttribute, out var boardObj);
            if (boardObj != null)
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


        private GeneralFulfillmentResponse Error(string message = "Something went wrong")
        {
            return new GeneralFulfillmentResponse
            {
                Data = new ContentFulfillmentWebhookData
                {
                    Content = message
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
