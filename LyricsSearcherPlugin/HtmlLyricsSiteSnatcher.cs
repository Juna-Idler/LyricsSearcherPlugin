using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

using System.Text.RegularExpressions;
using System.Web;

using System.Net.Http;

namespace LyricsSearcherPlugin
{
    public class HtmlLyricsSiteSnatcher
    {
        public struct ListParameter
        {
            public Uri Url { get; set; }
            public string ParamFormat { get; set; }

            public string ListBlockSelector { get; set; }
            public string ListItemSelector { get; set; }
            public string ItemUrlSelector { get; set; }
            public Regex ItemTitleRegex { get; set; }
            public Regex ItemArtistRegex { get; set; }
        }

        public struct ListData
        {
            public Uri LyricsPageUrl { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
        }

        private static readonly HttpClient client = new HttpClient();

        public static ListData[] GetList(ListParameter param, string title, string artist,Encoding encodingg)
        {
            title = HttpUtility.UrlEncode(title, encodingg);
            artist = HttpUtility.UrlEncode(artist, encodingg);

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

            Encoding content_encoding = CodePagesEncodingProvider.Instance.GetEncoding(response.Content.Headers.ContentType.CharSet);
            Stream stream = response.Content.ReadAsStream();
            StreamReader sr = new(stream, content_encoding);
            string content = sr.ReadToEnd();

            List<ListData> lists = new List<ListData>();

            AngleSharp.Html.Parser.HtmlParser parser = new();
            var doc = parser.ParseDocument(content);
            var block = doc.QuerySelector(param.ListBlockSelector);
            if (block == null)
                return null;
            var items = block.QuerySelectorAll(param.ListItemSelector);
            foreach (var elm in items)
            {
                ListData list = new();
                var a = elm.QuerySelector(param.ItemUrlSelector);
                if (a == null)
                    continue;
                list.LyricsPageUrl = new Uri(param.Url, a.GetAttribute("href"));
                string html = elm.InnerHtml;
                Match m = param.ItemTitleRegex.Match(html);
                if (!m.Success)
                    continue;
                list.Title = m.Groups[1].Value;
                m = param.ItemArtistRegex.Match(html);
                if (!m.Success)
                    continue;
                list.Artist = m.Groups[1].Value;
                lists.Add(list);
            }
            return lists.ToArray();
        }



        public struct LyricsParameter
        {
            public string BlockSelector { get; set; }

            public struct Replacer
            {
                public Regex Regex { get; set; }
                public string ReplaceText { get; set; }
            }
            public Replacer[] Replacers { get; set; }
        }



        public static string GetLyrics(Uri lyrics_page_url, LyricsParameter param)
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
            Encoding content_encoding = CodePagesEncodingProvider.Instance.GetEncoding(response.Content.Headers.ContentType.CharSet);
            Stream stream = response.Content.ReadAsStream();
            StreamReader sr = new(stream, content_encoding);
            string content = sr.ReadToEnd();

            AngleSharp.Html.Parser.HtmlParser parser = new();
            var doc = parser.ParseDocument(content);
            var block = doc.QuerySelector(param.BlockSelector);
            if (block == null)
                return null;
            string lyrics = block.InnerHtml;
            if (param.Replacers != null)
            {
                foreach (var r in param.Replacers)
                {
                    lyrics = r.Regex.Replace(lyrics, r.ReplaceText);
                }
            }

            return lyrics;
        }
    }

}

