using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MondayManager.Models.Monday
{
    public class Board
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Item[] Items { get; set; }
        public Group[] Groups { get; set; }
    }
}
