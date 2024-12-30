using System;
using System.Collections.Generic;
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


        public bool IsMisSpelled(string word) //Check if the word is in the dictionary, is a number, is a date, or a time
        {
            if (DictionaryDictionary.Contains(word))
            {
                return false;
            }

            double tempDouble;
            if (Double.TryParse(word, out tempDouble))
            {
                return false;
            }

            DateTime tempDateTime;
            if (DateTime.TryParse(word, out tempDateTime))
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
        public string[] ClosestWords(string word, int returnCount)
        {
            if (returnCount <= 0)
            {
                throw new Exception("num must be greater than zero");
            }

            int largestDistance = LongestWordLength > word.Length ? LongestWordLength : word.Length;
            List<List<string>> sortedWords = new List<List<string>>(largestDistance + 1);
            for (int index = 0; index < largestDistance + 1; ++index)
            {
                sortedWords.Add(new List<string>());
            }
            word = " " + word;
            EditDistanceMatrix = new int[word.Length, LongestWordLength + 1];
            for (int index = 0; index < word.Length; ++index)
            {
                EditDistanceMatrix[index, 0] = index;
            }
            for (int index = 1; index < LongestWordLength + 1; ++index)
            {
                EditDistanceMatrix[0, index] = index;
            }
            int editDistance;
            for (int index = 0; index < Dictionary.Length; ++index)
            {
                editDistance = _EditDistance(word, Dictionary[index]);
                sortedWords[editDistance].Add(Dictionary[index]);
            }
            int wordsLeft = returnCount;
            string[] suggestedWords = new string[returnCount];
            for (int index1 = 0; index1 < largestDistance + 1; ++index1)
            {
                for (int index2 = 0; index2 < sortedWords[index1].Count; ++index2)
                {
                    suggestedWords[returnCount-wordsLeft] = sortedWords[index1][index2];
                    if (--wordsLeft == 0)
                    {
                        return suggestedWords;
                    }
                }
            }
            return suggestedWords;
        }


        /// <summary>
        /// Returns an array of words that are closest to the word to check
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <param name="returnCount">The number of words to return</param>
        /// <returns>An array of words closest to the word to check</returns>
        public string[] ClosestWordsNew(string word, int returnCount, bool newRoute = false)
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
            EditDistanceMatrix = new int[word.Length, LongestWordLength + 1];
            for (int index = 0; index < word.Length; ++index)
            {
                EditDistanceMatrix[index, 0] = index;
            }
            for (int index = 1; index < LongestWordLength + 1; ++index)
            {
                EditDistanceMatrix[0, index] = index;
            }
            int editDistance;
            editDistance = newRoute ? _EditDistanceNew(word, Dictionary[0]) : _EditDistance(word, Dictionary[0]);
            sortedWords[editDistance].Add(Dictionary[0]);
            string lastWord = Dictionary[0];
            int greatestCommonCharacters;
            for (int index = 1; index < Dictionary.Length; ++index)
            {
                greatestCommonCharacters = 0;

                while (greatestCommonCharacters + 1 < lastWord.Length &&
                       greatestCommonCharacters + 1 < Dictionary[index].Length &&
                       lastWord[greatestCommonCharacters + 1] == Dictionary[index][greatestCommonCharacters + 1])
                {
                    greatestCommonCharacters++;
                }


                editDistance = newRoute ? _EditDistanceNew(word, Dictionary[index], greatestCommonCharacters) : _EditDistance(word, Dictionary[index], greatestCommonCharacters);
                sortedWords[editDistance].Add(Dictionary[index]);
                lastWord = Dictionary[index];

            }
            int wordsLeft = returnCount;
            string[] suggestedWords = new string[returnCount];
            for (int index1 = 0; index1 < largestDistance + 1; ++index1)
            {
                for (int index2 = 0; index2 < sortedWords[index1].Count; ++index2)
                {
                    suggestedWords[returnCount - wordsLeft] = sortedWords[index1][index2];
                    if (--wordsLeft == 0)
                    {
                        return suggestedWords;
                    }
                }
            }
            return suggestedWords;
        }


        /// <summary>
        /// Returns an array of words that are closest to the word to check
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <param name="returnCount">The number of words to return</param>
        /// <returns>An array of words closest to the word to check</returns>
        public string[] ClosestWordsNewNew(string word, int returnCount, bool newRoute = false)
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
            //EditDistanceMatrix = new int[word.Length, LongestWordLength + 1];
            //for (int index = 0; index < word.Length; ++index)
            //{
            //    EditDistanceMatrix[index, 0] = index;
            //}
            //for (int index = 1; index < LongestWordLength + 1; ++index)
            //{
            //    EditDistanceMatrix[0, index] = index;
            //}
            InitEditDistanceMatrix(word);
            int editDistance;
            editDistance = _EditDistanceNew(word, Dictionary[0]);
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
      
               
                editDistance = newRoute ? _EditDistanceNew(word, Dictionary[index], greatestCommonCharacters) : _EditDistance(word, Dictionary[index], greatestCommonCharacters);
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
                for (++lengthIndex; lengthIndex < word.Length + highestScore - 2 && lengthIndex < LongestWordLength; ++lengthIndex) //Closely Examine the start and stop points they're probably wrong
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
                editDistance = _EditDistanceNew(word, thisWord.Word, greatestCommonCharacters);
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
            string[] suggestedWords = new string[returnCount];
            for (int index1 = 0; index1 < largestDistance + 1; ++index1)
            {
                for (int index2 = 0; index2 < sortedWords[index1].Count; ++index2)
                {
                    suggestedWords[returnCount - wordsLeft] = sortedWords[index1][index2];
                    if (--wordsLeft == 0)
                    {
                        return suggestedWords;
                    }
                }
            }
            return suggestedWords;
        }





        public string ClosestWord(string word)
        {
            word = " " + word;
            //EditDistanceMatrix = new int[word.Length, LongestWordLength + 1];
            //for (int index = 0; index < word.Length; ++index)
            //{
            //    EditDistanceMatrix[index, 0] = index;
            //}
            //for (int index = 1; index < LongestWordLength + 1; ++index)
            //{
            //    EditDistanceMatrix[0, index] = index;
            //}
            InitEditDistanceMatrix(word);
            int leastDistance = int.MaxValue;
            string closestWord = "";
            int distance = 0;
            for (int index = 0; index < Dictionary.Length; ++index)
            {
                distance = _EditDistance(word, Dictionary[index]);
                if (distance < leastDistance)
                {
                    closestWord = Dictionary[index];
                    leastDistance = distance;
                }
            }
            return closestWord;
        }




        private int _EditDistance(string s1, string s2)
        {
            return _EditDistance(s1, s2, 0);
        }

        private int _EditDistanceTrevor(string userWord, string dictionaryWord, int reuseLetters)
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
                        if (EditDistanceMatrix[userIndex - 1, dictionaryIndex] < EditDistanceMatrix[userIndex - 1, dictionaryIndex - 1])
                        {
                            if (EditDistanceMatrix[userIndex - 1, dictionaryIndex] < EditDistanceMatrix[userIndex, dictionaryIndex - 1])
                            {
                                EditDistanceMatrix[userIndex, dictionaryIndex] = EditDistanceMatrix[userIndex - 1, dictionaryIndex] + 1;
                            }
                            else
                            {
                                EditDistanceMatrix[userIndex, dictionaryIndex] = EditDistanceMatrix[userIndex, dictionaryIndex - 1] + 1;
                            }
                        }
                        else
                        {
                            if (EditDistanceMatrix[userIndex - 1, dictionaryIndex - 1] < EditDistanceMatrix[userIndex, dictionaryIndex - 1])
                            {
                                EditDistanceMatrix[userIndex, dictionaryIndex] = EditDistanceMatrix[userIndex - 1, dictionaryIndex - 1] + 1;
                            }
                            else
                            {
                                EditDistanceMatrix[userIndex, dictionaryIndex] = EditDistanceMatrix[userIndex, dictionaryIndex - 1] + 1;
                            }
                        }
                    }
                }
            }

            return EditDistanceMatrix[userWord.Length - 1, dictionaryWord.Length - 1];
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


        private int _EditDistanceNew(string s1, string s2)
        {
            return _EditDistanceNew(s1, s2, 0);
        }

        private int _EditDistanceNew(string userWord, string dictionaryWord, int reuseLetters)
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




        public void CheckText(string text, int route = 0)
        {
            (string line, int location)[] lines = SpecializedSplit(text, ['\n', '\r', '\f']);
            (string word, int column)[][] words = new (string, int)[lines.Length][];
            (string word, int numSymbolsRemovedFromBeginningOfWord, bool isBeginningOfSentence)[][] processedWords = new (string word, int numSymbolsRemovedFromBeginningOfWord, bool isBeginningOfSentence)[lines.Length][];
            for (int lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
            {
                words[lineIndex] = SpecializedSplit(lines[lineIndex].line, [' ', '\t', '\v']);
                processedWords[lineIndex] = new (string word, int numSymbolsRemovedFromBeginningOfWord, bool isBeginningOfSentence)[words[lineIndex].Length];
                for (int wordIndex = 0; wordIndex < words[lineIndex].Length; ++wordIndex)
                {
                    processedWords[lineIndex][wordIndex] = ProcessWord(words[lineIndex][wordIndex].word, text, lines[lineIndex].location + words[lineIndex][wordIndex].column);
                }
            }
            List<(string word, int lineNumber, int column, int location, int numSymbolsRemovedFromBeginningOfWord)> misspelledWords =
                            new List<(string word, int lineNumber, int column, int location, int numSymbolsRemovedFromBeginningOfWord)>();

            for (int lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
            {
                for (int wordIndex = 0; wordIndex < words[lineIndex].Length; ++wordIndex)
                {
                    if (IsMisSpelled(processedWords[lineIndex][wordIndex].word))
                    {
                        misspelledWords.Add((processedWords[lineIndex][wordIndex].word,                                     //Word
                                            lineIndex + 1,                                                                  //Line
                                            words[lineIndex][wordIndex].column,                                             //Column
                                            lines[lineIndex].location + words[lineIndex][wordIndex].column,                 //Location
                                            processedWords[lineIndex][wordIndex].numSymbolsRemovedFromBeginningOfWord));    //NumSymbolsRemovedFromBeginningOfWord
                    }
                }
            }
            Console.WriteLine($"SpellChecker 5000:");
            Console.WriteLine($"\nNumber of Misspelled Words: {misspelledWords.Count}\n");
            for (int misspelledWordIndex = 0; misspelledWordIndex < misspelledWords.Count; ++misspelledWordIndex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(misspelledWords[misspelledWordIndex].word);
                Console.ForegroundColor= ConsoleColor.Green;
                Console.WriteLine($"Line: {misspelledWords[misspelledWordIndex].lineNumber}  Column: {misspelledWords[misspelledWordIndex].column}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Context:");
                int beginningOfContext = misspelledWords[misspelledWordIndex].location - 20;
                int endOfContext = beginningOfContext + misspelledWords[misspelledWordIndex].word.Length + 40;

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
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"{text.Substring(beginningOfContext, 
                    misspelledWords[misspelledWordIndex].location 
                    - beginningOfContext 
                    + misspelledWords[misspelledWordIndex].numSymbolsRemovedFromBeginningOfWord)}");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{misspelledWords[misspelledWordIndex].word}");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine($"{
                    text.Substring(misspelledWords[misspelledWordIndex].location 
                    + misspelledWords[misspelledWordIndex].word.Length
                    + misspelledWords[misspelledWordIndex].numSymbolsRemovedFromBeginningOfWord,
                    
                    endOfContext 
                    - misspelledWords[misspelledWordIndex].location 
                    - misspelledWords[misspelledWordIndex].word.Length
                    )}");

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Suggested Words:");
                Console.ForegroundColor = ConsoleColor.White;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                string[] suggestedWords;
                if (route == 0)
                {
                    suggestedWords = ClosestWords(misspelledWords[misspelledWordIndex].word, 10);
                }
                else if (route == 1)
                {
                    suggestedWords = ClosestWordsNew(misspelledWords[misspelledWordIndex].word, 10);
                }
                else
                {
                    suggestedWords = ClosestWordsNewNew(misspelledWords[misspelledWordIndex].word, 10);
                }
                sw.Stop();
                for (int suggestedWordIndex = 0; suggestedWordIndex < suggestedWords.Length; ++suggestedWordIndex)
                {
                    Console.Write($"{suggestedWords[suggestedWordIndex]} ");
                }
                Console.WriteLine($"\nElapsed: {sw.Elapsed.ToString()}");
                Console.Write("\n\n");

            }
        }



        private (string processedWord, int numSymbolsRemovedFromBeginningOfWord, bool isBeginningOfSentence) ProcessWord(string word, string text, int locationOfWord)
        {
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
            bool isBeginningOfSentence = false;
            int charIndex = locationOfWord - 1;
            char[] punctuationList = ['.', '!', '?'];
            while (charIndex >= 0 && text[charIndex] == ' ')
            {
                --locationOfWord;
            }
            if (charIndex < 0 || punctuationList.Contains(text[charIndex]))
            {
                isBeginningOfSentence = true;
            }
            word = word.Trim(symbolList);
            string lowerCaseWord = word.ToLower();
            return (lowerCaseWord, numSymbolsRemovedFromBeginningOfWord, isBeginningOfSentence);
        }




        /// <summary>
        /// Splits a string according to the provided deliminator and returns an array of tuples holding the split string and the loaction the string appears in the provided text
        /// </summary>
        /// <param name="text">the text to split</param>
        /// <param name="delim">the deliminator</param>
        /// <returns>An array of tuples holding a string and the location the string appears in the provided text</returns>
        private (string, int)[] SpecializedSplit(string text, char delim)
        {
            int location = 0;
            List<(string, int)> stringsAndLocations = new List<(string, int)> ();

            //Ignore prevailing deliminators
            for (; location < text.Length; ++location)
            {
                if (text[location] != delim)
                {
                    break;
                }
            }

            for (int index = location + 1; index < text.Length; ++index)
            {
                if (text[index] == delim)
                {
                    stringsAndLocations.Add((text.Substring(location, index - location), location));
                    for (++index; index < text.Length; ++index)
                    {
                        if (text[index] != delim)
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
    




        /// <summary>
        /// Splits a string according to the provided deliminators and returns an array of tuples holding the split string and the loaction the string appears in the provided text
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
