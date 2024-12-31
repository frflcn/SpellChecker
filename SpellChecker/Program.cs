using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Timers;

namespace SpellChecker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string textToCheck;
            string dictionary;
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:\nSpellChecker [[dictionary.txt] file-to-check.txt]");
                return;
            }
            else if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    textToCheck = File.ReadAllText(args[0]);
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using (Stream stream = assembly.GetManifestResourceStream("SpellChecker.scowl-60.txt"))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dictionary = reader.ReadToEnd();

                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{args[0]} does not exist");
                    return;
                }
            }
            else
            {
                if (File.Exists(args[0]))
                {
                    dictionary = File.ReadAllText(args[0]);
                }
                else
                {
                    Console.WriteLine($"{args[0]} does not exist");
                    return;
                }
                if (File.Exists(args[1]))
                {
                    textToCheck = File.ReadAllText(args[1]);
                }
                else
                {
                    Console.WriteLine($"{args[1]} does not exist");
                    return;
                }
            }






            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/dictionary.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/words.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/words_alpha.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/unix-words.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/scowl-60.txt");
            //string dictionary = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/scowl-95.txt");

            //string textToCheck = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/text-to-check.txt");
            //string textToCheck = File.ReadAllText("C:/Users/tmsta/source/repos/SpellChecker/SpellChecker/ChatGPT.txt");

            WagnerFischer wagner = new WagnerFischer(dictionary);
 
            wagner.CheckText(textToCheck);









        }
    }
}
