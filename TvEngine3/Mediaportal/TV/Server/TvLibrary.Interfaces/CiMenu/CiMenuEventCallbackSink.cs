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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu
{
  /// <summary>
  /// This class is used by client to provide delegates to the server that will
  /// fire events back through these delegates. Overriding OnServerEvent to capture
  /// the callback from the server
  /// </summary>
  public abstract class CiMenuEventCallbackSink : ICiMenuEventCallback
  {
    /// <summary>
    /// Called by the server to fire the call back to the client
    /// </summary>
    /// <param name="menu">a CiMenu object</param>
    public void FireCiMenuCallback(CiMenu menu)
    {
      //Console.WriteLine("Activating callback");
      CiMenuCallback(menu);
    }

    /// <summary>
    /// Client overrides this method to receive the callback events from the server
    /// </summary>
    /// <param name="menu">a CiMenu object</param>
    public abstract void CiMenuCallback(CiMenu menu);
  }
}