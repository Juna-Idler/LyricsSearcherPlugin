using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

using System.Text.RegularExpressions;
using System.Web;

using System.Net.Http;


namespace LyricsSearcherPlugin
{
    public class HtmlLyricsSiteScraper
    {
        public class SiteParameter
        {
            public string Name { get; set; }

            public string Encoding { get; set; }

            public Uri Url { get; set; }
            public string ParamFormat { get; set; }

            public string ListBlockRegex { get; set; }
            public string ListItemRegex { get; set; }
            public string ItemUrlRegex { get; set; }
            public string ItemTitleRegex { get; set; }
            public string ItemArtistRegex { get; set; }


            public string LyricsBlockRegex { get; set; }

            public struct Replacer
            {
                public string Regex { get; set; }
                public string ReplaceText { get; set; }
            }
            public Replacer[] LyricsReplacers { get; set; }
        }



        public struct ListData
        {
            public Uri LyricsPageUrl { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
        }

        private static readonly HttpClient client = new HttpClient();

        static HtmlLyricsSiteScraper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static ListData[] GetList(SiteParameter param, string title, string artist)
        {
            Encoding encoding = Encoding.GetEncoding(param.Encoding);

            title = HttpUtility.UrlEncode(title, encoding);
            artist = HttpUtility.UrlEncode(artist, encoding);

            string url_param = param.ParamFormat.Replace("{title}", title).Replace("{artist}", artist);

            HttpResponseMessage response;
            try
            {
                response = client.GetAsync(param.Url + url_param).Result;
            }
            catch (Exception e)
            {
                return null;
            }

            Encoding content_encoding = Encoding.GetEncoding(response.Content.Headers.ContentType.CharSet);
            using Stream stream = response.Content.ReadAsStream();
            using StreamReader sr = new(stream, content_encoding);
            string content = sr.ReadToEnd();

            List<ListData> lists = new();

            Match block = Regex.Match(content, param.ListBlockRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!block.Success)
                return null;
            var items = Regex.Matches(block.Groups[1].Value, param.ListItemRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            foreach (Match elm in items)
            {
                string item = elm.Groups[1].Value;
                ListData list = new();
                Match m = Regex.Match(item, param.ItemUrlRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (!m.Success)
                    continue;
                list.LyricsPageUrl = new Uri(param.Url, m.Groups[1].Value);
                m = Regex.Match(item, param.ItemTitleRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (!m.Success)
                    continue;
                list.Title = HttpUtility.HtmlDecode(m.Groups[1].Value.Trim());
                m = Regex.Match(item, param.ItemArtistRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (!m.Success)
                    continue;
                list.Artist = HttpUtility.HtmlDecode(m.Groups[1].Value.Trim());
                lists.Add(list);
            }
            return lists.ToArray();
        }

        public static string GetLyrics(Uri lyrics_page_url, SiteParameter param)
        {
            HttpResponseMessage response;
            try
            {
                response = client.GetAsync(lyrics_page_url).Result;
            }
            catch (Exception e)
            {
                return null;
            }
            Encoding content_encoding = Encoding.GetEncoding(response.Content.Headers.ContentType.CharSet);
            using Stream stream = response.Content.ReadAsStream();
            using StreamReader sr = new(stream, content_encoding);
            string content = sr.ReadToEnd();

            Match block = Regex.Match(content,param.LyricsBlockRegex, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!block.Success)
                return null;
            string lyrics = block.Groups[1].Value;
            if (param.LyricsReplacers != null)
            {
                foreach (var r in param.LyricsReplacers)
                {
                    lyrics = Regex.Replace(lyrics, r.Regex, r.ReplaceText, RegexOptions.Singleline);
                }
            }

            return HttpUtility.HtmlDecode(lyrics);

        }



    }
}
