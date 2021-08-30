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
            public HtmlLyricsSiteSnatcher.ListParameter ListParameter;
            public HtmlLyricsSiteSnatcher.LyricsParameter LyricsParameter;

        }

        public static Dictionary<string, Site> List = new();


        static HtmlLyricsSiteList()
        {
            List.Add("歌詞GET!!", 歌詞ＧＥＴ);
        }



        public static Site 歌詞ＧＥＴ = new()
        {
            ListParameter = new HtmlLyricsSiteSnatcher.ListParameter()
            {
                Url = "http://www.kget.jp",
                ParamFormat = "/search/index.php?c=0&r={artist}&t={title}&v=&f=",
                ListSelector = ".songlist > li",
                UrlSelector = ".lyric-anchor",
                TitleRegex = new(@"<h2 class=""title"">([^<]+)</h2>"),
                ArtistRegex = new(@"<p class=""artist""><a href=[^>]+>([^<]+)</a></p>")
            },
            LyricsParameter = new HtmlLyricsSiteSnatcher.LyricsParameter()
            {
                BlockSelector = "#lyric-trunk",
                Replacers = new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer[]
                {
                    new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer()
                    {
                        Regex = new Regex(@"(^\s+|\r\n|\n|\r|\s+$)"),
                        MatchEvaluator = (m)=>
                        {
                            return "";
                        }
                    },
                    new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer()
                    {
                        Regex = new Regex(@"<a[^>]*>.*?</a>"),
                        MatchEvaluator = (m)=>
                        {
                            return "";
                        }
                    },
                    new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer()
                    {
                        Regex = new Regex(@"<br[^>]*>"),
                        MatchEvaluator = (m)=>
                        {
                            return "\n";
                        }
                    },
                    new HtmlLyricsSiteSnatcher.LyricsParameter.Replacer()
                    {
                        Regex = new Regex(@"<[^>]*>"),
                        MatchEvaluator = (m)=>
                        {
                            return "";
                        }
                    }
                }
            }
        };

    }
}
