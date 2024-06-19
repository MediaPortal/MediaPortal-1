﻿#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX.Direct3D9;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Library
{
  public class PixelShaderCollection
  {
    #region Types
    private class ShaderProfile
    {
      public string Name { get; private set; }
      public string Shaders;
      public bool IsCustom { get; private set; }

      public ShaderProfile(string strName, string strShaders)
      {
        this.Name = strName;
        this.Shaders = strShaders;
        this.IsCustom = !_ProhibitedProfileNames.Any(p => p.Equals(strName, StringComparison.CurrentCultureIgnoreCase));
      }
    }
    #endregion

    #region Constants
    public const string SHADER_EXTENSION = ".hlsl";
    public const string SHADER_FOLDER_NAME = "Shaders";
    public const string SHADER_PROFILE_DEFAULT = "Default";

    private const string _SETTINGS_SECTION = "general";
    private const string _SETTINGS_ENTRY = "VideoPixelShader";
    private const string _SETTINGS_ENTRY_NAME_SUFFIX = "Name";
    #endregion

    #region Private fields
    private readonly Device _Device;
    private readonly List<KeyValuePair<string, PixelShader>> _PixelShaders = new List<KeyValuePair<string, PixelShader>>();
    private ShaderProfile _Profile;
    private List<ShaderProfile> _Profiles = null;
    private static readonly string[] _ProhibitedProfileNames = new string[] { "default", "sd", "hd", "uhd" };
    #endregion

    #region ctor
    public PixelShaderCollection(Device device)
    {
      this._Device = device;

      this._Profiles = new List<ShaderProfile>();
      using (Profile.Settings set = new Profile.MPSettings())
      {
        //Explicit profiles
        this._Profiles.Add(new ShaderProfile(SHADER_PROFILE_DEFAULT, set.GetValueAsString(_SETTINGS_SECTION, _SETTINGS_ENTRY + SHADER_PROFILE_DEFAULT, string.Empty)));
        this._Profiles.Add(new ShaderProfile("SD", set.GetValueAsString(_SETTINGS_SECTION, _SETTINGS_ENTRY + "SD", string.Empty)));
        this._Profiles.Add(new ShaderProfile("HD", set.GetValueAsString(_SETTINGS_SECTION, _SETTINGS_ENTRY + "HD", string.Empty)));
        this._Profiles.Add(new ShaderProfile("UHD", set.GetValueAsString(_SETTINGS_SECTION, _SETTINGS_ENTRY + "UHD", string.Empty)));

        //Custom profiles
        int iIdx = 0;
        string strName;
        while (true)
        {
          strName = set.GetValueAsString(_SETTINGS_SECTION, _SETTINGS_ENTRY + iIdx + _SETTINGS_ENTRY_NAME_SUFFIX, null);
          if (string.IsNullOrWhiteSpace(strName))
            break;

          this._Profiles.Add(new ShaderProfile(strName, set.GetValueAsString(_SETTINGS_SECTION, _SETTINGS_ENTRY + iIdx, string.Empty)));
          iIdx++;
        }
      }

      this._Profile = this._Profiles[0];
    }
    #endregion

    #region Properties

    /// <summary>
    /// Gets the shader at the specified index.
    /// </summary>
    /// <param name="iIdx">The zero-based index of the shader to get.</param>
    /// <returns>The shader at the specified index.</returns>
    public KeyValuePair<string, PixelShader> this[int iIdx]
    {
      get
      {
        if (iIdx < 0 || iIdx >= this._PixelShaders.Count)
          throw new IndexOutOfRangeException();

        return this._PixelShaders[iIdx];
      }
    }

    /// <summary>
    /// Get shader count.
    /// </summary>
    public int Count
    {
      get
      {
        return this._PixelShaders.Count;
      }
    }

    /// <summary>
    /// Get profile name
    /// </summary>
    public string Profile
    {
      get => this._Profile.Name;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Remove all shaders
    /// </summary>
    public void Clear()
    {
      this._PixelShaders.ForEach(ps => ps.Value.Dispose());
      this._PixelShaders.Clear();
    }

    /// <summary>
    /// Add shader to the end of the list
    /// </summary>
    /// <param name="strName">Shader filename without extension.</param>
    /// <returns>True if shader has been added.</returns>
    public bool Add(string strName)
    {
      KeyValuePair<string, PixelShader> p = this._PixelShaders.Find(ps => ps.Key.Equals(strName, StringComparison.CurrentCultureIgnoreCase));
      if (p.Value != null)
      {
        //Existing shader
        this._PixelShaders.Add(new KeyValuePair<string, PixelShader>(p.Key, p.Value));
        return true;
      }
      else
      {
        //New shader
        PixelShader ps = this.load(strName);
        if (ps != null)
        {
          this._PixelShaders.Add(new KeyValuePair<string, PixelShader>(strName, ps));
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Remove shader at specific position
    /// </summary>
    /// <param name="iIdx">Index position.</param>
    /// <returns>True if shader has been removed.</returns>
    public bool RemoveAt(int iIdx)
    {
      if (iIdx < 0 || iIdx >= this._PixelShaders.Count)
        return false;

      string strName = this._PixelShaders[iIdx].Key;
      if (this._PixelShaders.Count(ps => ps.Key.Equals(strName, StringComparison.CurrentCultureIgnoreCase)) < 2)
        this._PixelShaders[iIdx].Value.Dispose();

      this._PixelShaders.RemoveAt(iIdx);
      return true;
    }

    /// <summary>
    /// Load Pixel Shaders
    /// </summary>
    /// <param name="strProfile">Profile name.</param>
    public void Load(string strProfile)
    {
      if (string.IsNullOrWhiteSpace(strProfile))
        strProfile = SHADER_PROFILE_DEFAULT;

      ShaderProfile profile = this._Profiles.Find(p => p.Name.Equals(strProfile, StringComparison.CurrentCultureIgnoreCase));
      if (string.IsNullOrWhiteSpace(profile.Name))
        profile = this._Profiles[0];

      this.load(profile);
    }

    /// <summary>
    /// Get shader names separated by '|'
    /// </summary>
    /// <returns></returns>
    public string GetNames()
    {
      if (this._PixelShaders.Count > 0)
      {
        if (this._PixelShaders.Count == 1)
          return this._PixelShaders[0].Key;

        StringBuilder sb = new StringBuilder(256);
        this._PixelShaders.ForEach(ps =>
        {
          if (sb.Length > 0)
            sb.Append('|');

          sb.Append(ps.Key);
        });

        return sb.ToString();
      }

      return string.Empty;
    }

    /// <summary>
    /// Performs the specified action on each shader of the shader list.
    /// </summary>
    /// <param name="action">The System.Action delegate to perform on each shader of the shader list.</param>
    public void ForEach(Action<KeyValuePair<string, PixelShader>> action)
    {
      for (int i = 0; i < this._PixelShaders.Count; i++)
        action(this._PixelShaders[i]);
    }

    /// <summary>
    /// Show Pixel Shader dialog menu
    /// </summary>
    public void ShowPixelShaderMenu()
    {
      IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      while (true)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(200096) + " [" + this._Profile.Name + ']'); // Pixel Shaders [{profile}]

        dlg.AddLocalizedString(200098); //Create new profile
        dlg.AddLocalizedString(200099); //Edit current profile

        int iOffset = 2;

        if (this._Profile.IsCustom)
        {
          dlg.AddLocalizedString(200100); //Remove current profile
          iOffset++;
        }

        this._Profiles.ForEach(p => dlg.Add(GUILocalizeStrings.Get(424) + ": " + p.Name)); //Select: 

        // show dialog and wait for result
        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedId == -1)
          return; //exit

        if (dlg.SelectedId == 200098) //Create new profile
        {
          IDialogboxKeyboard keyBoard = (IDialogboxKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);

          if (keyBoard == null)
            return;

          keyBoard.Reset();
          keyBoard.Text = string.Empty;
          keyBoard.DoModal(GUIWindowManager.ActiveWindow);

          if (!keyBoard.IsConfirmed)
            continue;

          string strNewProfile = keyBoard.Text.Trim();
          if (string.IsNullOrWhiteSpace(strNewProfile) || _Profiles.Exists(p => p.Name.Equals(strNewProfile, StringComparison.CurrentCultureIgnoreCase)))
            continue;

          //New profile
          this._Profiles.Add(new ShaderProfile(strNewProfile, string.Empty));
        }
        else if (dlg.SelectedId == 200099) //Edit current profile
        {
          this.editPixelShaderMenu(dlg);
        }
        else if (dlg.SelectedId == 200100) //Remove current profile
        {
          this._Profiles.Remove(this._Profile);
          this.load(this._Profiles[0]); //load default profile
        }
        else //Select:
        {
          this.load(this._Profiles[dlg.SelectedLabel - iOffset]);
          continue;
        }

        #region Save profiles to the MP settings
        using (Profile.Settings set = new Profile.MPSettings())
        {
          int i = 0;
          this._Profiles.ForEach(profile =>
          {
            if (profile.IsCustom)
            {
              set.SetValue(_SETTINGS_SECTION, _SETTINGS_ENTRY + i + _SETTINGS_ENTRY_NAME_SUFFIX, profile.Name);
              set.SetValue(_SETTINGS_SECTION, _SETTINGS_ENTRY + i++, profile.Shaders);
            }
            else
              set.SetValue(_SETTINGS_SECTION, _SETTINGS_ENTRY + profile.Name, profile.Shaders);
          });

          //Clear others
          while (true)
          {
            string strEntry = _SETTINGS_ENTRY + i + _SETTINGS_ENTRY_NAME_SUFFIX;

            if (string.IsNullOrWhiteSpace(set.GetValueAsString(_SETTINGS_SECTION, strEntry, null)))
              break;

            set.RemoveEntry(_SETTINGS_SECTION, strEntry);
            set.RemoveEntry(_SETTINGS_SECTION, _SETTINGS_ENTRY + i++);
          }
        }
        #endregion
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Load shader from the file
    /// </summary>
    /// <param name="strName">Name of the shader without extension.</param>
    /// <returns>Compiled pixel shader.</returns>
    private PixelShader load(string strName)
    {
      try
      {
        string strFile = SHADER_FOLDER_NAME + '\\' + strName + SHADER_EXTENSION;
        if (File.Exists(strFile))
        {
          string strContent = File.ReadAllText(strFile);
          string strProfile = "ps_2_0";

          int iIdx = strContent.IndexOf("$MinimumShaderProfile:");
          if (iIdx >= 0)
          {
            iIdx += 22;
            int iIdxEnd = strContent.IndexOf("\r", iIdx);
            strProfile = strContent.Substring(iIdx, iIdxEnd - iIdx).Trim();
          }

          CompilationResult result = ShaderBytecode.Compile(strContent, "main", strProfile, ShaderFlags.None);
          if (result != null && !result.HasErrors)
            return new PixelShader(this._Device, result);
        }
      }
      catch (Exception ex)
      {
        Log.Error("PixelShaderCollection: load() Name:'{1}' Exception: {0}", ex.Message, strName);
      }

      return null;
    }

    private void load(ShaderProfile profile)
    {
      this.Clear();

      this._Profile = profile;

      if (profile.Shaders != null)
      {
        string[] names = profile.Shaders.Split('|');
        for (int i = 0; i < names.Length; i++)
          this.Add(names[i]);
      }
    }


    private void editPixelShaderMenu(IDialogbox dlg)
    {
      while (true)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(200096) + " [" + this._Profile.Name + ']'); // Pixel Shaders [{profile}]

        dlg.AddLocalizedString(300063); // Add

        this.ForEach(ps => dlg.Add(GUILocalizeStrings.Get(300064) + ": " + ps.Key)); // Remove:

        // show dialog and wait for result
        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedId == -1)
          return;

        if (dlg.SelectedLabel == 0)
        {
          string strName = this.showPixelShaderFileMenu(dlg);
          if (strName != null)
            this.Add(strName);
          else
            return;
        }
        else
          this.RemoveAt(dlg.SelectedLabel - 1);

        //Update current profile
        this._Profile.Shaders = this.GetNames();
      }
    }

    private string showPixelShaderFileMenu(IDialogbox dlg)
    {
      dlg.Reset();
      dlg.SetHeading(200097); // Select Pixel Shader

      if (Directory.Exists(SHADER_FOLDER_NAME))
      {
        DirectoryInfo di = new DirectoryInfo(SHADER_FOLDER_NAME);
        FileInfo[] files = di.GetFiles("*" + SHADER_EXTENSION);

        for (int i = 0; i < files.Length; i++)
        {
          string strName = files[i].Name.Substring(0, files[i].Name.Length - SHADER_EXTENSION.Length);

          dlg.Add(strName);
        }
      }

      // show dialog and wait for result
      dlg.DoModal(GUIWindowManager.ActiveWindow);

      if (dlg.SelectedId == -1)
        return null;

      return dlg.SelectedLabelText;
    }

    #endregion
  }
}
