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
    
    public async Task<QuestionDto> GetQuestionAsync(Topic topic, Grade grade, double rank)
    {
        var rlGrades = _context.RlGrades.SingleOrDefault(_ => _.CurrentGrade == grade);
        var nextGrade = _context.Grades.Single(_ => _.Id == rlGrades.NextGrade.Id);
        
        var question =  _context.Questions.Where(_ => _.Topic == topic && _.Rank >= rank && _.Rank < nextGrade.Score).OrderBy(o => Guid.NewGuid()).First();
        var questionDto = new QuestionDto()
        {
            Topic = topic.Name,
            Question = question.Name
        };
        
        var rlQuestions = await _context.RlQuestions.Where(_ => _.Question.Id == question.Id).ToListAsync();
        if (rlQuestions.Any())
        {
            foreach (var rlQuestion in rlQuestions)
            {
                var answer = rlQuestion.Answer;
                var answerDto = new AnswerDto()
                {
                    Answer = answer.Name,
                    Score = answer.Score
                };
                questionDto.Answers.Add(answerDto);
            }
        }

        return questionDto;
    }
}