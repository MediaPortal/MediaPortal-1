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

#region usings

using MediaPortal.GUI.Library;
using MediaPortal.Playlists;

#endregion

namespace MediaPortal.GUI.Pictures
{
  /// <summary>
  /// </summary>
  public class GUIPictureSlideShow : GUIInternalWindow
  {

    #region variables

    public static int _slideDirection = 0; //-1=backwards, 0=nothing, 1=forward

    #endregion

    public static int SlideDirection
    {
      get { return _slideDirection; }
      set { _slideDirection = value; }
    }

    #region GUIWindow overrides

    public GUIPictureSlideShow()
    {
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\slideshow.xml"));
    }

    #endregion
  }
}
