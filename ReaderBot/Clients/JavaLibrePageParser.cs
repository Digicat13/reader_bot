using ReaderBot.Models;
using System.Text.RegularExpressions;

namespace ReaderBot.Clients
{
    public static class JavaLibrePageParser
    {
        private const string Pattern = @"<div\sclass=""genrhru"">([\n\r\s]*)<a\sclass=""booklink""\shref=""(\/java-book\/book\/)(?<book_id>[0-9]*)"">(?<title>.*)<\/a>([\n\r\s]*)<span(.*)>(.*)<\/span>([\n\r\s]*)<br>([\n\r\s]*)<a(.*)>(?<genre>.*)<\/a>([\n\r\s]*)<br>([\n\r\s]*)<a\s(.*)\/author\/(?<author_id>[0-9]*)"">(?<author_name>.*)<\/a>([\n\r\s]*)<\/div>([\n\r\s]*)<div\sclass=""clear"">([\n\r\s]*)<\/div>";
        private const string BookPagePattern2 = @"<div\sid=""book_page""(.*\/Book"")>([\n\r\s]*)<div>([\n\r\s]*)<h2\sclass=""booktitle(.*)"" itemprop=""name"">(?<title>.*)<\/h2>([\n\r\s]*)<a\shref=""\/java-book(.*)""\sitemprop=""genre"">(?<genre>.*)<\/a>\/<a\shref=""\/java-book\/author\/(?<author_id>[0-9]*)""\sitemprop=""author"">(?<author_name>.*)<\/a><br>(\n|.)*<div\sitemprop=""description"">([\n\r\s]*)?(?<description>(\n|.)*?)<\/div>";
        private const string DownloadLinkPattern = @"<a\shref=""(?<link>.*fb2)""";
        private const string CookiePattern = @"(window.cookie_string\s=\s""(?<cookie_string>.*)"";)";

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
                    AuthorId = m.Groups["author_id"].Value,
                    Genre = m.Groups["genre"].Value
                })
                .ToList();

            return result;
        }

        public static Book GetBookInfo(string html)
        {
            var regex = new Regex(BookPagePattern2, RegexOptions.Multiline);

            var matches = regex.Matches(html);

            var result = matches
                .Select(m =>
                new Book
                {
                    Id = m.Groups["book_id"].Value,
                    Title = m.Groups["title"].Value,
                    Author = m.Groups["author_name"].Value,
                    AuthorId = m.Groups["author_id"].Value,
                    Description = m.Groups["description"].Value.Replace("<br />", "").Replace("<p>", "").Replace("</p>", ""),
                    Genre = m.Groups["genre"].Value
                })
                .FirstOrDefault();

            result.DonwloadUrl = GetBookUrl(html);
            result.CookieValue = GetBookCookie(html);

            return result;
        }

        private static string GetBookUrl(string html)
        {
            var regex = new Regex(DownloadLinkPattern, RegexOptions.Multiline);

            var matches = regex.Matches(html);

            var result = matches
                .Select(m => m.Groups["link"].Value)
                .FirstOrDefault();

            return result;
        }

        private static string GetBookCookie(string html)
        {
            var regex = new Regex(CookiePattern, RegexOptions.Multiline);

            var matches = regex.Matches(html);

            var result = matches
                .Select(m => m.Groups["cookie_string"].Value)
                .FirstOrDefault();

            return result;
        }

    }
}
