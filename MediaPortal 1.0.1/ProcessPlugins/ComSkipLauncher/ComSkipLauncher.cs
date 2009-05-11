using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace ProcessPlugins.ComSkipLauncher
{
  public class ComSkipLauncher : IPlugin, ISetupForm
  {
    #region Constants

    public static readonly string MPConfigFile = Config.GetFolder(Config.Dir.Config) + "\\MediaPortal.xml";

    public static readonly string DefaultProgram = "ComSkip.exe";
    public static readonly string DefaultParameters = "--playnice \"{0}\"";

    #endregion Constants

    #region Members

    private bool _runAtStart = false;
    private string _program = DefaultProgram;
    private string _parameters = DefaultParameters;

    #endregion Members

    #region IPlugin Members

    public void Start()
    {
      Log.Info("ComSkipLauncher plugin: Start");

      LoadSettings();

      Recorder.OnTvRecordingStarted += new Recorder.OnTvRecordingHandler(Recorder_OnTvRecordingStarted);
      Recorder.OnTvRecordingEnded += new Recorder.OnTvRecordingHandler(Recorder_OnTvRecordingEnded);
    }

    public void Stop()
    {
      Log.Info("ComSkipLauncher plugin: Stop");

      Recorder.OnTvRecordingStarted -= new Recorder.OnTvRecordingHandler(Recorder_OnTvRecordingStarted);
      Recorder.OnTvRecordingEnded -= new Recorder.OnTvRecordingHandler(Recorder_OnTvRecordingEnded);
    }

    #endregion

    #region ISetupForm Members

    public string Author()
    {
      return "and-81";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public string Description()
    {
      return "Launches ComSkip on recordings.";
    }

    public int GetWindowId()
    {
      return 0;
    }

    public bool HasSetup()
    {
      return true;
    }

    public string PluginName()
    {
      return "ComSkip Launcher";
    }

    public void ShowPlugin()
    {
      LoadSettings();

      Configuration configuration = new Configuration();
      configuration.RunAtStart = _runAtStart;
      configuration.Program = _program;
      configuration.Parameters = _parameters;

      if (configuration.ShowDialog() == DialogResult.OK)
      {
        _runAtStart = configuration.RunAtStart;
        _program = configuration.Program;
        _parameters = configuration.Parameters;

        SaveSettings();
      }
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = strButtonImage = strButtonImageFocus = strPictureImage = "";
      return false;
    }

    #endregion

    #region Implementation

    private void Recorder_OnTvRecordingStarted(string recordingFilename, TVRecording recording, TVProgram program)
    {
      if (!_runAtStart)
      {
        return;
      }

      Log.Debug("ComSkipLauncher plugin - Recorder_OnTvRecordingStarted(): \"{0}\"", recordingFilename);

      try
      {
        string parameters = ProcessParameters(_parameters, recordingFilename, recording.Channel);

        Log.Info(
          "ComSkipLauncher plugin: Recording started ({0} on {1}), launching program ({2} {3}) ...",
          recordingFilename,
          recording.Channel,
          _program,
          parameters
          );

        LaunchProcess(_program, parameters, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher plugin - Recorder_OnTvRecordingStarted(): {0}", ex.Message);
      }
    }

    private void Recorder_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      if (_runAtStart)
      {
        return;
      }

      Log.Debug("ComSkipLauncher plugin - Recorder_OnTvRecordingEnded(): \"{0}\"", recordingFilename);

      try
      {
        string parameters = ProcessParameters(_parameters, recordingFilename, recording.Channel);

        Log.Info(
          "ComSkipLauncher plugin: Recording ended ({0} on {1}), launching program ({2} {3}) ...",
          recordingFilename,
          recording.Channel,
          _program,
          parameters
          );

        LaunchProcess(_program, parameters, Path.GetDirectoryName(_program), ProcessWindowStyle.Hidden);
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher plugin - Recorder_OnTvRecordingEnded(): {0}", ex.Message);
      }
    }

    private void LoadSettings()
    {
      try
      {
        using (Settings xmlreader = new Settings(MPConfigFile))
        {
          _runAtStart = xmlreader.GetValueAsBool("ComSkipLauncher", "RunAtStart", false);
          _program = xmlreader.GetValueAsString("ComSkipLauncher", "Program", DefaultProgram);
          _parameters = xmlreader.GetValueAsString("ComSkipLauncher", "Parameters", DefaultParameters);
        }
      }
      catch (Exception ex)
      {
        _runAtStart = false;
        _program = DefaultProgram;
        _parameters = DefaultParameters;

        Log.Error("ComSkipLauncher plugin - LoadSettings(): {0}", ex.Message);
      }
    }

    private void SaveSettings()
    {
      try
      {
        using (Settings xmlwriter = new Settings(MPConfigFile))
        {
          xmlwriter.SetValueAsBool("ComSkipLauncher", "RunAtStart", _runAtStart);
          xmlwriter.SetValue("ComSkipLauncher", "Program", _program);
          xmlwriter.SetValue("ComSkipLauncher", "Parameters", _parameters);
        }
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher plugin - SaveSettings(): {0}", ex.Message);
      }
    }

    internal static string ProcessParameters(string input, string fileName, string channel)
    {
      string output = string.Empty;

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
        Log.Error("ComSkipLauncher plugin - ProcessParameters(): {0}", ex.Message);
      }

      return output;
    }

    internal static void LaunchProcess(string program, string parameters, string workingFolder,
                                       ProcessWindowStyle windowStyle)
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
        Log.Error("ComSkipLauncher plugin - LaunchProcess(): {0}", ex.Message);
      }
    }

    #endregion Implementation
  }
}