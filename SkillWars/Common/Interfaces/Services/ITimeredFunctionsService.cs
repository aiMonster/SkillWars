using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface ITimeredFunctionsService
    {
        Task<bool> Setup();
    }
}
