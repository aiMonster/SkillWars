using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWBot.Interfaces
{
    public interface ILogService
    {
       Task HandleLogAsync(string logLine);
    }
}
