using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public static class NetAclHelper
  {
    public static void AddAddress(string address)
    {
      AddAddress(address, Environment.UserDomainName, Environment.UserName);
    }

    public static void AddAddress(string address, string domain, string user)
    {
      string args = string.Format(@"http add urlacl url={0} user={1}\{2}", address, domain, user);
      ProcessStartInfo psi = new ProcessStartInfo("netsh", args)
        {
          Verb = "runas", 
          CreateNoWindow = true, 
          WindowStyle = ProcessWindowStyle.Hidden, 
          UseShellExecute = true
        };

      Process.Start(psi).WaitForExit();
    }
  }
}
