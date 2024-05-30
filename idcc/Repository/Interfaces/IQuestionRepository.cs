using idcc.Models;
using idcc.Models.Dto;

namespace idcc.Repository.Interfaces;

public interface IQuestionRepository
{
    Task<QuestionDto> GetQuestionAsync(Topic topic, Grade grade, double rank);
}