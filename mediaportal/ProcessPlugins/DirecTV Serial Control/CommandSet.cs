/* 
 *	Copyright (C) 2005-2009 Team MediaPortal - micheloe, patrick, diehard2
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

namespace DirecTV
{
  public class CommandSet
  {
    public string Name = string.Empty;
    public Command POWER_OFF;
    public Command POWER_ON;
    public Command SHOW_TEXT;
    public Command HIDE_TEXT;
    public Command GET_CHANNEL_NUMBER;
    public Command COLD_BOOT;
    public Command WARM_BOOT;
    public Command GET_SIGNAL_STRENGTH;
    public Command GET_DATE_TIME_DAY_OF_WEEK;
    public Command ENABLE_IR_REMOTE;
    public Command DISABLE_IR_REMOTE;
    public Command REMOTE_CONTROL_KEY;
    public Command SET_CHANNEL_NUMBER;
    public Command DISPLAY_TEXT;

    public CommandSet(
      string name,
      Command pwrOff,
      Command pwrOn,
      Command showText,
      Command hideText,
      Command getChanNum,
      Command coldBoot,
      Command warmBoot,
      Command getSignalStrength,
      Command getDateTimeDow,
      Command enableIrRemote,
      Command disableIrRemote,
      Command remoteControlKey,
      Command setChanNum,
      Command displayText
      )
    {
      this.Name = name;
      this.POWER_OFF = pwrOff;
      this.POWER_ON = pwrOn;
      this.SHOW_TEXT = showText;
      this.HIDE_TEXT = hideText;
      this.GET_CHANNEL_NUMBER = getChanNum;
      this.COLD_BOOT = coldBoot;
      this.WARM_BOOT = warmBoot;
      this.GET_SIGNAL_STRENGTH = getSignalStrength;
      this.GET_DATE_TIME_DAY_OF_WEEK = getDateTimeDow;
      this.ENABLE_IR_REMOTE = enableIrRemote;
      this.DISABLE_IR_REMOTE = disableIrRemote;
      this.REMOTE_CONTROL_KEY = remoteControlKey;
      this.SET_CHANNEL_NUMBER = setChanNum;
      this.DISPLAY_TEXT = displayText;
    }
  }
}