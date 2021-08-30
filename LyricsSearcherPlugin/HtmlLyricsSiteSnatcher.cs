using System;
using System.Collections.Generic;

using System.Globalization;

using System.Text.RegularExpressions;

using System.Net.Http;

namespace LyricsSearcherPlugin
{
    public class HtmlLyricsSiteSnatcher
    {
        public struct ListParameter
        {
            public string Url { get; set; }
            public string ParamFormat { get; set; }

            public string ListSelector { get; set; }
            public string UrlSelector { get; set; }
            public Regex TitleRegex { get; set; }
            public Regex ArtistRegex { get; set; }
        }

        public struct ListData
        {
            public string LyricsPageUrl { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
        }

        private static readonly HttpClient client = new HttpClient();

        public static ListData[] GetList(ListParameter param, string title, string artist)
        {
            title = Uri.EscapeUriString(title);
            artist = Uri.EscapeUriString(artist);

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
            string content = response.Content.ReadAsStringAsync().Result;

            List<ListData> lists = new List<ListData>();

            AngleSharp.Html.Parser.HtmlParser parser = new();
            var doc = parser.ParseDocument(content);
            var elems = doc.QuerySelectorAll(param.ListSelector);
            foreach (var elm in elems)
            {
                ListData list = new();
                var a = elm.QuerySelector(param.UrlSelector);
                if (a == null)
                    continue;
                list.LyricsPageUrl = param.Url + a.GetAttribute("href");
                string html = elm.InnerHtml;
                Match m = param.TitleRegex.Match(html);
                if (!m.Success)
                    continue;
                list.Title = m.Groups[1].Value;
                m = param.ArtistRegex.Match(html);
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
                public MatchEvaluator MatchEvaluator { get; set; }
            }
            public Replacer[] Replacers { get; set; }
        }



        public static string GetLyrics(string lyrics_page_url,LyricsParameter param)
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
            string content = response.Content.ReadAsStringAsync().Result;

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
                    lyrics = r.Regex.Replace(lyrics, r.MatchEvaluator);
                }
            }

            return lyrics;
        }
    }
}
