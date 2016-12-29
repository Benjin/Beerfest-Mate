using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.IO;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.Win32;

namespace BeerFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string untappdQueryEndpoint = @"https://untappd.com/search?q=";
        private const string ebfBeerPage = "https://www.beeradvocate.com/extreme/beer/";
        private const string microInvitationalBeerPage = "http://www.beeradvocate.com/micro/beer/";
        private string beerFestivalToLookUp;
        private List<Beer> masterBeerList;

        public MainWindow()
        {
            InitializeComponent();
            masterBeerList = new List<Beer>();

            beerFestivalToLookUp = ebfBeerPage;
        }

        private async Task<List<Beer>> GetBeerListOffBeerAdvocate()
        {
            HttpWebRequest request;
            HttpWebResponse response;

            request = (HttpWebRequest)WebRequest.Create(beerFestivalToLookUp);
            response = (HttpWebResponse)await request.GetResponseAsync();
            if (response.StatusCode != HttpStatusCode.OK) return null;

            var html = new HtmlDocument();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream, response.CharacterSet == null ? null : Encoding.GetEncoding(response.CharacterSet));
            var data = reader.ReadToEnd();
            response.Close();
            reader.Close();
            html.LoadHtml(data);

            return ParseBeerAdvocateWebpage(html.DocumentNode);
        }

        private List<Beer> ParseBeerAdvocateWebpage(HtmlNode document)
        {
            var divOfAllBeersAndBrewers = document.QuerySelector(".brewer").ParentNode;
            var divs = divOfAllBeersAndBrewers.Children();

            var preUntappdBeerList = new List<Beer>();
            var curBrewery = "";

            foreach (var div in divs)
            {
                var className = div.GetAttributeValue("class", "");
                switch (className)
                {
                    case "brewer":
                        // parse as brewer
                        curBrewery = div.FirstChild.InnerText;
                        break;
                    case "container":
                        // parse as beer
                        preUntappdBeerList.Add(this.ParseBeerAdvocateBeerDiv(div, curBrewery));
                        break;
                    default:
                        break;
                }
            }

            return preUntappdBeerList;
        }

        private Beer ParseBeerAdvocateBeerDiv(HtmlNode div, string curBrewery)
        {
            var divChildren = div.Children();

            double abv = 0.0;
            double.TryParse(divChildren.GetEleFromEnumerable(2).InnerText, out abv);

            var beer = new Beer(
                divChildren.GetEleFromEnumerable(0).InnerText,
                curBrewery,
                divChildren.GetEleFromEnumerable(1).InnerText,
                abv);

            return beer;
        }

        private async Task<List<Beer>> SearchBeersOnUntappd(List<Beer> beersToQuery)
        {
            HttpWebRequest request;
            HttpWebResponse response;

            List<Beer> allBeers = new List<Beer>();

            for (int i = 0; i < beersToQuery.Count; i++)
            {
                loadProgress.Value = i * 100 / beersToQuery.Count;
                var curBeer = beersToQuery[i];
                var beerToQuery = curBeer.Name + " " + curBeer.Brewery;
                request = (HttpWebRequest)WebRequest.Create(untappdQueryEndpoint + beerToQuery.Replace(' ', '+'));
                response = (HttpWebResponse)await request.GetResponseAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    continue;
                }

                var html = new HtmlDocument();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream, response.CharacterSet == null ? null : Encoding.GetEncoding(response.CharacterSet));
                var data = reader.ReadToEnd();

                response.Close();
                reader.Close();

                html.LoadHtml(data);
                var document = html.DocumentNode;

                // Purposefully finding only the first listed result.  If we ever want to grab more than
                // the first result then we will need to use QuerySelectorAll and go from there
                var beerNode = document.QuerySelector(".beer-item");

                if (beerNode != null)
                {
                    curBeer.AddFieldsFromDom(beerNode);
                    allBeers.Add(curBeer);
                }
            }

            return allBeers;
        }

        private void DoFilter()
        {
            List<BeerFilterType> listOfFilters = new List<BeerFilterType>();

            if ((bool)ipaCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.ipa);
            if ((bool)paleAleCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.paleAle);
            if ((bool)stoutCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.stout);
            if ((bool)porterCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.porter);
            if ((bool)brownCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.brown);
            if ((bool)wheatCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.wheat);
            if ((bool)pilsnerCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.pilsner);
            if ((bool)lagerCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.lager);
            if ((bool)belgianCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.belgian);
            if ((bool)sourCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.sour);
            if ((bool)ciderCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.cider);
            if ((bool)fruitCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.fruit);
            if ((bool)otherCheckbox.IsChecked)
                listOfFilters.Add(BeerFilterType.other);

            var filteredBeers = BeerFilterer.DoFiltering(masterBeerList, listOfFilters);
            filteredBeers.Sort();

            beerGrid.ItemsSource = filteredBeers;
        }

        private async void load_Click(object sender, RoutedEventArgs e)
        {
            var preUntappdBeerList = await GetBeerListOffBeerAdvocate();
            loadProgress.Value = 0;
            loadProgress.IsEnabled = true;
            masterBeerList = await SearchBeersOnUntappd(preUntappdBeerList);
            masterBeerList.Sort();
            beerGrid.ItemsSource = masterBeerList;
            loadProgress.Value = 0;
            loadProgress.IsEnabled = false;
        }

        private void WriteToTextFile(string fileName)
        {
            string textToWrite = "";

            masterBeerList.Sort();

            foreach (Beer b in masterBeerList)
            {
                textToWrite += b.Rating + "|" + b.Name + "|" + b.Brewery + "|" + b.Style + "|" + b.ABV + "\n";
            }

            System.IO.File.WriteAllText(fileName, textToWrite);
        }

        private void ReadFromFile(string fileName)
        {
            loadProgress.Value = 0;
            loadProgress.IsEnabled = true;
            this.masterBeerList.Clear();

            var lines = System.IO.File.ReadAllLines(fileName);
            var linesCount = lines.FindCountOfEnumerable();

            for (int i = 0; i < linesCount; i++)
            {
                loadProgress.Value = i * 100 / linesCount;

                var beerAttributes = lines[i].Split('|');

                if (beerAttributes.FindCountOfEnumerable() < 5)
                {
                    continue;
                }

                double abv = 0.0;
                double rating = 0.0;
                double.TryParse(beerAttributes[4], out abv);
                double.TryParse(beerAttributes[0], out rating);

                var beer = new Beer(beerAttributes[1], beerAttributes[2], beerAttributes[3], rating, abv, -1);

                masterBeerList.Add(beer);
            }

            masterBeerList.Sort();
            beerGrid.ItemsSource = masterBeerList;
            loadProgress.Value = 0;
            loadProgress.IsEnabled = false;
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();

            saveDialog.Filter = "txt files (*.txt)|*.txt";
            saveDialog.RestoreDirectory = true;

            var result = saveDialog.ShowDialog();

            if (result.Value)
            {
                WriteToTextFile(saveDialog.FileName);
            }
        }

        private void filter_Click(object sender, RoutedEventArgs e)
        {
            DoFilter();
        }

        private void loadSavedList_Click(object sender, RoutedEventArgs e)
        {
            var fileBrowser = new OpenFileDialog();
            fileBrowser.InitialDirectory = "c:\\";
            fileBrowser.Filter = "txt files (*.txt)|*.txt";
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = true;
            fileBrowser.Multiselect = false;

            var result = fileBrowser.ShowDialog();

            if (result.Value)
            {
                ReadFromFile(fileBrowser.FileName);
            }
        }
    }
}