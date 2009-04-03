#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#region usings

using MediaPortal.GUI.Library;

#endregion

namespace TvPlugin
{
  public class TVGuide : TvGuideBase
  {
    [SkinControl(98)] protected GUIImage videoBackground;
    [SkinControl(99)] protected GUIVideoControl videoWindow;

    #region Ctor

    public TVGuide()
      : base()
    {
      GetID = (int) Window.WINDOW_TVGUIDE;
    }

    #endregion

    #region Overrides

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\mytvguide.xml");
      GetID = (int) Window.WINDOW_TVGUIDE;
      Initialize();
      return result;
    }

    public override void OnAdded()
    {
      Log.Debug("TVGuide:OnAdded");
      GUIWindowManager.Replace((int) Window.WINDOW_TVGUIDE, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    public override bool IsTv
    {
      get { return true; }
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
    }

    #endregion
  }
}