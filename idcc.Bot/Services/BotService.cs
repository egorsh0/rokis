using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace idcc.Bot.Services;

public class BotService : IBotService
{
    private readonly ILogger<BotService> _logger;
    private readonly IIdccService _idccService;
    private readonly ITelegramBotClient _telegramBot;

    private int? _userId = null;
    private int? _sessionId = null;

    private DateTime _questionTime = default!;

    private readonly string _pattern = @"question\((.+),answer:(.+)\)";

    public BotService(
        ILogger<BotService> logger,
        IIdccService idccService,
        ITelegramBotClient telegramBot)
    {
        _logger = logger;
        _idccService = idccService;
        _telegramBot = telegramBot;
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message }                        => OnMessage(message),
            { CallbackQuery: { } callbackQuery }            => OnCallbackQuery(callbackQuery),
            // ChannelPost:
            // EditedChannelPost:
            // ShippingQuery:
            // PreCheckoutQuery:
            _                                               => UnknownUpdateHandlerAsync(update)
        });
    }
    
    private async Task OnMessage(Message msg)
    {
        _logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
            return;

        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            "/start" => Start(msg),
            "/testing" => StartSession(msg),
            "/question" => GetQuestion(msg),
            _ => throw new ArgumentOutOfRangeException()
        });
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
    }
    
    async Task<Message> Start(Message msg)
    {
        _logger.LogInformation("Create user");
        await _telegramBot.SendTextMessageAsync(msg.Chat, "Добро пожаловать на тестирование своих навыков.", parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        
        var inlineKeyboard = new InlineKeyboardMarkup(
            new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("QA", "qa"), 
                }
            });
                                
        return await _telegramBot.SendTextMessageAsync(
            msg.Chat.Id,
            "Выберите вашу роль",
            replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup
    }
    
    async Task<Message> StartSession(Message msg)
    {
        if (_userId is null)
        {
            return await _telegramBot.SendTextMessageAsync(
                msg.Chat.Id,
                "Вы не зарегистрированы",
                parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        var (session, error) = await _idccService.StartSessionAsync(_userId.Value);
        if (error is not null)
        {
            return await _telegramBot.SendTextMessageAsync(msg.Chat, error, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }
        
        _sessionId = session!.Id;
        return await _telegramBot.SendTextMessageAsync(
            msg.Chat.Id,
            "Сессия запущена",
            parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
    
    async Task<Message> GetQuestion(Message msg)
    {
        if (_sessionId is null)
        {
            return await _telegramBot.SendTextMessageAsync(
                msg.Chat.Id,
                "Сессия не запущена",
                parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        var (question, message) = await _idccService.GetQuestionAsync(_sessionId.Value);

        if (message is not null)
        {
            return await _telegramBot.SendTextMessageAsync(msg.Chat, message, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        var answers = question?.Answers;
        var callbackAnswerData = new List<InlineKeyboardButton[]>();

        foreach (var answer in answers!)
        {
            var answerData = new[]
            {
                InlineKeyboardButton.WithCallbackData(answer.Content, $"question({question?.Id},answer:{answer.Id})")
            };
            callbackAnswerData.Add(answerData);
        }
        var inlineKeyboard = new InlineKeyboardMarkup(callbackAnswerData);
                  
        _questionTime = DateTime.Now;
        return await _telegramBot.SendTextMessageAsync(
            msg.Chat.Id,
            question?.Content!,
            replyMarkup: inlineKeyboard);
    }
    
    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        var user = callbackQuery.From;
        _logger.LogInformation($"{user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");
        var chat = callbackQuery.Message?.Chat;

        var data = callbackQuery.Data?.ToLower();
        var parseData = ParseCallbackData(callbackQuery.Data?.ToLower()!);
        
        switch (data)
        {
            case "qa":
            {
                var (userFullDto, message) = await _idccService.CreateUserAsync(user.Username!, "QA");
                if (message is not null)
                {
                    _logger.LogInformation(message);
                    await _telegramBot.SendTextMessageAsync(chat!, $"Ошибка при создании пользователя {message}");
                }

                _userId = userFullDto!.Id;
                _logger.LogInformation("Пользователь создан!");
                
                await _telegramBot.SendTextMessageAsync(chat!, $"Пользователь зарегистрирован в системе от {userFullDto.RegistrationDate}");
                return;
            }
            default:
            {
                if (parseData is not null)
                {
                    var sessionId = _sessionId!.Value;
                    var message = await _idccService.SendAnswerAsync(sessionId, parseData.Value.Item1, parseData.Value.Item2,
                        _questionTime);

                    if (message is not null)
                    {
                        await _telegramBot.SendTextMessageAsync(chat!, message, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                        return;
                    }
                    
                    _logger.LogInformation("Ответ отправлен!");
                }
                break;
            }
        }
    }
    
    
    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    (int, int)? ParseCallbackData(string data)
    {
        var regex = new Regex(_pattern);
        var match = regex.Match(data);
        if (!match.Success)
        {
            return null;
        }

        var questionId = Convert.ToInt32(match.Groups[0].Value);
        var answerId = Convert.ToInt32(match.Groups[1].Value);
        return (questionId, answerId);
    }
}