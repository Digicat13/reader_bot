using ReaderBot.Interfaces;
using ReaderBot.Models;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReaderBot
{
    //TODO move consts to file
    public class BotClient : BackgroundService
    {
        private const string Token = "5776005980:AAHOaQwO6cHIgrMxSYDhlr1eUbabNhnTpT8";

        private TelegramBotClient _client;
        private readonly ILibraryClient _libraryClient;
        private readonly Dictionary<long, string> _userInputMap;

        public BotClient(ILibraryClient libraryClient)
        {
            _libraryClient = libraryClient;
            _client = new TelegramBotClient(Token);
            _userInputMap = new();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Connect();
        }


        public async Task Connect()
        {
            using var cts = new CancellationTokenSource();

            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: new()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                },
                cancellationToken: cts.Token
            );

            var me = await _client.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message


            if (update.CallbackQuery is { } callback)
            {
                await HandleUpdateCallbackASync(botClient, callback.Message.Chat.Id, callback.Data, cancellationToken);
            }

            if (update.Message is not { } message)
            {
                return;
            }
            // Only process text messages
            if (message.Text is not { } messageText)
            {
                return;
            }

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            _userInputMap[chatId] = messageText;
            var pageNumber = 1;
            await SendMessageAsync(botClient, messageText, chatId, pageNumber, cancellationToken);
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task SendMessageAsync(ITelegramBotClient botClient, string input, long chatId, int pageNumber, CancellationToken cancellationToken)
        {
            var books = await _libraryClient.SearchBooksAsync(input);
            var list = books
                .Skip((pageNumber - 1) * Constants.PageSize)
                .Take(Constants.PageSize)
                .ToList();

            List<KeyboardButton[]> buttons = new();

            var text = "";
            if (list.Count == 0)
            {
                text = "Нічого не знайдено :(";
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    text += $"{(pageNumber - 1) * Constants.PageSize + i + 1}: {list[i].Title} - {list[i].Author} \n\n";
                }
            }

            var inlineButtons = list
                .Select((book, index) => InlineKeyboardButton
                    .WithCallbackData(
                    text: ((pageNumber - 1) * Constants.PageSize + index + 1).ToString(),
                    callbackData: JsonSerializer.Serialize(new DownloadCallback() { Type = CallbackType.Download, BookId = book.Id })))
                .ToList();

            var buttonsTable = new List<List<InlineKeyboardButton>>();

            buttonsTable.Add(inlineButtons);


            var totalPages = Math.Ceiling((double)books.Count() / Constants.PageSize);
            var row = new List<InlineKeyboardButton>();
            if (pageNumber > 1)
            {
                row.Add(InlineKeyboardButton.WithCallbackData(
                    text: Constants.PreviousPageText,
                    callbackData: JsonSerializer.Serialize(new NavigationCallback() { Type = CallbackType.PreviousPage, PageNumber = pageNumber - 1 })));
            }
            if (pageNumber < totalPages)
            {
                row.Add(InlineKeyboardButton.WithCallbackData(
                    text: Constants.NextPageText,
                    callbackData: JsonSerializer.Serialize(new NavigationCallback() { Type = CallbackType.NextPage, PageNumber = pageNumber + 1 })));
            }
            buttonsTable.Add(row);

            var inlineKeyboard = new InlineKeyboardMarkup(buttonsTable);

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        private async Task HandleUpdateCallbackASync(ITelegramBotClient botClient, long chatId, string callbackData, CancellationToken cancellationToken)
        {
            var callback = JsonSerializer.Deserialize<Callback>(callbackData);
            var input = _userInputMap[chatId];

            switch (callback.Type)
            {
                case CallbackType.NextPage:
                case CallbackType.PreviousPage:
                    var navigationCallback = JsonSerializer.Deserialize<NavigationCallback>(callbackData);
                    await SendMessageAsync(botClient, input, chatId, navigationCallback.PageNumber, cancellationToken);
                    break;
                default:
                    await SendBookFileAsync(botClient, chatId, callbackData, cancellationToken);
                    break;
            }
        }

        private async Task SendBookFileAsync(ITelegramBotClient botClient, long chatId, string callbackData, CancellationToken cancellationToken)
        {
            const string format = "fb2";

            var callback = JsonSerializer.Deserialize<DownloadCallback>(callbackData);
            var input = _userInputMap[chatId];

            using var fileStream = await _libraryClient.DownloadBookAsync(callback.BookId, format, cancellationToken);

            await botClient.SendDocumentAsync(
               chatId: chatId,
               document: new InputOnlineFile(fileStream, $"{input}.{format}"));
        }
    }
}
