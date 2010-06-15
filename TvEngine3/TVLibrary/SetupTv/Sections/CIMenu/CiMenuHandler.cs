#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace SetupTv.Sections
{

  #region CI Menu

  /// <summary>
  /// Handler class for gui interactions of ci menu
  /// </summary>
  public class CiMenuHandler : CiMenuCallbackSink
  {
    private CI_Menu_Dialog refDlg;

    public void SetCaller(CI_Menu_Dialog caller)
    {
      refDlg = caller;
    }

    /// <summary>
    /// eventhandler to show CI Menu dialog
    /// </summary>
    /// <param name="Menu"></param>
    protected override void CiMenuCallback(CiMenu Menu)
    {
      try
      {
        Log.Debug("Callback from tvserver {0}", Menu.Title);

        // pass menu to calling dialog
        if (refDlg != null)
          refDlg.CiMenuCallback(Menu);
      }
      catch
      {
        Menu = new CiMenu("Remoting Exception", "Communication with server failed", null,
                          TvLibrary.Interfaces.CiMenuState.Error);
        // pass menu to calling dialog
        if (refDlg != null)
          refDlg.CiMenuCallback(Menu);
      }
    }
  }

  #endregion
}