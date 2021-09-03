using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace LyricsSearcherPlugin
{
    public class HtmlLyricsSiteList
    {

        public class Site
        {
            public Encoding Encoding = Encoding.UTF8;
            public HtmlLyricsSiteSnatcher.ListParameter ListParameter;
            public HtmlLyricsSiteSnatcher.LyricsParameter LyricsParameter;

        }

        public static Dictionary<string, Site> List = new();


        static HtmlLyricsSiteList()
        {
            List.Add("歌詞GET!!", 歌詞ＧＥＴ);
            List.Add("うたまっぷ", うたまっぷ);
        }



        public static Site 歌詞ＧＥＴ = new()
        {
            ListParameter = new HtmlLyricsSiteSnatcher.ListParameter()
            {
                Url = new("http://www.kget.jp"),
                ParamFormat = "/search/index.php?c=0&r={artist}&t={title}&v=&f=",
                ListBlockSelector = ".songlist",
                ListItemSelector = "li",
                ItemUrlSelector = ".lyric-anchor",
                ItemTitleRegex = new(@"<h2 class=""title"">([^<]+)</h2>"),
                ItemArtistRegex = new(@"<p class=""artist""><a href=[^>]+>([^<]+)</a></p>")
            },
            LyricsParameter = new HtmlLyricsSiteSnatcher.LyricsParameter()
            {
                BlockSelector = "#lyric-trunk",
                Replacers = new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer[]
                {
                    new(){Regex = new Regex(@"(^\s+|\r\n|\n|\r|\s+$)"), ReplaceText = ""},
                    new(){Regex = new Regex(@"<a[^>]*>.*?</a>"),ReplaceText = "" },
                    new(){Regex = new Regex(@"<br[^>]*>"),ReplaceText = "\n" },
                    new(){Regex = new Regex(@"<[^>]*>"),ReplaceText = "" },
                }
            }
        };


        public static Site うたまっぷ = new()
        {
            //受信はeuc-jpなのに送信はshift-jisとか意味が分からない
            Encoding = CodePagesEncodingProvider.Instance.GetEncoding("shift-jis"),
            ListParameter = new HtmlLyricsSiteSnatcher.ListParameter()
            {
                Url = new("https://www.utamap.com"),
                ParamFormat = "/searchkasi.php?searchname=title&word={title}&act=search&search_by_keyword=%8C%9F%26%23160%3B%26%23160%3B%26%23160%3B%8D%F5&sortname=1&pattern=3",
                ListBlockSelector = "body > table:nth-child(4) > tbody > tr > td:nth-child(1) > table:nth-child(1)",
                ListItemSelector = "tr",
                ItemUrlSelector = "a",

                ItemTitleRegex = new(@"<a href=""[^""]+"">(.+)</a>"),
                ItemArtistRegex = new(@"^\s*<td.*?</td>\s*<td[^>]*>(.*)</td>")
            },
            LyricsParameter = new HtmlLyricsSiteSnatcher.LyricsParameter()
            {
                BlockSelector = ".kasi_honbun",
                Replacers = new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer[]
                {
                    new(){Regex= new(@"(^\s+|\r\n|\n|\r|\s+$)"),ReplaceText=""},
                    new(){Regex= new(@"<br[^>]*>"),ReplaceText="\n"},
                    new(){Regex= new(@"<[^>]*>"),ReplaceText=""},
                }
            }
        };

    }
}
