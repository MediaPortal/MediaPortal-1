#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
  /// Indicates that a method can be used in XML skin as a function.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  internal class XMLSkinFunctionAttribute : Attribute
  {
    private readonly string _functionName;

    public XMLSkinFunctionAttribute()
    {
      _functionName = null;
    }

    public XMLSkinFunctionAttribute(string functionName)
    {
      _functionName = functionName;
    }

    public string FunctionName
    {
      get { return _functionName; }
    }
  }
}