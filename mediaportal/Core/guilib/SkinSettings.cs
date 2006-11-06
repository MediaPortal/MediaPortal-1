using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public class SkinSettings
  {
    class SkinString
    {
      public string Name;
      public string Value;
    };

    class SkinBool
    {
      public string Name;
      public bool Value;
    };

    static Dictionary<int, SkinString> _skinStringSettings = new Dictionary<int, SkinString>();
    static Dictionary<int, SkinBool> _skinBoolSettings = new Dictionary<int, SkinBool>();

    public static int TranslateSkinString(string line)
    {
      Dictionary<int, SkinString>.Enumerator enumer = _skinStringSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinString skin = enumer.Current.Value;
        if (skin.Name == line) return enumer.Current.Key;
      }
      SkinString newString = new SkinString();
      newString.Name = line;
      newString.Value = "";
      int key = _skinBoolSettings.Count;
      _skinStringSettings[key] = newString;
      return key;
    }
    public static string GetSkinString(int key)
    {
      if (_skinStringSettings.ContainsKey(key)) return _skinStringSettings[key].Value;
      return "";
    }

    public static int TranslateSkinBool(string setting)
    {
      Dictionary<int, SkinBool>.Enumerator enumer=_skinBoolSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinBool skin = enumer.Current.Value;
        if (skin.Name == setting) return enumer.Current.Key;
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
      if (_skinBoolSettings.ContainsKey(key)) return _skinBoolSettings[key].Value;
      return false;
    }
  }
}
