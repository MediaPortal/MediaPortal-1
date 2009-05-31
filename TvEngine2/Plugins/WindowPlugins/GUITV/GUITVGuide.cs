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

using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

#endregion

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITVGuide : GUITvGuideBase
  {
    [SkinControl(98)] protected GUIImage videoBackground;
    [SkinControl(99)] protected GUIVideoControl videoWindow;

    public GUITVGuide() : base()
    {
      GetID = (int) Window.WINDOW_TVGUIDE;
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\mytvguide_TVE2.xml");
      Initialize();
      return result;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      CheckNewTVGuide();
      // check if there's a new TVguide.xml
      try
      {
        List<TVChannel> channels = new List<TVChannel>();
        TVDatabase.GetChannels(ref channels);
        if (channels.Count == 0)
        {
          StartImportXML();
        }
        channels = null;
      }
      catch (Exception)
      {
      }

      TVDatabase.OnProgramsChanged += new TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
      TVDatabase.OnNotifiesChanged += new TVDatabase.OnChangedHandler(TVDatabase_On_notifyListChanged);
      ConflictManager.OnConflictsUpdated +=
        new ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      TVDatabase.OnProgramsChanged -= new TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
      TVDatabase.OnNotifiesChanged -= new TVDatabase.OnChangedHandler(TVDatabase_On_notifyListChanged);
      ConflictManager.OnConflictsUpdated -=
        new ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);

      base.OnPageDestroy(newWindowId);

      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }
    }
  }
}