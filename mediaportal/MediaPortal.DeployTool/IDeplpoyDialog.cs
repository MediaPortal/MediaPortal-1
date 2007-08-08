using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace MediaPortal.DeployTool
{
  public interface IDeployDialog
  {
    DeployDialog GetNextDialog();

    bool SettingsValid();
    void SetProperties();
  }
}
