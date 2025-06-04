using idcc.Dtos;
using idcc.Dtos.AdminDto;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IQuestionRepository
{
    Task<QuestionDto?> GetQuestionAsync(UserTopicDto? userTopic);
    
    Task<Question?> GetQuestionAsync(int id);
    Task<List<Answer>> GetAnswersAsync(Question question);
    Task<List<string>> CreateAsync(List<QuestionAdminDto> questions);
}