﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using MondayManager.Models.Monday;
using Newtonsoft.Json;
using ServiceResult;

namespace MondayManager.Providers
{
    public class MondayDataProvider : IMondayDataProvider
    {
        private readonly GraphQLHttpClient _client;

        public MondayDataProvider(GraphQLHttpClient client)
        {
            _client = client;
        }

        public async Task<Result<Board[]>> GetAllBoards(string accessToken)
        {
            var boards = await SendQuery<BoardsResponse>(accessToken, @"{boards {
                  id
                  name
                  groups {
                    id,
   	                title
                  }
                  items {
                    name,
                    group {
                      id
                    }
                  }
                }}");


            return new SuccessResult<Board[]>(boards.Boards);
        }

        public async Task<Result<string>> MakeRawQueryRequest(string accessToken, string query)
        {
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", accessToken);
                var response = await client.PostAsync("https://api.monday.com/v2", new StringContent(JsonConvert.SerializeObject(new { query }), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                    return new SuccessResult<string>(await response.Content.ReadAsStringAsync());
                else
                    return new InvalidResult<string>(await response.Content.ReadAsStringAsync());

            }
        }

        private async Task<T> SendQuery<T>(string accessToken, string query)
        {
            _client.HttpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
            var response = await _client.SendQueryAsync<T>(new GraphQL.GraphQLRequest(query));
            return response.Data;
        }
    }
}
