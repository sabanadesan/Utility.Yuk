using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Utility.Pkg;

namespace Utility.Console
{
    class Program2
    {
        static void Main(string[] args)
        {
            

            var test1FilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test\\test_1.txt";
            var test2FilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test\\test_2.txt";

            var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test.zip";

            var filePackage = new FilePackage
            {
                FilePath = packageFilePath,
                ContentFilePathList = new List<string>
                {
                    test1FilePath, test2FilePath
                }
            };

            //// 2. ZIP IN MEMORY. Create zipped byte array from string
            var filePackageWriter = new FilePackageWriter(filePackage);
            var zippedBuffer = filePackageWriter.GeneratePackageMemory();

            var filePackageReader = new FilePackageReader();
            var filenameFileContentDictionary = filePackageReader.GetFilenameFileContentDictionaryMemory(zippedBuffer);

            foreach (var keyValuePair in filenameFileContentDictionary)
            {
                System.Console.WriteLine("Filename: " + keyValuePair.Key);
                System.Console.WriteLine("Content: " + keyValuePair.Value);
            }

            //File.WriteAllBytes(packageFilePath, zippedBuffer);
        }


        static void Main2(string[] args)
        {
            var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test.zip";
            var rawFileStream = File.OpenRead(packageFilePath);
            byte[] zippedtoTextBuffer = new byte[rawFileStream.Length];
            rawFileStream.Read(zippedtoTextBuffer, 0, (int)rawFileStream.Length);

            var filePackageReader = new FilePackageReader();
            var filenameFileContentDictionary = filePackageReader.GetFilenameFileContentDictionaryMemory(zippedtoTextBuffer);

            foreach (var keyValuePair in filenameFileContentDictionary)
            {
                System.Console.WriteLine("Filename: " + keyValuePair.Key);
                System.Console.WriteLine("Content: " + keyValuePair.Value);
            }
        }

        static void Main0(string[] args)
        {
            var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test.zip";


            var test1FilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test\\test_1.txt";
            var test2FilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest\\test\\test_2.txt";

            var filePackage = new FilePackage
            {
                FilePath = packageFilePath,
                ContentFilePathList = new List<string>
                {
                    test1FilePath, test2FilePath
                }
            };

            var filePackageWriter = new FilePackageWriter(filePackage);
            filePackageWriter.GeneratePackage(false);

            var filePackageReader = new FilePackageReader();
            var filenameFileContentDictionary = filePackageReader.GetFilenameFileContentDictionary(packageFilePath);

            foreach (var keyValuePair in filenameFileContentDictionary)
            {
                System.Console.WriteLine("Filename: " + keyValuePair.Key);
                System.Console.WriteLine("Content: " + keyValuePair.Value);
            }
        }
    }
}
