
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

    }
}
