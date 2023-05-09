using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace MoogleEngine
{
    public class SearchController
    {
        //Listado de documentos.
        public List<Document> Documnets { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path"></param>
        public SearchController(string path)
        {
            Documnets = new List<Document>();
            LoadDocuments(path);
        }

        /// <summary>
        /// Carga los documentos de un directorio y los adiciona a la lista de documentos.
        /// </summary>
        /// <param name="path"></param>
        private void LoadDocuments(string path)
        {
            var DirInfo = new DirectoryInfo(path);
            foreach (var item in DirInfo.GetFiles("*.txt"))
            {
                Documnets.Add(new Document(item.Name, item.FullName));
            }
        }

        /// <summary>
        /// Calcula la relevancia de la palabra en cada documneto. 
        /// </summary>
        /// <param name="terms">Las palabras a buscar</param>
        /// <returns></returns>
        public SearchResult ComputeRelevaceWord(string[] query)
        {
            var items = new List<SearchItem>();

            //Obtengo el peso de cada termino.
            var terms = GetWordWeigthers(query);

            foreach (var document in Documnets)
            {
                string snippet = string.Empty;
                float score = 0;
                int rest = 0;
                foreach (var term in terms)
                {

                    float TFIDF = 0;
                    if (document.FrecuencyWords.ContainsKey(term.Word))
                    {
                        if (!IsStopWord(term.Word))
                        {
                            var TF = (float)document.FrecuencyWords[term.Word] / document.MaxFrecuency;
                            TFIDF = TF * IDF(term.Word);
                            if (term.Weigth == 0)
                            {
                                score = 0;
                            }
                            else
                            {
                                score += TFIDF * term.Weigth;
                            }
                            snippet += document.GetSnippet(term.Word.ToLower());

                            int index = document.FrecuencyWords.Keys.ToList().IndexOf(term.Word);
                            int last = rest;
                            rest = index - last;
                        }
                    }
                    if (term.Operator == '^' && TFIDF == 0)
                    {
                        score = 0;
                    }
                }
                if (terms.Any(x => x.Operator == '~') && rest != 0)
                {
                    score = (float) score / rest;
                }
                items.Add(new SearchItem(document.Title, snippet, score));
            }
            // Ordeno los documentos a retornar.
            var result = items.OrderByDescending(x => x.Score).ToArray();

            return new SearchResult(result, GetAllSugestion(terms));
        }

        /// <summary>
        /// Calcula el Idf de la plabra para cada documento.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private float IDF(string word)
        {
            int count = 0;

            foreach (var doc in Documnets)
            {
                if (doc.FrecuencyWords.ContainsKey(word))
                {
                    count++;
                }
            }
            return (float)Math.Log(Documnets.Count() / count);
        }

        /// <summary>
        /// Retorna true is la plabara es comun.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool IsStopWord(string word)
        {
            int count = 0;
            foreach (var doc in Documnets)
            {
                if (doc.FrecuencyWords.ContainsKey(word))
                {
                    count++;
                }
            }
            var r = (float)count / Documnets.Count();
            return r > 0.8;
        }

        /// <summary>
        /// Devulve el suggestion de cada termino concatenado en un solo string.
        /// </summary>
        /// <param name="terms">Terminos</param>
        /// <returns>Suggestion concatenada</returns>
        private string GetAllSugestion(WordWeigther[] terms)
        {
            string result = "";
            foreach (var term in terms)
            {
                result += GetSuggestion(term.Word) + " ";
            }
            return result;
        }

        /// <summary>
        /// Devuleve la palabra que mas se parezca en los documentos a la que se desea buscar 
        /// </summary>
        /// <param name="term"></param>
        /// <returns>Suggestion</returns>
        private string GetSuggestion(string term)
        {
            double distance = Double.MaxValue;
            string suggestion = string.Empty;

            foreach (var doc in Documnets)
            {
                foreach (var word in doc.FrecuencyWords.Keys)
                {
                    var dist = LevenshteinDistance(word, term);
                    if (distance > dist)
                    {
                        distance = dist;
                        suggestion = word;
                    }
                }
            }
            return suggestion;
        }

        /// <summary>
        /// Metodo que asigna cada peso a cada operador.
        /// </summary>
        /// <param name="terms"></param>
        /// <returns>los terminos con su peso correspondiente</returns>
        private WordWeigther[] GetWordWeigthers(string[] terms)
        {
            var result = new WordWeigther[terms.Length];
            for (int i = 0; i < terms.Length; i++)
            {
                string word = terms[i];
                char Operator = ' ';
                float weigth = 1;

                if (terms[i].StartsWith("^"))
                {
                    word = terms[i].Substring(1);
                    Operator = '^';
                }

                else if (terms[i].StartsWith("!"))
                {
                    word = terms[i].Substring(1);
                    Operator = '!';
                    weigth = 0;
                }

                else if (terms[i].StartsWith("*"))
                {
                    var count = terms[i].Split('*').Length - 1;
                    word = terms[i].Substring(count);
                    Operator = '*';
                    weigth = count;
                }
                else if (terms[i].StartsWith("~"))
                {
                    word = terms[i].Substring(1);
                    Operator = '~';
                }
                result[i] = new WordWeigther(word.ToLower(), weigth, Operator);
            }
            return result;
        }


        /// <summary>
        /// Calcula la disimilitud entre dos palabras.
        /// </summary>
        /// <param name="s">palabra 1</param>
        /// <param name="t">palabra 2</param>
        /// <returns></returns>
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }
}
