using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace MoogleEngine
{
    public class Document
    {
        public string Title { get; private set; }
        public string Path { get; private set; }
        public string Text { get; set; }
        public int MaxFrecuency { get; private set; }

        //Dicionario de palabra contra frecuencia.
        public Dictionary<string, int> FrecuencyWords { get; private set; }

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="path"></param>
        public Document(string title, string path)
        {
            Title = title;
            Path = path;
            FrecuencyWords = new Dictionary<string, int>();
            LoadFrecuency();
        }

        /// <summary>
        /// Extrae del fichero las palabras y llena el dicionario de FrecuencyWords. 
        /// </summary>
        private void LoadFrecuency()
        {
            //Permite iterar el fichero.
            using (StreamReader sr = new StreamReader(Path))
            {
                MaxFrecuency = 1;
                Text = sr.ReadToEnd().ToLower();
                var words = Split(Text);

                // Agrupa las palabras y las convierte en un diccionario de palabra contra catidad de veces que se repite.
                FrecuencyWords = words
                    .GroupBy(word => word.ToLower())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
                //Obtengo del diccionario de palabra contra frecuencia el valor maximo que tiene.
                MaxFrecuency = FrecuencyWords.Values.Max();
            }
        }

        /// <summary>
        /// Devulve un fragmneto del documjento donde se encuentra la palabra.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public string GetSnippet(string word)
        {
            string result = string.Empty;
            int count = 0;
            var words = Split(Text).ToList();
            var index = words.IndexOf(word);
            if (index >= 0)
            {
                for (int i = index; i < words.Count; i++)
                {
                    if (count > 100)
                    {
                        break;
                    }
                    else
                    {
                        result += words[i] + " ";
                        count++;
                    }
                }
            }
            return result;

        }

        /// <summary>
        /// Permite hacer split por ese conjunto de caracteres.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string[] Split(string text)
        {
            return text.Split(new char[] { '.','?', '+', ' ', '{', '}', '[', ']', '-', '_', '\\', '*', ';', ',', '(', ')', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
