
using System.IO;

using System.Text.Json;
using System.Diagnostics;

using System.Web;


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

            foreach (var site in SiteList)
            {
                var list = HtmlLyricsSiteScraper.GetList(site, title, artists[0]);
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        string lyrics = HtmlLyricsSiteScraper.GetLyrics(item.LyricsPageUrl, site);
                        if (lyrics != null)
                        {
                            ProcessStartInfo pi = new()
                            {
                                FileName = item.LyricsPageUrl.AbsoluteUri,
                                UseShellExecute = true,
                            };
                            Process.Start(pi);
                            lyrics += $"\n\n{site.Name}\n{HttpUtility.HtmlDecode(item.Title)}\n{HttpUtility.HtmlDecode(item.Artist)}\n{item.LyricsPageUrl}";
                            return new string[] { lyrics };
                        }
                    }
                }
            }
            return null;
        }
    }
}
