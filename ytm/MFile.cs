using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ytm
{
    public class MFile
    {
        public string FullPath { get; set; }
        public string Album { get; set; }
        public string Name { get; set; }

        public string ImageFile { get; set; }

        public bool IsSame(MFile other)
        {
            return other.Album.Equals(Album, StringComparison.InvariantCultureIgnoreCase)
                   && other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}