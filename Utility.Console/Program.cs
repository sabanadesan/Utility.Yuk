using System;

using System.Threading.Tasks;

using UtilityPlus;
using Utility.Yuk;

namespace Utility.Console
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            var task = Task.Run(() => CPU.ConsumeCPU(50));

            while (true)
            {
                await Task.Delay(2000);
                var cpuUsage = await CPU.GetCpuUsageForProcess();

                System.Console.WriteLine(cpuUsage);
            }
        }

        static void Main2(string[] args)
        {
            UtilityPlus.Class1.msg();

            System.Console.WriteLine("Hello World!");
            SomeAction("yes");

            string param1 = "test";
            bool result = Stopwatcher.Track(() => SomeAction(param1), "test");
            Stopwatcher.Show();

            System.Console.WriteLine(Memory.Show());
        }

        [Stopwatcher]
        static bool SomeAction(string param1)
        {
            System.Console.WriteLine("1");
            for (int i = 0; i < 100000000; i++)
            {
                int a = 1 + 5;
            }
            return true;
        }
    }
}
