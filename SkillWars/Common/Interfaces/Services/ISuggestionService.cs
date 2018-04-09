using Common.DTO.Communication;
using Common.DTO.Suggestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface ISuggestionService
    {
        Task<bool> CreateSuggestionAsync(SuggestionRequest request, int userId);
        Task<List<SuggestionDTO>> GetSuggestionsAsync(int skip, int take, bool confirmed);
        Task<Response<bool>> ConfirmSuggestionAsync(ConfirmSuggestionRequest request);
        Task<Response<bool>> DeclineSuggestionAsync(int suggestionId);
        Task<Response<bool>> ChangeProgressAsync(int suggestionId, int progress);
    }
}
