using System.Diagnostics;
using System.Timers;

namespace SpellChecker
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/dictionary.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/words.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/words_alpha.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/unix-words.txt");
            string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/scowl-60.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/scowl-95.txt");

            //string textToCheck = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/text-to-check.txt");
            string textToCheck = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/ChatGPT.txt");
            Stopwatch sw = new Stopwatch();
            //sw.Start();
            WagnerFischer wagner = new WagnerFischer(dictionary);
            //sw.Stop();
            //Console.WriteLine($"Init Elapsed: {sw.Elapsed}");



            //sw.Restart();
            //wagner.CheckText(textToCheck);
            //sw.Stop();
            //TimeSpan first = sw.Elapsed;
            //sw.Restart();
            //wagner.CheckText(textToCheck, 1);
            //sw.Stop();
            //TimeSpan second = sw.Elapsed;
            sw.Restart();
            wagner.CheckText(textToCheck, 2);
            sw.Stop();
            TimeSpan third = sw.Elapsed;
            //Console.WriteLine($"Elapsed1: {first}");
            //Console.WriteLine($"Elapsed2: {second}");
            Console.WriteLine($"Elapsed3: {third}");


            //sw.Restart();
            //wagner.ClosestWord("peanutbuter");
            //sw.Stop();
            ////Console.WriteLine($"Word: Elapsed: {sw.Elapsed}");


            string word = "a";
            Console.WriteLine(word);
            while (true)
            {
                //sw.Restart();
                //(string word, int score)[] words = wagner.ClosestWords(word, 10);
                //sw.Stop();
                //Console.WriteLine($"Words: Elapsed: {sw.Elapsed}");

                //sw.Restart();
                //(string word, int score)[] newWords = wagner.ClosestWordsNew(word, 10, true);
                //sw.Stop();
                //Console.WriteLine($"WordsNew: Elapsed: {sw.Elapsed}");

                sw.Restart();
                (string word, int score)[] newNewWords = wagner.ClosestWordsNewNew(word, 10);
                sw.Stop();
                Console.WriteLine($"WordsNewNew: Elapsed: {sw.Elapsed}");

                //Console.Write($"Words:");

                //for (int i = 0; i < newWords.Length; ++i)
                //{
                //    Console.Write($" {words[i].word}");

                //}
                //Console.Write($"\nNewWd:");
                //for (int i = 0; i < newWords.Length; ++i)
                //{

                //    Console.Write($" {newWords[i].word}");
                //}
                Console.Write($"\nNNwWd:");
                for (int i = 0; i < newNewWords.Length; ++i)
                {

                    Console.Write($" {newNewWords[i].word}");
                }
                Console.WriteLine("\n");
                word = Console.ReadLine();
            }





        }
    }
}
