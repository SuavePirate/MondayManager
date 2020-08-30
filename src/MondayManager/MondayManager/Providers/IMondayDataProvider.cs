using MondayManager.Models.Monday;
using ServiceResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MondayManager.Providers
{
    public interface IMondayDataProvider
    {
        Task<Result<Board[]>> GetAllBoards(string accessToken);
        Task<Result<Board[]>> GetItemsForBoard(string accessToken, string boardId);
        Task<Result<string>> MakeRawQueryRequest(string accessToken, string query);
        Task<Result<Item>> CreateItem(string accessToken, string boardId, string groupId, string title);
    }
}
