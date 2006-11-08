#region Copyright (C) 2006 Team MediaPortal
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections.Generic;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Class generic for parser data.
  /// </summary>
  public class ParserData : IParserData
  {
    #region Variables
    Dictionary<string, string> _data;
    #endregion

    #region Constructors/Destructors
    public ParserData()
    {
      _data = new Dictionary<string, string>();
    }
    #endregion

    #region Properties
    public int Count
    {
      get { return _data.Count; }
    }
    #endregion

    #region Public Methods
    public string GetElement(string tag)
    {
      return _data[tag];
    }

    public string GetKey(int index)
    {
      Dictionary<string, string>.Enumerator enumerator = _data.GetEnumerator();

      enumerator.MoveNext();
      for (int i = 0; i < index; i++)
        enumerator.MoveNext();

      return enumerator.Current.Key;
    }

    public string GetData(int index)
    {
      Dictionary<string, string>.Enumerator enumerator = _data.GetEnumerator();

      enumerator.MoveNext();
      for (int i = 0; i < index; i++)
        enumerator.MoveNext();

      return enumerator.Current.Value;
    }
    #endregion

    #region IParserData Implementations
    public void SetElement(string tag, string value)
    {
      _data.Add(tag, value);
    }
    #endregion
  }
}
