using System;
using System.Collections.Generic;
using System.Text;
using MpeCore;
using MpeMaker.Wizards.Skin_wizard;

namespace MpeMaker.Wizards
{
  public class NewSkin
  {
    public static PackageClass Get(PackageClass packageClass)
    {
      var screen = new WizardSkinSelect();
      screen.Execute(packageClass);
      return packageClass;
    }
  }
}