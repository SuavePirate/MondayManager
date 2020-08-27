using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using MondayManager.Models.Monday;
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

        private async Task<T> SendQuery<T>(string accessToken, string query)
        {
            _client.HttpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
            var response = await _client.SendQueryAsync<T>(new GraphQL.GraphQLRequest(query));
            return response.Data;
        }
    }
}
