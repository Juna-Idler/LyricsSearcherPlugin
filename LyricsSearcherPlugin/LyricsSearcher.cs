using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

using LyricsSearcherPlugin;

//アセンブリ名はユニークなものへ変更
//
//ここの名前空間名、クラス名、メソッド名は全てこのまま固定

namespace Titalyver2
{
    public class LyricsSearcher
    {
        public string[] Search(string title,
                               string[] artists,
                               string album,
                               string path,     //ファイルの場合ファイルのパス
                               string param)    //SeachListに書かれたパラメータ
        {
            タイムタグ情報DataBase.SearchOrder[] orders =
                タイムタグ情報DataBase.Search(title, artists[0], "", album);
            if (orders == null)
                return null;

            List<string> result = new();

            foreach (タイムタグ情報DataBase.SearchOrder order in orders)
            {
                if (order.Hit < 70)
                    break;

                string lyrics = null;
                if (!HtmlLyricsSiteList.List.ContainsKey(order.Website))
                    continue;
                HtmlLyricsSiteSnatcher.ListData[] lists = HtmlLyricsSiteSnatcher.GetList(HtmlLyricsSiteList.List[order.Website].ListParameter, order.Title, order.Artist, HtmlLyricsSiteList.List[order.Website].Encoding);
                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        if (list.Title == order.Title && list.Artist == order.Artist)
                        {
                            lyrics = HtmlLyricsSiteSnatcher.GetLyrics(list.LyricsPageUrl, HtmlLyricsSiteList.List[order.Website].LyricsParameter);
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
