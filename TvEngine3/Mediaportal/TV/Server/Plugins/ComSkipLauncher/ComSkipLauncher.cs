#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using Castle.Core;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.ComSkipLauncher
{
  [Interceptor("PluginExceptionInterceptor")]
  public class ComSkipLauncher : ITvServerPlugin
  {


    #region Constants

    private const bool DefaultRunAtStrart = true;
    private const string DefaultProgram = "ComSkip.exe";
    private const string DefaultParameters = "\"{0}\"";

    #endregion Constants

    #region Members

    private static bool _runAtStart = DefaultRunAtStrart;
    private static string _program = DefaultProgram;
    private static string _parameters = DefaultParameters;

    #endregion Members

    #region Properties

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get { return "ComSkipLauncher"; }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get { return "1.0.2.0"; }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get { return "and-81"; }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get { return false; }
    }

    internal static bool RunAtStart
    {
      get { return _runAtStart; }
      set { _runAtStart = value; }
    }

    internal static string Program
    {
      get { return _program; }
      set { _program = value; }
    }

    internal static string Parameters
    {
      get { return _parameters; }
      set { _parameters = value; }
    }

    #endregion Properties

    #region IPlugin Members

    [CLSCompliant(false)]
    public void Start(IInternalControllerService controllerService)
    {      
      this.LogInfo("plugin: ComSkipLauncher start");

      LoadSettings();

      GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent += ComSkipLauncher_OnTvServerEvent;
    }

    public void Stop()
    {
      this.LogInfo("plugin: ComSkipLauncher stop");

      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= ComSkipLauncher_OnTvServerEvent;
      }
    }

    [CLSCompliant(false)]
    public SectionSettings Setup
    {
      get { return new ComSkipSetup(); }
    }

    #endregion

    #region Implementation

    private static void ComSkipLauncher_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      try
      {
        TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;
        
        var recording = RecordingManagement.GetRecording(tvEvent.Recording);
        if (tvEvent.EventType == TvServerEventType.RecordingStarted && _runAtStart)
        {
          if (recording.IdChannel.HasValue)
          {
            Channel channel = ChannelManagement.GetChannel(recording.IdChannel.GetValueOrDefault());

            string parameters = ProcessParameters(_parameters, recording.FileName, channel.DisplayName);

            Log.Info("ComSkipLauncher: Recording started ({0} on {1}), launching program ({2} {3}) ...",
                     recording.FileName, channel.DisplayName, _program, parameters);

            LaunchProcess(_program, parameters, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
          }
        }
        else if (tvEvent.EventType == TvServerEventType.RecordingEnded && !_runAtStart)
        {
          if (recording.IdChannel.HasValue)
          {
            Channel channel = ChannelManagement.GetChannel(recording.IdChannel.GetValueOrDefault());
            string parameters = ProcessParameters(_parameters, recording.FileName, channel.DisplayName);

            Log.Info("ComSkipLauncher: Recording ended ({0} on {1}), launching program ({2} {3}) ...",
                     recording.FileName, channel.DisplayName, _program, parameters);

            LaunchProcess(_program, parameters, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ComSkipLauncher - ComSkipLauncher_OnTvServerEvent()");
      }
    }

    internal static void LoadSettings()
    {
      try
      {
        
        _runAtStart =
          Convert.ToBoolean(SettingsManagement.GetSetting("ComSkipLauncher_RunAtStart", DefaultRunAtStrart.ToString()).Value);
        _program = SettingsManagement.GetSetting("ComSkipLauncher_Program", DefaultProgram).Value;
        _parameters = SettingsManagement.GetSetting("ComSkipLauncher_Parameters", DefaultParameters).Value;
      }
      catch (Exception ex)
      {
        _runAtStart = DefaultRunAtStrart;
        _program = DefaultProgram;
        _parameters = DefaultParameters;

        Log.Error(ex, "ComSkipLauncher - LoadSettings()");
      }
    }

    internal static void SaveSettings()
    {
      try
      {
        SettingsManagement.SaveSetting("ComSkipLauncher_RunAtStart", _runAtStart.ToString());
        SettingsManagement.SaveSetting("ComSkipLauncher_Program", _program);
        SettingsManagement.SaveSetting("ComSkipLauncher_Parameters", _parameters);        
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ComSkipLauncher - SaveSettings()");
      }
    }

    internal static string ProcessParameters(string input, string fileName, string channel)
    {
      string output = String.Empty;

      try
      {
        output = string.Format(
          input, // Format
          fileName, // {0} = Recorded filename (includes path)
          Path.GetFileName(fileName), // {1} = Recorded filename (w/o path)
          Path.GetFileNameWithoutExtension(fileName), // {2} = Recorded filename (w/o path or extension)
          Path.GetDirectoryName(fileName), // {3} = Recorded file path
          DateTime.Now.ToShortDateString(), // {4} = Current date
          DateTime.Now.ToShortTimeString(), // {5} = Current time
          channel // {6} = Channel name
          );
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ComSkipLauncher - ProcessParameters()");
      }

      return output;
    }

    internal static void LaunchProcess(string program, string parameters, string workingFolder,
                                       ProcessWindowStyle windowStyle)
    {
      try
      {
        using (var process = new Process()) 
        {
          process.StartInfo = new ProcessStartInfo();
          process.StartInfo.Arguments = parameters;
          process.StartInfo.FileName = program;
          process.StartInfo.WindowStyle = windowStyle;
          process.StartInfo.WorkingDirectory = workingFolder;
          if (OSInfo.OSInfo.VistaOrLater())
          {
            process.StartInfo.Verb = "runas";
          }

          process.Start();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ComSkipLauncher - LaunchProcess()");
      }
    }

    #endregion Implementation
  }
}