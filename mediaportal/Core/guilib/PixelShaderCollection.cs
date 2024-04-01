#region Copyright (C) 2005-2024 Team MediaPortal

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
    #region Constants
    public const string SHADER_EXTENSION = ".hlsl";
    public const string SHADER_FOLDER_NAME = "Shaders";
    public const string SHADER_PROFILE_DEFAULT = "Default";
    #endregion

    #region Private fields
    private readonly Device _Device;
    private readonly List<KeyValuePair<string, PixelShader>> _PixelShaders = new List<KeyValuePair<string, PixelShader>>();
    private string _Profile = SHADER_PROFILE_DEFAULT;
    #endregion

    #region ctor
    public PixelShaderCollection(Device device)
    {
      this._Device = device;
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
      get => this._Profile;
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
    /// <param name="strNames">Shader filenames separated by '|' (without extension).</param>
    /// <param name="strProfile">Profile name.</param>
    public void Load(string strNames, string strProfile)
    {
      this.Clear();

      this._Profile = !string.IsNullOrWhiteSpace(strProfile) ? strProfile : SHADER_PROFILE_DEFAULT;

      if (strNames != null)
      {
        string[] names = strNames.Split('|');
        for (int i = 0; i < names.Length; i++)
          this.Add(names[i]);
      }
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
      for (int i = 0; i< this._PixelShaders.Count; i++)
        action(this._PixelShaders[i]);
    }

    /// <summary>
    /// Show Pixel Shader dialog menu
    /// </summary>
    public void ShowPixelShaderMenu()
    {
      IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(200096) + " [" + this._Profile + ']'); // Pixel Shaders [{profile}]

      dlg.AddLocalizedString(300063); // Add

      this.ForEach(ps => dlg.Add(GUILocalizeStrings.Get(300064) + ": " + ps.Key)); // Remove:

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

      using (Profile.Settings xmlWritter = new Profile.MPSettings())
      {
        xmlWritter.SetValue("general", "VideoPixelShader" + this._Profile, this.GetNames());
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
