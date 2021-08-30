using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using System.Globalization;

using System.Net.Http;
using System.Web;


namespace Titalyver2
{
    public static class タイムタグ情報DataBase
    {
        private static readonly string url = "http://timetag.main.jp/herodb/herodb.cgi?table=timetag&";

        private static readonly Encoding ShiftJis = CodePagesEncodingProvider.Instance.GetEncoding("shift-jis");


        public struct SearchOrder
        {
            public int Hit { get; set; }
            public string Key { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Website { get; set; }
        }



        private static readonly HttpClient client = new HttpClient();

        public static SearchOrder[] Search(string title, string artist, string website, string album)
        {
            title = HttpUtility.UrlEncode(title,ShiftJis);
            artist = HttpUtility.UrlEncode(artist, ShiftJis);
            website = HttpUtility.UrlEncode(website, ShiftJis);
            album = HttpUtility.UrlEncode(album, ShiftJis);

            HttpResponseMessage response;
            try
            {
                string param = $"MODE=Search&SEARCH={title}%3c%3e{artist}%3c%3e{website}%3c%3e{album}&ORDER=hit+key+title+artist+website";
                response = client.GetAsync(url + param).Result;
            }
            catch (Exception e)
            {
                return null;
            }
            Stream stream = response.Content.ReadAsStream();
            StreamReader sr = new(stream, ShiftJis);
            string content = sr.ReadToEnd();

            int start = content.IndexOf("success", StringComparison.InvariantCulture);
            if (start < 0)
                return null;

            string[] lines = content[(start + 9)..].Split("\r\n");
            List<SearchOrder> result = new();
            SearchOrder current = new();
            foreach (string line in lines)
            {
                if (line.IndexOf("[EOM]", StringComparison.InvariantCulture) == 0)
                {
                    result.Add(current);
                    break;
                }

                if (line.IndexOf("Rank", StringComparison.InvariantCulture) == 0)
                {
                    result.Add(current);
                    current = new SearchOrder();
                    continue;
                }
                string[] pair = line.Split('=');
                switch (pair[0].ToLower(CultureInfo.InvariantCulture))
                {
                    case "hit":
                        current.Hit = int.Parse(pair[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "key":
                        current.Key = pair[1];
                        break;
                    case "title":
                        current.Title = pair[1];
                        break;
                    case "artist":
                        current.Artist = pair[1];
                        break;
                    case "website":
                        current.Website = pair[1];
                        break;
                }
            }
            result.RemoveAt(0);
            return result.ToArray();
        }

        public static string Get(string key)
        {
            HttpResponseMessage response;
            try
            {
                response = client.GetAsync(url + $"MODE=Get&KEY={key}&ORDER=timtetag").Result;
            }
            catch (Exception e)
            {
                return null;
            }
            string content = response.Content.ReadAsStringAsync().Result;

            int start = content.IndexOf("success", StringComparison.InvariantCulture);
            if (start < 0)
                return null;

            int timetag_start = content.IndexOf("TIMETAG=", start, StringComparison.InvariantCulture);
            if (timetag_start < 0)
                return null;
            int timetag_end = content.IndexOf("[EOM]", timetag_start, StringComparison.InvariantCulture);
            if (timetag_end < 0)
                return null;

            return content[(timetag_start + 8)..timetag_end];
        }

        public static string Apply(string key ,string lyrics)
        {
            lyrics = HttpUtility.UrlEncode(lyrics, ShiftJis);

            HttpResponseMessage response;
            try
            {
                string param = $"MODE=Apply&KEY={key}&TIMETAG={lyrics}";
                response = client.GetAsync(url + param).Result;
            }
            catch (Exception e)
            {
                return null;
            }
            Stream stream = response.Content.ReadAsStream();
            StreamReader sr = new(stream, ShiftJis);
            string content = sr.ReadToEnd();

            int timetag_start = content.IndexOf("TIMETAG=", StringComparison.InvariantCulture);
            if (timetag_start < 0)
                return null;
            int timetag_end = content.IndexOf("[EOM]", timetag_start, StringComparison.InvariantCulture);
            if (timetag_end < 0)
                return null;

            return content[(timetag_start + 8)..timetag_end];
        }
    }
}
