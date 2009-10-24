using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes.SectionPanel;

namespace MpeCore.Classes
{
    public class WizardNavigator
    {
        public PackageClass Package { get; set; }
        private Stack<int> stack = new Stack<int>();
        public SectionResponseEnum Response { get; set; }


        public WizardNavigator(PackageClass pak)
        {
            Package = pak;
            Response = SectionResponseEnum.Error;
        }

        public SectionResponseEnum Navigate()
        {
            int pos = 0;
            while (pos < Package.Sections.Items.Count)
            {
                SectionItem currentItem = Package.Sections.Items[pos];
                if (!string.IsNullOrEmpty(currentItem.ConditionGroup))
                {
                    if (!Package.Groups[currentItem.ConditionGroup].Checked)
                    {
                        pos++;
                        continue;
                    }
                }
                Response = MpeInstaller.SectionPanels[currentItem.PanelName].Execute(Package,
                                                                                     currentItem);
                switch (Response)
                {
                    case SectionResponseEnum.Back:
                        pos = stack.Pop();
                        break;
                    case SectionResponseEnum.Next:
                        stack.Push(pos);
                        pos++;
                        break;
                    case SectionResponseEnum.Cancel:
                        break;
                    case SectionResponseEnum.Ok:
                        break;
                    case SectionResponseEnum.Error:
                        break;
                }
                if (Response != SectionResponseEnum.Back && Response != SectionResponseEnum.Next)
                {
                    break;
                }
            }

            return SectionResponseEnum.Ok;
        }
    }
}
