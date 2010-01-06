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
  /// Class which hold information about a key press
  /// </summary>
  public class Key
  {
    private int m_iChar = 0; // character 
    private int m_iCode = 0; // character code 

    /// <summary>
    /// empty constructor
    /// </summary>
    public Key() {}

    /// <summary>
    /// copy constructor
    /// </summary>
    /// <param name="key"></param>
    public Key(Key key)
    {
      m_iChar = key.KeyChar;
      m_iCode = key.KeyCode;
    }

    public Key(int iChar, int iCode)
    {
      m_iChar = iChar;
      m_iCode = iCode;
    }

    public int KeyChar
    {
      get { return m_iChar; }
    }

    public int KeyCode
    {
      get { return m_iCode; }
    }
  }
}