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
using System.Xml;

namespace System.Windows.Serialization
{
  public class XamlParserException : Exception
  {
    #region Constructors

    public XamlParserException(string message, string filename, XmlTextReader reader)
      : base(string.Format("{0}({1},{2}): {3}", filename, reader.LineNumber, reader.LinePosition, message))
    {
      _lineNumber = reader.LineNumber;
      _linePosition = reader.LinePosition;
    }

    #endregion Constructors

    #region Properties

    public int LineNumber
    {
      get { return _lineNumber; }
    }

    public int LinePosition
    {
      get { return _linePosition; }
    }

    #endregion Properties

    #region Fields

    private int _lineNumber;
    private int _linePosition;

    #endregion Fields
  }
}