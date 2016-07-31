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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Thumbnailer
{
  /// <summary>
  /// The main thumbnailer class.
  /// </summary>
  public class Thumbnailer : IDisposable
  {
    private const string GENERATOR_EXECUTABLE_NAME = "ffmpeg.exe";
    private const string FORMAT_FILE_EXTENSION = ".jpg";

    #region variables

    private object _lockQueue = new object();
    private Queue<Recording> _queue = new Queue<Recording>();

    private Thread _thread = null;
    private AutoResetEvent _threadQueueEvent = null;

    private object _lockConfig = new object();
    private bool _isEnabled = true;
    private ThumbnailSettings _settings = null;
    private bool _copyToRecordingFolder = false;

    private bool _isRegisteredForTveEvents = false;

    #endregion

    ~Thumbnailer()
    {
      Dispose(false);
    }

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Stop();
      }
    }

    #endregion

    /// <summary>
    /// Get the thumbnail storage path.
    /// </summary>
    private static string ThumbnailPath
    {
      get
      {
        return Path.Combine(PathManager.GetDataPath, "Thumbnails");
      }
    }

    /// <summary>
    /// Get the full thumbnail path and file name for a recording.
    /// </summary>
    /// <param name="recordingFileName">The recording's file name.</param>
    public static string GetThumbnailFileName(string recordingFileName)
    {
      return Path.Combine(ThumbnailPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(recordingFileName), FORMAT_FILE_EXTENSION));
    }

    /// <summary>
    /// Get the file contents of a thumbnail for a recording.
    /// </summary>
    /// <param name="recordingFileName">The recording's file name.</param>
    /// <returns>the contents of the thumbnail file</returns>
    public static byte[] GetThumbnailForRecording(string recordingFileName)
    {
      string thumbnailFileName = GetThumbnailFileName(recordingFileName);
      if (!File.Exists(thumbnailFileName))
      {
        return new byte[0];
      }
      try
      {
        using (FileStream fileStream = new FileStream(thumbnailFileName, FileMode.Open, FileAccess.Read))
        {
          long length = fileStream.Length;
          byte[] data = new byte[length];
          fileStream.Read(data, 0, (int)length);
          fileStream.Close();
          return data;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "thumbnailer: failed to read thumbnail for recording, file name = {0}", recordingFileName);
      }
      return new byte[0];
    }

    /// <summary>
    /// Create thumbnails for all recordings that currently don't have thumbnails.
    /// </summary>
    public void CreateMissingThumbnails()
    {
      Log.Info("thumbnailer: create missing thumbnails");
      // Do this in a thread because there might be lots of recordings.
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          IList<Recording> recordings = RecordingManagement.ListAllRecordingsByMediaType(MediaType.Television);
          lock (_lockQueue)
          {
            foreach (Recording recording in recordings)
            {
              _queue.Enqueue(recording);
              Log.Debug("  {0}", recording.FileName ?? string.Empty);
            }
          }
          _threadQueueEvent.Set();
        }
      );
    }

    /// <summary>
    /// Delete all existing thumbnails.
    /// </summary>
    public static void DeleteExistingThumbnails()
    {
      Log.Info("thumbnailer: delete existing thumbnails");
      // Do this in a thread because there might be lots of files.
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          string[] fileNames = new string[0];
          try
          {
            fileNames = Directory.GetFiles(ThumbnailPath);
          }
          catch (Exception ex)
          {
            Log.Warn(ex, "thumbnailer: failed to get list of existing thumbnails for deletion");
            return;
          }
          foreach (string fileName in fileNames)
          {
            if (string.Equals(Path.GetExtension(fileName).ToLowerInvariant(), FORMAT_FILE_EXTENSION.ToLowerInvariant()))
            {
              Log.Debug("  {0}", fileName);
              try
              {
                File.Delete(fileName);
              }
              catch (Exception ex)
              {
                Log.Warn(ex, "thumbnailer: failed to delete thumbnail, file name = {0}", fileName);
              }
            }
          }
        }
      );
    }

    /// <summary>
    /// Reload the thumbnailer's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("thumbnailer: reload configuration");

      lock (_lockConfig)
      {
        _isEnabled = SettingsManagement.GetValue("thumbnailerEnabled", true);
        _copyToRecordingFolder = SettingsManagement.GetValue("thumbnailerCopyToRecordingFolder", false);

        RecordingThumbnailQuality thumbnailQuality = (RecordingThumbnailQuality)SettingsManagement.GetValue("thumbnailerQuality", (int)RecordingThumbnailQuality.Highest);
        _settings = new ThumbnailSettings(thumbnailQuality);
        _settings.ColumnCount = SettingsManagement.GetValue("thumbnailerColumnCount", 1);
        _settings.RowCount = SettingsManagement.GetValue("thumbnailerRowCount", 1);
        _settings.TimeOffset = new TimeSpan(0, SettingsManagement.GetValue("thumbnailerTimeOffset", 3), 0);
        foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
        {
          if (info.FilenameExtension.ToLowerInvariant().Contains(FORMAT_FILE_EXTENSION.ToLowerInvariant()))
          {
            _settings.ImageCodecInfo = info;
            break;
          }
        }

        this.LogDebug("  enabled?           = {0}", _isEnabled);
        this.LogDebug("  quality            = {0}", thumbnailQuality);
        this.LogDebug("  column count       = {0}", _settings.ColumnCount);
        this.LogDebug("  row count          = {0}", _settings.RowCount);
        this.LogDebug("  time offset        = {0} minutes", _settings.TimeOffset.TotalMinutes);
        this.LogDebug("  copy to rec. dir.? = {0}", _copyToRecordingFolder);
      }
    }

    /// <summary>
    /// Start the thumbnailer.
    /// </summary>
    /// <returns><c>true</c> if the thumbnailer is started successfully, otherwise <c>false</c></returns>
    public bool Start()
    {
      this.LogInfo("thumbnailer: start");

      ReloadConfiguration();

      if (!_isRegisteredForTveEvents)
      {
        if (!GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
        {
          this.LogError("thumbnailer: failed to register for events, notifier not registered");
          return false;
        }
        ITvServerEvent notifier = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
        if (notifier == null)
        {
          this.LogError("thumbnailer: failed to register for events, notifier not registered");
          return false;
        }
        notifier.OnTvServerEvent += OnTvServerEvent;
        _isRegisteredForTveEvents = true;
      }

      _threadQueueEvent = new AutoResetEvent(false);
      _thread = new Thread(ThumbnailProcessor);
      _thread.IsBackground = true;
      _thread.Priority = ThreadPriority.Lowest;
      _thread.Name = "thumbnail processor";
      _thread.Start();
      return true;
    }

    /// <summary>
    /// Stop the thumbnailer.
    /// </summary>
    public void Stop()
    {
      this.LogInfo("thumbnailer: stop");

      if (_isRegisteredForTveEvents && GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        ITvServerEvent notifier = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
        if (notifier != null)
        {
          notifier.OnTvServerEvent -= OnTvServerEvent;
          _isRegisteredForTveEvents = false;
        }
      }

      if (_thread == null)
      {
        return;
      }

      if (!_thread.IsAlive)
      {
        this.LogWarn("thumbnail processor: aborting thread");
        _thread.Abort();
      }
      else
      {
        lock (_lockQueue)
        {
          _queue.Enqueue(null);
        }
        _threadQueueEvent.Set();

        if (!_thread.Join(2000))
        {
          this.LogWarn("thumbnail processor: failed to join thread, aborting thread");
          _thread.Abort();
        }
      }
      _thread = null;

      if (_threadQueueEvent != null)
      {
        _threadQueueEvent.Close();
        _threadQueueEvent = null;
      }
    }

    /// <summary>
    /// This delegate is invoked when TV Server fires an event.
    /// </summary>
    /// <remarks>
    /// If the event indicates that a new recording has finished, create a thumbnail for the recording.
    /// </remarks>
    private void OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tveEvent = eventArgs as TvServerEventArgs;
      if (tveEvent != null && tveEvent.EventType == TvServerEventType.RecordingEnded)
      {
        Recording r = RecordingManagement.GetRecording(tveEvent.Recording);
        if (r == null)
        {
          this.LogWarn("thumbnailer: failed to get recording detail, ID = {0}", tveEvent.Recording);
        }
        else if (r.MediaType != (int)MediaType.Television)
        {
          this.LogDebug("thumbnailer: non-TV recording, thumbnail not applicable");
        }
        else
        {
          this.LogDebug("thumbnailer: enqueue recording, ID = {0}, file name = {0}", r.IdRecording, r.FileName ?? string.Empty);
          lock (_lockQueue)
          {
            _queue.Enqueue(r);
          }
          _threadQueueEvent.Set();
        }
      }
    }

    /// <summary>
    /// Thread function that processes the recording queue.
    /// </summary>
    private void ThumbnailProcessor()
    {
      this.LogDebug("thumbnail processor: start");

      try
      {
        while (true)
        {
          int queueCount = 0;
          lock (_lockQueue)
          {
            queueCount = _queue.Count;
          }
          if (queueCount == 0)
          {
            _threadQueueEvent.WaitOne();  // Wait indefinitely for a recording to be added to the queue.
          }
          Recording recording;
          lock (_lockQueue)
          {
            recording = _queue.Dequeue();
          }

          // If the recording is null it means stop processing.
          if (recording == null || recording.FileName == null)
          {
            break;
          }

          if (!File.Exists(recording.FileName))
          {
            this.LogError("thumbnail processor: failed to create thumbnail for missing recording, recording = {0}", recording.FileName);
            continue;
          }

          string thumbnailPath = ThumbnailPath;
          if (!Directory.Exists(thumbnailPath))
          {
            try
            {
              Directory.CreateDirectory(thumbnailPath);
            }
            catch (Exception ex)
            {
              this.LogError(ex, "thumbnail processor: failed to create thumbnail path, path = {0}", thumbnailPath);
              continue;
            }
          }

          lock (_lockConfig)
          {
            if (!_isEnabled)
            {
              continue;
            }

            string thumbnailFileName = GetThumbnailFileName(recording.FileName);
            if (!File.Exists(thumbnailFileName))
            {
              try
              {
                if (!CreateThumbnail(recording.FileName, thumbnailFileName, _settings))
                {
                  continue;
                }
                this.LogDebug("thumbnail processor: successfully created thumbnail, recording = {0}, thumbnail = {1}", recording.FileName, thumbnailFileName);
              }
              catch (Exception ex)
              {
                this.LogError(ex, "thumbnail processor: failed to create thumbnail, recording = {0}, thumbnail = {1}", recording.FileName, thumbnailFileName);
                continue;
              }
            }

            if (_copyToRecordingFolder)
            {
              try
              {
                File.Copy(thumbnailFileName, Path.ChangeExtension(recording.FileName, FORMAT_FILE_EXTENSION), true);
              }
              catch (Exception ex)
              {
                this.LogWarn(ex, "thumbnail processor: failed to copy to recording directory, recording = {0}, thumbnail = {1}", recording.FileName, thumbnailFileName);
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "thumbnail processor: unhandled exception");
        return;
      }
      this.LogDebug("thumbnail processor: stop");
    }

    /// <summary>
    /// Create a thumbnail for a video.
    /// </summary>
    /// <param name="videoPath">The full video input path and file name.</param>
    /// <param name="thumbnailPath">The full thumbnail output path and file name.</param>
    /// <param name="settings">The current thumbnail processor settings.</param>
    /// <returns><c>true</c> if the thumbnail is created successfully, otherwise <c>false</c></returns>
    private static bool CreateThumbnail(string videoPath, string thumbnailPath, ThumbnailSettings settings)
    {
      TimeSpan duration = MediaInfo.GetVideoDuration(videoPath);
      if (duration == TimeSpan.Zero)
      {
        Log.Warn("thumbnail processor: input video is corrupt, file name = {0}", videoPath);
        return false;
      }

      TimeSpan timeOffset = settings.TimeOffset;
      if (settings.TimeOffset > duration)
      {
        timeOffset = new TimeSpan(0, 0, (int)duration.TotalSeconds * 20 / 100);   // fall-back = 20%
      }
      int tileCount = settings.ColumnCount * settings.RowCount;
      int thumbnailResolutionVertical = settings.HorizontalResolution * 9 / 16;
      bool success = false;

      // Use the temp folder as a working directory.
      string tempPath = Path.GetTempPath();
      string thumbnailFileNameWithoutExtension = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(videoPath));
      string tempThumbnailPath = string.Format("{0}{1}", thumbnailFileNameWithoutExtension, FORMAT_FILE_EXTENSION);   // This is the full path and file name of the final thumbnail we'll generate in the temp folder.
      if (tileCount == 1)
      {
        success = StartGenerator(videoPath, tempThumbnailPath, tempPath, timeOffset, settings.HorizontalResolution, thumbnailResolutionVertical, 1, 1, TimeSpan.Zero);
        if (!success)
        {
          Log.Warn("thumbnail processor: failed to create single tile thumbnail, file name = {0}", videoPath);
          KillGenerator();
        }
      }
      else
      {
        // Generate the tiles.
        TimeSpan timeBetweenTiles = new TimeSpan(0, 0, (int)((duration - timeOffset).TotalSeconds / (tileCount + 1)));
        int tileResolutionHorizontal = 600;
        int tileResolutionVertical = 337;
        string tiledThumbnailPath = string.Format("{0}_tiled{1}", thumbnailFileNameWithoutExtension, FORMAT_FILE_EXTENSION);  // This is the full path and file name of the unscaled thumbnail we'll generate in the temp folder.
        List<string> tilePaths = new List<string>();
        for (int i = 0; i < tileCount; i++)
        {
          timeOffset += timeBetweenTiles;
          string tilePath = string.Format("{0}_{1}{2}", thumbnailFileNameWithoutExtension, i, FORMAT_FILE_EXTENSION);

          success = StartGenerator(videoPath, tilePath, tempPath, timeOffset, tileResolutionHorizontal, tileResolutionVertical, 1, 1, timeBetweenTiles);
          if (!success)
          {
            Log.Warn("thumbnail processor: failed to create tile {0}, time offset = {1} s, file name = {2}", i, timeOffset, videoPath);
            KillGenerator();
            Thread.Sleep(500);  // Make sure processes have enough time to die before we use the generator again.
            break;
          }

          tilePaths.Add(tilePath);
        }

        if (success)
        {
          // Merge the tiles.
          success = CreateThumbnailFromTiles(tilePaths, tileResolutionHorizontal, tileResolutionVertical, tiledThumbnailPath, settings);
          if (!success)
          {
            Log.Warn("thumbnail processor: failed to create tiled thumbnail, file name = {0}", videoPath);
          }
        }
        if (!success)
        {
          // Try again with the encoder doing the tiling.
          success = StartGenerator(videoPath, tiledThumbnailPath, tempPath, timeOffset, tileResolutionHorizontal, tileResolutionVertical, settings.ColumnCount, settings.RowCount, timeBetweenTiles);
          if (!success)
          {
            Log.Error("thumbnail processor: failed to create tiled thumbnail with fall-back method, file name = {0}", videoPath);
            KillGenerator();
          }
        }

        foreach (string tilePath in tilePaths)
        {
          try
          {
            File.Delete(tilePath);
          }
          catch
          {
            Log.Warn("thumbnail processor: failed to clean up tile, file name = {0}", tilePath);
          }
        }

        if (!success)
        {
          return false;
        }

        // If necessary, down-scale the tiled thumbnail to the required size.
        if (settings.HorizontalResolution == tileResolutionHorizontal)
        {
          tempThumbnailPath = tiledThumbnailPath;
        }
        else
        {
          success = CreateThumbnailFromTiles(new List<string> { tiledThumbnailPath }, settings.HorizontalResolution, thumbnailResolutionVertical, tempThumbnailPath, settings);
          try
          {
            File.Delete(tiledThumbnailPath);
          }
          catch
          {
            Log.Warn("thumbnail processor: failed to clean up tiled thumbnail, file name = {0}", tiledThumbnailPath);
          }
        }
      }

      if (!success)
      {
        return false;
      }

      try
      {
        File.Move(tempThumbnailPath, thumbnailPath);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "thumbnail processor: failed to move thumbnail, file name = {0}", thumbnailPath);
        success = false;
        try
        {
          File.Delete(tempThumbnailPath);
        }
        catch (Exception ex2)
        {
          Log.Warn(ex2, "thumbnail processor: failed to clean up final thumbnail, file name = {0}", tempThumbnailPath);
        }
      }
      return success;
    }

    /// <summary>
    /// Create a thumbnail from a set of tiles.
    /// </summary>
    /// <remarks>
    /// All tiles are expected to be the same size.
    /// The resulting thumbnail will be the same size as one tile.
    /// If only one tile is provided, the resulting thumbnail will be a scaled version of that tile.
    /// </remarks>
    /// <param name="tilePaths">The full path and file name for each tile.</param>
    /// <param name="tileResolutionHorizontal">The horizontal resolution of each tile (and the resulting thumbnail).</param>
    /// <param name="tileResolutionVertical">The vertical resolution of each tile (and the resulting thumbnail).</param>
    /// <param name="thumbnailPath">The full path and file name of the output thumbnail.</param>
    /// <param name="settings">The current thumbnail processor settings.</param>
    /// <returns><c>true</c> if the thumbnail is created successfully, otherwise <c>false</c></returns>
    private static bool CreateThumbnailFromTiles(List<string> tilePaths, int tileResolutionHorizontal, int tileResolutionVertical, string thumbnailPath, ThumbnailSettings settings)
    {
      int columnCount = settings.ColumnCount;
      int rowCount = settings.RowCount;
      if (tilePaths.Count == 1)
      {
        columnCount = 1;
        rowCount = 1;
      }
      int newTileResolutionHorizontal = tileResolutionHorizontal / columnCount;
      int newTileResolutionVertical = tileResolutionVertical / rowCount;

      try
      {
        // Add each tile to a bitmap, then encode the bitmap.
        using (Bitmap bitmap = new Bitmap(tileResolutionHorizontal, tileResolutionVertical))
        {
          using (Graphics g = Graphics.FromImage(bitmap))
          {
            g.CompositingQuality = settings.CompositingQuality;
            g.InterpolationMode = settings.InterpolationMode;
            g.SmoothingMode = settings.SmoothingMode;

            for (int r = 0; r < rowCount; r++)
            {
              for (int c = 0; c < columnCount; c++)
              {
                using (FileStream fs = new FileStream(tilePaths[(r * columnCount) + c], FileMode.Open, FileAccess.Read))
                {
                  using (Image i = Image.FromStream(fs, true, false))
                  {
                    if (i != null)
                    {
                      g.DrawImage(i, c * newTileResolutionHorizontal, r * newTileResolutionVertical, newTileResolutionHorizontal, newTileResolutionVertical);
                      i.Dispose();
                    }
                  }
                }
              }
            }
            bitmap.Save(thumbnailPath, settings.ImageCodecInfo, settings.EncoderParams);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        if (tilePaths.Count == 1)
        {
          Log.Error(ex, "thumbnail processor: failed to scale thumbnail, file name = {0}", tilePaths[0]);
        }
        else
        {
          Log.Error(ex, "thumbnail processor: failed to encode tiled thumbnail");
        }
      }
      return false;
    }

    /// <summary>
    /// Start the thumbnail generator.
    /// </summary>
    /// <param name="pathInput">The full video input path and file name.</param>
    /// <param name="pathOutput">The full thumbnail output path and file name.</param>
    /// <param name="pathWorking">The working director for the generator to use.</param>
    /// <param name="timeOffset">The time offset from the start of the video at which to capture the thumbnail.</param>
    /// <param name="scaleResolutionHorizontal">The target horizontal resolution for the thumbnail.</param>
    /// <param name="scaleResolutionVertical">The target vertical resolution for the thumbnail.</param>
    /// <param name="tileCountHorizontal">The number of tile columns in the thumbnail.</param>
    /// <param name="tileCountVertical">The number of tile rows in the thumbnail.</param>
    /// <param name="timeBetweenTiles">The time between each tile capture.</param>
    /// <returns><c>true</c> if the thumbnail is generated successfully, otherwise <c>false</c></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private static bool StartGenerator(string pathInput, string pathOutput, string pathWorking, TimeSpan timeOffset, int scaleResolutionHorizontal, int scaleResolutionVertical, int tileCountHorizontal, int tileCountVertical, TimeSpan timeBetweenTiles)
    {
      // Refer to https://ffmpeg.org/ffmpeg-filters.html for argument syntax details.
      string tileConfig = string.Empty;
      if (tileCountHorizontal > 1 || tileCountVertical > 1)
      {
        // We want to minimise thumbnail generation time while maximising time between tile
        // captures. Unfortunately this tiling method involves decoding all frames from the initial
        // offset ("ss" switch). Decoding runs in approximately real time. To stay within our 2
        // minute generation time limit, we must limit the tile gap.
        if (timeBetweenTiles.TotalSeconds > 10)
        {
          timeBetweenTiles = new TimeSpan(0, 0, 10);
        }
        tileConfig = string.Format(@",select='isnan(prev_selected_t)+gte(t-prev_selected_t\,{0}'),tile={1}x{2}", (int)timeBetweenTiles.TotalSeconds, tileCountHorizontal, tileCountVertical);
      }
      ProcessStartInfo startInfo = new ProcessStartInfo(PathManager.BuildAssemblyRelativePath(GENERATOR_EXECUTABLE_NAME));
      startInfo.Arguments = string.Format("-loglevel quiet -ss {0} -i \"{1}\" -y -vf yadif=0:-1:0,scale={2}:{3},setsar=1:1{4} -vframes 1 -vsync 0 -an \"{5}\"", (int)timeOffset.TotalSeconds, pathInput, scaleResolutionHorizontal, scaleResolutionVertical, tileConfig, pathOutput);
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardError = true;
      startInfo.RedirectStandardOutput = true;
      startInfo.WorkingDirectory = pathWorking;
      startInfo.CreateNoWindow = true;
      startInfo.ErrorDialog = false;

      bool success = false;
      Process p = new Process();
      p.OutputDataReceived += new DataReceivedEventHandler(GeneratorOutputDataHandler);
      p.ErrorDataReceived += new DataReceivedEventHandler(GeneratorErrorDataHandler);
      p.EnableRaisingEvents = true;
      p.StartInfo = startInfo;
      try
      {
        p.Start();
        try
        {
          p.PriorityClass = ProcessPriorityClass.BelowNormal;
        }
        catch (Exception ex)
        {
          Log.Warn(ex, "thumbnail processor: failed to set process priority");
        }

        // Read in asynchronous mode to avoid deadlocks (if error stream is full).
        // http://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.redirectstandarderror.aspx
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
        p.WaitForExit(120000);

        success = p.HasExited && p.ExitCode == 0;

        p.OutputDataReceived -= new DataReceivedEventHandler(GeneratorOutputDataHandler);
        p.ErrorDataReceived -= new DataReceivedEventHandler(GeneratorErrorDataHandler);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "thumbnail processor: failed to execute process");
      }
      return success;
    }

    /// <summary>
    /// Kill the thumbnail generator processes.
    /// </summary>
    private static void KillGenerator()
    {
      try
      {
        Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GENERATOR_EXECUTABLE_NAME));
        if (processes == null || processes.Length == 0)
        {
          return;
        }
        Log.Warn("thumbnail processor: killing {0} process(es)", processes.Length);
        foreach (Process p in processes)
        {
          try
          {
            p.Kill();
          }
          catch (Exception ex)
          {
            Log.Error(ex, "thumbnail processor: failed to kill process");
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "thumbnail processor: failed to get kill process list");
      }
    }

    private static void GeneratorOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (!string.IsNullOrEmpty(outLine.Data))
      {
        Log.Debug("thumbnail processor: process standard output - {0}", outLine.Data);
      }
    }

    private static void GeneratorErrorDataHandler(object sendingProcess, DataReceivedEventArgs errorLine)
    {
      if (!string.IsNullOrEmpty(errorLine.Data))
      {
        Log.Error("thumbnail processor: process error output - {0}", errorLine.Data);
      }
    }
  }
}