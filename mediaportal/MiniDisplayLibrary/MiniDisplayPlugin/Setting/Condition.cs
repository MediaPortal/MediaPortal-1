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

using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlInclude(typeof (IsNullCondition)), XmlInclude(typeof (AndCondition)),
   XmlInclude(typeof (OrCondition)), XmlInclude(typeof (NotNullCondition))]
  public abstract class Condition
  {
    [XmlIgnore] protected Property Property;

    protected Condition() {}

    public abstract bool Evaluate();

    [DefaultValue(""), XmlAttribute]
    public string Value
    {
      get
      {
        if (this.Property == null)
        {
          return "";
        }
        return this.Property.value;
      }
      set { this.Property = new Property(value); }
    }
  }
}