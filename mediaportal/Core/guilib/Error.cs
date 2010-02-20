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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for Error.
  /// </summary>
  public class Error
  {
    private static string errorReason = string.Empty;
    private static string errorDescription = string.Empty;

    public static string Description
    {
      get { return errorDescription; }
      set
      {
        if (value == null)
        {
          return;
        }
        errorDescription = value;
      }
    }

    public static string Reason
    {
      get { return errorReason; }
      set
      {
        if (value == null)
        {
          return;
        }
        errorReason = value;
      }
    }

    public static int ReasonId
    {
      set { Reason = GUILocalizeStrings.Get(value); }
    }

    public static int DescriptionId
    {
      set { Description = GUILocalizeStrings.Get(value); }
    }

    public static void SetError(string reason, string description)
    {
      Reason = reason;
      Description = description;
    }

    public static void SetError(int reasonId, int descriptionId)
    {
      ReasonId = reasonId;
      DescriptionId = descriptionId;
    }

    public static void Clear()
    {
      Reason = string.Empty;
      Description = string.Empty;
    }
  }
}