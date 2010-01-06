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

namespace System.Windows.Controls
{
  public class DataTemplate : FrameworkTemplate
  {
    #region Constructors

    public DataTemplate() {}

    public DataTemplate(object dataType)
    {
      _dataType = dataType;
    }

    #endregion Constructors

    #region Methods

    protected override void ValidateTemplatedParent(FrameworkElement templatedParent)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public object DataType
    {
      get { return _dataType; }
      set { _dataType = value; }
    }

    #endregion Properties

    #region Fields

    private object _dataType;

    #endregion Fields
  }
}