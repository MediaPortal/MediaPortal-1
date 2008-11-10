using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class InternetChecker
  {

    InternetChecker()
    {
    }
    
    public static bool ConnectionExists()
    {
      try
      {
        TcpClient clnt = new TcpClient("www.google.com", 80);
        clnt.Close();
        return true;
      }
      catch (System.Exception ex)
      {
        return false;
      }
    }

  }
}
