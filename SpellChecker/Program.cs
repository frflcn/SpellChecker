using System.Diagnostics;
using System.Timers;

namespace SpellChecker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/dictionary.txt");
            string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/words.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/words_alpha.txt");
            string textToCheck = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/text-to-check.txt");

            WagnerFischer wagner = new WagnerFischer(dictionary);
            

            Stopwatch sw = new Stopwatch();
            sw.Start();
            wagner.CheckText(textToCheck);
            sw.Stop();
            Console.WriteLine($"Elapsed: {sw.Elapsed}");

            


        }
    }
}
