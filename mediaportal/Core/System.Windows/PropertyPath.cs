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

using System.ComponentModel;

namespace System.Windows
{
  [TypeConverter(typeof (PropertyPathConverter))]
  public sealed class PropertyPath
  {
    #region Constructors

    public PropertyPath() {}

    private PropertyPath(string path, object[] propertyInfoArray)
    {
      _path = path;
      _propertyInfoArray = propertyInfoArray;
    }

    #endregion Constructors

    #region Properties

    public string Path
    {
      get { return _path; }
      set
      {
        if (string.Compare(_path, value, true) == 0)
        {
          _propertyInfoArray = null;
        }
      }
    }

    #endregion Properties

    #region Fields

    private string _path = string.Empty;
    private object[] _propertyInfoArray;

    #endregion Fields
  }
}