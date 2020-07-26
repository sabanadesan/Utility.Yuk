using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
namespace Utility.Pkg
{
    public class FilePackageReader
    {
        private Dictionary<string, string> _filenameFileContentDictionary;

        public FilePackageReader()
        {

        }

        public Dictionary<string, string> GetFilenameFileContentDictionary(string filepath)
        {
            try
            {
                _filenameFileContentDictionary = new Dictionary<string, string>();

                // Open the package file
                using (var fs = new FileStream(filepath, FileMode.Open))
                {
                    // Open the package file as a ZIP
                    using (var archive = new ZipArchive(fs))
                    {
                        // Iterate through the content files and add them to a dictionary
                        foreach (var zipArchiveEntry in archive.Entries)
                        {
                            using (var stream = zipArchiveEntry.Open())
                            {
                                using (var zipSr = new StreamReader(stream))
                                {
                                    _filenameFileContentDictionary.Add(zipArchiveEntry.Name, zipSr.ReadToEnd());
                                }
                            }
                        }
                    }
                }

                return _filenameFileContentDictionary;
            }
            catch (Exception e)
            {
                var errorMessage = "Unable to open/read the package. " + e.Message;
                throw new Exception(errorMessage);
            }
        }

        public Dictionary<string, string> GetFilenameFileContentDictionaryMemory(byte[] zippedBuffer)
        {
            try
            {
                _filenameFileContentDictionary = new Dictionary<string, string>();

                    // Open the package file
                    using (var zs = new MemoryStream(zippedBuffer))
                    {
                    // Open the package file as a ZIP
                    using (var archive = new ZipArchive(zs))
                    {
                        // Iterate through the content files and add them to a dictionary
                        foreach (var zipArchiveEntry in archive.Entries)
                        {
                            using (var stream = zipArchiveEntry.Open())
                            {
                                using (var ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    var unzippedArray = ms.ToArray();

                                    _filenameFileContentDictionary.Add(zipArchiveEntry.Name, Encoding.Default.GetString(unzippedArray));
                                }
                            }
                        }
                    }
                }

                return _filenameFileContentDictionary;
            }
            catch (Exception e)
            {
                var errorMessage = "Unable to open/read the package. " + e.Message;
                throw new Exception(errorMessage);
            }
        }
    }
}
