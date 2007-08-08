using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public enum CheckResult
  {
    NOT_INSTALLED,
    INSTALLED,
    PACKAGE_MISSING,
    VERSION_MISMATCH
  }

  interface IInstallationPackage
  {
    string GetDisplayName();

    bool Download();
    bool Install();
    bool UnInstall();
    CheckResult CheckStatus();
  }
}
