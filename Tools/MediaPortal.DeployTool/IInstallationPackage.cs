using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public enum CheckState
  {
    NOT_INSTALLED,
    INSTALLED,
    NOT_CONFIGURED,
    CONFIGURED,
    NOT_REMOVED,
    REMOVED,
    DOWNLOADED,
    NOT_DOWNLOADED,
    VERSION_MISMATCH,
    VERSION_LOOKUP_FAILED
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
