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
using Fizzler;
using System.Net;
using System.IO;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace BeerFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string queryEndpointUrl = @"https://untappd.com/search?q=";
        List<Beer> masterBeerList;

        public MainWindow()
        {
            InitializeComponent();
            masterBeerList = new List<Beer>();
        }

        private List<string> CleanInput(string fileName)
        {
            StreamReader file = new StreamReader(fileName);

            string line;

            List<string> lines = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                if (line.Length < 5)
                    continue;

                if (!line.Contains("\t"))
                {
                    line += file.ReadLine();
                }

                line = line.Trim();

                string[] split = line.Split('\t');

                lines.Add(split[0] + " " + split[1]);
            }

            file.Close();

            return lines;
        }

        private async Task<List<Beer>> SearchBeers(List<string> queries)
        {
            HttpWebRequest request;
            HttpWebResponse response;

            List<Beer> allBeers = new List<Beer>();

            for (int i = 0; i < queries.Count; i++)
            {
                loadProgress.Value = i * 100 / queries.Count;
                string query = queries[i];
                request = (HttpWebRequest)WebRequest.Create(queryEndpointUrl + query.Replace(' ', '+'));
                response = (HttpWebResponse) await request.GetResponseAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream, response.CharacterSet == null ? null : Encoding.GetEncoding(response.CharacterSet));

                    string data = reader.ReadToEnd();

                    response.Close();
                    reader.Close();

                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(data);
                    HtmlNode document = html.DocumentNode;
                    
                    IList<Beer> beers = SearchResultsToBeers(document.QuerySelectorAll(".beer-item"));

                    if (beers.Count <= 2)
                    {
                        foreach (Beer beer in beers)
                            allBeers.Add(beer);
                    }
                    else // too many search results
                    {
                        allBeers.Add(beers[0]);
                        allBeers.Add(beers[1]);
                    }
                }
            }

            return allBeers;
        }

        private IList<Beer> SearchResultsToBeers(IEnumerable<HtmlNode> html)
        {
            IList<Beer> beers = new List<Beer>();

            foreach (HtmlNode node in html)
            {
                Beer beer = Beer.CreateFromDom(node);

                if (beer.InProduction)
                    beers.Add(beer);
            }

            return beers;
        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)allCheckbox.IsChecked)
            {
                beerGrid.ItemsSource = masterBeerList;
                return;
            }

            string filterKeyword;

            if ((bool)pilsnerCheckbox.IsChecked)
                filterKeyword = "pilsner";
            else if ((bool)wheatCheckbox.IsChecked)
                filterKeyword = "wheat";
            else if ((bool)ipaCheckbox.IsChecked)
                filterKeyword = "ipa";
            else if ((bool)stoutCheckbox.IsChecked)
                filterKeyword = "stout";
            else if ((bool)ciderCheckbox.IsChecked)
                filterKeyword = "cider";
            else if ((bool)sourCheckbox.IsChecked)
                filterKeyword = "sour";
            else if ((bool)porterCheckbox.IsChecked)
                filterKeyword = "porter";
            else
            {
                beerGrid.ItemsSource = masterBeerList;
                return;
            }

            List<Beer> filteredBeers = new List<Beer>();
            
            foreach (Beer beer in masterBeerList)
                if (beer.Style.ToLower().Contains(filterKeyword))
                    filteredBeers.Add(beer);

            beerGrid.ItemsSource = filteredBeers;
        }

        private async void load_Click(object sender, RoutedEventArgs e)
        {
            string fileName = @"..\..\BeerList.txt";
            List<string> lines = CleanInput(fileName);
            loadProgress.Value = 0;
            loadProgress.IsEnabled = true;
            masterBeerList = await SearchBeers(lines);
            beerGrid.ItemsSource = masterBeerList;
            loadProgress.Value = 0;
            loadProgress.IsEnabled = false;
        }

        private void filter_Click(object sender, RoutedEventArgs e)
        {
            apply_Click(sender, e);
        }
    }
}
