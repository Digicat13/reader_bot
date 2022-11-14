using ReaderBot.Models;

namespace ReaderBot.Interfaces
{
    public interface ILibraryClient
    {
        public Task<IEnumerable<Book>> SearchBooksAsync(string input, CancellationToken cancellationToken = default);

        public Task<Stream> DownloadBookAsync(string bookId, string format, CancellationToken cancellationToken = default);

        public Task<Stream> DownloadBookByUrlAsync(string url, string cookieValue = null, CancellationToken cancellationToken = default);

        public Task<Book> GetBookInfoByIdAsync(string bookId, CancellationToken cancellationToken = default);
    }
}
