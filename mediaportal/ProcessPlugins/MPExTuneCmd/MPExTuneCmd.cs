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
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.MPExTuneCmd
{
  /// <summary>
  /// Summary description for MPExTuneCmd.
  /// </summary>
  public class MPExTuneCmd : IPlugin, ISetupForm
  {
    public static int WINDOW_MPExTuneCmd = 9099; // a window ID shouldn't be needed when a non visual plugin ?!
    private static string s_TuneCmd = "";
    private static string s_TuneParam = "";
    private const string s_version = "0.1";

    public MPExTuneCmd()
    {
    }

    public void Start()
    {
      Log.Info("MPExTuneCmd {0} plugin starting.", s_version);

      LoadSettings();

      Log.Info("Adding message handler for MPExTuneCmd {0}.", s_version);

      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return;
    }

    public void Stop()
    {
      Log.Info("MPExTuneCmd {0} plugin stopping.", s_version);
      return;
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        s_TuneCmd = xmlreader.GetValueAsString("MPExTuneCmd", "commandloc", "C:\\dtvcon\\dtvcmd.exe");
        s_TuneParam = xmlreader.GetValueAsString("MPExTuneCmd", "commanddelim", "");
      }
    }

    private void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
          bool bIsInteger;
          double retNum;
          bIsInteger = Double.TryParse(message.Label, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
          this.ChangeTunerChannel(message.Label);
          break;
      }
    }

    public void ChangeTunerChannel(string channel_data)
    {
      Log.Info("MPExTuneCmd processing external tuner cmd: {0}", s_TuneCmd + " " + s_TuneParam + channel_data);
      this.RunProgram(s_TuneCmd, s_TuneParam + channel_data);
    }

    /// <summary>
    /// Runs a particular program in the local file system
    /// </summary>
    /// <param name="exeName"></param>
    /// <param name="argsLine"></param>
    private void RunProgram(string exeName, string argsLine)
    {
      ProcessStartInfo psI = new ProcessStartInfo(exeName, argsLine);
      Process newProcess = new Process();

      try
      {
        newProcess.StartInfo.FileName = exeName;
        newProcess.StartInfo.Arguments = argsLine;
        newProcess.StartInfo.UseShellExecute = true;
        newProcess.StartInfo.CreateNoWindow = true;
        newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        newProcess.Start();
      }

      catch (Exception e)
      {
        throw e;
      }
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "MPExTuneCmd";
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return WINDOW_MPExTuneCmd;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = "MPExTuneCmd Plugin";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public string Author()
    {
      return "901Racer";
    }

    public string Description()
    {
      return "Controls your external tuner";
    }

    /// <summary>
    /// This method is called by the plugin screen to show the configuration for the foobar plugin
    /// </summary>
    public void ShowPlugin()
    {
      Form setup = new MPExTuneCmdForm();
      setup.ShowDialog();
    }

    #endregion
  }
}