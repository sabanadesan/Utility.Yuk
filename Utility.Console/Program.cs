using System;

using System.Threading.Tasks;

using UtilityPlus;
using Utility.Yuk;
using Utility.Cmd;

namespace Utility.Console
{
    class Program
    {
        public class ApplicationArguments
        {
            public int RecordId { get; set; }
            public bool Silent { get; set; }
            public string NewValue { get; set; }
        }

        static void Main(string[] args)
        {
            // create a generic parser for the ApplicationArguments type
            var p = new FluentCommandLineParser<ApplicationArguments>();

            // specify which property the value will be assigned too.
            p.Setup(arg => arg.RecordId)
             .As('r', "record") // define the short and long option name
             .Required(); // using the standard fluent Api to declare this Option as required.

            p.Setup(arg => arg.NewValue)
             .As('v', "value")
             .Required();

            p.Setup(arg => arg.Silent)
             .As('s', "silent")
             .SetDefault(false); // use the standard fluent Api to define a default value if non is specified in the arguments

            var result = p.Parse(args);

            if (result.HasErrors == false)
            {
                // use the instantiated ApplicationArguments object from the Object property on the parser.
                
                //application.Run(p.Object);
            }
        }

        public static async Task Main3(string[] args)
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
