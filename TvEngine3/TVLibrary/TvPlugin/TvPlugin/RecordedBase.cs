using System;
using MediaPortal.GUI.Library;
using TvDatabase;
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
      DateTime now = DateTime.Now;
      DateTime today = now.Date;

      var thisMonth = new DateTime(today.Year, today.Month, 1);
      var lastMonth = thisMonth.AddMonths(-1);

      DayOfWeek firstDayOfWeek = WeekEndTool.GetFirstWorkingDay();
      DateTime firstDayOfThisWeek = today;
      while (firstDayOfThisWeek.DayOfWeek != firstDayOfWeek)
        firstDayOfThisWeek = firstDayOfThisWeek.AddDays(-1);
      int daysToStartOfWeek = (aStartTime.Date - firstDayOfThisWeek).Days;
      int daysToStartOfLastWeek = daysToStartOfWeek + 7;

      if (now < aStartTime)
        return GUILocalizeStrings.Get(6095); // "Future"
      else if (today.Equals(aStartTime.Date))
        return GUILocalizeStrings.Get(6030); // "Today"
      else if (today.Equals(aStartTime.AddDays(1).Date))
        return GUILocalizeStrings.Get(6040); // "Yesterday"
      else if (0 <= daysToStartOfWeek && daysToStartOfWeek < 5) // current week excluding today and yesterday
        return GUILocalizeStrings.Get(6055); // "Earlier this week";
      else if (0 <= daysToStartOfLastWeek && daysToStartOfLastWeek < 7)
        return GUILocalizeStrings.Get(6056); // "Last week"
      else if (thisMonth.Equals(new DateTime(aStartTime.Year, aStartTime.Month, 1)))
        return GUILocalizeStrings.Get(6062); //"Earlier this month"
      else if (lastMonth.Equals(new DateTime(aStartTime.Year, aStartTime.Month, 1)))
        return GUILocalizeStrings.Get(6065); // "Last month"
      else if (today.Year.Equals(aStartTime.Year))
        return GUILocalizeStrings.Get(6075); // "Earlier this year"
      else if (today.Year.Equals(aStartTime.AddYears(1).Year))
        return GUILocalizeStrings.Get(6080); // "Last year";
      else
        return GUILocalizeStrings.Get(6090); // "Older"
    }
  }
}
