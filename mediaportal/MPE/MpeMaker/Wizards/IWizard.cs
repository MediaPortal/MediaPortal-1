using System;
using System.Collections.Generic;
using System.Text;
using MpeCore;

namespace MpeMaker.Wizards
{
  internal interface IWizard
  {
    bool Execute(PackageClass packageClass);
  }
}