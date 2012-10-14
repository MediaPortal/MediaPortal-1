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
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl
{
  public static class UserFactory
  {
    public const int EPG_PRIORITY = 1;    
    public const int USER_PRIORITY = 2;
    public const int SCHEDULER_PRIORITY = 101;
    public const string EPG_TAGNAME = "PriorityEPG";
    public const string USER_TAGNAME = "PriorityUser";
    public const string SCHEDULER_TAGNAME = "PriorityScheduler";
    public const string CUSTOM_TAGNAME = "PrioritiesCustom";

    public const string NAME_EPG = "epg";
    //private const bool IS_ADMIN_EPG = false;

    public const string NAME_SCHEDULER = "scheduler";
    
    private const int PRIORITY_MAX_VALUE = 100;
    private const int PRIORITY_MIN_VALUE = 1;

    //private const bool IS_ADMIN_SCHEDULER = true;
    //private const bool IS_ADMIN_USER = false;    

    private static readonly int _priorityEpg;
    private static readonly int _priorityUser;
    private static readonly int _priorityScheduler;
    private static readonly IDictionary<string, int> _priorityCustomUsers = new Dictionary<string, int>();

    private static decimal ValueSanityCheck(decimal value, int min, int max)
    {
      if (value < min)
        return min;
      if (value > max)
        return max;
      return value;
    }

    static UserFactory()
    {
        
      
      try
      {
        var settingService = GlobalServiceProvider.Get<ISettingService>();
        _priorityEpg = (int)ValueSanityCheck(
        Convert.ToDecimal(settingService.GetSettingWithDefaultValue(EPG_TAGNAME, EPG_PRIORITY.ToString()).Value), PRIORITY_MIN_VALUE, PRIORITY_MAX_VALUE);        

        Log.Debug("UserFactory setting PriorityEPG : {0}", _priorityEpg);

        _priorityUser = (int)ValueSanityCheck(
          Convert.ToDecimal(settingService.GetSettingWithDefaultValue(USER_TAGNAME, USER_PRIORITY.ToString()).Value), PRIORITY_MIN_VALUE, PRIORITY_MAX_VALUE);
        Log.Debug("UserFactory setting PriorityUser : {0}", _priorityUser);

        _priorityScheduler = (int)ValueSanityCheck(
          Convert.ToDecimal(settingService.GetSettingWithDefaultValue(SCHEDULER_TAGNAME, SCHEDULER_PRIORITY.ToString()).Value), PRIORITY_MIN_VALUE, PRIORITY_MAX_VALUE);
        Log.Debug("UserFactory setting PriorityScheduler : {0}", _priorityScheduler);

        Setting setting = settingService.GetSettingWithDefaultValue(CUSTOM_TAGNAME, "");
        string[] users = setting.Value.Split(';');
        foreach (string user in users)
        {
          string[] shareItem = user.Split(',');
          bool hasItems = shareItem.Length.Equals(2);
          if (hasItems)
          {
            string host = shareItem[0].Trim();
            int priority;
            if (host.Length > 0 && Int32.TryParse(shareItem[1].Trim(), out priority))
            {
              Log.Debug("UserFactory setting PriorityCustomUser : {0} - {1}", host, priority);
              _priorityCustomUsers[host] = priority;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("UserFactory - error reading priority settings from database", ex);        
      }
      
    }

    public static IUser CreateEpgUser()
    {
      IUser egpUser = new User(NAME_EPG, UserType.EPG, -1, _priorityEpg);
      return egpUser;
    }

    public static IUser CreateSchedulerUser(int scheduleId, int cardId)
    {
      string name = NAME_SCHEDULER + scheduleId;
      IUser schedulerUser = new User(name, UserType.Scheduler, cardId, _priorityScheduler);
      return schedulerUser;  
    }

    public static IUser CreateSchedulerUser()
    {
      IUser schedulerUser = new User("", UserType.Scheduler);
      schedulerUser.Priority = _priorityScheduler;
      return schedulerUser;
    }

    public static IUser CreateSchedulerUser(int scheduleId)
    {
      return CreateSchedulerUser(scheduleId, -1);
    }

    public static IUser CreateBasicUser(string name, int cardId, int? defaultPriority)
    {
      return CreateBasicUser(name, cardId, defaultPriority, UserType.Normal); //used by setuptv-testchannels
    }

    public static IUser CreateBasicUser(string name, int cardId, int? defaultPriority, UserType userType)
    {
      int priorityCustomUser = GetDefaultPriority(name, defaultPriority);
      IUser basicUser = new User(name, userType, cardId, priorityCustomUser);
      return basicUser;  
    }

    public static IUser CreateBasicUser(string name)
    {
      return CreateBasicUser(name, -1, USER_PRIORITY);
    }

    public static IUser CreateBasicUser(string name, int? defaultPriority)
    {
      return CreateBasicUser(name, -1, defaultPriority);
    }

    public static int? GetDefaultPriority(string name)
    {
      return GetDefaultPriority(name, USER_PRIORITY);
    }

    public static int GetDefaultPriority(string name, int? defaultPriority)
    {
      int priorityCustomUser;
      if (!_priorityCustomUsers.TryGetValue(name, out priorityCustomUser))
      {
        if (defaultPriority.HasValue)
        {
          priorityCustomUser = defaultPriority.GetValueOrDefault(-1);
        }
        else
        {
          priorityCustomUser = USER_PRIORITY;
        }
      }
      return priorityCustomUser;
    }

    
  }
}
