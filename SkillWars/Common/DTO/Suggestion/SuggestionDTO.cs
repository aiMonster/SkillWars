using Common.DTO.Account;
using Common.Entity;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Suggestion
{
    public class SuggestionDTO
    {
        public int Id { get; set; }
        
        public UserInfo User { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public PriorityTypes Priority { get; set; }
        public SuggestionCategroryTypes Category { get; set; }
        public DateTime CreationTime { get; set; }
        
        public DateTime PublishingTime { get; set; }

        public int Price { get; set; }
        public int Earned { get; set; }

        public bool IsDone { get; set; }
        public int ProgressPercents { get; set; }

        public DateTime Deadline { get; set; }


        public SuggestionDTO() { }
        public SuggestionDTO(SuggestionEntity entity)
        {
            Id = entity.Id;            
            User = new UserInfo(entity.User);
            Title = entity.Title;
            Description = entity.Description;
            Priority = entity.Priority;
            Category = entity.Category;
            CreationTime = entity.CreationTime;
            PublishingTime = entity.PublishingTime;
            Price = entity.Price;
            Earned = entity.Earned;
            IsDone = entity.IsDone;
            ProgressPercents = entity.ProgressPercents;
            Deadline = entity.Deadline;
        }
    }
}
