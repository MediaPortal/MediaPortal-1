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

using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// Builds GUIListItem from a Cue track
  /// </summary>
  internal class CueTrackFileGUIListItemBuilder : ICueTrackFileBuilder<GUIListItem>
  {
    #region Public Methods

    public string getFileName(GUIListItem fobj)
    {
      return fobj.Path;
    }

    public GUIListItem build(string cueFileName, CueSheet cueSheet, Track track)
    {
      GUIListItem res = new GUIListItem();
      res.IsFolder = false;
      res.Path = CueUtil.buildCueFakeTrackFileName(cueFileName, track);
      MediaPortal.Util.Utils.SetDefaultIcons(res);
      if (track.Performer != null && track.Performer != cueSheet.Performer)
      {
        res.Label = track.Performer + " - " + track.Title;
      }
      else
      {
        res.Label = track.Title;
      }

      res.Label = track.TrackNumber.ToString("00") + " " + res.Label;

      return res;
    }

    #endregion
  }
}