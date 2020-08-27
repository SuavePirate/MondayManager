using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MondayManager.Models.Monday
{
    public class MondayApiResponse<T>
    {
        public T Data { get; set; }
    }
}
