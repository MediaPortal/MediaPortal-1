using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public enum CheckResult
  {
    NOT_INSTALLED,
    INSTALLED,
    PACKAGE_MISSING
  }

  interface IInstallationChecker
  {
    string GetDisplayName();
    string GetDownloadURL();
    CheckResult Check();
  }
}
