/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System;
using DirectShowLib;

namespace TvLibrary
{
  /// <summary>
  /// Class which holds the details about a country like the 
  /// short country id, the full name of the country and the iso code for the country
  /// </summary>
  [Serializable]
  public class Country
  {
    private readonly int _id;
    private int _index;
    private readonly string _name;
    private readonly string _code;
    private AnalogVideoStandard _standard;

    /// <summary>
    /// Initializes a new instance of the <see cref="Country"/> class.
    /// </summary>
    public Country() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="Country"/> class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="country">The country.</param>
    /// <param name="code">The code.</param>
    /// <param name="standard">The AnalogVideoStandard.</param>
    public Country(int id, string country, string code, AnalogVideoStandard standard)
    {
      _id = id;
      _name = country;
      _code = code;
      _standard = standard;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Country"/> class.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="country">The country.</param>
    /// <param name="code">The code.</param>
    /// <param name="index">The index.</param>
    public Country(int id, string country, string code, int index)
    {
      _id = id;
      _name = country;
      _code = code;
      _index = index;
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }

    /// <summary>
    /// Gets the index.
    /// </summary>
    /// <value>The index.</value>
    public int Index
    {
      get { return _index; }
      set { _index = value; }
    }

    /// <summary>
    /// get/sets  the country id
    /// </summary>
    public int Id
    {
      get { return _id; }
    }

    /// <summary>
    /// gets/sets the country name
    /// </summary>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// gets/sets the country code
    /// </summary>
    public string Code
    {
      get { return _code; }
    }

    ///<summary>
    /// Gets/Sets the analog video standard
    ///</summary>
    public AnalogVideoStandard VideoStandard
    {
      get { return _standard; }
      set { _standard = value; }
    }
  }
}