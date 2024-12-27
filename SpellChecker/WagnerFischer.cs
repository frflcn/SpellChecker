using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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



        public WagnerFischer(string dictionary)
        {
            SetDictionary(dictionary);
        }




        public void SetDictionary(string dictionary)
        {
            DictionaryDictionary = new HashSet<string>();
            Dictionary = dictionary.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            LongestWordLength = 0;
            for (int index = 0; index < Dictionary.Length; ++index)
            {
                if (Dictionary[index].Length > LongestWordLength)
                {
                    LongestWordLength = Dictionary[index].Length;
                }
                DictionaryDictionary.Add(Dictionary[index]);
                Dictionary[index] = " " + Dictionary[index];
                
            }
        }




        public bool IsMisSpelled(string word)
        {
            return !DictionaryDictionary.Contains(word);
        }
        /// <summary>
        /// Returns an array of words that are closest to the word to check
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <param name="num">The number of words to return</param>
        /// <returns>An array of words closest to the word to check</returns>
        public string[] ClosestWords(string word, int num)
        {
            if (num < 0)
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
            int wordsLeft = num;
            string[] suggestedWords = new string[num];
            for (int index1 = 0; index1 < largestDistance + 1; ++index1)
            {
                for (int index2 = 0; index2 < sortedWords[index1].Count; ++index2)
                {
                    suggestedWords[num-wordsLeft] = sortedWords[index1][index2];
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
            EditDistanceMatrix = new int[word.Length, LongestWordLength + 1];
            for (int index = 0; index < word.Length; ++index)
            {
                EditDistanceMatrix[index, 0] = index;
            }
            for (int index = 1; index < LongestWordLength + 1; ++index)
            {
                EditDistanceMatrix[0, index] = index;
            }
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
            for (int index1 = 1; index1 < s1.Length; ++index1)
            {
                for (int index2 = 1; index2 < s2.Length; ++index2)
                {
                    if (s1[index1] == s2[index2])
                    {
                        EditDistanceMatrix[index1, index2] = EditDistanceMatrix[index1 - 1, index2 - 1];
                    }
                    else
                    {
                        if (EditDistanceMatrix[index1 - 1, index2] < EditDistanceMatrix[index1 - 1, index2 - 1])
                        {
                            if (EditDistanceMatrix[index1 - 1, index2] < EditDistanceMatrix[index1, index2 - 1])
                            {
                                EditDistanceMatrix[index1, index2] = EditDistanceMatrix[index1 - 1, index2] + 1;
                            }
                            else
                            {
                                EditDistanceMatrix[index1, index2] = EditDistanceMatrix[index1, index2 - 1] + 1;
                            }
                        }
                        else
                        {
                            if (EditDistanceMatrix[index1 - 1, index2 - 1] < EditDistanceMatrix[index1, index2 - 1])
                            {
                                EditDistanceMatrix[index1, index2] = EditDistanceMatrix[index1 - 1, index2 - 1] + 1;
                            }
                            else
                            {
                                EditDistanceMatrix[index1, index2] = EditDistanceMatrix[index1, index2 - 1] + 1;
                            }
                        }
                    }
                }
            }

            return EditDistanceMatrix[s1.Length - 1, s2.Length - 1];
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




        public void CheckText(string text)
        {
            (string line, int location)[] lines = SpecializedSplit(text, ['\n', '\r', '\f']);
            (string word, int column)[][] words = new (string, int)[lines.Length][];
            string[][] processedWords = new string[lines.Length][];
            for (int index = 0; index < lines.Length; ++index)
            {
                words[index] = SpecializedSplit(lines[index].line, [' ', '\t', '\v']);
                processedWords[index] = new string[words[index].Length];
                for (int index2 = 0; index2 < words[index].Length; ++index2)
                {
                    processedWords[index][index2] = ProcessWord(words[index][index2].word);
                }
            }
            List<(string word, int lineNumber, int column, int location)> misspelledWords = new List<(string word, int lineNumber, int column, int location)>();
            for (int index = 0; index < lines.Length; ++index)
            {
                for (int index2 = 0; index2 < words[index].Length; ++index2)
                {
                    if (IsMisSpelled(processedWords[index][index2]))
                    {
                        misspelledWords.Add((processedWords[index][index2], index + 1, words[index][index2].column, lines[index].location + words[index][index2].column));
                    }
                }
            }
            Console.WriteLine($"SpellChecker 5000:");
            Console.WriteLine($"\nNumber of Misspelled Words: {misspelledWords.Count}\n");
            for (int index = 0; index < misspelledWords.Count; ++index)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(misspelledWords[index].word);
                Console.ForegroundColor= ConsoleColor.Green;
                Console.WriteLine($"Line: {misspelledWords[index].lineNumber}  Column: {misspelledWords[index].column}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Context:");
                int beginning = misspelledWords[index].location - 20;
                int end = beginning + misspelledWords[index].word.Length + 40;

                //Adjust for words near the beginning or end of the text
                if (beginning < 0)
                {
                    end -= beginning;
                    beginning = 0;
                }
                if (end > text.Length)
                {
                    end = text.Length;
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{text.Substring(beginning, misspelledWords[index].location - beginning)}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{misspelledWords[index].word}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{text.Substring(misspelledWords[index].location + misspelledWords[index].word.Length, end - misspelledWords[index].location - misspelledWords[index].word.Length)}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Suggested Words:");
                Console.ForegroundColor = ConsoleColor.White;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                string[] suggestedWords = ClosestWords(misspelledWords[index].word, 10);
                sw.Stop();
                for (int index2 = 0; index2 < suggestedWords.Length; ++index2)
                {
                    Console.Write($"{suggestedWords[index2]} ");
                }
                Console.WriteLine($"\nElapsed: {sw.Elapsed.ToString()}");
                Console.Write("\n\n");

            }
        }




        private string ProcessWord(string word)
        {
            return word;
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
