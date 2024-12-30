using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    public class ProcessedWord
    {
        public string Word;
        public string? Processed_Word;
        public int LineNumber;
        public int ColumnNumber;
        public int LocationInText;
        public int NumSymbolsRemovedFromBeginningOfWord;
        public bool IsStartOfSentence;
        public string[]? SpellingSuggestions;

    }
}
