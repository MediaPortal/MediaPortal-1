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

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  [Flags]
  public enum ProgramState
  {
    None = 0,
    Notify = 1,
    RecordOnce = 2,
    RecordSeries = 4,
    RecordManual = 8,
    Conflict = 16,
    RecordOncePending = 32,             // Used to indicate recording icon in the guide, even though the recording hasn't begun yet.
    RecordSeriesPending = 64,           // Used to indicate recording icon in the guide, even though the recording hasn't begun yet.
    PartialRecordSeriesPending = 128    // Used to indicate partial recording icon in the guide, even though the recording hasn't begun yet.
  }
}