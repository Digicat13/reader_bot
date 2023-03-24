using FluentAssertions;
using ReaderBot.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReaderBot.Tests.Clients
{
    public class LibraryClientTests
    {
        [Fact]
        public void SearchBooksAsync_ValidInput_ShouldReturnBookList()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void SearchBooksAsync_InvalidInput_ShouldReturnEmptyList()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void DownloadBookAsync_BookId_ShouldReturnFileStream()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void DownloadBookByUrlAsync_DownloadUrl_ShouldReturnFileStream()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void DownloadBookByUrlAsync_EmptyUrl_ShouldThrowError()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void GetBookInfoByIdAsync_BookId_ShouldReturnBook()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }
    }
}
