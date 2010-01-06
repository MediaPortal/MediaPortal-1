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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Indicates that a field can be initialized from XML skin data.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class XMLSkinElementAttribute : Attribute
  {
    private string m_xmlElementName;

    public XMLSkinElementAttribute(string xmlElementName)
    {
      m_xmlElementName = xmlElementName;
    }

    public string XmlElementName
    {
      get { return m_xmlElementName; }
    }
  }

  /// <summary>
  /// Indicates that a field can be initialized from XML skin data.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class XMLSkinAttribute : Attribute
  {
    private string _xmlElementName;
    private string _attributeName;

    public XMLSkinAttribute(string xmlElementName, string attributeName)
    {
      _xmlElementName = xmlElementName;
      _attributeName = attributeName;
    }

    public string XmlElementName
    {
      get { return _xmlElementName; }
    }

    public string XmlAttributeName
    {
      get { return _attributeName; }
    }
  }
}