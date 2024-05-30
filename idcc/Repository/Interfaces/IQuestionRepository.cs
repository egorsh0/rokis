using idcc.Models;
using idcc.Models.Dto;

namespace idcc.Repository.Interfaces;

public interface IQuestionRepository
{
    QuestionDto GetQuestionAsync(Topic topic, Grade grade, double rank);
}