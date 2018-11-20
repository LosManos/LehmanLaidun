using System;
using System.Collections.Generic;

namespace LehmanLaidun.FileSystem.Test
{
    public partial class FileItemTest
    {
        private static IEnumerable<object[]> TestData
        {
            get
            {
                return new[] {
                    // TODO:Randomise data, for instance with VacheTache: https://github.com/LosManos/VacheTache
                    new object[] { @"C:\", "MyFile.jpg", "anything", new DateTime(2018, 10, 16, 21, 43, 33) },
                    new object[] { @"C:\MyFolder", "MyFile.jpg", "anything in file", new DateTime(2018, 10, 16, 21, 43, 34) },
                };
            }
        }
    }
}
