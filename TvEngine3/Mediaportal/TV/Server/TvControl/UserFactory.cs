#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVControl
{
  public static class UserFactory
  {
    public const int DEFAULT_PRIORITY_SCHEDULER = 100;
    public const int DEFAULT_PRIORITY_EPG_GRABBER = 1;    
    public const int DEFAULT_PRIORITY_OTHER = 2;

    public const string SETTING_NAME_PRIORITY_SCHEDULER = "userPriorityScheduler";
    public const string SETTING_NAME_PRIORITY_EPG_GRABBER = "userPriorityEpgGrabber";
    public const string SETTING_NAME_PRIORITY_OTHER_DEFAULT = "userPriorityOtherDefault";
    public const string SETTING_NAME_PRIORITIES_OTHER_CUSTOM = "userPrioritiesOtherCustom";

    private const string NAME_EPG = "epg";
    private const string NAME_SCHEDULER = "scheduler";

    private static int _priorityScheduler = -1;
    private static int _priorityEpgGrabber = -1;
    private static int _priorityOtherDefault = -1;
    private static readonly IDictionary<string, int> _prioritiesOtherCustom = new Dictionary<string, int>();

    static UserFactory()
    {
      ReloadConfiguration();
    }

    public static void ReloadConfiguration()
    {
      Log.Debug("user factory: reload configuration");

      var settingService = GlobalServiceProvider.Get<ISettingService>();
      _priorityScheduler = settingService.GetValue(SETTING_NAME_PRIORITY_SCHEDULER, DEFAULT_PRIORITY_SCHEDULER);
      _priorityEpgGrabber = settingService.GetValue(SETTING_NAME_PRIORITY_EPG_GRABBER, DEFAULT_PRIORITY_EPG_GRABBER);
      _priorityOtherDefault = settingService.GetValue(SETTING_NAME_PRIORITY_OTHER_DEFAULT, DEFAULT_PRIORITY_OTHER);
      Log.Debug("  scheduler = {0}", _priorityScheduler);
      Log.Debug("  EPG       = {0}", _priorityEpgGrabber);
      Log.Debug("  default   = {0}", _priorityOtherDefault);

      _prioritiesOtherCustom.Clear();
      string[] users = settingService.GetValue(SETTING_NAME_PRIORITIES_OTHER_CUSTOM, string.Empty).Split(';');
      foreach (string user in users)
      {
        int lastCommaIndex = user.LastIndexOf(",");
        if (lastCommaIndex < 0)
        {
          continue;
        }
        string host = user.Substring(0, lastCommaIndex).Trim();
        string priorityString = user.Substring(lastCommaIndex + 1);
        int priority;
        if (!string.IsNullOrEmpty(host) && int.TryParse(priorityString, out priority))
        {
          Log.Debug("  {0,-9} = {1}", host, priority);
          _prioritiesOtherCustom[host] = priority;
        }
      }
    }

    public static IUser CreateSchedulerUser(int scheduleId, int cardId = -1)
    {
      return new User(NAME_SCHEDULER + scheduleId, UserType.Scheduler, cardId, _priorityScheduler);
    }

    public static IUser CreateSchedulerUser()
    {
      return new User(string.Empty, UserType.Scheduler, -1, _priorityScheduler);
    }

    public static IUser CreateEpgUser()
    {
      return new User(NAME_EPG, UserType.EPG, -1, _priorityEpgGrabber);
    }

    public static IUser CreateBasicUser(string name, int? overridePriority = null, int cardId = -1)
    {
      return new User(name, UserType.Normal, cardId, overridePriority.HasValue ? overridePriority : GetDefaultPriority(name));
    }

    public static IUser CreateCustomUser(string name, int priority, int cardId, UserType userType)
    {
      return new User(name, userType, cardId, priority);
    }

    public static int GetDefaultPriority(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return _priorityOtherDefault;
      }
      if (name.Equals(NAME_EPG))
      {
        return _priorityEpgGrabber;
      }
      if (name.StartsWith(NAME_SCHEDULER))
      {
        return _priorityScheduler;
      }
      int priority;
      if (!_prioritiesOtherCustom.TryGetValue(name, out priority))
      {
        return _priorityOtherDefault;
      }
      return priority;
    }
  }
}