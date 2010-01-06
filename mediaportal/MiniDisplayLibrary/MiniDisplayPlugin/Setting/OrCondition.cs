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
using System.Collections;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlType("Or")]
  public class OrCondition : Condition
  {
    private ArrayList m_Conditions = new ArrayList();

    public override bool Evaluate()
    {
      for (int i = 0; i < this.m_Conditions.Count; i++)
      {
        if (((Condition)this.m_Conditions[i]).Evaluate())
        {
          return true;
        }
      }
      return false;
    }

    [XmlArray, XmlArrayItem(typeof (Condition))]
    public IList Conditions
    {
      get { return this.m_Conditions; }
    }
  }
}