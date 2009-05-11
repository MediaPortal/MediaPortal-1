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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using Toub.MediaCenter.Dvrms.Metadata;

namespace MediaPortal.TV.DiskSpace
{
  internal class RecordingImporterWorker
  {
    private static bool importing = false;
    private static List<TVChannel> channels = new List<TVChannel>();
    private static List<TVRecorded> recordings = new List<TVRecorded>();

    public static void ImportDvrMsFiles()
    {
      //dont import during recording...
      if (Recorder.IsAnyCardRecording())
      {
        return;
      }
      if (importing)
      {
        return;
      }
      Thread WorkerThread = new Thread(new ThreadStart(ImportWorkerThreadFunction));
      WorkerThread.SetApartmentState(ApartmentState.STA);
      WorkerThread.IsBackground = true;
      WorkerThread.Name = "Recording Importer";
      WorkerThread.Start();
    }

    private static void ImportWorkerThreadFunction()
    {
      Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
      try
      {
        importing = true;
        //dont import during recording...
        if (Recorder.IsAnyCardRecording())
        {
          return;
        }
        TVDatabase.GetRecordedTV(ref recordings);
        TVDatabase.GetChannels(ref channels);
        for (int i = 0; i < Recorder.Count; i++)
        {
          TVCaptureDevice dev = Recorder.Get(i);
          if (dev == null)
          {
            continue;
          }
          ProcessDirectory(dev.RecordingPath);
          string[] directories = Directory.GetDirectories(dev.RecordingPath, "*", SearchOption.AllDirectories);
          foreach (string directory in directories)
          {
            ProcessDirectory(directory);
          } //foreach (string directory in directories)
        } //for (int i=0; i < Recorder.Count;++i)
      }
      catch (Exception)
      {
      }
      finally
      {
        importing = false;
      }
    } //static void ImportDvrMsFiles()


    private static void ProcessDirectory(string directory)
    {
      try
      {
        int index = directory.IndexOf("card");
        if ((index == -1) || ((index != -1) && ((directory.Substring(index - 1, 1) != "\\"))))
        {
          string[] files = Directory.GetFiles(directory, "*.dvr-ms");
          foreach (string file in files)
          {
            Thread.Sleep(100);
            bool add = true;
            foreach (TVRecorded rec in recordings)
            {
              if (Recorder.IsAnyCardRecording())
              {
                return;
              }
              if (rec.FileName != null)
              {
                if (String.Compare(rec.FileName, file, true) == 0)
                {
                  add = false;
                  break;
                }
              }
            }
            if (add)
            {
              TVRecorded rec = AddFileToTvDatabase(file);
              if (rec != null)
              {
                recordings.Add(rec);
              }
              Thread.Sleep(100);
            } //if (add)
          } //foreach (string file in files)
        } //if ((index == -1)
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }


    private static TVRecorded AddFileToTvDatabase(string fileName)
    {
      //Log.Info("Recorder: import recording {0}", file);
      try
      {
        using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(fileName))
        {
          TVRecorded newRec = new TVRecorded();
          newRec.FileName = fileName;

          IDictionary dict = editor.GetAttributes();
          if (dict == null)
          {
            return null;
          }
          foreach (MetadataItem item in dict.Values)
          {
            if (item == null)
            {
              continue;
            }
            if (item.Name == null)
            {
              continue;
            }
            //Log.WriteFile(LogType.Recorder,"attribute:{0} value:{1}", item.Name,item.Value.ToString());
            try
            {
              if (item.Name.ToLower() == "channel")
              {
                newRec.Channel = (string) item.Value.ToString();
              }
            }
            catch (Exception)
            {
            }
            try
            {
              if (item.Name.ToLower() == "title")
              {
                newRec.Title = (string) item.Value.ToString();
              }
            }
            catch (Exception)
            {
            }
            try
            {
              if (item.Name.ToLower() == "programtitle")
              {
                newRec.Title = (string) item.Value.ToString();
              }
            }
            catch (Exception)
            {
            }
            try
            {
              if (item.Name.ToLower() == "genre")
              {
                newRec.Genre = (string) item.Value.ToString();
              }
            }
            catch (Exception)
            {
            }
            try
            {
              if (item.Name.ToLower() == "details")
              {
                newRec.Description = (string) item.Value.ToString();
              }
            }
            catch (Exception)
            {
            }
            try
            {
              if (item.Name.ToLower() == "start")
              {
                newRec.Start = (long) UInt64.Parse(item.Value.ToString());
              }
            }
            catch (Exception)
            {
            }
            try
            {
              if (item.Name.ToLower() == "end")
              {
                newRec.End = (long) UInt64.Parse(item.Value.ToString());
              }
            }
            catch (Exception)
            {
            }
          }

          if (newRec.Channel == null)
          {
            string name = Util.Utils.GetFilename(fileName);
            foreach (TVChannel channel in channels)
            {
              if (name.Contains(channel.Name))
              {
                if ((newRec.Channel != null) && (newRec.Channel.Length > 0))
                {
                  if (newRec.Channel.Length > channel.Name.Length)
                  {
                    continue;
                  }
                }
                newRec.Channel = channel.Name;
              }
            }
            if ((newRec.Channel == null) && (channels.Count > 0)) // still no channel found
            {
              newRec.Channel = channels[0].Name; // assign the first one to have it in the DB
            }
          }

          if (newRec.Title == null || newRec.Title.Length == 0)
          {
            newRec.Title = Util.Utils.GetFilename(fileName);
              // to have at least one info in the data base about what it is
          }

          if (newRec.Channel != null && newRec.Channel.Length > 0)
          {
            int id = TVDatabase.AddRecordedTV(newRec);
            if (id >= 0)
            {
              return newRec;
            }
            //Log.Info("Recorder: import recording {0} failed");
          }
          else
          {
            //Log.Info("Recorder: import recording {0} failed, unknown tv channel", file);
          }
        } //using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(file))
      }
      catch (Exception)
      {
        //Log.Error("Recorder:Unable to import {0} reason:{1} {2} {3}", file, ex.Message, ex.Source, ex.StackTrace);
      }
      return null;
    }
  }
}