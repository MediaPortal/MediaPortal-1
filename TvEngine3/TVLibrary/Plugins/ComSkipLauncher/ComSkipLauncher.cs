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
using TvLibrary.Log;
using TvControl;
using SetupTv;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvDatabase;

namespace TvEngine
{
  public class ComSkipLauncher : ITvServerPlugin
  {
    #region Constants

    private const bool DefaultRunAtStrart = true;
    private const string DefaultProgram = "ComSkip.exe";
    private const string DefaultParameters = "\"{0}\"";
    private const ProcessPriorityClass DefaultPriority = ProcessPriorityClass.BelowNormal;

    #endregion Constants

    #region Members

    private static bool _runAtStart = DefaultRunAtStrart;
    private static string _program = DefaultProgram;
    private static string _parameters = DefaultParameters;
    private static ProcessPriorityClass _priority = DefaultPriority;

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
      get { return "1.0.3.0"; }
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

    internal static ProcessPriorityClass Priority
    {
      get { return _priority; }
      set { _priority = value; }
    }

    #endregion Properties

    #region IPlugin Members

    [CLSCompliant(false)]
    public void Start(IController controller)
    {
      Log.Info("plugin: ComSkipLauncher start");

      LoadSettings();

      GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent += ComSkipLauncher_OnTvServerEvent;
    }

    public void Stop()
    {
      Log.Info("plugin: ComSkipLauncher stop");

      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= ComSkipLauncher_OnTvServerEvent;
      }
    }

    [CLSCompliant(false)]
    public SectionSettings Setup
    {
      get { return new SetupTv.Sections.ComSkipSetup(); }
    }

    #endregion

    #region Implementation

    private void ComSkipLauncher_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      try
      {
        TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;

        if ((tvEvent.EventType == TvServerEventType.RecordingStarted && _runAtStart)
          || (tvEvent.EventType == TvServerEventType.RecordingEnded && !_runAtStart))
        {
          Channel channel = Channel.Retrieve(tvEvent.Recording.IdChannel);

          LaunchProcess(tvEvent.Recording.FileName, channel.DisplayName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - ComSkipLauncher_OnTvServerEvent(): {0}", ex.Message);
      }
    }

    internal static void LoadSettings()
    {
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();

        _runAtStart =
          Convert.ToBoolean(layer.GetSetting("ComSkipLauncher_RunAtStart", DefaultRunAtStrart.ToString()).Value);
        _program = layer.GetSetting("ComSkipLauncher_Program", DefaultProgram).Value;
        _parameters = layer.GetSetting("ComSkipLauncher_Parameters", DefaultParameters).Value;

        var priorityString = layer.GetSetting("ComSkipLauncher_Priority", DefaultPriority.ToString()).Value;
        _priority = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), priorityString);
      }
      catch (Exception ex)
      {
        _runAtStart = DefaultRunAtStrart;
        _program = DefaultProgram;
        _parameters = DefaultParameters;

        Log.Error("ComSkipLauncher - LoadSettings(): {0}", ex.Message);
      }
    }

    internal static void SaveSettings()
    {
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();

        Setting setting = layer.GetSetting("ComSkipLauncher_RunAtStart");
        setting.Value = _runAtStart.ToString();
        setting.Persist();

        setting = layer.GetSetting("ComSkipLauncher_Program");
        setting.Value = _program;
        setting.Persist();

        setting = layer.GetSetting("ComSkipLauncher_Parameters");
        setting.Value = _parameters;
        setting.Persist();

        setting = layer.GetSetting("ComSkipLauncher_Priority");
        setting.Value = _priority.ToString();
        setting.Persist();
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - SaveSettings(): {0}", ex.Message);
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
        Log.Error("ComSkipLauncher - ProcessParameters(): {0}", ex.Message);
      }

      return output;
    }

    internal void LaunchProcess(string fileName, string channel)
    {
      string parameters = ProcessParameters(_parameters, fileName, channel);

      Log.Info("ComSkipLauncher: Recording " + ((_runAtStart) ? "started" : "ended") + " ({0} on {1}), launching program ({2} {3}) priority: {4} ...",
               fileName, channel, _program, parameters, _priority.ToString());

      LaunchProcess(_program, parameters, _priority, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
    }

    internal static void LaunchProcess(string program, string parameters, ProcessPriorityClass priority, string workingFolder,
                                       ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
    {
      try
      {
        Process process = new Process();
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
        process.PriorityClass = priority;
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - LaunchProcess(): {0}", ex.Message);
      }
    }

    #endregion Implementation
  }
}