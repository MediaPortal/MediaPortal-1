#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Collections.Generic;
//using System.Windows.Forms;

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
        SettingKey other = (SettingKey) obj;
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
        ISettingsPrefetchable prefetcher = (ISettingsPrefetchable) provider;
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

#if DEBUG
    ~CacheSettingsProvider()
    {
      Log.Info("Filename: {0} Cachehit: {1} Cachemiss: {2}", provider.FileName, cacheHit, cacheMiss);
    }
#endif
  }
}