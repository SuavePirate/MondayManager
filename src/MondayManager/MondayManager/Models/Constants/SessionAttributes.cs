using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MondayManager.Models.Constants
{
    public static class SessionAttributes
    {
        public const string BoardsSessionAttribute = "monday:boards";
        public const string CurrentBoardSessionAttribute = "monday:boards:current";
        public const string ItemsSessionAttribute = "monday:items";
        public const string CurrentItemSessionAttribute = "monday:items:current";
    }
}
