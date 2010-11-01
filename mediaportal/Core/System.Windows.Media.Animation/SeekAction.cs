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

namespace System.Windows.Media.Animation
{
  public class SeekAction : TimelineAction
  {
    #region Constructors

    public SeekAction() {}

    #endregion Constructors

    #region Properties

    public TimeSpan Offset
    {
      get { return _offset; }
      set { _offset = value; }
    }

    public TimeSeekOrigin Origin
    {
      get { return _origin; }
      set { _origin = value; }
    }

    #endregion Properties

    #region Fields

    private TimeSpan _offset;
    private TimeSeekOrigin _origin;

    #endregion Fields
  }
}