using idcc.Context;
using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class QuestionRepository : IQuestionRepository
{
    private readonly IdccContext _context;

    public QuestionRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<QuestionDto?> GetQuestionAsync(UserTopic userTopic)
    {
        var weight = await _context.Weights.SingleOrDefaultAsync(_ => _.Grade == userTopic.Grade);
        if (weight is null)
        {
            return null;
        }
        
        var question = await _context.Questions.Where(_ => _.Topic == userTopic.Topic && _.Weight >= userTopic.Weight && _.Weight <= weight.Max).OrderBy(o => Guid.NewGuid()).FirstOrDefaultAsync();
        if (question is null)
        {
            return null;
        }
        
        // TODO проверить на отсутствие ответов к вопросу
        var answers = await _context.Answers.Where(_ => _.Question == question).Select(a => new AnswerDto()
        {
            Id = a.Id,
            Content = a.Content
        }).ToListAsync();

        var dto = new QuestionDto()
        {
            Id = question.Id,
            Topic = userTopic.Topic,
            Content = question.Content,
            Answers = answers
        };

        return dto;
    }

    public async Task<QuestionDataDto?> GetQuestionAsync(int id)
    {
        var question =  await _context.Questions.Where(_ => _.Id == id).FirstAsync();
        var answers = await _context.Answers.Where(_ => _.Question.Id == id).ToListAsync();

        var dto = new QuestionDataDto()
        {
            Question = question,
            Answers = answers
        };

        return dto;
    }
}