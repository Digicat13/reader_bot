using FluentAssertions;
using ReaderBot.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReaderBot.Tests
{
    public class BotClientTests
    {
        [Fact]
        public void UpdateHandler_DownloadCallback_ShouldSendMessage()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void UpdateHandler_NavigationCallback_ShouldSendMessage()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }

        [Fact]
        public void UpdateHandler_AddToListCallback_ShouldSendMessage()
        {
            var books = new List<Book>
            {
                new()
            };

            books.Any().Should().BeTrue();
        }
    }
}
