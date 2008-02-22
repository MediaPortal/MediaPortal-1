using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace MediaPortal.DeployTool
{
  class InstallationProperties: NameValueCollection
  {
    #region Singleton implementation
    static readonly InstallationProperties _instance = new InstallationProperties();
    static InstallationProperties()
    {
    }
    InstallationProperties() 
      : base()
    {

    }
    public static InstallationProperties Instance
    {
      get
      {
        return _instance;
      }
    }
    #endregion
  }
}
