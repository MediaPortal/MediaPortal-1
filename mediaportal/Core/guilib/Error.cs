#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

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