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
using MediaPortal.Services;

#endregion

namespace MediaPortal.TV.Recording
{
  public class SetAudioLanguageCommand : CardCommand
  {
    protected int _audioPid;

    public SetAudioLanguageCommand(int audioPid)
    {
      _audioPid = audioPid;
    }

    public int AudioPid
    {
      get { return _audioPid; }
      set { _audioPid = value; }
    }

    public override void Execute(CommandProcessor handler)
    {
      if (handler.TVCards.Count == 0)
      {
        ErrorMessage = GUILocalizeStrings.Get(753); // "No tuner cards installed";
        Succeeded = false;
        return;
      }
      if (handler.CurrentCardIndex < 0)
      {
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(755); // "No tuner is viewing tv";
        return;
      }
      Log.WriteFile(LogType.Recorder, "Command:Select audio stream with pid: 0x{0:X}", AudioPid);
      TVCaptureDevice device = handler.TVCards[handler.CurrentCardIndex];
      device.SetAudioLanguage(AudioPid);
      Succeeded = true;
    }
  }
}