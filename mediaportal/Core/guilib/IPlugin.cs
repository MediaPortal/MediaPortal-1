#region Copyright (C) 2005-2010 Team MediaPortal

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

#endregion

using System.Windows.Forms;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Plugin interface for non-GUI plugins (process plugins)
  /// process plugins dont have an window and do all their processing in the background
  /// Example of a process plugin is the winlirc plugin which receives the actions from a remote control
  /// and sends them to mediaportal
  /// Process plugins should be copied in the plugins/process folder of mediaportal
  /// Process plugins can have their own setup by implementing ISetupForm
  /// </summary>
  public interface IPlugin
  {
    /// <summary>
    /// This method will be called by mediaportal to start your process plugin
    /// </summary>
    void Start();


    /// <summary>
    /// This method will be called by mediaportal to stop your process plugin
    /// </summary>
    void Stop();
  }

  public interface IPluginReceiver : IPlugin
  {
    /// <summary>
    /// This method will be called by mediaportal to send system messages to your process plugin,
    /// if the plugin implements WndProc (optional) / added by mPod
    /// </summary>
    bool WndProc(ref Message msg);
  }
}