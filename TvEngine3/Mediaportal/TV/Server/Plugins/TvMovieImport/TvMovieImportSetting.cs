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

namespace Mediaportal.TV.Server.Plugins.TvMovieImport
{
  internal sealed class TvMovieImportSetting
  {
    private readonly string _name;
    private static readonly IDictionary<string, TvMovieImportSetting> _values = new Dictionary<string, TvMovieImportSetting>();
    public static readonly TvMovieImportSetting DatabaseFile = new TvMovieImportSetting("tvMovieImportDatabaseFile");
    public static readonly TvMovieImportSetting UpdateTimeFrequency = new TvMovieImportSetting("tvMovieImportScheduledActionsTimeFrequency");
    public static readonly TvMovieImportSetting UpdateTimeBetweenStart = new TvMovieImportSetting("tvMovieImportScheduledActionsTimeBetweenStart");
    public static readonly TvMovieImportSetting UpdateTimeBetweenEnd = new TvMovieImportSetting("tvMovieImportScheduledActionsTimeBetweenEnd");
    public static readonly TvMovieImportSetting UpdateTimeOnStartup = new TvMovieImportSetting("tvMovieImportScheduledActionsTimeOnStartup");

    public static readonly TvMovieImportSetting UsePartialMatching = new TvMovieImportSetting("tvMovieImportUsePartialMatching");

    private TvMovieImportSetting(string name)
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
      TvMovieImportSetting setting = obj as TvMovieImportSetting;
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

    public static ICollection<TvMovieImportSetting> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator TvMovieImportSetting(string name)
    {
      TvMovieImportSetting value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(TvMovieImportSetting setting)
    {
      return setting._name;
    }
  }
}