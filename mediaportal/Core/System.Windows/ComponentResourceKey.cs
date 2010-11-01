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

using System.Reflection;

namespace System.Windows
{
  public class ComponentResourceKey : ResourceKey
  {
    #region Constructors

    public ComponentResourceKey() {}

    public ComponentResourceKey(Type targetType, object id)
    {
      _targetType = targetType;
      _id = id;
    }

    #endregion Constructors

    #region Properties

    public override Assembly Assembly
    {
      get { return _targetType.Assembly; }
    }

    public object ID
    {
      get { return _id; }
      set { _id = value; }
    }

    public Type TargetType
    {
      get { return _targetType; }
      set { _targetType = value; }
    }

    #endregion Properties

    #region Fields

    private object _id;
    private Type _targetType;

    #endregion Fields
  }
}