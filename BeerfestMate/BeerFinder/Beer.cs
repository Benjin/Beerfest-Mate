using System;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace BeerFinder
{
    public class Beer
    {
        public double Rating { get; private set; }

        private int ibuValue;
        public int? IBU
        {
            get
            {
                return ibuValue == -1 ? (int?)null : ibuValue;
            }
        }

        private double abvValue;
        public double? ABV
        {
            get
            {
                return abvValue == -1 ? (double?)null : abvValue;
            }
        }

        public string Name { get; private set; }
        public string Brewery { get; private set; }
        public string Style { get; private set; }
        public bool InProduction { get; set; }

    public Beer(string name, string brewery, string style, double rating) : this(name, brewery, style, rating, abv: -1, ibu: -1)
        {

        }

        public Beer(string name, string brewery, string style, double rating, double abv, int ibu)
        {
            Name = name;
            Brewery = brewery;
            Style = style;
            Rating = rating;
            abvValue = abv;
            ibuValue = ibu;
        }

        public override string ToString()
        {
            return Name;
        }

        public static Beer CreateFromDom(HtmlNode node)
        {
            string name = node.QuerySelector(".name").InnerText.Trim();
            string brewery = node.QuerySelector(".brewery").InnerText.Trim();
            string style = node.QuerySelector(".style").InnerText.Trim();
            double rating = Double.Parse(node.QuerySelector(".num").InnerText.Trim().Trim('(', ')'));

            string abvString = node.QuerySelector(".abv").InnerText.Trim();
            double abv = abvString.Contains("N/A") ? -1 : Double.Parse(abvString.Remove(abvString.Length - 5, 5));

            string ibuString = node.QuerySelector(".ibu").InnerText.Trim();
            int ibu = ibuString.Contains("N/A") ? -1 : Int32.Parse(ibuString.Remove(ibuString.Length - 4, 4));

            Beer beer = new Beer(name, brewery, style, rating, abv, ibu);
            beer.InProduction = node.QuerySelector(".oop") == null;

            return beer;
        }
    }
}
