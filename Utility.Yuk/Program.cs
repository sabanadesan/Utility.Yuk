using System;
using UtilityPlus;

namespace Utility.Yuk
{
    class Program
    {
        static void Main(string[] args)
        {
            UtilityPlus.Class1.msg();

            Console.WriteLine("Hello World!");
            SomeAction("yes");

            string param1 = "test";
            bool result = Stopwatcher.Track(() => SomeAction(param1), "test");
            Stopwatcher.Show();
        }

        [Stopwatcher]
        static bool SomeAction(string param1)
        {
            Console.WriteLine("1");
            for (int i = 0; i < 1000000000; i++)
            {
                int a = 1 + 5;
            }
            return true;
        }
    }
}
