
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
            SetUpDictionary(dictionary);
        }




        public void SetUpDictionary(string dictionary)
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
            FindNextWords(DictionaryWords[0], 0, LongestWordLength);
            for (int index = 1; index < DictionaryWords.Length; ++index)
            {
                FindNextWords(DictionaryWords[index], DictionaryWords[index - 1]);
            }
        }

        /// <summary>
        /// Initializes DictionaryWord
        /// For each length, finds the next word in DictionaryWords and stores it along with the distance to it in the DictionaryWord
        /// </summary>
        /// <param name="word">The DictionaryWord to Initialize</param>
        /// <param name="wordIndex">The index at which the word lives in</param>
        /// <param name="longestWordLength">The longest word in DictionaryWords</param>
        public void FindNextWords(DictionaryWord word, int wordIndex, int longestWordLength)
        {
            word.Index = wordIndex;
            word.NextDictionaryWords = new DictionaryWord[longestWordLength];
            word.JumpToNextWords = new int[longestWordLength];
            for (int lengthIndex = 0; lengthIndex < longestWordLength; ++lengthIndex)
            {
                word.JumpToNextWords[lengthIndex] = int.MaxValue;
            }
            //For each length find the next word
            for (++wordIndex; wordIndex < DictionaryWords.Length; ++wordIndex)
            {
                if (word.NextDictionaryWords[DictionaryWords[wordIndex].Word.Length - 2] == null)
                {
                    word.NextDictionaryWords[DictionaryWords[wordIndex].Word.Length - 2] = DictionaryWords[wordIndex];
                    word.JumpToNextWords[DictionaryWords[wordIndex].Word.Length - 2] = wordIndex - word.Index;
                }
            }
            //Search the beginning of the array if words haven't been found
            for (wordIndex = 0; wordIndex < word.Index; ++wordIndex)
            {
                if (word.NextDictionaryWords[DictionaryWords[wordIndex].Word.Length - 2] == null)
                {
                    word.NextDictionaryWords[DictionaryWords[wordIndex].Word.Length - 2] = DictionaryWords[wordIndex];
                    word.JumpToNextWords[DictionaryWords[wordIndex].Word.Length - 2] = wordIndex + DictionaryWords.Length - wordIndex;
                }
            }
        }
        /// <summary>
        /// Initializes DictionaryWord
        /// For each length, finds the next word in DictionaryWords and stores it along with the distance to it in the DictionaryWord
        /// Using the prevDictionaryWord as a starting point
        /// </summary>
        /// <param name="word">The DictionaryWord to Initialize</param>
        /// <param name="prevDictionaryWord">The previous DictionaryWords</param>
        public void FindNextWords(DictionaryWord word, DictionaryWord prevDictionaryWord)
        {
            //Use the previous DictionaryWord as a starting point
            int wordIndex = prevDictionaryWord.Index + 1;
            word.Index = wordIndex;
            word.NextDictionaryWords = new DictionaryWord?[prevDictionaryWord.NextDictionaryWords.Length];
            prevDictionaryWord.NextDictionaryWords.CopyTo(word.NextDictionaryWords, 0);
            word.JumpToNextWords = new int[prevDictionaryWord.JumpToNextWords.Length];
            prevDictionaryWord.JumpToNextWords.CopyTo(word.JumpToNextWords, 0);
            for (int lengthIndex = 0; lengthIndex < word.JumpToNextWords.Length; ++lengthIndex)
            {
                if (word.JumpToNextWords[lengthIndex] != int.MaxValue)
                {
                    --word.JumpToNextWords[lengthIndex];
                }
            }


            //Only need to search for the next word of the same length
            for (++wordIndex; wordIndex < DictionaryWords.Length; ++wordIndex)
            {
                if (DictionaryWords[wordIndex].Word.Length == word.Word.Length)
                {
                    word.NextDictionaryWords[word.Word.Length - 2] = DictionaryWords[wordIndex];
                    word.JumpToNextWords[word.Word.Length - 2] = wordIndex - word.Index;
                    return;
                }
            }
            //Search the beginning of the array if a word hasnt been found yet
            for (wordIndex = 0; wordIndex <= word.Index; wordIndex++)
            {
                if (DictionaryWords[wordIndex].Word.Length == word.Word.Length)
                {
                    if (wordIndex != word.Index)
                    {
                        word.NextDictionaryWords[word.Word.Length - 2] = DictionaryWords[wordIndex];
                        word.JumpToNextWords[word.Word.Length - 2] = wordIndex + (DictionaryWords.Length - word.Index);
                        return;
                    }
                    else
                    {
                        word.NextDictionaryWords[word.Word.Length - 2] = null;
                        word.JumpToNextWords[word.Word.Length - 2] = int.MaxValue;
                        return;
                    }

                }
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
        public (string word, int score)[] ClosestWords(string word, int returnCount)
        {
            if (returnCount <= 0)
            {
                throw new Exception("returnCount must be greater than zero");
            }

            //Initializations
            int largestDistance = LongestWordLength > word.Length ? LongestWordLength : word.Length;
            List<List<string>> sortedWords = new List<List<string>>(largestDistance + 1);
            for (int index = 0; index < largestDistance + 1; ++index)
            {
                sortedWords.Add(new List<string>());
            }
            word = " " + word;
            InitEditDistanceMatrix(word);


            //Grab the first x suggestions, where x is the number of words to return
            int editDistance = _EditDistance(word, Dictionary[0]);
            sortedWords[editDistance].Add(Dictionary[0]);
            DictionaryWord lastWord = DictionaryWords[0];
            int greatestCommonCharacters;
            for (int wordIndex = 1; wordIndex < returnCount; ++wordIndex)
            {
                greatestCommonCharacters = 0;

                while (greatestCommonCharacters + 1 < lastWord.Word.Length &&
                       greatestCommonCharacters + 1 < Dictionary[wordIndex].Length &&
                       lastWord.Word[greatestCommonCharacters + 1] == Dictionary[wordIndex][greatestCommonCharacters + 1])
                {
                    greatestCommonCharacters++; 
                }
                editDistance = _EditDistance(word, Dictionary[wordIndex], greatestCommonCharacters);
                sortedWords[editDistance].Add(Dictionary[wordIndex]);
                lastWord = DictionaryWords[wordIndex];
            }



            //Find the highest score of the suggestions already calculated
            int wordsLeftBeforeRescore = returnCount;
            int highestScore = 0;
            for (int scoreIndex = 0;  scoreIndex < largestDistance + 1; ++scoreIndex)
            {
                wordsLeftBeforeRescore -= sortedWords[scoreIndex].Count;
                if (wordsLeftBeforeRescore == 0)
                {
                    highestScore = scoreIndex;
                    wordsLeftBeforeRescore = sortedWords[scoreIndex].Count;
                    break;
                }
            }



            //Repeatedly narrow the search to only the words with length close enough to
            //the word's length to have a chance at a score better than the words already found
            int lengthIndex = word.Length - highestScore - 1;
            lengthIndex = Math.Max(0, lengthIndex);
            DictionaryWord thisWord = lastWord.NextDictionaryWords[lengthIndex]!;
            int closestJump =  lastWord.JumpToNextWords[lengthIndex]!;
            int wordsLeftToAddToArray;
            while (true)
            {
                lengthIndex = word.Length - highestScore - 1;
                lengthIndex = Math.Max(0, lengthIndex);
                closestJump = lastWord.JumpToNextWords[lengthIndex]!;
                thisWord = lastWord.NextDictionaryWords[lengthIndex]!;

                //Find the next word with appropriate length to check 
                for (++lengthIndex; lengthIndex < word.Length + highestScore - 2 && lengthIndex < LongestWordLength; ++lengthIndex) 
                {
                    if (lastWord.JumpToNextWords[lengthIndex] < closestJump)
                    {
                        closestJump = lastWord.JumpToNextWords[lengthIndex];
                        thisWord = lastWord.NextDictionaryWords[lengthIndex]!;
                    }
                }

                //If thisWord is null there are no more valid words, if thisWord.Index < lastWord.Index we've searched the entire dictionary
                if (thisWord == null || thisWord.Index < lastWord.Index)
                {
                    break;
                }

                //Check the number of common characters between the lastWord and thisWord to pass to _EditDistance to speed up algorithm
                greatestCommonCharacters = 0;
                while (greatestCommonCharacters + 1 < lastWord.Word.Length &&
                       greatestCommonCharacters + 1 < thisWord.Word.Length &&
                       lastWord.Word[greatestCommonCharacters + 1] == thisWord.Word[greatestCommonCharacters + 1])
                {
                    greatestCommonCharacters++;
                }
                editDistance = _EditDistance(word, thisWord.Word, greatestCommonCharacters);

                //If The score is good enough add the word to sortedWords and adjust the highestScore as needed
                if (editDistance < highestScore)
                {
                    sortedWords[editDistance].Add(thisWord.Word);
                    --wordsLeftBeforeRescore;
                    if (wordsLeftBeforeRescore == 0)
                    {
                        wordsLeftToAddToArray = returnCount;
                        for (int scoreIndex = 0; scoreIndex < largestDistance + 1; ++scoreIndex)
                        {
                            wordsLeftToAddToArray -= sortedWords[scoreIndex].Count; //Not actually adding to array, just counting for now
                            if (wordsLeftToAddToArray <= 0)
                            {
                                highestScore = scoreIndex;
                                wordsLeftBeforeRescore = sortedWords[scoreIndex].Count + wordsLeftToAddToArray;
                                break;
                            }
                        }
                    }
                }
                
                lastWord = thisWord;

            }
            wordsLeftToAddToArray = returnCount;

            //Grab the top suggestions in sortedWords
            (string word, int score)[] suggestedWords = new (string word, int score)[returnCount];
            for (int scoreIndex = 0; scoreIndex < largestDistance + 1; ++scoreIndex)
            {
                for (int wordIndex = 0; wordIndex < sortedWords[scoreIndex].Count; ++wordIndex)
                {
                    suggestedWords[returnCount - wordsLeftToAddToArray] = (sortedWords[scoreIndex][wordIndex], scoreIndex);
                    if (--wordsLeftToAddToArray == 0)
                    {
                        return suggestedWords;
                    }
                }
            }
            return suggestedWords;
        }



        /// <summary>
        /// Checks the edit distance between two words
        /// </summary>
        /// <param name="userWord"></param>
        /// <param name="dictionaryWord"></param>
        /// <returns>The edit distance between two words</returns>
        private int _EditDistance(string userWord, string dictionaryWord)
        {
            return _EditDistance(userWord, dictionaryWord, 0);
        }


        /// <summary>
        /// Checks the edit distance between two words and reuses parts of the EditDistanceMatrix for the dictionaryWord
        /// </summary>
        /// <param name="userWord">the users word to check</param>
        /// <param name="dictionaryWord">the dictionary word to check</param>
        /// <param name="reuseLetters">the number of letters to reuse in EditDistanceMatrix for the dictionaryWord</param>
        /// <returns>the edit distance between the two words</returns>
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



        /// <summary>
        /// Initializes the EditDistanceMatrix used in the WagnerFischer Algorithm
        /// </summary>
        /// <param name="userWord">The users's word with a leading space</param>
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



        /// <summary>
        /// Checks the provided text for any misspellings against the dictionary and prints information about suggested words
        /// </summary>
        /// <param name="text">the text to check</param>
        /// <param name="suggestedWordReturnCount">the number of suggested words to print per misspelled word</param>
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
                Console.WriteLine($"Line: {misspelledWords[misspelledWordIndex].LineNumber}  Column: {misspelledWords[misspelledWordIndex].ColumnNumber}\n");




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
                Console.WriteLine("\nSuggested Words:");
                Console.ForegroundColor = ConsoleColor.White;
                List<(string word, int score)> suggestedWords;

                suggestedWords = ClosestWords(misspelledWords[misspelledWordIndex].Word, suggestedWordReturnCount).ToList();
                

                //If the word is start of sentence, check it's lowercase version as well
                if (misspelledWords[misspelledWordIndex].IsStartOfSentence)
                {
                    List<(string word, int score)> lowerCaseSuggestedWords = ClosestWords(misspelledWords[misspelledWordIndex].Word.ToLower(), suggestedWordReturnCount).ToList();

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

                    //Merge uppercase and lowercase suggestions
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
               
                for (int suggestedWordIndex = 0; suggestedWordIndex < suggestedWords.Count; ++suggestedWordIndex)
                {
                    Console.Write($"{suggestedWords[suggestedWordIndex].word.Substring(1)} ");
                }

                Console.Write("\n\n\n\n\n");

            }
        }


        /// <summary>
        /// Removes leading and trailing symbols, checks if it starts a sentence and packs all of this including its location in a ProcessedWord
        /// </summary>
        /// <param name="word">the word to be processed</param>
        /// <param name="text">the text the words lives in</param>
        /// <param name="lineNumber">the line number the word is located at</param>
        /// <param name="lineLocation">the index at which the line starts at within the text</param>
        /// <param name="columnLocation">the index of the start of the word within its line</param>
        /// <returns>a ProcessedWord</returns>
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
