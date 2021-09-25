using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

using System.IO;
using System.Text.Json;

using LyricsSearcherPlugin;

//アセンブリ名はユニークなものへ変更
//
//ここの名前空間名、クラス名、メソッド名は全てこのまま固定

namespace Titalyver2
{
    public class LyricsSearcher
    {
        private string JsonPath;
        private HtmlLyricsSiteScraper.SiteParameter[] SiteList;

        public string[] Search(string title,
                               string[] artists,
                               string album,
                               string path,     //ファイルの場合ファイルのパス
                               string param)    //SeachListに書かれたパラメータ
        {
            if (JsonPath == null)
            {
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string directory = Path.GetDirectoryName(assemblyPath);
                JsonPath = Path.Combine(directory, "HtmlLyricsSitePluginSettings.json");
            }

            if (SiteList == null)
            {
                using FileStream fs = File.OpenRead(JsonPath);
                var task = JsonSerializer.DeserializeAsync<HtmlLyricsSiteScraper.SiteParameter[]>(fs);
                SiteList = task.AsTask().Result;
            }

            タイムタグ情報DataBase.SearchOrder[] orders =
                    タイムタグ情報DataBase.Search(title, string.Join(" ",artists), "", album);
            if (orders == null)
                return null;

            List<string> result = new();

            foreach (タイムタグ情報DataBase.SearchOrder order in orders)
            {
                if (order.Hit < 70)
                    break;

                string lyrics = null;
                int i = Array.FindIndex(SiteList, a => order.Website == a.Name);
                if (i < 0)
                    continue;

                string artist = Regex.Replace(order.Artist, @"\(.*\)", "").Trim();
                HtmlLyricsSiteScraper.ListData[] lists = HtmlLyricsSiteScraper.GetList(SiteList[i], order.Title, artist);
                if (lists == null)
                    continue;

                foreach (var list in lists)
                {
                    if (list.Title == order.Title && list.Artist.Contains(artist))
                    {
                        lyrics = HtmlLyricsSiteScraper.GetLyrics(list.LyricsPageUrl, SiteList[i]);
                        if (lyrics != null)
                        {
                            lyrics = list.LyricsPageUrl + "\n" + lyrics;

                            ProcessStartInfo pi = new()
                            {
                                FileName = list.LyricsPageUrl.AbsoluteUri,
                                UseShellExecute = true,
                            };
                            Process.Start(pi);
                            break;
                        }
                    }
                }

                if (lyrics != null)
                {
                    //                    string timetag = タイムタグ情報DataBase.Get(order.Key);

                    string[] url = lyrics.Split("\n", 2);
                    string timetaged_lyrics = タイムタグ情報DataBase.Apply(order.Key, url[1]);
                    result.Add(url[0] + "\n" + timetaged_lyrics);
                }
            }

            return result.ToArray();
        }
    }
}
