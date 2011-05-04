#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Windows.Forms;
#if MediaPortal
using MediaPortal.ServiceImplementations;

#else
using TvLibrary.Log;

#endif

namespace OSPrerequisites
{
  ///<summary>
  /// OS related checks
  ///</summary>
  public class OSPrerequisites
  {
    private const string MSG_NOT_SUPPORTED =
      "Your platform is not supported by MediaPortal Team because it lacks critical hotfixes! \nPlease check our Wiki's requirements page.";

    private const string MSG_NOT_INSTALLABLE =
      "Your platform is not supported and cannot be used for MediaPortal/TV-Server! \nPlease check our Wiki's requirements page.";

    private const string MSG_BETA_SERVICE_PACK =
      "You are running a BETA version of Service Pack {0}.\n Please don't do bug reporting with such configuration.";

    ///<summary>
    /// Log and warn user if OS is not supported or is blacklisted
    ///</summary>
    public static void OsCheck(bool dispMessage)
    {
      DialogResult res;

      switch (OSInfo.OSInfo.GetOSSupported())
      {
        case OSInfo.OSInfo.OsSupport.Blocked:
          Log.Error("*******************************************");
          Log.Error("* ERROR, OS can't be used for MediaPortal *");
          Log.Error("*******************************************");
          if (dispMessage)
          {
            MessageBox.Show(MSG_NOT_INSTALLABLE, OSInfo.OSInfo.GetOSDisplayVersion(), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
          }
          Environment.Exit(-100);
          break;
        case OSInfo.OSInfo.OsSupport.NotSupported:
          //Used .Info as .Warning is missing
          Log.Info("*******************************************");
          Log.Info("* WARNING, OS not officially supported    *");
          Log.Info("*******************************************");
          if (dispMessage)
          {
            res = MessageBox.Show(MSG_NOT_SUPPORTED, OSInfo.OSInfo.GetOSDisplayVersion(), MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning);
            if (res == DialogResult.Cancel) Environment.Exit(-200);
          }
          break;
        default:
          break;
      }
      if (dispMessage && OSInfo.OSInfo.OSServicePackMinor != 0)
      {
        res = MessageBox.Show(MSG_BETA_SERVICE_PACK, OSInfo.OSInfo.GetOSDisplayVersion(), MessageBoxButtons.OKCancel,
                              MessageBoxIcon.Warning);
        if (res == DialogResult.Cancel) Environment.Exit(-300);
      }
    }
  }
}