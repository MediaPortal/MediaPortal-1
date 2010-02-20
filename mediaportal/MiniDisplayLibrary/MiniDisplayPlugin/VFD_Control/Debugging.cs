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
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.VFD_Control
{
  public class Debugging
  {
    /// <summary>
    /// Get text that describes the result of an API call.
    /// </summary>
    /// <param name="FunctionName">the name of the API function.</param>
    /// <returns>The text.</returns>
    /// <remarks>
    /// Used only in Debug.Write statements.
    /// </remarks>
    internal static string ResultOfAPICall(string FunctionName)
    {
      string ResultString = string.Empty;

      try
      {
        ResultString = new string('\0', 129);

        // Returns the result code for the last API call.
        int ResultCode = Marshal.GetLastWin32Error();
        long lZero = 0;
        // Get the result message that corresponds to the code.
        int Bytes =
          DebuggingDeclarations.FormatMessage(DebuggingDeclarations.FORMAT_MESSAGE_FROM_SYSTEM, ref lZero, ResultCode, 0,
                                              ResultString, 128, 0);

        // Subtract two characters from the message to strip the CR and LF.
        if (Bytes > 2)
        {
          ResultString = ResultString.Remove(Bytes - 2, 2);
        }

        // Create the string to return.
        ResultString = FunctionName + "  Result = " + ResultString.TrimEnd('\0');
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return ResultString;
    }
  }
}