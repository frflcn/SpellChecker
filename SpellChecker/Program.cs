using System.Reflection;


namespace SpellChecker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string textToCheck;
            string dictionary;

            //Process args
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:\nSpellChecker [[dictionary.txt] file-to-check.txt]");
                return;
            }
            else if (args.Length == 1)
            {
                //Use built in dictionary
                if (File.Exists(args[0]))
                {
                    textToCheck = File.ReadAllText(args[0]);
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using (Stream stream = assembly.GetManifestResourceStream("SpellChecker.scowl-60.txt")!)
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

            //Load Dictionary
            WagnerFischer wagner = new WagnerFischer(dictionary);
 
            //Spell Check
            wagner.CheckText(textToCheck);

        }
    }
}
