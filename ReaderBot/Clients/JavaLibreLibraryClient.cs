using ReaderBot.Interfaces;
using ReaderBot.Models;

namespace ReaderBot.Clients
{
    public class JavaLibreLibraryClient : ILibraryClient
    {
        public const string ApiUrl = "https://javalibre.com.ua";
        public const string DonwloadUrl = "http://flibusta.is/booksearch?ask=";
        public const string SearchUrl = "https://javalibre.com.ua/extended_search/results/1?bookname=";

        private readonly HttpClient _httpClient;

        public JavaLibreLibraryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string input, CancellationToken cancellationToken = default)
        {
            var url = $"{SearchUrl}{input}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var html = await response.Content.ReadAsStringAsync(cancellationToken);

                var result = JavaLibrePageParser.GetBooks(html);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Book>();
            }
        }

        public async Task<Book> GetBookInfoByIdAsync(string bookId, CancellationToken cancellationToken = default)
        {
            var url = $"{ApiUrl}/java-book/book/{bookId}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var html = await response.Content.ReadAsStringAsync(cancellationToken);

                var result = JavaLibrePageParser.GetBookInfo(html);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new Book();
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

        public async Task<Stream> DownloadBookByUrlAsync(string url, string cookieValue = null, CancellationToken cancellationToken = default)
        {
            var downloadUrl = $"{ApiUrl}{url}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Add("Cookie", $"java-book={cookieValue}"); 

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var temp = await response.Content.ReadAsStringAsync();
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
