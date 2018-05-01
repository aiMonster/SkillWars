using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Lobbie;
using Common.DTO.Sockets;
using Common.Entity;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.WebSockets.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.LobbieService
{
    public class LobbieService : ILobbieService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
        private readonly LobbieHandler _socketHandler;
       

        public LobbieService(SkillWarsContext context, ILoggerFactory loggerFactory, LobbieHandler socketHandler)
        {
             _logger = loggerFactory.CreateLogger<LobbieService>();
            _context = context;
            _socketHandler = socketHandler;
        }

        public async Task<Response<LobbieDTO>> CreateLobbieAsync(LobbieRequest request, int userId)
        {
            _logger.LogDebug("Creating lobbie");
            var response = new Response<LobbieDTO>();

            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user.TeamId != null)
            {
                response.Error = new Error(403, "You already participate in lobbie and can't create another");
                return response;
            }
            else if (user.Balance < request.Bet)
            {
                response.Error = new Error(403, "Not enough money");
                return response;
            }

            var lobbie = new LobbieEntity(request);            
            lobbie.Teams.Add(new TeamEntity() { LobbieId = lobbie.Id, Type = TeamTypes.First });
            lobbie.Teams.Add(new TeamEntity() { LobbieId = lobbie.Id, Type = TeamTypes.Second });
            lobbie.Teams[0].Users.Add(user);
            await _context.AddAsync(lobbie);
            await _context.SaveChangesAsync();

            response.Data = new LobbieDTO(lobbie);
           
            await _socketHandler.SendMessageToAllAsync(new SocketMessage<LobbieDTO>(response.Data, NotificationTypes.NewLobbie));
            return response;
        }

        public async Task<Response<bool>> LeaveLobbieAsync(int userId)
        {
            _logger.LogDebug("Leaving lobbie");
            var response = new Response<bool>();

            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

            var team = await _context.Teams.Where(t => t.Id == user.TeamId)
                                           .Include(t => t.Lobbie)
                                            .ThenInclude(l=> l.Teams)
                                           .FirstOrDefaultAsync();

            if(team == null)
            {
                response.Error = new Error(403, "Seems like you don't participate here");
                return response;
            }
            else if(team.Lobbie.Status == LobbieStatusTypes.Started)
            {
                response.Error = new Error(403, "Lobbie already started idiot");
                return response;
            }


            team.Users.Remove(user);            
            await _context.SaveChangesAsync();

            if (team.Lobbie.Teams[0].Users.Count == 0 && team.Lobbie.Teams[1].Users.Count == 0)
            {
                _context.Lobbies.Remove(team.Lobbie);
                await _context.SaveChangesAsync();
                
                await _socketHandler.SendMessageToAllAsync(new SocketMessage<int>(team.Lobbie.Id, NotificationTypes.LobbieRemoved));
            }
            else
            {                 
                await _socketHandler.SendMessageToAllAsync(new SocketMessage<TeamChangedSRequest>(
                                                       new TeamChangedSRequest()
                                                       { User = new UserInfo(user), LobbieId = team.Lobbie.Id },
                                                       NotificationTypes.UserLeaved));
            }

            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> ParticipateLobbieAsync(ParticipatingRequest request, int userId)
        {
            _logger.LogDebug("Participating lobbie");
            var response = new Response<bool>();

            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if(user.TeamId != null)
            {
                response.Error = new Error(403, "Seems like you already participate in the any lobbie");
                return response;
            }

            var lobbie = await _context.Lobbies.Where(l => l.Id == request.LobbieId)
                                               .Include(l => l.Teams)
                                                 .ThenInclude(t => t.Users)
                                               .FirstOrDefaultAsync();

            if(lobbie == null)
            {
                response.Error = new Error(404, "Such lobbie not found");
                return response;
            }                        
            else if(lobbie.Status == LobbieStatusTypes.Started)
            {
                response.Error = new Error(403, "Lobbie already started");
                return response;
            }
            else if(lobbie.Teams.Where(t => t.Type == request.TeamType).FirstOrDefault().Users.Count == lobbie.AmountPlayers/2)
            {
                response.Error = new Error(403, "Team is full");
                return response;
            }
            else if(user.Balance < lobbie.Bet)
            {
                response.Error = new Error(403, "Not enough money");
                return response;
            }
            else if (lobbie.IsPrivate && (String.IsNullOrEmpty(request.Password) || lobbie.Password != SkillWarsEncoder.Encript(request.Password)))
            {
                response.Error = new Error(403, "Incorrect password");
                return response;
            }

            lobbie.Teams.Where(t => t.Type == request.TeamType).FirstOrDefault().Users.Add(user);
            await _context.SaveChangesAsync();

            response.Data = true;
            
            await _socketHandler.SendMessageToAllAsync(new SocketMessage<TeamChangedSRequest>(
                                                       new TeamChangedSRequest()
                                                       { User = new UserInfo(user), LobbieId = lobbie.Id },
                                                       NotificationTypes.UserParticipated));

            if (lobbie.Teams[0].Users.Count + lobbie.Teams[1].Users.Count == lobbie.AmountPlayers)
            {
                await StartGameAsync(lobbie.Id);
            }

            return response;
        }
        
        public async Task<List<LobbieDTO>> GetAllLobbiesAsync()
        {
            return await _context.Lobbies.AsNoTracking().Include(l => l.Teams).ThenInclude(t => t.Users).Select(l => new LobbieDTO(l)).ToListAsync();
        }

        public async Task<Response<LobbieDTO>> GetLobbieByIdAsync(int id)
        {
            var response = new Response<LobbieDTO>();

            var lobbie = await _context.Lobbies.Where(l => l.Id == id)
                                               .AsNoTracking()
                                               .Include(l => l.Teams)
                                               .ThenInclude(t => t.Users)
                                               .Select(l => new LobbieDTO(l))
                                               .FirstOrDefaultAsync();

            if(lobbie == null)
            {
                response.Error = new Error(404, "Lobbie not found");
                return response;
            }
            response.Data = lobbie;
            return response;            
        }

        public async Task CheckLobbiesAsync()
        {            
            var toUpdate = await _context.Lobbies.Where(l => l.StartingTime <= DateTime.UtcNow && l.Status == LobbieStatusTypes.Expecting)
                                                 .Include(l => l.Teams)
                                                    .ThenInclude(t => t.Users)
                                                 .ToListAsync();

            foreach(var lobbie in toUpdate)
            {                
                if(lobbie.Teams[0].Users.Count + lobbie.Teams[1].Users.Count < lobbie.AmountPlayers)
                {
                    _context.Lobbies.Remove(lobbie);
                    await _context.SaveChangesAsync();
                    await _socketHandler.SendMessageToAllAsync(new SocketMessage<int>(lobbie.Id, NotificationTypes.LobbieRemoved));
                }
                else
                {                    
                    await StartGameAsync(lobbie.Id);                    
                }                
            }
        }

        private async Task StartGameAsync(int lobbieId)
        {
            //notifications for all users
            await _socketHandler.SendMessageToAllAsync(new SocketMessage<int>(lobbieId, NotificationTypes.LobbieStarted));
            var lobbie = await _context.Lobbies.Where(l => l.Id == lobbieId)
                                         .Include(l => l.Teams)
                                             .ThenInclude(t=> t.Users)
                                         .FirstOrDefaultAsync();

            lobbie.Status = LobbieStatusTypes.Started;            

            //Notification only for participants
            var notification = new NotificationEntity()
            {                
                Time = DateTime.UtcNow,
                Text = "Here is info about started server for lobbie with id - " + lobbie.Id                
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            foreach(var team in lobbie.Teams)
            {
                foreach(var user in team.Users)
                {
                    user.Notifications.Add(new UserNotificationsEntity() { NotificationId = notification.Id, UserId = user.Id });
                    await _socketHandler.SendMessageByUserId(user.Id, new SocketMessage<NotificationDTO>(new NotificationDTO(notification), NotificationTypes.GameStarted)); 
                }
            }
            await _context.SaveChangesAsync();                  
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
