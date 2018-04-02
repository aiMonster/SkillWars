using Common.DTO.Communication;
using Common.DTO.Lobbie;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface ILobbieService
    {
        Task<Response<LobbieDTO>> CreateLobbieAsync(LobbieRequest request, int userId);
        Task<Response<bool>> LeaveLobbieAsync(int userId);
        Task<List<LobbieDTO>> GetAllLobbiesAsync();
        Task<Response<bool>> ParticipateLobbieAsync(ParticipatingRequest request, int userId);
        Task CheckLobbies();

        Task<Response<bool>> RemoveLobbieById(int lobbieId);
    }
}
