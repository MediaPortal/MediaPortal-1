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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;

namespace TvLibrary
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
          //Log.Log.Debug("  Release {0} returns {1}", o, hr);
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
        //Log.Log.Debug("  Release {0} returns {1}", line, hr);
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
          Log.Log.Error("  Error in Dispose of {0}: {1}", line, ex.Message);
        }

        int remainingReferences = Release.ComObject(o);
        //if (remainingReferences > 0)
        Log.Log.WriteFile("  Release {0} leaves {1} references", line, remainingReferences);

        o = default(E);
      }
    }

    /// <summary>
    /// Disposes a object if possible and sets the reference to null.
    /// </summary>
    /// <param name="o">Object reference to dispose</param>
    public static void DisposeToNull<E>(ref E o)
    {
      System.IDisposable oDisp = o as System.IDisposable;
      if (oDisp != null)
      {
        oDisp.Dispose();
      }
      o = default(E);
    }

    /// <summary>
    /// Disposes a object if possible.
    /// </summary>
    /// <param name="oDisp">IDisposable to dispose</param>
    public static void Dispose(System.IDisposable oDisp)
    {
      if (oDisp != null)
      {
        oDisp.Dispose();
      }
    }
  }
}