using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Sockets;
using Common.DTO.Suggestion;
using Common.Entity;
using Common.Enums;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.WebSockets.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SuggestionService
{
    public class SuggestionService : ISuggestionService
    {
        private readonly ILogger _logger;
        private readonly SkillWarsContext _context;
        private readonly LobbieHandler _socketHandler;

        public SuggestionService(SkillWarsContext context, ILoggerFactory loggerFactory, LobbieHandler socketHandler)
        {
            _logger = loggerFactory.CreateLogger<SuggestionService>();
            _context = context;
            _socketHandler = socketHandler;
        }

        public async Task<bool> CreateSuggestionAsync(SuggestionRequest request, int userId)
        {
            
            var entity = new SuggestionEntity(request, userId);
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<SuggestionDTO>> GetSuggestionsAsync(int skip, int take, bool confirmed)
        {
            return await _context.Suggestions.Where(s => s.IsConfirmed == confirmed)
                                             .AsNoTracking()
                                             .Skip(skip)
                                             .Take(take) 
                                             .Include(s => s.User)
                                             .Select(s => new SuggestionDTO(s))
                                             .ToListAsync();
        }
        
        public async Task<Response<bool>> ConfirmSuggestionAsync(ConfirmSuggestionRequest request)
        {
            var response = new Response<bool>();

            var entity = await _context.Suggestions.Where(s => s.Id == request.Id).FirstOrDefaultAsync();
            if(entity == null)
            {
                response.Error = new Error(404, "Sugggestion not found");
                return response;
            }
            if(entity.IsConfirmed)
            {
                response.Error = new Error(403, "Sugggestion already confirmed");
                return response;
            }
            entity.Confirm(request);
            await _context.SaveChangesAsync();

             var notification = new NotificationEntity()
            {
                Time = DateTime.UtcNow,
                Text = "Your suggestion has been declined - " + entity.Title
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            var user = await _context.Users.Where(u => u.Id == entity.UserId).FirstOrDefaultAsync();
            user.Notifications.Add(new UserNotificationsEntity() { NotificationId = notification.Id, UserId = user.Id });
            await _socketHandler.SendMessageByUserId(user.Id, new SocketMessage<NotificationDTO>(new NotificationDTO(notification), NotificationTypes.SuggestionAccepted));
            await _context.SaveChangesAsync();

            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> DeclineSuggestionAsync(int suggestionId)
        {
            var response = new Response<bool>();

            var entity = await _context.Suggestions.Where(s => s.Id == suggestionId).FirstOrDefaultAsync();
            if (entity == null)
            {
                response.Error = new Error(404, "Sugggestion not found");
                return response;
            }

            _context.Remove(entity);
            await _context.SaveChangesAsync();


            var notification = new NotificationEntity()
            {
                Time = DateTime.UtcNow,
                Text = "Your suggestion has been declined - " + entity.Title
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            var user = await _context.Users.Where(u => u.Id == entity.UserId).FirstOrDefaultAsync();
            user.Notifications.Add(new UserNotificationsEntity() { NotificationId = notification.Id, UserId = user.Id });
            await _socketHandler.SendMessageByUserId(user.Id, new SocketMessage<NotificationDTO>(new NotificationDTO(notification), NotificationTypes.SuggestionDeclined));
            await _context.SaveChangesAsync();

            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> ChangeProgressAsync(int suggestionId, int progress)
        {
            var response = new Response<bool>();

            var entity = await _context.Suggestions.Where(s => s.Id == suggestionId).FirstOrDefaultAsync();
            if (entity == null)
            {
                response.Error = new Error(404, "Sugggestion not found");
                return response;
            }

            entity.ProgressPercents = progress;
            if(progress == 100)
            {                
                entity.IsDone = true;

                var notification = new NotificationEntity()
                {
                    Time = DateTime.UtcNow,
                    Text = "Your suggestion has been declined - " + entity.Title
                };
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                var user = await _context.Users.Where(u => u.Id == entity.UserId).FirstOrDefaultAsync();
                user.Notifications.Add(new UserNotificationsEntity() { NotificationId = notification.Id, UserId = user.Id });
                await _socketHandler.SendMessageByUserId(user.Id, new SocketMessage<NotificationDTO>(new NotificationDTO(notification), NotificationTypes.SuggestionDone));
            }

            await _context.SaveChangesAsync();

            response.Data = true;
            return response;
        }
    }
}
