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
using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// Classification advisory flags.
  /// </summary>
  [Flags]
  public enum ContentAdvisory
  {
    /// <summary>
    /// None.
    /// </summary>
    None = 0,
    /// <summary>
    /// Sexual situations.
    /// </summary>
    [Description("S")]
    SexualSituations = 1,
    /// <summary>
    /// Course or crude language.
    /// </summary>
    [Description("L")]
    CourseOrCrudeLanguage = 2,
    /// <summary>
    /// Fantasy violence.
    /// </summary>
    [Description("FV")]
    FantasyViolence = 4,
    /// <summary>
    /// Violence.
    /// </summary>
    [Description("V")]
    Violence = 8,
    /// <summary>
    /// Nudity.
    /// </summary>
    [Description("N")]
    Nudity = 16,
    /// <summary>
    /// Suggestive dialogue.
    /// </summary>
    [Description("D")]
    SuggestiveDialogue = 32,
    /// <summary>
    /// Mild sensuality.
    /// </summary>
    [Description("mQ")]
    MildSensuality = 64,
    /// <summary>
    /// Mild peril.
    /// </summary>
    [Description("mK")]
    MildPeril = 128
  }
}