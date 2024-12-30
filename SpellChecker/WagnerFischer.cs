using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    public class WagnerFischer
    {
        public string[] Dictionary { get; set; }
        private int LongestWordLength { get; set; }
        private int[,] EditDistanceMatrix { get; set; }
        private HashSet<string> DictionaryDictionary { get; set; }

        public DictionaryWord[] DictionaryWords{ get; set; }



        public WagnerFischer(string dictionary)
        {
            SetDictionary(dictionary);
        }




        public void SetDictionary(string dictionary)
        {
            DictionaryDictionary = new HashSet<string>();
            Dictionary = dictionary.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Array.Sort(Dictionary, StringComparer.Ordinal);
            DictionaryWords = new DictionaryWord[Dictionary.Length];
            LongestWordLength = 0;
            for (int index = 0; index < Dictionary.Length; ++index)
            {
                if (Dictionary[index].Length > LongestWordLength)
                {
                    LongestWordLength = Dictionary[index].Length;
                }
                DictionaryDictionary.Add(Dictionary[index]);
                Dictionary[index] = " " + Dictionary[index];
                DictionaryWords[index] = new DictionaryWord(Dictionary[index]);
            }
            DictionaryWords[0].FindNextWords(DictionaryWords, 0, LongestWordLength);
            for (int index = 1; index < DictionaryWords.Length; ++index)
            {
                DictionaryWords[index].FindNextWords(DictionaryWords, index, DictionaryWords[index - 1]);
            }
        }


        public bool IsMisspelled(ProcessedWord word) //Check if the word is in the dictionary, is a number, is a date, or a time
        {
            if (DictionaryDictionary.Contains(word.Word))
            {
                return false;
            }

            //If the word is the start of a sentence check it's lowercase version as well
            if (word.IsStartOfSentence && DictionaryDictionary.Contains(word.Word.ToLower()))
            {
                return false;
            }

            double tempDouble;
            if (Double.TryParse(word.Word, out tempDouble))
            {
                return false;
            }

            DateTime tempDateTime;
            if (DateTime.TryParse(word.Word, out tempDateTime))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Returns an array of words that are closest to the word to check
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <param name="returnCount">The number of words to return</param>
        /// <returns>An array of words closest to the word to check</returns>
        public (string word, int score)[] ClosestWordsNewNew(string word, int returnCount, bool newRoute = false)
        {
            if (returnCount <= 0)
            {
                throw new Exception("returnCount must be greater than zero");
            }

            int largestDistance = LongestWordLength > word.Length ? LongestWordLength : word.Length;
            List<List<string>> sortedWords = new List<List<string>>(largestDistance + 1);
            for (int index = 0; index < largestDistance + 1; ++index)
            {
                sortedWords.Add(new List<string>());
            }
            word = " " + word;

            InitEditDistanceMatrix(word);
            int editDistance;
            editDistance = _EditDistance(word, Dictionary[0]);
            sortedWords[editDistance].Add(Dictionary[0]);
            DictionaryWord lastWord = DictionaryWords[0];
            int greatestCommonCharacters;
            for (int index = 1; index < returnCount; ++index)
            {
                greatestCommonCharacters = 0;

                while (greatestCommonCharacters + 1 < lastWord.Word.Length &&
                       greatestCommonCharacters + 1 < Dictionary[index].Length &&
                       lastWord.Word[greatestCommonCharacters + 1] == Dictionary[index][greatestCommonCharacters + 1])
                {
                    greatestCommonCharacters++; 
                }
      
               
                editDistance = _EditDistance(word, Dictionary[index], greatestCommonCharacters);
                sortedWords[editDistance].Add(Dictionary[index]);
                lastWord = DictionaryWords[index];

            }
            int wordsLeft = returnCount;
            int highestScore = 0;
            for (int scoreIndex = 0;  scoreIndex < largestDistance + 1; ++scoreIndex)
            {
                wordsLeft -= sortedWords[scoreIndex].Count;
                if (wordsLeft == 0)
                {
                    highestScore = scoreIndex;
                    wordsLeft = sortedWords[scoreIndex].Count;
                    break;
                }
            }
            int lengthIndex = word.Length - highestScore - 1;
            lengthIndex = Math.Max(0, lengthIndex);
            DictionaryWord thisWord = lastWord.NextDictionaryWords[lengthIndex]!;
            int closestJump =  lastWord.JumpToNextWords[lengthIndex]!;
            while (true)
            {
                lengthIndex = word.Length - highestScore - 1;
                lengthIndex = Math.Max(0, lengthIndex);
                closestJump = lastWord.JumpToNextWords[lengthIndex]!;
                thisWord = lastWord.NextDictionaryWords[lengthIndex]!;
                for (++lengthIndex; lengthIndex < word.Length + highestScore - 2 && lengthIndex < LongestWordLength; ++lengthIndex) 
                {
                    if (lastWord.JumpToNextWords[lengthIndex] < closestJump)
                    {
                        closestJump = lastWord.JumpToNextWords[lengthIndex];
                        thisWord = lastWord.NextDictionaryWords[lengthIndex]!;
                    }
                }
                if (thisWord == null || thisWord.Index < lastWord.Index)
                {
                    break;
                }
                greatestCommonCharacters = 0;
                while (greatestCommonCharacters + 1 < lastWord.Word.Length &&
                       greatestCommonCharacters + 1 < thisWord.Word.Length &&
                       lastWord.Word[greatestCommonCharacters + 1] == thisWord.Word[greatestCommonCharacters + 1])
                {
                    greatestCommonCharacters++;
                }
                editDistance = _EditDistance(word, thisWord.Word, greatestCommonCharacters);
                if (editDistance < highestScore)
                {
                    sortedWords[editDistance].Add(thisWord.Word);
                    --wordsLeft;
                    if (wordsLeft == 0)
                    {
                        wordsLeft = returnCount;
                        for (int scoreIndex = 0; scoreIndex < largestDistance + 1; ++scoreIndex)
                        {
                            wordsLeft -= sortedWords[scoreIndex].Count;
                            if (wordsLeft <= 0)
                            {
                                highestScore = scoreIndex;
                                wordsLeft = sortedWords[scoreIndex].Count + wordsLeft;
                                break;
                            }
                        }
                    }
                }
                
                lastWord = thisWord;

            }
            wordsLeft = returnCount;
            (string word, int score)[] suggestedWords = new (string word, int score)[returnCount];
            for (int scoreIndex = 0; scoreIndex < largestDistance + 1; ++scoreIndex)
            {
                for (int wordIndex = 0; wordIndex < sortedWords[scoreIndex].Count; ++wordIndex)
                {
                    suggestedWords[returnCount - wordsLeft] = (sortedWords[scoreIndex][wordIndex], scoreIndex);
                    if (--wordsLeft == 0)
                    {
                        return suggestedWords;
                    }
                }
            }
            return suggestedWords;
        }




        private int _EditDistance(string userWord, string dictionaryWord)
        {
            return _EditDistance(userWord, dictionaryWord, 0);
        }



        private int _EditDistance(string userWord, string dictionaryWord, int reuseLetters)
        {
            for (int dictionaryIndex = 1 + reuseLetters; dictionaryIndex < dictionaryWord.Length; ++dictionaryIndex)
            {
                for (int userIndex = 1; userIndex < userWord.Length; ++userIndex)
                {
                    if (userWord[userIndex] == dictionaryWord[dictionaryIndex])
                    {
                        EditDistanceMatrix[userIndex, dictionaryIndex] = EditDistanceMatrix[userIndex - 1, dictionaryIndex - 1];
                    }
                    else
                    {
                        int dictionaryMinus1 = EditDistanceMatrix[userIndex, dictionaryIndex - 1];
                        int userMinus1 = EditDistanceMatrix[userIndex - 1, dictionaryIndex];
                        int dictionaryUserMinus1 = EditDistanceMatrix[userIndex - 1, dictionaryIndex - 1];

                        EditDistanceMatrix[userIndex, dictionaryIndex] = Math.Min(dictionaryMinus1, Math.Min(userMinus1, dictionaryUserMinus1)) + 1;
                    }
                }
            }

            return EditDistanceMatrix[userWord.Length - 1, dictionaryWord.Length - 1];
        }



        public int EditDistance(string s1, string s2)
        {
            s1 = " " + s1;
            s2 = " " + s2;
            EditDistanceMatrix = new int[s1.Length, s2.Length];
            for (int index = 0; index < s1.Length; ++index)
            {
                EditDistanceMatrix[index, 0] = index;
            }
            for (int index = 1; index < s2.Length; ++index)
            {
                EditDistanceMatrix[0, index] = index;
            }

            return _EditDistance(s1, s2);

        }



        private void InitEditDistanceMatrix(string userWord)
        {
            EditDistanceMatrix = new int[userWord.Length, LongestWordLength + 1];
            for (int userIndex = 0;  userIndex < userWord.Length; ++userIndex)
            {
                EditDistanceMatrix[userIndex, 0] = userIndex;
            }
            for (int dictionaryIndex = 1;  dictionaryIndex < LongestWordLength + 1;  ++dictionaryIndex)
            {
                EditDistanceMatrix[0, dictionaryIndex] = dictionaryIndex;
            }
        }




        public void CheckText(string text, int suggestedWordReturnCount = 10)
        {
            //Split text into lines, then split into words keeping track of the location of all words, then process words
            (string line, int location)[] lines = SpecializedSplit(text, ['\n', '\r', '\f']);
            (string word, int column)[][] words = new (string, int)[lines.Length][];
            ProcessedWord[][] processedWords = new ProcessedWord[lines.Length][];
            for (int lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
            {
                words[lineIndex] = SpecializedSplit(lines[lineIndex].line, [' ', '\t', '\v']);
                processedWords[lineIndex] = new ProcessedWord[words[lineIndex].Length];
                for (int wordIndex = 0; wordIndex < words[lineIndex].Length; ++wordIndex)
                {
                    processedWords[lineIndex][wordIndex] = ProcessWord(words[lineIndex][wordIndex].word, text, lineIndex, lines[lineIndex].location, words[lineIndex][wordIndex].column);
                }
            }


            //Check if words are misspelled
            List<ProcessedWord> misspelledWords =
                            new List<ProcessedWord>();
            for (int lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
            {
                for (int wordIndex = 0; wordIndex < words[lineIndex].Length; ++wordIndex)
                {
                    if (IsMisspelled(processedWords[lineIndex][wordIndex]))
                    {
                        misspelledWords.Add(processedWords[lineIndex][wordIndex]);    
                    }
                }
            }

            //Print Data about misspelled words
            PrintMisspellings(text, misspelledWords);

        }

        public void PrintMisspellings(string text, List<ProcessedWord> misspelledWords, int suggestedWordReturnCount = 10)
        {
            Console.WriteLine($"SpellChecker 5000:");
            Console.WriteLine($"\nNumber of Misspelled Words: {misspelledWords.Count}\n");

            for (int misspelledWordIndex = 0; misspelledWordIndex < misspelledWords.Count; ++misspelledWordIndex)
            {
                //Print word, Line Number and Column Number
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(misspelledWords[misspelledWordIndex].Word);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Line: {misspelledWords[misspelledWordIndex].LineNumber}  Column: {misspelledWords[misspelledWordIndex].ColumnNumber}");




                //Print context around misspelled word
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Context:");
                int beginningOfContext = misspelledWords[misspelledWordIndex].LocationInText - 20;
                int endOfContext = beginningOfContext + misspelledWords[misspelledWordIndex].Word.Length + 40;

                //Adjust for words near the beginning or end of the text
                if (beginningOfContext < 0)
                {
                    endOfContext -= beginningOfContext;
                    beginningOfContext = 0;
                }
                if (endOfContext > text.Length)
                {
                    endOfContext = text.Length;
                }

                //Print context before word
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{text.Substring(beginningOfContext,
                    misspelledWords[misspelledWordIndex].LocationInText
                    - beginningOfContext
                    + misspelledWords[misspelledWordIndex].NumSymbolsRemovedFromBeginningOfWord)}");

                //Print misspelled word in context as red
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{misspelledWords[misspelledWordIndex].Word}");
                
                //Print context after word
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{text.Substring(misspelledWords[misspelledWordIndex].LocationInText
                    + misspelledWords[misspelledWordIndex].Word.Length
                    + misspelledWords[misspelledWordIndex].NumSymbolsRemovedFromBeginningOfWord,

                    endOfContext
                    - misspelledWords[misspelledWordIndex].LocationInText
                    - misspelledWords[misspelledWordIndex].Word.Length
                    )}");


                //Print suggested Words
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Suggested Words:");
                Console.ForegroundColor = ConsoleColor.White;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<(string word, int score)> suggestedWords;

                suggestedWords = ClosestWordsNewNew(misspelledWords[misspelledWordIndex].Word, suggestedWordReturnCount).ToList();
                
                if (misspelledWords[misspelledWordIndex].IsStartOfSentence)
                {
                    List<(string word, int score)> lowerCaseSuggestedWords = ClosestWordsNewNew(misspelledWords[misspelledWordIndex].Word.ToLower(), suggestedWordReturnCount).ToList();

                    //Remove Duplicates
                    int lowerCaseIndex;
                    int upperCaseIndex;
                    for (upperCaseIndex = 0; upperCaseIndex < suggestedWords.Count; ++upperCaseIndex)
                    {
                        for (lowerCaseIndex = 0; lowerCaseIndex < lowerCaseSuggestedWords.Count; ++lowerCaseIndex)
                        {
                            if (suggestedWords[upperCaseIndex].word == lowerCaseSuggestedWords[lowerCaseIndex].word)
                            {
                                if (suggestedWords[upperCaseIndex].score < lowerCaseSuggestedWords[lowerCaseIndex].score)
                                {
                                    lowerCaseSuggestedWords.RemoveAt(lowerCaseIndex);
                                    --lowerCaseIndex;
                                }
                                else
                                {
                                    suggestedWords.RemoveAt(upperCaseIndex);
                                    --upperCaseIndex;
                                    break;
                                }
                            }
                        }
                    }
                    List<(string word, int score)> tempSuggestedWords = new List<(string word, int score)>();
                    lowerCaseIndex = 0;
                    upperCaseIndex = 0;
                    for (int wordsLeft = 0; wordsLeft < suggestedWordReturnCount; ++wordsLeft)
                    {
                        if (suggestedWords[upperCaseIndex].score < lowerCaseSuggestedWords[lowerCaseIndex].score)
                        {
                            tempSuggestedWords.Add(suggestedWords[upperCaseIndex]);
                            ++upperCaseIndex;
                        }
                        else
                        {
                            tempSuggestedWords.Add(lowerCaseSuggestedWords[lowerCaseIndex]);
                            ++lowerCaseIndex;
                        }
                    }
                    suggestedWords = tempSuggestedWords;
                    
                }
               
                sw.Stop();
                for (int suggestedWordIndex = 0; suggestedWordIndex < suggestedWords.Count; ++suggestedWordIndex)
                {
                    Console.Write($"{suggestedWords[suggestedWordIndex].word.Substring(1)} ");
                }
                Console.WriteLine($"\nElapsed: {sw.Elapsed.ToString()}");
                Console.Write("\n\n");

            }
        }



        private ProcessedWord ProcessWord(string word, string text, int lineNumber, int lineLocation, int columnLocation)
        {
            //Document number of symbols removed from the beginning of word, used later for pretty printing
            char[] symbolList = ['`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '{', '}', '[', ']', '\\', '|', ':', ';', '"', '\'', ',', '<', '>', '.', '?', '/'];
            int numSymbolsRemovedFromBeginningOfWord = 0;
            for (int letterIndex = 0; letterIndex < word.Length; ++letterIndex)
            {
                if (symbolList.Contains(word[letterIndex]))
                {
                    ++numSymbolsRemovedFromBeginningOfWord;
                }
                else
                {
                    break;
                }
            }

            //Check if word is the beginning of a sentence, used for spellchecking
            bool isBeginningOfSentence = false;
            int charIndex = lineLocation + columnLocation - 1;
            char[] punctuationList = ['.', '!', '?'];
            while (charIndex >= 0 && Char.IsWhiteSpace(text[charIndex]))
            {
                --charIndex;
            }
            if (charIndex < 0 || punctuationList.Contains(text[charIndex]) || text[charIndex] == '"')
            {
                isBeginningOfSentence = true;
            }


            //Remove symbols from either side of word
            word = word.Trim(symbolList);

            //string lowerCaseWord = word.ToLower();
            return new ProcessedWord() {
                Word = word,
                NumSymbolsRemovedFromBeginningOfWord = numSymbolsRemovedFromBeginningOfWord,
                IsStartOfSentence = isBeginningOfSentence,
                LineNumber = lineNumber,
                ColumnNumber = columnLocation,
                LocationInText = columnLocation + lineLocation
            };
        }
 




        /// <summary>
        /// Splits a string according to the provided deliminators and returns an array of tuples holding the split string and the location the string appears in the provided text
        /// </summary>
        /// <param name="text">the text to split</param>
        /// <param name="delims">an array of deliminators</param>
        /// <returns>An array of tuples holding a string and the location the string appears in the provided text</returns>
        private (string, int)[] SpecializedSplit(string text, char[] delims)
        {
            int location = 0;
            List<(string, int)> stringsAndLocations = new List<(string, int)>();

            //Ignore prevailing deliminators
            for (; location < text.Length; ++location)
            {
                if (!delims.Contains(text[location]))
                {
                    break;
                }
            }
            
            for (int index = location + 1; index < text.Length; ++index)
            {
                
                if (delims.Contains(text[index]))
                {
                    stringsAndLocations.Add((text.Substring(location, index - location), location));
                    for (++index; index < text.Length; ++index)
                    {
                        if (!delims.Contains(text[index]))
                        {
                            break;
                        }
                    }
                    location = index;
                }
            }
            if (location < text.Length)
            {
                stringsAndLocations.Add((text.Substring(location, text.Length - location), location));
            }

            return stringsAndLocations.ToArray();

        }
    }
}
