using System;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace BeerFinder
{
    public class Beer : IComparable
    {
        public double Rating { get; private set; }

        private int ibuValue;
        public int? IBU
        {
            get
            {
                return (ibuValue == -1) ? (int?)null : ibuValue;
            }
        }

        private double abvValue;
        public double? ABV
        {
            get
            {
                return (abvValue == -1) ? (double?)null : abvValue;
            }
        }

        public string Name { get; set; }
        public string Brewery { get; set; }
        public string Style { get; set; }
        public bool InProduction { get; set; }
        public string OnUntappd { get; set; }


        public Beer(string name, string brewery, string style, double abv) : this(name, brewery, style, 0.0, abv, ibu: -1) { }

        public Beer(string name, string brewery, string style, double rating, double abv, int ibu)
        {
            Name = name;
            Brewery = brewery;
            Style = style;
            Rating = rating;
            abvValue = abv;
            ibuValue = ibu;
        }

        public void AddFieldsFromDom(HtmlNode node)
        {
            this.Name = node.QuerySelector(".name").InnerText.Trim();
            this.Brewery = node.QuerySelector(".brewery").InnerText.Trim();
            this.Style = node.QuerySelector(".style").InnerText.Trim();
            this.InProduction = node.QuerySelector(".oop") == null;
            this.Rating = Double.Parse(node.QuerySelector(".num").InnerText.Trim().Trim('(', ')'));

            string abvString = node.QuerySelector(".abv").InnerText.Trim();
            this.abvValue = abvString.Contains("N/A") ? -1 : Double.Parse(abvString.Remove(abvString.Length - 5, 5));

            string ibuString = node.QuerySelector(".ibu").InnerText.Trim();
            this.ibuValue = ibuString.Contains("N/A") ? -1 : Int32.Parse(ibuString.Remove(ibuString.Length - 4, 4));

            this.InProduction = node.QuerySelector(".oop") == null;
            this.OnUntappd = "Yes";
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            var otherBeer = (Beer)obj;
            if (Rating > otherBeer.Rating)
            {
                return -1;
            }
            return 1;
        }
    }
}