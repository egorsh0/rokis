using idcc.Models;
using idcc.Models.AdminDto;
using idcc.Models.Dto;

namespace idcc.Repository.Interfaces;

public interface IQuestionRepository
{
    Task<QuestionDto?> GetQuestionAsync(UserTopic userTopic);
    
    Task<Question?> GetQuestionAsync(int id);
    Task<List<Answer>> GetAnswersAsync(Question question);
    Task<List<string>> CreateAsync(List<QuestionAdminDto> questions);
}