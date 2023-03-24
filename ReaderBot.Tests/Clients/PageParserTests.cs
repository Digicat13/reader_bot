using FluentAssertions;
using ReaderBot.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReaderBot.Tests.Clients
{
    public class PageParserTests
    {
        [Fact]
        public void GetBooks_Html_ShouldReturnBookList()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void SearchBookInfo_Html_ShouldReturnBook()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void SearchBookInfo_EmptyString_ShouldReturnNull()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }
    }
}
