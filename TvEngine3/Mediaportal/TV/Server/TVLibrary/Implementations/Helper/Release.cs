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

using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  /// <summary>
  /// Helper class for releasing a com object
  /// </summary>
  public class Release
  {


    public static int ComObject(object o)
    {
      int hr = 0;
      if (o != null)
      {
        DsUtils.ReleaseComObject(o);
        if (hr != 0)
        {
          //StackTrace st = new StackTrace(true);
          //Log.this.LogDebug("  Release {0} returns {1}", o, hr);
        }
      }
      return hr;
    }

    /// <summary>
    /// Releases a com object
    /// </summary>
    /// <param name="line">Log line input</param>
    /// <param name="o">Object to release</param>
    public static void ComObject(string line, object o)
    {
      if (o != null)
      {
        DsUtils.ReleaseComObject(o);
        //Log.this.LogDebug("  Release {0} returns {1}", line, hr);
      }
    }
    /// <summary>
    /// Releases a com object
    /// </summary>
    /// <param name="line">Log line input</param>
    /// <param name="o">Object to release</param>
    public static void ComObjectToNull<E>(string line, ref E o)
    {
      if (o != null)
      {
        // check if object is disposable
        try
        {
          if (o is System.IDisposable)
          {
            (o as System.IDisposable).Dispose();
          }
        }
        catch (System.Exception ex)
        {
          Log.Error(ex, "  Error in Dispose of {0}", line);
        }

        int remainingReferences = Release.ComObject(o);
        //if (remainingReferences > 0)
        Log.Debug("  Release {0} leaves {1} references", line, remainingReferences);

        o = default(E);
      }
    }   
  }
}