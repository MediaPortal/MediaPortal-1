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
using MediaPortal.Player;
using MediaPortal.Services;

#endregion

namespace MediaPortal.TV.Recording
{
  public class StopRadioCommand : CardCommand
  {
    public override void Execute(CommandProcessor handler)
    {
      bool stopped = false;

      //Log.WriteFile(LogType.Recorder, "Command:Stop radio");

      if (g_Player.Playing && g_Player.IsRadio)
      {
        handler.StopPlayer();
      }

      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753); //"No tuner cards installed";
        Succeeded = false;
        return;
      }
      for (int i = 0; i < handler.TVCards.Count; ++i)
      {
        TVCaptureDevice dev = handler.TVCards[i];
        if (dev.IsRadio)
        {
          Log.WriteFile(LogType.Recorder, "Recorder: StopRadio() stop radio on card:{0}", dev.CommercialName);
          dev.StopRadio();
          if (i == handler.CurrentCardIndex)
          {
            handler.CurrentCardIndex = -1;
            stopped = true;
          }
        }
      }
      Succeeded = stopped;
    }
  }
}