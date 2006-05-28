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

using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
    /// <summary>
    /// The abstract base class for all values.
    /// </summary>
    [XmlInclude(typeof(Property))]
    [XmlInclude(typeof(Text))]
    [XmlInclude(typeof(Parse))]
    [XmlInclude(typeof(ProcessPlugins.ExternalDisplay.Setting.PerformanceCounter))]
    [Serializable]
    public abstract class Value
    {
        [XmlElement("IsNull", typeof(IsNullCondition))]
        [XmlElement("NotNull", typeof(NotNullCondition))]
        [XmlElement("And", typeof(AndCondition))]
        [XmlElement("Or", typeof(OrCondition))]
        [DefaultValue(null)]
        public Condition Condition = null;

        public abstract string Evaluate();
    }
}