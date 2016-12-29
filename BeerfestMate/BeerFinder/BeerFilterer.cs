using System;
using System.Collections.Generic;

namespace BeerFinder
{
    public enum BeerFilterType
    {
        ipa,
        paleAle,
        stout,
        porter,
        brown,
        wheat,
        pilsner,
        lager,
        belgian,
        sour,
        cider,
        fruit,
        other
    }

    public static class BeerFilterer
    {
        private static Dictionary<BeerFilterType, string[]> beerFiltersToAssociatedKeywords = new Dictionary<BeerFilterType, string[]>()
        {
            { BeerFilterType.ipa, new string[] { "ipa", "i.p.a", "india pale" } },
            { BeerFilterType.paleAle, new string[] { "pale ale" } },
            { BeerFilterType.stout, new string[] { "stout" } },
            { BeerFilterType.porter, new string[] { "porter" } },
            { BeerFilterType.brown, new string[] { "brown" } },
            { BeerFilterType.wheat, new string[] { "wheat" } },
            { BeerFilterType.pilsner, new string[] { "pilsner"} },
            { BeerFilterType.lager, new string[] { "lager"} },
            { BeerFilterType.belgian, new string[] { "belgian", "dubbel", "tripel", "quad" } },
            { BeerFilterType.sour, new string[] { "sour", "berliner", "wild", "lambic", "gueuze", "gose", "kriek", "flanders" } },
            { BeerFilterType.cider, new string[] { "cider"} },
            { BeerFilterType.fruit, new string[] { "fruit", "framboise", "shandy", "radler", "apricot", "melon", "peach", "berry", "lemon", "orange", "lime" } },
        };

        public static List<Beer> DoFiltering(List<Beer> input, List<BeerFilterType> listOfSelectedFilters)
        {
            var numFilterTypes = Enum.GetValues(typeof(BeerFilterType)).Length;

            // If we have all filters on, don't do any filtering
            if (listOfSelectedFilters.Count == numFilterTypes)
            {
                return input;
            }

            List<Beer> filtered = new List<Beer>();

            // If we have all filters turned off, then return an empty list
            if (listOfSelectedFilters.Count == 0)
            {
                return filtered;
            }

            foreach (var beer in input)
            {
                if (BeerMatchesTheGivenFilters(beer, listOfSelectedFilters))
                {
                    filtered.Add(beer);
                }
            }

            // in "Other" case, we find all beers that do not pertain to the unselected filter types and add them
            if (listOfSelectedFilters.Contains(BeerFilterType.other))
            {
                // Find the list of unselected filter types
                List<BeerFilterType> listOfUnselectedFilterTypes = new List<BeerFilterType>();
                foreach (BeerFilterType enumVal in Enum.GetValues(typeof(BeerFilterType)))
                {
                    if (!listOfSelectedFilters.Contains(enumVal))
                    {
                        listOfUnselectedFilterTypes.Add(enumVal);
                    }
                }

                // Add other beers
                foreach (var beer in input)
                {
                    if (!filtered.Contains(beer))
                    {
                        if (!BeerMatchesTheGivenFilters(beer, listOfUnselectedFilterTypes))
                        {
                            filtered.Add(beer);
                        }
                    }
                }
            }


            return filtered;
        }

        private static bool BeerMatchesTheGivenFilters(Beer beer, List<BeerFilterType> listOfSelectedFilters)
        {
            foreach (var filterType in listOfSelectedFilters)
            {
                // Other case is handled specially
                if (filterType == BeerFilterType.other) continue;

                foreach (var keyword in beerFiltersToAssociatedKeywords[filterType])
                {
                    if (beer.Style.ToLower().Contains(keyword))
                    {
                        return true;
                    }

                    // Fruit is a special case where brewers often don't change the style
                    // but only mention the fruityness of it in the name
                    // Example: name is "Apricot Saison" but the style is a saison
                    if (filterType == BeerFilterType.fruit)
                    {
                        if (beer.Name.ToLower().Contains(keyword))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
