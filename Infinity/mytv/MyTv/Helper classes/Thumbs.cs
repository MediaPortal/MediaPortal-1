using System;
using System.Collections.Generic;
using System.Text;

namespace MyTv
{
  class Thumbs
  {
    static public readonly string TvNotifyIcon = @"skin\default\gfx\tvguide_notify_button.png";
    static public readonly string TvRecordingIcon = @"skin\default\gfx\tvguide_record_button.png";
    static public readonly string TvRecordingSeriesIcon = @"skin\default\gfx\tvguide_recordserie_button.png";
    static public readonly string TvConflictRecordingIcon = @"skin\default\gfx\tvguide_recordconflict_button.png";
    static public readonly string TvConflictRecordingSeriesIcon = @"skin\default\gfx\tvguide_recordserie_conflict_button.png";

    static public string GetLogoFileName(string name)
    {
        string logoName = name.Replace(@"\", "_");
        logoName = name.Replace("/", "_");
        logoName = name.Replace("*", "_");
        logoName = name.Replace(":", "_");
        logoName = name.Replace("\"", "_");
        logoName = name.Replace("<", "_");
        logoName = name.Replace(">", "_");
        logoName = String.Format(@"thumbs\{0}.png", logoName);
        return logoName;
    }
  }
}
