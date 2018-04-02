using Common.DTO.Communication;
using Common.DTO.Lobbie;
using Common.Entity;
using Common.Enums;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.LobbieService
{
    public class LobbieService : ILobbieService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
       

        public LobbieService(SkillWarsContext context, ILoggerFactory loggerFactory)
        {
             _logger = loggerFactory.CreateLogger<LobbieService>();
            _context = context;            
        }

        public async Task<Response<LobbieDTO>> CreateLobbieAsync(LobbieRequest request, int userId)
        {
            var response = new Response<LobbieDTO>();

            var user = await _context.Users.FirstOrDefaultAsync();
            if(user.TeamId != null)
            {
                response.Error = new Error(403, "You already participate in lobbie and can't create another");
                return response;
            }

            var lobbie = new LobbieEntity(request);            
            lobbie.Teams.Add(new TeamEntity() { LobbieId = lobbie.Id, Type = TeamTypes.First });
            lobbie.Teams.Add(new TeamEntity() { LobbieId = lobbie.Id, Type = TeamTypes.Second });
            lobbie.Teams[0].Users.Add(user);
            await _context.AddAsync(lobbie);
            await _context.SaveChangesAsync();

            response.Data = new LobbieDTO(lobbie);

            //send notifications for all users that new lobbie created
            return response;
        }

        public async Task<Response<bool>> LeaveLobbieAsync(int userId)
        {
            var response = new Response<bool>();

            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

            var team = await _context.Teams.Where(t => t.Id == user.TeamId)
                                           .Include(t => t.Lobbie)
                                           .FirstOrDefaultAsync();

            if(team == null)
            {
                response.Error = new Error(404, "Seems like you don't participate anywhere");
                return response;
            }

            team.Users.Remove(user);            
            await _context.SaveChangesAsync();

            response.Data = true;
            return response;
        }


        public async Task<List<LobbieDTO>> GetAllLobbiesAsync()
        {
            return await _context.Lobbies.AsNoTracking().Include(l => l.Teams).ThenInclude(t => t.Users).Select(l => new LobbieDTO(l)).ToListAsync();
        }

        //================== FOR TESTS ONLY ========================        

        public async Task<Response<bool>> RemoveLobbieById(int lobbieId)
        {
            var response = new Response<bool>();
            var lobbie = await _context.Lobbies.FirstOrDefaultAsync(u => u.Id == lobbieId);
            if (lobbie == null)
            {
                response.Error = new Error(404, "Lobbie not found");
                return response;
            }
            _context.Remove(lobbie);
            await _context.SaveChangesAsync();
            response.Data = true;
            return response;
        }

    }
}
