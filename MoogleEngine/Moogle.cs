using System.Collections.Generic;
using System.IO;

namespace MoogleEngine
{
    public static class Moogle
    {
        // Diireccion donde se almacenan los ficheros.
        private static string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\First project moogle-2021\Content";

        private static SearchController searcher = new SearchController(path);

       
        /// <summary>
        /// Metodo que hace la busqueda.
        /// </summary>
        /// <param name="query">terminos a buscar</param>
        /// <returns></returns>
        public static SearchResult Query(string query)
        {
            var term = query.Split(' ');

            List<SearchItem> SearchItems = new List<SearchItem>();

            var searchRersult = searcher.ComputeRelevaceWord(term);
            
            // Aqui solo me quedo con los documentos  que tengan un score > 0
            foreach (var current in searchRersult.Items())
            {
                if (current.Score > 0)
                {
                    SearchItems.Add(current);
                }
            }
            return new SearchResult(SearchItems.ToArray(), searchRersult.Suggestion);
        }
    }
}