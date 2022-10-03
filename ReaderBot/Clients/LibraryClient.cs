using ReaderBot.Interfaces;
using ReaderBot.Models;

namespace ReaderBot.Clients
{
    public class LibraryClient : ILibraryClient
    {

        public const string ApiUrl = "http://flibusta.is/";
        public const string DonwloadUrl = "http://flibusta.is/booksearch?ask=";

        private readonly HttpClient _httpClient;

        public LibraryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string input, CancellationToken cancellationToken = default)
        {
            var url = $"{ApiUrl}booksearch?ask={input}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var html = await response.Content.ReadAsStringAsync(cancellationToken);

                var result = PageParser.GetBooks(html);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Book>();
            }
        }

        public async Task<Stream> DownloadBookAsync(string bookId, string format, CancellationToken cancellationToken = default)
        {
            var url = $"{ApiUrl}b/{bookId}/{format}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var file = response.Content.ReadAsStream(cancellationToken);

                return file;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
            }
        }
    }
}
