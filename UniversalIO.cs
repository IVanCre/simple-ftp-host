using System;


namespace CustomFTP
{
    class UniversalIO
    {
        public static void Print(string message)
        {
            Console.WriteLine(message);
        }
        public static string EnterMessage()
        {
            return Console.ReadLine();
        }
    }
}
