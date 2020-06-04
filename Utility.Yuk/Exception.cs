using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Yuk
{
    public class Exception
    {
        public static void CheckIsIntializedOrThrow(params Object[] args)
        {
            int i = 1;
            foreach (Object arg in args)
            {
                if (arg == null)
                {
                    throw new System.Exception("Please initialize parameter: " + i);
                }

                i++;
            }
        }
    }
}
