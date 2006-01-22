/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion


namespace MediaPortal.TV.Recording
{
  public class StopTimeShiftingCommand : CardCommand
  {
    public virtual void Execute(CommandProcessor handler)
    {
      Log.WriteFile(Log.LogType.Recorder, "Command:Stop timeshifting");
      for (int i=0; i < handler.TVCards.Count;++i)
      {
        TVCaptureDevice card = handler.TVCards[i];
        if (!card.IsRecording)
        {
          if (card.IsTimeShifting)
          {
            Log.WriteFile(Log.LogType.Recorder, "Recorder: stop timeshifting on card:{0} channel:{1}",
                              card.ID, card.TVChannel);
            card.Stop();
          }
        }
      }
    }
  }
}
