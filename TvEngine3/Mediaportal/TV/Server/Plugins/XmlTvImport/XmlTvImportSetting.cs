#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System.Collections.Generic;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  internal sealed class XmlTvImportSetting
  {
    private readonly string _name;
    private static readonly IDictionary<string, XmlTvImportSetting> _values = new Dictionary<string, XmlTvImportSetting>();
    public static readonly XmlTvImportSetting File = new XmlTvImportSetting("xmlTvFile");

    public static readonly XmlTvImportSetting UseTimeCorrection = new XmlTvImportSetting("xmlTvUseTimeCorrection");
    public static readonly XmlTvImportSetting TimeCorrectionHours = new XmlTvImportSetting("xmlTvTimeCorrectionHours");
    public static readonly XmlTvImportSetting TimeCorrectionMinutes = new XmlTvImportSetting("xmlTvTimeCorrectionMinutes");

    public static readonly XmlTvImportSetting UsePartialMatching = new XmlTvImportSetting("xmlTvUsePartialMatching");

    public static readonly XmlTvImportSetting ScheduledActionsDownload = new XmlTvImportSetting("xmlTvScheduledActionsDownload");
    public static readonly XmlTvImportSetting ScheduledActionsDownloadUrl = new XmlTvImportSetting("xmlTvScheduledActionsDownloadUrl");

    public static readonly XmlTvImportSetting ScheduledActionsProgram = new XmlTvImportSetting("xmlTvScheduledActionsProgram");
    public static readonly XmlTvImportSetting ScheduledActionsProgramLocation = new XmlTvImportSetting("xmlTvScheduledActionsProgramLocation");

    public static readonly XmlTvImportSetting ScheduledActionsTimeFrequency = new XmlTvImportSetting("xmlTvScheduledActionsTimeFrequency");
    public static readonly XmlTvImportSetting ScheduledActionsTimeBetweenStart = new XmlTvImportSetting("xmlTvScheduledActionsTimeBetweenStart");
    public static readonly XmlTvImportSetting ScheduledActionsTimeBetweenEnd = new XmlTvImportSetting("xmlTvScheduledActionsTimeBetweenEnd");
    public static readonly XmlTvImportSetting ScheduledActionsTimeOnStartup = new XmlTvImportSetting("xmlTvScheduledActionsTimeOnStartup");

    private XmlTvImportSetting(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      XmlTvImportSetting setting = obj as XmlTvImportSetting;
      if (setting != null && this == setting)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<XmlTvImportSetting> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator XmlTvImportSetting(string name)
    {
      XmlTvImportSetting value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(XmlTvImportSetting setting)
    {
      return setting._name;
    }
  }
}