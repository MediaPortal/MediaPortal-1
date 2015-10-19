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
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception
{
  /// <summary>
  /// Exception thrown by the TV library when a DirectShow error is encountered.
  /// </summary>
  [Serializable]
  public class TvExceptionDirectShowError : TvException
  {
    [DllImport("quartz.dll", CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
    private static extern int AMGetErrorText(int hr, StringBuilder buf, int max);

    private const int MAX_ERROR_TEXT_LEN = 160;

    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionDirectShowError"/> class.
    /// </summary>
    /// <param name="hresult">The HRESULT code.</param>
    /// <param name="message">A contextual message.</param>
    /// <param name="args">The context message arguments.</param>
    public TvExceptionDirectShowError(int hresult, string message, params object[] args)
      : base(message, args)
    {
    }

    /// <summary>
    /// Throw an exception for a non-S_OK DirectShow HRESULT code.
    /// </summary>
    /// <param name="hresult">The HRESULT code.</param>
    /// <param name="message">A contextual message.</param>
    /// <param name="args">The context message arguments.</param>
    public static void Throw(int hresult, string message, params object[] args)
    {
      if (hresult == 0) // S_OK
      {
        return;
      }

      StringBuilder buffer = new StringBuilder(MAX_ERROR_TEXT_LEN, MAX_ERROR_TEXT_LEN);
      string errorDetail = string.Format("HRESULT = 0x{0:x8}", hresult);
      if (AMGetErrorText(hresult, buffer, MAX_ERROR_TEXT_LEN) > 0)
      {
        throw new TvExceptionDirectShowError(hresult, "{0} {1}, description = {2}.", string.Format(message, args), errorDetail, buffer.ToString());
      }
      else
      {
        throw new TvExceptionDirectShowError(hresult, "{0} {1}.", string.Format(message, args), errorDetail);
      }
    }
  }
}