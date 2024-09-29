using idcc.Bot.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace idcc.Bot.Controllers;

public class UpdateController : Controller
{
    private readonly ITelegramBotClient _telegramBot;
    private readonly ILogger<UpdateController> _logger;
    private readonly IBotService _botService;

    public UpdateController(
        ITelegramBotClient telegramBot,
        ILogger<UpdateController> logger,
        IBotService botService)
    {
        _telegramBot = telegramBot;
        _logger = logger;
        _botService = botService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, [FromServices] ITelegramBotClient bot, [FromServices] IBotService handleUpdateService, CancellationToken ct)
    {
        try
        {
            await handleUpdateService.HandleUpdateAsync(bot, update, ct);
        }
        catch (Exception exception)
        {
            await handleUpdateService.HandleErrorAsync(bot, exception, ct);
        }
        return Ok();
    }
}