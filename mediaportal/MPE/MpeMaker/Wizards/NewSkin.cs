using System;
using System.Collections.Generic;
using System.Text;
using MpeCore;
using MpeMaker.Wizards.Skin_wizard;

namespace MpeMaker.Wizards
{
    public class NewSkin
    {
        private static List<IWizard> screens = new List<IWizard>();
        
        public static PackageClass Get(PackageClass packageClass)
        {
            screens.Add(new WizardSkinSelect());
            PackageClass pak = new PackageClass();
            foreach (var screen in screens)
            {
                screen.Execute(packageClass);
            }
            return pak;
        }
    }
}
