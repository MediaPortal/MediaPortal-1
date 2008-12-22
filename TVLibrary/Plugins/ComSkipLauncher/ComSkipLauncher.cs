using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

using TvLibrary.Log;
using TvControl;
using SetupTv;
using TvEngine;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvDatabase;

namespace TvEngine
{

  public class ComSkipLauncher : ITvServerPlugin
  {

    #region Constants

    static readonly bool    DefaultRunAtStrart  = true;
    static readonly string  DefaultProgram      = "ComSkip.exe";
    static readonly string  DefaultParameters   = "\"{0}\"";

    #endregion Constants

    #region Members

    static bool   _runAtStart   = DefaultRunAtStrart;
    static string _program      = DefaultProgram;
    static string _parameters   = DefaultParameters;

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
    public void Start(IController controller)
    {
      Log.Info("ComSkipLauncher: Start");

      LoadSettings();

      GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent += new TvServerEventHandler(ComSkipLauncher_OnTvServerEvent);
    }

    public void Stop()
    {
      Log.Info("ComSkipLauncher: Stop");

      GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= new TvServerEventHandler(ComSkipLauncher_OnTvServerEvent);
    }

    [CLSCompliant(false)]
    public SetupTv.SectionSettings Setup
    {
      get { return new SetupTv.Sections.ComSkipSetup(); }
    }

    #endregion

    #region Implementation

    void ComSkipLauncher_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      try
      {
        TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;

        if (tvEvent.EventType == TvServerEventType.RecordingStarted && _runAtStart)
        {
          Channel channel = TvDatabase.Channel.Retrieve(tvEvent.Recording.IdChannel);
          
          string parameters = ProcessParameters(_parameters, tvEvent.Recording.FileName, channel.DisplayName);
          
          Log.Info("ComSkipLauncher: Recording started ({0} on {1}), launching program ({2} {3}) ...", tvEvent.Recording.FileName, channel.DisplayName, _program, parameters);

          LaunchProcess(_program, parameters, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
        }
        else if (tvEvent.EventType == TvServerEventType.RecordingEnded && !_runAtStart)
        {
          Channel channel = TvDatabase.Channel.Retrieve(tvEvent.Recording.IdChannel);

          string parameters = ProcessParameters(_parameters, tvEvent.Recording.FileName, channel.DisplayName);

          Log.Info("ComSkipLauncher: Recording ended ({0} on {1}), launching program ({2} {3}) ...", tvEvent.Recording.FileName, channel.DisplayName, _program, parameters);

          LaunchProcess(_program, parameters, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
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

        _runAtStart   = Convert.ToBoolean(layer.GetSetting("ComSkipLauncher_RunAtStart", DefaultRunAtStrart.ToString()).Value);
        _program     = layer.GetSetting("ComSkipLauncher_Program", DefaultProgram).Value;
        _parameters  = layer.GetSetting("ComSkipLauncher_Parameters", DefaultParameters).Value;
      }
      catch (Exception ex)
      {
        _runAtStart = DefaultRunAtStrart;
        _program    = DefaultProgram;
        _parameters = DefaultParameters;

        Log.Error("ComSkipLauncher - LoadSettings(): {0}", ex.Message);
      }
    }
    internal static void SaveSettings()
    {
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting;

        setting = layer.GetSetting("ComSkipLauncher_RunAtStart");
        setting.Value = _runAtStart.ToString();
        setting.Persist();

        setting = layer.GetSetting("ComSkipLauncher_Program");
        setting.Value = _program;
        setting.Persist();

        setting = layer.GetSetting("ComSkipLauncher_Parameters");
        setting.Value = _parameters;
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
          input,                                      // Format
          fileName,                                   // {0} = Recorded filename (includes path)
          Path.GetFileName(fileName),                 // {1} = Recorded filename (w/o path)
          Path.GetFileNameWithoutExtension(fileName), // {2} = Recorded filename (w/o path or extension)
          Path.GetDirectoryName(fileName),            // {3} = Recorded file path
          DateTime.Now.ToShortDateString(),           // {4} = Current date
          DateTime.Now.ToShortTimeString(),           // {5} = Current time
          channel                                     // {6} = Channel name
        );
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - ProcessParameters(): {0}", ex.Message);
      }

      return output;
    }

    internal static void LaunchProcess(string program, string parameters, string workingFolder, ProcessWindowStyle windowStyle)
    {
      try
      {
        Process process = new Process();
        process.StartInfo = new ProcessStartInfo();
        process.StartInfo.Arguments = parameters;
        process.StartInfo.FileName = program;
        process.StartInfo.WindowStyle = windowStyle;
        process.StartInfo.WorkingDirectory = workingFolder;

        process.Start();
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - LaunchProcess(): {0}", ex.Message);
      }
    }

    #endregion Implementation

  }

}
