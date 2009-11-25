using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Interfaces;

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

        public SectionItem Add(string name)
        {
            SectionItem item = new SectionItem();
            ISectionPanel panel = MpeInstaller.SectionPanels[name];
            if (panel == null)
                return null;
            item.Name = panel.DisplayName;
            item.PanelName = panel.DisplayName;
            item.Params = panel.GetDefaultParams();
            Add(item);
            return item;
        }
    }
}
