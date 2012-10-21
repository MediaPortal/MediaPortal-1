using System;
using MediaPortal.GUI.Library;
using WindowPlugins;

namespace TvPlugin
{
  public class RecordedBase : WindowPluginBase
  {

    /// <summary>
    /// Convert how long ago a recording took place into a meaningful description
    /// </summary>
    /// <param name="aStartTime">A recordings start time</param>
    /// <returns>The spoken date label</returns>
    protected static string GetSpokenViewDate(DateTime aStartTime)
    {
      var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
      var lastMonth = thisMonth.AddMonths(-1);

      if (DateTime.Today.Equals(aStartTime.Date))
        return GUILocalizeStrings.Get(6030); // "Today"
      else if (DateTime.Today.Subtract(aStartTime) < new TimeSpan(1, 0, 0, 0))
        return GUILocalizeStrings.Get(6040); // "Yesterday"
      else if (DateTime.Today.Subtract(aStartTime) < new TimeSpan(2, 0, 0, 0))
        return GUILocalizeStrings.Get(6041); // "Two days ago"
      else if (thisMonth.Equals(new DateTime(aStartTime.Year, aStartTime.Month, 1)))
        return GUILocalizeStrings.Get(6060); // "Current month";
      else if (lastMonth.Equals(new DateTime(aStartTime.Year, aStartTime.Month, 1)))
        return GUILocalizeStrings.Get(6065); // "Last month";
      else if (DateTime.Now.Year.Equals(aStartTime.Year))
        return GUILocalizeStrings.Get(6070); // "Current year";
      else if (DateTime.Now.Year.Equals(aStartTime.AddYears(1).Year))
        return GUILocalizeStrings.Get(6080); // "Last year";
      else return GUILocalizeStrings.Get(6090); // "Older";
    }
  }
}
