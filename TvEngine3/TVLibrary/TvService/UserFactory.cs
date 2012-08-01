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
using System.Linq;
using System.Text;
using TvControl;
using TvDatabase;
using TvLibrary.Log;

namespace TvService
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

    private const string NAME_EPG = "epg";
    private const bool IS_ADMIN_EPG = false;

    private const string NAME_SCHEDULER = "scheduler";
    
    private const int PRIORITY_MAX_VALUE = 100;
    private const int PRIORITY_MIN_VALUE = 1;

    private const bool IS_ADMIN_SCHEDULER = true;
    private const bool IS_ADMIN_USER = false;    

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
      var layer = new TvBusinessLayer();  

      try
      {
        _priorityEpg = (int)ValueSanityCheck(
        Convert.ToDecimal(layer.GetSetting(EPG_TAGNAME, EPG_PRIORITY.ToString()).Value), PRIORITY_MIN_VALUE, PRIORITY_MAX_VALUE);
        Log.Debug("UserFactory setting PriorityEPG : {0}", _priorityEpg);

        _priorityUser = (int)ValueSanityCheck(
          Convert.ToDecimal(layer.GetSetting(USER_TAGNAME, USER_PRIORITY.ToString()).Value), PRIORITY_MIN_VALUE, PRIORITY_MAX_VALUE);
        Log.Debug("UserFactory setting PriorityUser : {0}", _priorityUser);

        _priorityScheduler = (int)ValueSanityCheck(
          Convert.ToDecimal(layer.GetSetting(SCHEDULER_TAGNAME, SCHEDULER_PRIORITY.ToString()).Value), PRIORITY_MIN_VALUE, PRIORITY_MAX_VALUE);
        Log.Debug("UserFactory setting PriorityScheduler : {0}", _priorityScheduler);

        Setting setting = layer.GetSetting(CUSTOM_TAGNAME, "");
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
      IUser egpUser = new User(NAME_EPG, IS_ADMIN_EPG, -1, _priorityEpg);
      return egpUser;
    }

    public static IUser CreateSchedulerUser(int scheduleId, int cardId)
    {
      string name = NAME_SCHEDULER + scheduleId;
      IUser schedulerUser = new User(name, IS_ADMIN_SCHEDULER, cardId, _priorityScheduler);
      return schedulerUser;  
    }

    public static IUser CreateSchedulerUser()
    {
      IUser schedulerUser = new User("", true);
      return schedulerUser;
    }

    public static IUser CreateSchedulerUser(int scheduleId)
    {
      return CreateSchedulerUser(scheduleId, -1);
    }

    public static IUser CreateBasicUser(string name, int cardId, int? defaultPriority)
    {
      return CreateBasicUser(name, cardId, defaultPriority, IS_ADMIN_USER);
    }

    public static IUser CreateBasicUser(string name, int cardId, int? defaultPriority, bool isAdmin)
    {
      int priorityCustomUser = GetDefaultPriority(name, defaultPriority);
      IUser basicUser = new User(name, isAdmin, cardId, priorityCustomUser);
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
