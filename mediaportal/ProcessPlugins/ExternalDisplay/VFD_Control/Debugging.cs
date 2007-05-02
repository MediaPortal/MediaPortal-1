using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.VFD_Control
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
        ResultString = "\n" + FunctionName + "\n" + "  Result = " + ResultString.TrimEnd('\0') + "\n";
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return ResultString;
    }
  }
}