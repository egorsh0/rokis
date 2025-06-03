using idcc.Dtos.AdminDto;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/administator")]
public class AdministatorController : ControllerBase
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<AdministatorController> _logger;

    public AdministatorController(IQuestionRepository questionRepository, ILogger<AdministatorController> logger)
    {
        _questionRepository = questionRepository;
        _logger = logger;
    }

    [HttpPost]
    [Route("question/create")]
    public async Task<IResult> Create([FromBody] List<QuestionAdminDto> questions)
    {
        var notAddedQuestions = await _questionRepository.CreateAsync(questions);

        return Results.Ok(notAddedQuestions);
    }
}