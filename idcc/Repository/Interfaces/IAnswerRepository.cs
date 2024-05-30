using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IAnswerRepository
{
    Task<List<Answer>> GetAnswersAsync(Question question);
}