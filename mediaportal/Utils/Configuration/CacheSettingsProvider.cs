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
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Profile
{
  public class CacheSettingsProvider : ISettingsProvider
  {
    private Dictionary<SettingKey, object> cache = new Dictionary<SettingKey, object>();
    private ISettingsProvider provider;
    private int cacheHit = 0;
    private int cacheMiss = 0;

    #region class SettingKey

    private class SettingKey
    {
      private string section;
      private string entry;

      public SettingKey(string section, string entry)
      {
        this.entry = entry;
        this.section = section;
      }

      public string Entry
      {
        get { return entry; }
        set { entry = value; }
      }

      public string Section
      {
        get { return section; }
        set { section = value; }
      }

      public override int GetHashCode()
      {
        return section.GetHashCode() ^ entry.GetHashCode();
      }

      public override bool Equals(object obj)
      {
        SettingKey other = (SettingKey)obj;
        if (this.entry == other.entry && this.section == other.section)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    #endregion

    public CacheSettingsProvider(ISettingsProvider provider)
    {
      this.provider = provider;

      if (provider is ISettingsPrefetchable)
      {
        ISettingsPrefetchable prefetcher = (ISettingsPrefetchable)provider;
        prefetcher.Prefetch(Remember);
      }
    }

    private void Remember(string section, string entry, object value)
    {
      SettingKey key = new SettingKey(section, entry);
      cache.Add(key, value);
    }

    public string FileName
    {
      get { return provider.FileName; }
    }

    public object GetValue(string section, string entry)
    {
      SettingKey key = new SettingKey(section, entry);

      object obj;
      if (!cache.TryGetValue(key, out obj))
      {
        cacheMiss++;
        obj = provider.GetValue(section, entry);
        cache.Add(key, obj);
      }
      else
      {
        cacheHit++;
      }
      return obj;
    }

    /// <summary>
    /// Will return true if the specified settings section exists.
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public bool HasSection<T>(string section)
    {
      bool result = true;
      SettingKey key = new SettingKey(section, "");

      object obj;
      if (!cache.TryGetValue(key, out obj))
      {
        result = provider.HasSection<T>(section);
      }
      return result;
    }

    /// <summary>
    /// Will return a cached dictionary of all settings in the specified section.
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public IDictionary<string, T> GetSection<T>(string section)
    {
      SettingKey key = new SettingKey(section, "");

      object obj;
      if (!cache.TryGetValue(key, out obj))
      {
        cacheMiss++;
        obj = provider.GetSection<T>(section);
        cache.Add(key, obj);
      }
      else
      {
        cacheHit++;
      }
      return (Dictionary<string, T>)obj;
    }

    public void RemoveEntry(string section, string entry)
    {
      SettingKey key = new SettingKey(section, entry);
      cache.Remove(key);
      provider.RemoveEntry(section, entry);
    }

    public void Save()
    {
      provider.Save();
    }

    public void SetValue(string section, string entry, object value)
    {
      SettingKey key = new SettingKey(section, entry);
      cache.Remove(key);
      cache.Add(key, value);
      provider.SetValue(section, entry, value);
    }

    /// <summary>
    /// Moves an entry from the 'from' section to the 'to' section
    /// </summary>
    /// <param name="fromSection"></param>
    /// <param name="toSection"></param>
    /// <param name="entry"></param>
    public void MoveEntry(string fromSection, string toSection, string entry)
    {
      object value = GetValue(fromSection, entry);

      if (value == null)
        return;

      SetValue(toSection, entry, value);

      RemoveEntry(fromSection, entry);
    }

#if DEBUG
    ~CacheSettingsProvider()
    {
      Log.Info("Filename: {0} Cachehit: {1} Cachemiss: {2}", provider.FileName, cacheHit, cacheMiss);
    }
#endif
  }
}