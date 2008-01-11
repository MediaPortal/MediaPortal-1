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

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;

namespace MediaPortal.Support
{
  /// <summary>
  /// Interface to all installed hotfixes on the current computer.
  /// </summary>
  public class HotfixInformation : IEnumerable<HotfixItem>
  {
    private Dictionary<string, HotfixItem> hotfixes = new Dictionary<string, HotfixItem>();
    private SortedList<string, string> categories = new SortedList<string, string>();

    public HotfixInformation()
    {
      try
      {
        using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\UnInstall"))
        {
          foreach (string keyName in rk.GetSubKeyNames())
          {
            if (keyName.StartsWith("KB"))
            {
              ParseHotfixKey(rk.OpenSubKey(keyName));
            }
          }
        }
      }
      catch (Exception)
      {
      }
    }

    void ParseHotfixKey(RegistryKey rk)
    {
      HotfixItem item = new HotfixItem();
      item.Name = GetName(rk.Name);

      foreach (string valueName in rk.GetValueNames())
      {
        switch (valueName)
        {
          case "DisplayName":
            item.DisplayName = Convert.ToString(rk.GetValue(valueName));
            break;
          case "ParentKeyName":
            item.Category = Convert.ToString(rk.GetValue(valueName));
            break;
          case "HelpLink":
            item.URL = Convert.ToString(rk.GetValue(valueName));
            break;
          case "ReleaseType":
            item.ReleaseType = Convert.ToString(rk.GetValue(valueName));
            break;
          case "InstallDate":
            item.InstallDate = Convert.ToString(rk.GetValue(valueName));
            break;
          case "UninstallString":
            item.UninstallString = Convert.ToString(rk.GetValue(valueName));
            break;
        }
      }

      hotfixes.Add(item.Name, item);
      if (!categories.ContainsKey(item.Category))
        categories.Add(item.Category, null);
    }

    private static string GetName(string name)
    {
      if (name.LastIndexOf(@"\") == -1)
        return name;
      else
        return name.Substring(name.LastIndexOf(@"\") + 1);
    }

    public string[] GetCategories()
    {
      string[] returnValue = new string[categories.Count];
      categories.Keys.CopyTo(returnValue, 0);
      return returnValue;
    }

    public IEnumerator<HotfixItem> GetHotfixes(string category)
    {
      foreach (KeyValuePair<string, HotfixItem> kvp in hotfixes)
      {
        if (kvp.Value.Category == category)
          yield return kvp.Value;
      }
    }

    public IEnumerator<HotfixItem> GetEnumerator()
    {
      IEnumerator<HotfixItem> enumerator = ((IEnumerable<HotfixItem>)hotfixes.Values).GetEnumerator();
      return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return hotfixes.GetEnumerator();
    }
  }
}
