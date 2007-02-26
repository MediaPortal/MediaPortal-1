using System;
using System.Collections.Generic;
using System.Text;

namespace DirecTV
{
  public class CommandSet
  {
    public string Name = String.Empty;
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
