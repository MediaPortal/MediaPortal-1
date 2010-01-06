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

#endregion

namespace TvEngine.Interfaces
{
  // Summary:
  //     Indicates the system's power status.
  public enum PowerEventType
  {
    QuerySuspend,
    QueryStandBy,
    QuerySuspendFailed,
    QueryStandByFailed,
    Suspend,
    StandBy,
    ResumeCritical,
    ResumeSuspend,
    ResumeStandBy,
    ResumeAutomatic
  }

  public delegate bool PowerEventHandler(PowerEventType powerStatus);

  public interface IPowerEventHandler
  {
    void AddPowerEventHandler(PowerEventHandler handler);
    void RemovePowerEventHandler(PowerEventHandler handler);
  }
}