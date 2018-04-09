using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Suggestion
{
    public class SuggestionRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public PriorityTypes Priority { get; set; }
        public SuggestionCategroryTypes Category { get; set; }
    }
}
