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

using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public enum ValueTypeEnum
  {
    String,
    File,
    Template,
    Bool,
    Script
  }

  public class SectionParam
  {
    public SectionParam()
    {
      Name = string.Empty;
      Value = string.Empty;
      ValueType = ValueTypeEnum.String;
      Description = string.Empty;
    }

    public SectionParam(SectionParam sectionParam)
    {
      Name = sectionParam.Name;
      Value = sectionParam.Value;
      ValueType = sectionParam.ValueType;
      Description = sectionParam.Description;
    }


    public SectionParam(string name, string value, ValueTypeEnum valueType, string description)
    {
      Name = name;
      Value = value;
      ValueType = valueType;
      Description = description;
    }

    [XmlAttribute]
    public string Name { get; set; }

    public string Value { get; set; }
    public ValueTypeEnum ValueType { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Gets the value as a real path. 
    /// This function only usable if the type is Template
    /// </summary>
    /// <returns></returns>
    public string GetValueAsPath()
    {
      return MpeInstaller.TransformInRealPath(Value);
    }

    /// <summary>
    /// Gets the value as bool.
    /// This function only usable if the type is Bool 
    /// </summary>
    /// <returns></returns>
    public bool GetValueAsBool()
    {
      if (this.Value.ToUpper() == "YES")
        return true;
      return false;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}