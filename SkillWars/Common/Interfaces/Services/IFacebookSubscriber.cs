using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface IFacebookSubscriber
    {
        void Setup(string path);
        List<string> GetLogs1();
        List<string> GetLogs2();        
    }
}
