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


using System;
using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvDatabase;

namespace TvLibrary.Implementations.Helper.Providers
{
  public class Provider
  {
    #region Fields

    /// <summary>
    /// Singleton
    /// </summary>
    private static readonly Provider _instance = new Provider();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the singleton instance
    /// </summary>
    public static Provider Instance
    {
      get
      {
        return _instance;
      }
    }

    /// <summary>
    /// Gets if at least 1 provider has EPG grabbing enabled
    /// </summary>
    public bool ProviderEPGGrabbingEnabled
    {
      get
      {
        return SkyUK.Instance.EPGGrabbingEnabled || SkyItaly.Instance.EPGGrabbingEnabled;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Protected cstr
    /// </summary>
    protected Provider()
    { }

    /// <summary>
    /// Sets a setting value
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    protected void SetSetting(string name, object value)
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      string valueString;

      if (value is bool)
        valueString = (((bool)value) ? "true" : "false");
      else
        valueString = value.ToString();

      Setting setting = layer.GetSetting(name);
      setting.Value = valueString;
      setting.Persist();
    }

    /// <summary>
    /// Gets a setting value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    protected T GetSetting<T>(string name, T defaultValue)
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      if (typeof(T) == typeof(int))
      {
        int parsedIntValue = (int)(object)defaultValue;

        int.TryParse(layer.GetSetting(name, parsedIntValue.ToString()).Value, out parsedIntValue);
        return (T)(object)parsedIntValue;
      }

      if (typeof(T) == typeof(bool))
      {
        bool parsedBoolValue = (layer.GetSetting(name, ((bool)(object)defaultValue ? "true" : "false")).Value == "true" ? true : false);

        return (T)(object)parsedBoolValue;
      }

      if (typeof(T) == typeof(string))
      {
        return (T)(object)layer.GetSetting(name, (string)(object)defaultValue).Value;
      }

      throw new Exception("Invalid type");
    }

    #endregion
  }
}
