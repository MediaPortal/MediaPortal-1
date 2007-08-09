using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public enum CheckState
  {
    NOT_INSTALLED,
    INSTALLED,
    VERSION_MISMATCH
  }
  public struct CheckResult
  {
    public CheckState state;
    public bool needsDownload;
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
