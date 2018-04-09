using Common.DTO.Suggestion;
using Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Entity
{
    public class SuggestionEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public UserEntity User { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public PriorityTypes Priority { get; set; }
        public SuggestionCategroryTypes Category { get; set; }
        public DateTime CreationTime { get; set; }

        public bool IsConfirmed { get; set; }
        public DateTime PublishingTime { get; set; }

        public int Price { get; set; }
        public int Earned { get; set; }

        public bool IsDone { get; set; }
        public int ProgressPercents { get; set; }

        public DateTime Deadline { get; set; }

        public SuggestionEntity() { }

        public SuggestionEntity(SuggestionRequest request, int userId)
        {
            UserId = userId;
            Title = request.Title;
            Description = request.Description;
            Priority = request.Priority;
            Category = request.Category;
            CreationTime = DateTime.UtcNow;
        }

        public void Confirm(ConfirmSuggestionRequest request)
        {
            Title = request.Title;
            Description = request.Description;           
            PublishingTime = DateTime.UtcNow;
            Price = request.Price;
            Deadline = request.Deadline;
            IsConfirmed = true;
        }
    }
}
