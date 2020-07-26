using System;
using System.Collections.Generic;

namespace Utility.Pkg
{
    public class FilePackage
    {
        public string FilePath { get; set; }
        public IEnumerable<string> ContentFilePathList { get; set; }
    }
}
