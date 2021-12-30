using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BearMLLib.Core
{
    internal class Writer
    {
        private readonly string _path;

        internal Writer(string path)
        {
            _path = path;
        }

        
        internal void Write(ConfigsContainer container)
        {
            File.WriteAllLines(_path, container.Raw);
        }
    }
}
