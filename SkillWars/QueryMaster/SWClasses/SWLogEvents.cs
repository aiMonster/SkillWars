using QueryMaster.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace QueryMaster.SWClasses
{
    public class SWLogEvents: LogEvents
    {
        public SWLogEvents(IPEndPoint endPoint): base(endPoint) { }

        public void SWProcessLog(string log)
        {
            ProcessLog(log);
        }
        
    }
}
