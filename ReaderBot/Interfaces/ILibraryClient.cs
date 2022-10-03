using ReaderBot.Models;

namespace ReaderBot.Interfaces
{
    public interface ILibraryClient
    {
        public Task<IEnumerable<Book>> SearchBooksAsync(string input, CancellationToken cancellationToken = default);

        public Task<Stream> DownloadBookAsync(string bookId, string format, CancellationToken cancellationToken = default);
    }
}
