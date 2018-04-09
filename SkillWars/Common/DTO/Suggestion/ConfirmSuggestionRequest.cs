using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Suggestion
{
    public class ConfirmSuggestionRequest
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }     
        public int Price { get; set; }
        public DateTime Deadline { get; set; }
    }
}
