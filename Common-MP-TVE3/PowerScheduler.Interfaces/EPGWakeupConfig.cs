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

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace TvEngine.PowerScheduler
{

  #region Enums

  public enum EPGGrabDays
  {
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
  }

  #endregion

  [Serializable]
  public class EPGWakeupConfig
  {
    public DateTime LastRun = DateTime.MinValue;
    public List<EPGGrabDays> Days = new List<EPGGrabDays>();
    public int Hour;
    public int Minutes;

    public EPGWakeupConfig() {}

    public EPGWakeupConfig(string serializedConfig)
    {
      EPGWakeupConfig cfg = new EPGWakeupConfig();
      try
      {
        BinaryFormatter formatter = new BinaryFormatter();
        byte[] buffer = Convert.FromBase64String(serializedConfig);
        using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length))
        {
          cfg = (EPGWakeupConfig)formatter.Deserialize(stream);
        }
      }
      catch (Exception) {}
      Hour = cfg.Hour;
      Minutes = cfg.Minutes;
      Days = cfg.Days;
      LastRun = cfg.LastRun;
    }

    public string SerializeAsString()
    {
      BinaryFormatter formatter = new BinaryFormatter();
      string result;
      using (MemoryStream stream = new MemoryStream())
      {
        formatter.Serialize(stream, this);
        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[stream.Length];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        result = Convert.ToBase64String(buffer, 0, bytesRead);
      }
      return result;
    }

    public override bool Equals(object obj)
    {
      if (obj is EPGWakeupConfig)
      {
        EPGWakeupConfig cfg = (EPGWakeupConfig)obj;
        if (cfg.Hour == Hour && cfg.Minutes == Minutes)
        {
          foreach (EPGGrabDays day in cfg.Days)
          {
            if (!Days.Contains(day))
            {
              return false;
            }
          }
          foreach (EPGGrabDays day in Days)
          {
            if (!cfg.Days.Contains(day))
            {
              return false;
            }
          }
          return true;
        }
      }
      return false;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}