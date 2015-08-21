using System;
using MediaPortal.GUI.Library;
using TvDatabase;
using Common.GUIPlugins;
using Action = MediaPortal.GUI.Library.Action;

namespace TvPlugin
{
  public abstract class RecordedBase : WindowPluginBase
  {

    protected RecordedBase()
    {
      GUIWindowManager.OnNewAction += OnNewAction;
    }

    /// <summary>
    /// OnAction only receives ACTION_PLAY event when the player is not playing.  Ensure all actions are processed
    /// </summary>
    /// <param name="action">Action command</param>
    private void OnNewAction(Action action)
    {
      if ((action.wID != Action.ActionType.ACTION_PLAY) || GUIWindowManager.ActiveWindow != GetID)
      {
        return;
      }

      var item = facadeLayout.SelectedListItem;
      if (item == null || item.IsFolder)
      {
        return;
      }

      if (GetFocusControlId() == facadeLayout.GetID)
      {
        // only start something is facade is focused
        OnSelectedRecording(facadeLayout.SelectedListItemIndex);
      }
    }

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

    protected abstract bool OnSelectedRecording(int iItem);
  }
}
