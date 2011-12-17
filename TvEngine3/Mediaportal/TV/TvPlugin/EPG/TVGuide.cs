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

#region usings

using MediaPortal.GUI.Library;

#endregion

namespace Mediaportal.TV.TvPlugin.EPG
{
  public sealed class TVGuide : TvGuideBase
  {
    [SkinControl(98)] private GUIImage _videoBackground;
    [SkinControl(99)] private GUIVideoControl _videoWindow;

    #region Ctor

    public TVGuide()
    {
      GetID = (int)Window.WINDOW_TVGUIDE;
    }

    #endregion

    #region Overrides

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\mytvguide.xml");
      GetID = (int)Window.WINDOW_TVGUIDE;
      return result;
    }

    public override bool IsTv
    {
      get { return true; }
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    #endregion
  }
}