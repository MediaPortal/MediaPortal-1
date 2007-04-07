using System;
using System.Collections.Generic;
using System.Text;

namespace MyTv
{
  class Thumbs
  {
    static public readonly string TvNotifyIcon = @"media\images\tvguide_notify_button.png";
    static public readonly string TvRecordingIcon = @"media\images\tvguide_record_button.png";
    static public readonly string TvRecordingSeriesIcon = @"media\images\tvguide_recordserie_button.png";
    static public readonly string TvConflictRecordingIcon = @"media\images\tvguide_recordconflict_button.png";
    static public readonly string TvConflictRecordingSeriesIcon = @"media\images\tvguide_recordserie_conflict_button.png";

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
