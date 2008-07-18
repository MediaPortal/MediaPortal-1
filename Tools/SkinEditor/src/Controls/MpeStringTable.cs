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

using System.Collections;

namespace Mpe.Controls
{
  /// <summary>
  /// Summary description for MpeStringTable.
  /// </summary>
  public class MpeStringTable
  {
    #region Variables

    private SortedList strings;
    private string language;

    #endregion

    #region Constructors

    public MpeStringTable(string language)
    {
      strings = new SortedList();
      this.language = language;
    }

    #endregion

    #region Methods

    public void Add(int id, string value)
    {
      strings.Add(id, value);
    }

    public void Clear()
    {
      strings.Clear();
    }

    #endregion

    #region Properties

    public string this[int id]
    {
      get { return (string) strings[id]; }
    }

    public int[] Keys
    {
      get
      {
        ICollection c = strings.Keys;
        int[] ids = new int[c.Count];
        int i = 0;
        IEnumerator e = c.GetEnumerator();
        while (e.MoveNext())
        {
          ids[i++] = (int) e.Current;
        }
        return ids;
      }
    }

    public string Language
    {
      get { return language; }
    }

    #endregion
  }
}