using System.Collections.Generic;

namespace BeerFinder
{
    public static class ExtensionMethods
    {
        public static T GetEleFromEnumerable<T>(this IEnumerable<T> nodes, int index)
        {
            int count = 0;
            foreach (var node in nodes)
            {
                if (count == index)
                {
                    return node;
                }
                count++;
            }
            return default(T);
        }

        public static int FindCountOfEnumerable<T>(this IEnumerable<T> nodes)
        {
            var count = 0;
            foreach (var node in nodes)
            {
                count++;
            }
            return count;
        }
    }
}
