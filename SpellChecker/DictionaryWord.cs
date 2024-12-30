using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    /// <summary>
    /// A class designed to hold information about it's place in the dictionary, and the upcoming words, that the spellchecker can use to speed up its search
    /// </summary>
    public class DictionaryWord
    {
        public string Word { get; set; } = string.Empty;
        public int Index { get; set; }
        public DictionaryWord?[] NextDictionaryWords { get; set; } = [];
        public int[] JumpToNextWords { get; set; } = [];

        public DictionaryWord(string word) 
        {
            Word = word;
        }
        public DictionaryWord() { }

        public void FindNextWords(DictionaryWord[] dictionary, int index, int longestWordLength)
        {
            Index = index;
            NextDictionaryWords = new DictionaryWord[longestWordLength];
            JumpToNextWords = new int[longestWordLength];
            for (int index2 = 0; index2 < longestWordLength; ++index2)
            {
                JumpToNextWords[index2] = int.MaxValue;
            }
            for (++index; index <  dictionary.Length; ++index)
            {
                if (NextDictionaryWords[dictionary[index].Word.Length - 2] == null)
                {
                    NextDictionaryWords[dictionary[index].Word.Length - 2] = dictionary[index];
                    JumpToNextWords[dictionary[index].Word.Length - 2] = index - Index;
                }
            }
            for (index = 0; index < Index; ++index)
            {
                if (NextDictionaryWords[dictionary[index].Word.Length - 2] == null)
                {
                    NextDictionaryWords[dictionary[index].Word.Length - 2] = dictionary[index];
                    JumpToNextWords[dictionary[index].Word.Length - 2] = index + dictionary.Length - Index;
                }
            }
        }
        public void FindNextWords(DictionaryWord[] dictionary, int index, DictionaryWord prevDictionaryWord)
        {
            Index = index;
            NextDictionaryWords = new DictionaryWord?[prevDictionaryWord.NextDictionaryWords.Length];
            prevDictionaryWord.NextDictionaryWords.CopyTo(NextDictionaryWords, 0);
            JumpToNextWords = new int[prevDictionaryWord.JumpToNextWords.Length];
            prevDictionaryWord.JumpToNextWords.CopyTo(JumpToNextWords, 0);
            for (int index2 = 0; index2 < JumpToNextWords.Length; ++index2)
            {
                if (JumpToNextWords[index2] != int.MaxValue)
                {
                    --JumpToNextWords[index2];
                }
            }
            for (++index; index < dictionary.Length; ++index)
            {
                if (dictionary[index].Word.Length == Word.Length)
                {
                    NextDictionaryWords[Word.Length - 2] = dictionary[index];
                    JumpToNextWords[Word.Length - 2] = index - Index;
                    return;
                }
            }
            for (index = 0; index <= Index; index++)
            {
                if (dictionary[index].Word.Length == Word.Length)
                {
                    if (index != Index)
                    {
                        NextDictionaryWords[Word.Length - 2] = dictionary[index];
                        JumpToNextWords[Word.Length - 2] = index + (dictionary.Length - Index);
                        return;
                    }
                    else
                    {
                        NextDictionaryWords[Word.Length - 2] = null;
                        JumpToNextWords[Word.Length - 2] = int.MaxValue;
                        return;
                    }
                    
                }
            }
        }
    }
}
