using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class WordWeigther
    {
        public string Word { get; set; }
        public char Operator { get; set; }
        public float Weigth { get; set; }

        public WordWeigther(string word, float weigth, char operators)
        {
            Word = word;
            Weigth = weigth;
            Operator = operators;
        }

    }
}
