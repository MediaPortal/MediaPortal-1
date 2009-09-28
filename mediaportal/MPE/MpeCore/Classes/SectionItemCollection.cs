using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class SectionItemCollection
    {
        public SectionItemCollection()
        {
            Items = new List<SectionItem>();

        }

        public List<SectionItem> Items { get; set; }

        public void Add(SectionItem sectionItem)
        {
            Items.Add(sectionItem);
        }
    }
}
