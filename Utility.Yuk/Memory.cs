using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

namespace Utility.Yuk
{
    public class Memory
    {
        public static long Show()
        {
            var proc = Process.GetCurrentProcess();
            var mbUsed = (proc.PrivateMemorySize64 / 1024) / 1024;

            return mbUsed;
        }
    }
}
