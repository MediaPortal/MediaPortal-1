using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class ActionItemCollection
    {
        public ActionItemCollection()
        {
            Items = new List<ActionItem>();

        }

        public List<ActionItem> Items { get; set; }

        public void Add(ActionItem sectionItem)
        {
            Items.Add(sectionItem);
        }
    }
}
