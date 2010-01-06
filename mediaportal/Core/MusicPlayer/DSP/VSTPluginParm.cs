#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Xml.Serialization;

namespace MediaPortal.Player.DSP
{
  /// <summary>
  /// This class holds information about a Paramter of a VST Plugin
  /// </summary>
  [Serializable]
  public class VSTPluginParm
  {
    /// <summary>
    /// Needed for XmlSerializer
    /// </summary>
    public VSTPluginParm() {}

    public VSTPluginParm(string name, int index, string value)
    {
      Name = name;
      Index = index;
      Value = value;
    }

    /// <summary>
    /// Name of the Parameter
    /// </summary>
    [XmlAttribute] public string Name = "";

    /// <summary>
    /// Index of the Parameter
    /// </summary>
    [XmlAttribute] public int Index = 0;

    /// <summary>
    /// Value of the Parameter
    /// </summary>
    [XmlAttribute] public string Value = "";
  }
}