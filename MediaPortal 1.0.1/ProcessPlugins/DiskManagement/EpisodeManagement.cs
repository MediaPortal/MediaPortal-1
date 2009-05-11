#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace ProcessPlugins.DiskSpace
{
  public class EpisodeManagement : IPlugin, ISetupForm
  {
    public EpisodeManagement()
    {
    }

    public List<TVRecorded> GetEpisodes(string title, List<TVRecorded> recordings)
    {
      List<TVRecorded> episodes = new List<TVRecorded>();
      foreach (TVRecorded recording in recordings)
      {
        if (String.Compare(title, recording.Title, true) == 0)
        {
          episodes.Add(recording);
        }
      }
      return episodes;
    }

    public TVRecorded GetOldestEpisode(List<TVRecorded> episodes)
    {
      TVRecorded oldestEpisode = null;
      DateTime oldestDateTime = DateTime.MaxValue;
      foreach (TVRecorded rec in episodes)
      {
        if (rec.StartTime < oldestDateTime)
        {
          oldestDateTime = rec.StartTime;
          oldestEpisode = rec;
        }
      }
      return oldestEpisode;
    }

    #region Episode disk management

    private void OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      Log.Info("EpisodeManagement: recording {0} ended. type: {1} max episodes: {2}",
               recording.Title, recording.RecType.ToString(), recording.EpisodesToKeep);

      CheckEpsiodesForRecording(recording);
    }

    private void CheckEpsiodesForRecording(TVRecording recording)
    {
      if (!recording.DoesUseEpisodeManagement)
      {
        return;
      }

      //check how many episodes we got
      while (true)
      {
        List<TVRecorded> recordings = new List<TVRecorded>();
        TVDatabase.GetRecordedTV(ref recordings);

        List<TVRecorded> episodes = GetEpisodes(recording.Title, recordings);
        if (episodes.Count <= recording.EpisodesToKeep)
        {
          return;
        }

        TVRecorded oldestEpisode = GetOldestEpisode(episodes);
        if (oldestEpisode == null)
        {
          return;
        }
        Log.Info("EpisodeManagement: Delete episode {0} {1} {2} {3}",
                 oldestEpisode.Channel,
                 oldestEpisode.Title,
                 oldestEpisode.StartTime.ToLongDateString(),
                 oldestEpisode.StartTime.ToLongTimeString());

        Recorder.DeleteRecording(oldestEpisode);
      }
    }

    #endregion

    #region IPlugin Members

    public void Start()
    {
      Recorder.OnTvRecordingEnded += new Recorder.OnTvRecordingHandler(OnTvRecordingEnded);
    }

    public void Stop()
    {
      Recorder.OnTvRecordingEnded -= new Recorder.OnTvRecordingHandler(OnTvRecordingEnded);
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Handles TV episodes management";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      // TODO:  Add CallerIdPlugin.GetWindowId implementation
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      // TODO:  Add CallerIdPlugin.GetHome implementation
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "TV Episodes Cleanup";
    }

    public bool HasSetup()
    {
      // TODO:  Add CallerIdPlugin.HasSetup implementation
      return false;
    }

    public void ShowPlugin()
    {
      // TODO:  Add CallerIdPlugin.ShowPlugin implementation
    }

    #endregion
  }
}