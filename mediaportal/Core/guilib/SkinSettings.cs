#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace MediaPortal.GUI.Library
{
  public class SkinSettings
  {
    private class SkinString
    {
      public string Name;
      public string Value;
    } ;

    private class SkinBool
    {
      public string Name;
      public bool Value;
    } ;

    private static Dictionary<int, SkinString> _skinStringSettings = new Dictionary<int, SkinString>();
    private static Dictionary<int, SkinBool> _skinBoolSettings = new Dictionary<int, SkinBool>();

    public static int TranslateSkinString(string line)
    {
      Dictionary<int, SkinString>.Enumerator enumer = _skinStringSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinString skin = enumer.Current.Value;
        if (skin.Name == line)
        {
          return enumer.Current.Key;
        }
      }
      SkinString newString = new SkinString();
      newString.Name = line;
      newString.Value = line;
      int key = _skinStringSettings.Count;
      _skinStringSettings[key] = newString;
      return key;
    }

    public static string GetSkinString(int key)
    {
      if (_skinStringSettings.ContainsKey(key))
      {
        return _skinStringSettings[key].Value;
      }
      return "";
    }

    public static int TranslateSkinBool(string setting)
    {
      Dictionary<int, SkinBool>.Enumerator enumer = _skinBoolSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinBool skin = enumer.Current.Value;
        if (skin.Name == setting)
        {
          return enumer.Current.Key;
        }
      }
      SkinBool newBool = new SkinBool();
      newBool.Name = setting;
      newBool.Value = false;
      newBool.Value = false;
      int key = _skinBoolSettings.Count;
      _skinBoolSettings[key] = newBool;
      return key;
    }

    public static bool GetSkinBool(int key)
    {
      if (_skinBoolSettings.ContainsKey(key))
      {
        return _skinBoolSettings[key].Value;
      }
      return false;
    }
  }
}