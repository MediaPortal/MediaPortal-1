﻿#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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

using MediaPortal.GUI.Library;

namespace MediaPortal.Player
{
  public abstract class BaseExternalAudioPlayer : BaseAudioPlayer, ISetupForm, IExternalPlayer
  {
    public override bool IsExternal
    {
      get { return true; }
    }

    public override bool HasVideo
    {
      get { return false; }
    }

    #region ISetupForm Members

    public string PluginName()
    {
      return PlayerName;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public virtual string Description()
    {
      return "External Player for: " + string.Join(",", GetAllSupportedExtensions());
    }

    public string Author()
    {
      return AuthorName;
    }

    public virtual void ShowPlugin()
    {
      ; //nothing to show
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool CanEnable()
    {
      return true;
    }

    public virtual int GetWindowId()
    {
      return -1;
    }

    public virtual bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = string.Empty;
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return false;
    }

    #endregion

    /// <summary>
    /// Property to enable/disable the external audio player
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// This method returns the name of the external player
    /// </summary>
    /// <returns>string representing the name of the external player</returns>
    public abstract string PlayerName { get; }

    /// <summary>
    /// This method returns the version number of the plugin
    /// </summary>
    public abstract string VersionNumber { get; }

    /// <summary>
    /// This method returns the author of the external player
    /// </summary>
    /// <returns></returns>
    public abstract string AuthorName { get; }

    /// <summary>
    /// Returns all the extensions that the external player supports.  
    /// The return value is an array of extensions of the form: .wma, .mp3, etc...
    /// </summary>
    /// <returns>array of strings of extensions in the form: .wma, .mp3, etc..</returns>
    public abstract string[] GetAllSupportedExtensions();

    /// <summary>
    /// Returns true or false depending if the filename passed is supported or not.
    /// The filename could be just the filename or the complete path of a file.
    /// </summary>
    /// <param name="filename">a fully qualified path and filename or just the filename</param>
    /// <returns>true or false if the file is supported by the player</returns>
    public abstract bool SupportsFile(string filename);
  }
}
