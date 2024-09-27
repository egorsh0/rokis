using idcc.Models;
using idcc.Models.Dto;

namespace idcc.Repository.Interfaces;

public interface IQuestionRepository
{
    Task<QuestionDto?> GetQuestionAsync(UserTopic userTopic);
    
    Task<QuestionDataDto?> GetQuestionAsync(int id);
}