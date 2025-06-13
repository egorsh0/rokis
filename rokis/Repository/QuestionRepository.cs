using rokis.Context;
using rokis.Dtos;
using rokis.Dtos.AdminDto;
using rokis.Models;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IQuestionRepository
{
    /// <summary>
    /// Получение уникального вопроса.
    /// </summary>
    /// <param name="answeredIds">Список id уже отвеченных вопросов.</param>
    /// <param name="topicId">Идентификатор темы.</param>
    /// <param name="topicWeight">Вес темы.</param>
    /// <param name="maxWeight">Максимальный вес вопроса (грейда)</param>
    /// <returns></returns>
    Task<Question?> GetQuestionAsync(List<int> answeredIds, int topicId, double topicWeight, double maxWeight);

    /// <summary>
    /// Получить список ответов для вопроса.
    /// </summary>
    /// <param name="questionId">Идентификатор вопроса.</param>
    /// <returns></returns>
    Task<List<AnswerDto>> GetAnswersAsync(int questionId);
    
    /// <summary>
    /// Получение вопроса.
    /// </summary>
    /// <param name="questionId"></param>
    /// <returns></returns>
    Task<QuestionSmartDto?> GetQuestionAsync(int questionId);
    
    Task<List<string>> CreateAsync(List<QuestionAdminDto> questions);
}

public class QuestionRepository : IQuestionRepository
{
    private readonly RokisContext _context;

    public QuestionRepository(RokisContext context)
    {
        _context = context;
    }


    public async Task<Question?> GetQuestionAsync(List<int> answeredIds, int topicId, double topicWeight, double maxWeight)
    {
        return await _context.Questions
            .Where(q => !answeredIds.Contains(q.Id))
            .Where(q => q.Topic.Id == topicId && q.Weight >= topicWeight && q.Weight <= maxWeight)
            .OrderBy(o => Guid.NewGuid())
            .FirstOrDefaultAsync();
    }

    public async Task<List<AnswerDto>> GetAnswersAsync(int questionId)
    {
        var answers = await _context.Answers
            .Where(a => a.Question.Id == questionId)
            .Select(answer => answer)
            .ToListAsync();
        return answers.Select(a => new AnswerDto()
        {
            Id = a.Id, 
            IsCorrect = a.IsCorrect, 
            Content = a.Content
        }).ToList();
    }

    public async Task<QuestionSmartDto?> GetQuestionAsync(int id)
    {
        var question = await _context.Questions
            .AsNoTracking()
            .Include(q=> q.Topic)
                .ThenInclude(t => t.Direction)
            .SingleOrDefaultAsync(q => q.Id == id);
        return question is null ? null : new QuestionSmartDto(question.Id, question.IsMultipleChoice, question.Weight);
    }

    public async Task<List<string>> CreateAsync(List<QuestionAdminDto> questions)
    {
        var notAdded = new List<string>();
        foreach (var question in questions)
        {
            var topic = await _context.Topics.Where(t => t.Name == question.Topic).SingleOrDefaultAsync();
            if (topic is null)
            {
                notAdded.Add(question.Content);
            }
            else
            {
                try
                {
                    var q = await _context.Questions.AddAsync(new Question()
                    {
                        Topic = topic,
                        Content = question.Content,
                        Weight = question.Weight,
                        IsMultipleChoice = question.IsMultipleChoice
                    });


                    foreach (var answer in question.Answers)
                    {
                        await _context.Answers.AddAsync(new Answer()
                        {
                            Question = q.Entity,
                            Content = answer.Content,
                            IsCorrect = answer.IsCorrect
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    notAdded.Add(question.Content);
                }
            }
        }

        return notAdded;
    }
}