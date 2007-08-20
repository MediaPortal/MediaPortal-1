using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.MPInstaller;

namespace MediaPortal.MPInstaller
{
  public interface IMPIInternalPlugin
  {
    bool OnStartInstall(ref MPpackageStruct pk);
    bool OnEndInstall(ref MPpackageStruct pk);
    bool OnStartUnInstall(ref MPpackageStruct pk);
    bool OnEndUnInstall(ref MPpackageStruct pk);
  }
}
