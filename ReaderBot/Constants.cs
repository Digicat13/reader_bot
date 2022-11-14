namespace ReaderBot
{
    public class Constants
    {
        public const string Token = "5776005980:AAHOaQwO6cHIgrMxSYDhlr1eUbabNhnTpT8";

        public const int PageSize = 5;
        public const int ColumnSize = 2;

        public const string NextPageText = "->";
        public const string PreviousPageText = "<-";

        public const int StarEmoji = 0x2B50;
        public const int RightwardsArrowEmoji = 0x27A1;
        public const int LeftwardsArrowEmoji = 0x2B05;

        public const string StartCommand = "/start";
        public const string InstructionCommand = "Допомога ❔";
        public const string BookListCommand = "Список прочитаного 📚";
        public const string SearchBookCommand = "Пошук 🔎";

        public const string HelloMessage = "Привіт! Я допоможу тобі знайти бажану електронну книгу! Для початку обери команду з меню.";
        public const string UserHelpMessage = @"Я бот для пошуку електронних книг. 
Для цього я використовую відкриту електронну бібліотеку. Щоб розпочати пошук обери команду з меню ""Пошук 🔎"" 
і введи назву книги. Також ти можеш додати книгу до особистого списку вже прочитаних книг. 
Для цього тобі потрібно лише натиснути біля знайденої книги команду ""Додати до прочитаного"". 
Аби переглянути свій список доданих книг обери команду з меню ""Список прочитаного 📚"".
Приємного користування ❤️";
        public const string NotFoundMessage = "Нічого не знайдено 😿";
    }
}
