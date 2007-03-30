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

    public static readonly string DefaultProgram     = "\\Program Files\\ComSkip\\ComSkip.exe";
    public static readonly string DefaultParameters  = "\"{0}\"";

    #endregion Constants

    #region Members

    bool _runAtStart    = true;
    string _program     = DefaultProgram;
    string _parameters  = DefaultParameters;

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
      get { return new SetupTv.Sections.PluginSetup(); }
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
          string parameters = ProcessParameters(_parameters, tvEvent.Recording.FileName, tvEvent.channel.Name);

          Log.Info("ComSkipLauncher: Recording started ({0} on {1}), launching program ({2} {3}) ...", tvEvent.Recording.FileName, tvEvent.channel.Name, _program, parameters);

          LaunchProcess(_program, parameters, Path.GetDirectoryName(tvEvent.Recording.FileName), ProcessWindowStyle.Hidden);
        }
        else if (tvEvent.EventType == TvServerEventType.RecordingEnded && !_runAtStart)
        {
          string parameters = ProcessParameters(_parameters, tvEvent.Recording.FileName, tvEvent.channel.Name);

          Log.Info("ComSkipLauncher: Recording ended ({0} on {1}), launching program ({2} {3}) ...", tvEvent.Recording.FileName, tvEvent.channel.Name, _program, parameters);

          LaunchProcess(_program, parameters, Path.GetDirectoryName(tvEvent.Recording.FileName), ProcessWindowStyle.Hidden);
        }
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - ComSkipLauncher_OnTvServerEvent(): {0}", ex.Message);
      }
    }

    void LoadSettings()
    {
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();

        _runAtStart  = Convert.ToBoolean(layer.GetSetting("ComSkipLauncher_RunAtStart", "False").Value);
        _program     = layer.GetSetting("ComSkipLauncher_Program", DefaultProgram).Value;
        _parameters  = layer.GetSetting("ComSkipLauncher_Parameters", DefaultParameters).Value;
      }
      catch (Exception ex)
      {
        _runAtStart = true;
        _program    = DefaultProgram;
        _parameters = DefaultParameters;

        Log.Error("ComSkipLauncher - LoadSettings(): {0}", ex.Message);
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
