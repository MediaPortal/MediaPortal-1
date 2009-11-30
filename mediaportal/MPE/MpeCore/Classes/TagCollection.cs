using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class TagCollection
    {
        public TagCollection()
        {
            Tags = new List<string>();
        }
        public List<string> Tags { get; set; }
    }
}
