using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Parsing;

namespace Titanic
{
    // The UI class through which every interaction (read/write) with the user goes. It is static
    // because there will only be one UI throughout the program.

    // Although it is a concrete class (because I wanted to keep things simple), it would be easy
    // to separate it into an interface (in the programming sense of the word, i.e. the minimal set 
    // of functionality a class has to satisfy) and its implementation. This would allow the program
    // to support different types of interfaces (in the common sense of the word, i.e. that which 
    // the user interacts with), as long as they can implement all the public methods defined below. 
    // Abstracting the dirty details away is a good thing in terms of design.
    public static class UI
    {
        // Wrapper around the console to output colored messages.
        // It is defined as private and is just a helper function for the public methods such as
        // PrintMessage or PrintSuccess. Indeed, exposing it as public would tie the program
        // to the console, because of the 'color' argument which is specific to the console
        // through its type. So it would make the maintainer's life harder if say he wanted to
        // port the code so it could be used with a graphical interface, or on a mobile device.
        private static void PrintColoredMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintMessage(string message)
        {
            PrintColoredMessage(message, Console.ForegroundColor);
        }

        public static void PrintSuccess(string message)
        {
            PrintColoredMessage(message, ConsoleColor.Green);
        }

        public static void PrintError(string message)
        {
            PrintColoredMessage(message, ConsoleColor.Red);
        }

        // The following two functions are also private, for the same reason as above.
        // The concept of a 'Key' is specific to an environment where there is a keyboard
        // with instantaneous input, so keeping it hidden from the rest of the code
        // helps maintain the UI functions abstract.
        private static string GetKey()
        {
            return Console.ReadKey(true).Key.ToString();
        }

        private static void WaitKey()
        {
            PrintColoredMessage("<Press a key to continue...>", ConsoleColor.Yellow);
            GetKey();
        }

        public static void Wait()
        {
            WaitKey();
        }

        public static bool GetYesOrNo(string message)
        {
            PrintMessage(String.Format("{0} [y/n]", message));
            
            while (true)
            {
                switch (GetKey())
                {
                    case "Y":
                        return true;
                    case "N":
                        return false;
                    default:
                        break;
                }
            }
        }

        public static string GetLine()
        {
            Console.Write(">");
            return Console.ReadLine();
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void WaitMessage(string message)
        {
            PrintMessage(message);
            Wait();
            UI.Clear();
        }
    }
}
