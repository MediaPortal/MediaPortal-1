using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public interface IDeployDialog
  {
    DeployDialog GetNextDialog();
  }
}
