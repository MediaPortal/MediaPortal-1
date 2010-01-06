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

namespace System.Windows
{
  public struct LocalValueEntry
  {
    #region Constructors

    internal LocalValueEntry(DependencyProperty property, object value)
    {
      _property = property;
      _value = value;
    }

    #endregion Constructors

    #region Properties

    public DependencyProperty Property
    {
      get { return _property; }
    }

    public object Value
    {
      get { return _value; }
    }

    #endregion Properties

    #region Fields

    private DependencyProperty _property;
    private object _value;

    #endregion Fields
  }
}