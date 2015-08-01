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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu
{
  /// <summary>
  /// CiMenuEntry class to store a single entry
  /// </summary>
  [Serializable]
  public class CiMenuEntry
  {
    private readonly Int32 _index;
    private readonly String _message;

    /// <summary>
    /// Index of menu entry
    /// </summary>
    public int Index
    {
      get { return _index; }
    }

    /// <summary>
    /// Message of menu entry
    /// </summary>
    public String Message
    {
      get { return _message; }
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="index">Index of entry</param>
    /// <param name="message">Message</param>
    public CiMenuEntry(Int32 index, String message)
    {
      _index = index;
      _message = message;
    }

    /// <summary>
    /// Formatted choice text
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("{0}) {1}", _index, _message);
    }
  }
}