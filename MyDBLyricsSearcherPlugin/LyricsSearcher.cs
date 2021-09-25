
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using System.Web;
using System.Net.Http;

using System.Text.Json;


//アセンブリ名はユニークなものへ変更
//
//ここの名前空間名、クラス名、メソッド名は全てこのまま固定

namespace Titalyver2
{
    public class LyricsSearcher
    {


        static private string url = "https://juna.npkn.net/get-lyrics/";

        private static readonly HttpClient client = new HttpClient();

        public string[] Search(string title,
                               string[] artists,
                               string album,
                               string path,     //ファイルの場合ファイルのパス
                               string param)    //SeachListに書かれたパラメータ
        {


            title = HttpUtility.UrlEncode(title);
            string artist = HttpUtility.UrlEncode(string.Join(" ",artists));

            string url_param = $"?title={title}&artist={artist}";

            HttpResponseMessage response;
            try
            {
                response = client.GetAsync(url + url_param).Result;
            }
            catch (Exception e)
            {
                return null;
            }
            string content_string = response.Content.ReadAsStringAsync().Result;
            byte[] content = response.Content.ReadAsByteArrayAsync().Result;

            using JsonDocument document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }
            List<string> result = new();

            int i = 0;
            foreach (JsonElement a in document.RootElement.EnumerateArray())
            {
                string db_title;
                string db_artist;
                string db_album;
                string db_comment;
                string db_sync;
                string db_url = "";
                string db_picker = null;
                string[] db_replacers = null;
                foreach (JsonProperty e in a.EnumerateObject())
                {
                    switch (e.Name.ToLowerInvariant())
                    {
                        case "title":
                            db_title = e.Value.GetString();
                            break;
                        case "artist":
                            db_artist = e.Value.GetString();
                            break;
                        case "album":
                            db_album = e.Value.GetString();
                            break;
                        case "comment":
                            db_comment = e.Value.GetString();
                            break;
                        case "sync":
                            db_sync = e.Value.GetString();
                            break;
                        case "url":
                            db_url = e.Value.GetString();
                            break;
                        case "picker":
                            db_picker = e.Value.GetString();
                            break;
                        case "replacers":
                            db_replacers = e.Value.EnumerateArray().Select(r => r.GetString()).ToArray();
                            break;
                    }
                }
                string r = Scrape(db_url, db_picker, db_replacers);
                if (!string.IsNullOrEmpty(r))
                    result.Add(r);
            }

            return result.ToArray();
        }

            static public string Scrape(string url, string picker, string[] replacers)
            {
                HttpResponseMessage response;
                try
                {
                    response = client.GetAsync(url).Result;
                }
                catch (Exception e)
                {
                    return null;
                }
                string lyrics = response.Content.ReadAsStringAsync().Result;

                if (picker != null)
                {
                    Match m = Regex.Match(lyrics, picker, RegexOptions.Singleline);
                    if (!m.Success)
                        return null;
                    lyrics = m.Groups[1].Value;
                }

                if (replacers != null)
                {
                    foreach (string r in replacers)
                    {
                        string[] replacer = r.Split('\0');
                        if (replacer.Length != 2)
                            continue;
                        lyrics = Regex.Replace(lyrics, replacer[0], replacer[1], RegexOptions.Singleline);
                    }
                }
                return lyrics;
            }

        }
    }
