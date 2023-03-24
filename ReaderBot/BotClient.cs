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
    public class BotClient : BackgroundService
    {
        private const string Token = "5776005980:AAHOaQwO6cHIgrMxSYDhlr1eUbabNhnTpT8";


        private TelegramBotClient _client;
        private readonly ILibraryClient _libraryClient;
        private readonly Dictionary<long, string> _userInputMap;
        private readonly Dictionary<long, string> _userBookListMap;

        public BotClient(ILibraryClient libraryClient)
        {
            _libraryClient = libraryClient;
            _client = new TelegramBotClient(Token);
            _userInputMap = new();
            _userBookListMap = new();
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

            if (update.CallbackQuery is { } callback)
            {
                await HandleUpdateCallbackASync(botClient, callback, cancellationToken);
            }

            if (update.Message is not { } message)
            {
                return;
            }

            if (message.Text is not { } messageText)
            {
                return;
            }

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");


            switch (message.Text)
            {
                case Constants.StartCommand:
                    await SendHelloMessageAsync(botClient, chatId, cancellationToken);
                    break;
                case Constants.InstructionCommand:
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: Constants.UserHelpMessage,
                    cancellationToken: cancellationToken);
                    break;
                case Constants.BookListCommand:
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: _userBookListMap[chatId],
                    cancellationToken: cancellationToken);
                    break;
                case Constants.SearchBookCommand:
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Введи назву книжки для пошуку",
                        cancellationToken: cancellationToken);
                    break;
                default:
                    var pageNumber = 1;
                    _userInputMap[chatId] = messageText;
                    await SendBookSearchResultAsync(botClient, messageText, chatId, pageNumber, cancellationToken);
                    break;
            }
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

        private async Task SendBookSearchResultAsync(ITelegramBotClient botClient, string input, long chatId, int pageNumber, CancellationToken cancellationToken)
        {
            var books = await _libraryClient.SearchBooksAsync(input);
            var list = books
                .Skip((pageNumber - 1) * Constants.PageSize)
                .Take(Constants.PageSize)
                .ToList();

            var text = "";
            if (list.Count == 0)
            {
                text = Constants.NotFoundMessage;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    text += $"{(pageNumber - 1) * Constants.PageSize + i + 1}: <i><b>{list[i].Title}</b> - {list[i].Author}</i>\n    {list[i].Genre}\n\n";
                }
            }

            List<KeyboardButton[]> buttons = new();

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
                    text: char.ConvertFromUtf32(Constants.LeftwardsArrowEmoji),
                    callbackData: JsonSerializer.Serialize(new NavigationCallback() { Type = CallbackType.PreviousPage, PageNumber = pageNumber - 1 })));
            }
            if (pageNumber < totalPages)
            {
                row.Add(InlineKeyboardButton.WithCallbackData(
                    text: char.ConvertFromUtf32(Constants.RightwardsArrowEmoji),
                    callbackData: JsonSerializer.Serialize(new NavigationCallback() { Type = CallbackType.NextPage, PageNumber = pageNumber + 1 })));
            }
            buttonsTable.Add(row);

            var inlineKeyboard = new InlineKeyboardMarkup(buttonsTable);

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken,
                parseMode: ParseMode.Html);
        }

        private async Task HandleUpdateCallbackASync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var callback = JsonSerializer.Deserialize<Callback>(callbackQuery.Data);
            var chatId = callbackQuery.Message.Chat.Id;

            if (!_userInputMap.TryGetValue(chatId, out string input))
            {
                return;
            }

            switch (callback.Type)
            {
                case CallbackType.NextPage:
                case CallbackType.PreviousPage:
                    var navigationCallback = JsonSerializer.Deserialize<NavigationCallback>(callbackQuery.Data);
                    await SendBookSearchResultAsync(botClient, input, chatId, navigationCallback.PageNumber, cancellationToken);
                    break;
                case CallbackType.AddToList:
                    await AddBookToListAsync(chatId, callbackQuery.Data, cancellationToken);
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Успішно додано {char.ConvertFromUtf32(Constants.StarEmoji)}");
                    break;
                default:
                    await SendBookInfoAsync(botClient, chatId, callbackQuery.Data, cancellationToken);
                    break;
            }
        }

        private async Task SendBookInfoAsync(ITelegramBotClient botClient, long chatId, string callbackData, CancellationToken cancellationToken)
        {
            const string format = "fb2";

            var callback = JsonSerializer.Deserialize<DownloadCallback>(callbackData);

            var book = await _libraryClient.GetBookInfoByIdAsync(callback.BookId, cancellationToken);

            using var fileStream = await _libraryClient.DownloadBookByUrlAsync(book.DonwloadUrl, book.CookieValue, cancellationToken);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Додати до прочитаного", callbackData:  JsonSerializer.Serialize(new AddToListCallback() { Type = CallbackType.AddToList, BookId = callback.BookId }))
                }
            });

            var text = $"{book.Title} - {book.Author}\n\n <i>{book.Description}</i>";

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                cancellationToken: cancellationToken,
                parseMode: ParseMode.Html);

            await botClient.SendDocumentAsync(
               chatId: chatId,
               document: new InputOnlineFile(fileStream, $"{book.Title} - {book.Author}.{format}"),
               replyMarkup: inlineKeyboard);
        }

        private async Task SendHelloMessageAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {

            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] {
                    Constants.InstructionCommand,
                    Constants.BookListCommand,
                    Constants.SearchBookCommand},
            })
            {
                ResizeKeyboard = true
            };

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: Constants.HelloMessage,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        private async Task AddBookToListAsync(long chatId, string callbackData, CancellationToken cancellationToken)
        {
            var callback = JsonSerializer.Deserialize<AddToListCallback>(callbackData);

            var book = await _libraryClient.GetBookInfoByIdAsync(callback.BookId, cancellationToken);

            if(_userBookListMap.ContainsKey(chatId))
            {
                _userBookListMap[chatId] += $"{book.Title} - {book.Author}\n\n";
            }
            else
            {
                _userBookListMap.Add(chatId, $"{book.Title} - {book.Author}\n\n");
            }
           
        }

        private async Task SendBookFileAsync(ITelegramBotClient botClient, long chatId, string callbackData, CancellationToken cancellationToken)
        {
            const string format = "fb2";

            var callback = JsonSerializer.Deserialize<DownloadCallback>(callbackData);
            var input = _userInputMap[chatId];

            using var fileStream = await _libraryClient.DownloadBookAsync(callback.BookId, format, cancellationToken);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Додати до прочитаного", callbackData:  JsonSerializer.Serialize(new AddToListCallback() { Type = CallbackType.AddToList, BookId = callback.BookId }))
                }
            });

            await botClient.SendDocumentAsync(
               chatId: chatId,
               document: new InputOnlineFile(fileStream, $"{input}.{format}"),
               replyMarkup: inlineKeyboard);
        }

        private async Task GetBookInfoByIdAsync(ITelegramBotClient botClient, long chatId, string callbackData, CancellationToken cancellationToken)
        {
            const string format = "fb2";

            var callback = JsonSerializer.Deserialize<DownloadCallback>(callbackData);
            var input = _userInputMap[chatId];

            using var fileStream = await _libraryClient.DownloadBookAsync(callback.BookId, format, cancellationToken);
        }
    }
}
