using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.IO;
using System.Reflection;


namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

//        private static readonly string dllpath = @"..\..\..\..\TTDBLyricsSearcherPlugin\bin\Debug\net5.0";
        //        private static readonly string dllpath = @"..\..\..\..\MyDBLyricsSearcherPlugin\bin\Debug\net5.0";
        private static readonly string dllpath = @"..\..\..\..\HtmlLyricsSitePlugin\bin\Debug\net5.0";

        private dynamic Searcher;
        private void Button_Click(object sender, RoutedEventArgs eve)
        {
            string[] dll = Directory.GetFiles(dllpath, "*.dll");

            try
            {
                if (Searcher == null)
                {
                    Assembly assembly = Assembly.LoadFrom(dll[0]);
                    Type type = assembly.GetType("Titalyver2.LyricsSearcher", true);
                    Searcher = Activator.CreateInstance(type);
                    if (Searcher == null)
                        return;
                }
                
                string[] result = Searcher.Search(TextBoxTitle.Text, new string[] { TextBoxArtist.Text }, TextBoxAlbum.Text, TextBoxPath.Text, TextBoxParam.Text);
                if (result != null)
                {
                    TextBoxResult.Text = string.Join("\n========== multi result separate ==========\n", result);
                }
            }
            catch (Exception e)
            {
                TextBoxResult.Text += "例外が発生しました。\n" + e.Message;
                return;
            }
        }
    }
}
