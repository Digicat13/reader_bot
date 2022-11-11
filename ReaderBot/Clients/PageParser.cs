using ReaderBot.Models;
using System.Text.RegularExpressions;

namespace ReaderBot.Clients
{
    public static class PageParser
    {
        private const string Pattern = @"<a\shref=""(\/b\/)(?<book_id>[0-9]*)"">((<span|<b)(.*)>(?<title>.*)..(span>|b>))<\/a>\s-\s<\w*\s*href=""(\/a\/)((?<author_id>.*))"">(?<author_name>.*)(<\/a>)";

        public static IEnumerable<Book> GetBooks(string html)
        {
            var regex = new Regex(Pattern, RegexOptions.Multiline);

            var matches = regex.Matches(html);
           
            var result = matches
                .Select(m =>
                new Book
                {
                    Id = m.Groups["book_id"].Value,
                    Title = m.Groups["title"].Value,
                    Author = m.Groups["author_name"].Value,
                    AuthorId = m.Groups["author_id"].Value
                })
                .ToList();

            return result;
        }
    }
}
