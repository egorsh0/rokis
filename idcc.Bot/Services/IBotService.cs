using Telegram.Bot;
using Telegram.Bot.Types;

namespace idcc.Bot.Services;

public interface IBotService
{
    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken);

    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}