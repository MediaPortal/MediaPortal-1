/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections;
using System.Reflection;
using System.Drawing;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// This class will manage all tagreader plugins
  /// See the ITag.cs for more information about tagreader plugins
  /// It will load all tagreader plugins and when Mediaportal wants information for a given music file
  /// it will check which tagreader plugin supports it and ask it to read the information
  /// which is then returned to mediaportal
  /// </summary>
  public class TagReader
  {
    static ArrayList m_readers = new ArrayList();

    /// <summary>
    /// Constructor
    /// This will load all tagreader plugins from plugins/tagreaders
    /// </summary>
    static TagReader()
    {

      Log.Info("TagReader: Loading tag reader plugins");
      string[] strFiles = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "tagreaders"), "*.dll");
      foreach (string strFile in strFiles)
      {
        try
        {
          Assembly assem = Assembly.LoadFrom(strFile);
          if (assem != null)
          {
            Type[] types = assem.GetExportedTypes();

            foreach (Type t in types)
            {
              try
              {
                if (t.IsClass)
                {
                  if (t.GetInterface("ITag") != null)
                  {
                    object newObj = (object)Activator.CreateInstance(t);
                    Log.Debug("  found plugin: {0} in {1}", t.ToString(), strFile);
                    ITag reader = (ITag)newObj;
                    m_readers.Add(reader);
                  }
                }
              }
              catch (System.NullReferenceException)
              {
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    /// <summary>
    /// This method is called by mediaportal when it wants information for a music file
    /// The method will check which tagreader supports the file and ask it to extract the information from it
    /// </summary>
    /// <param name="strFile">filename of the music file</param>
    /// <returns>
    /// MusicTag instance when file has been read
    /// null when file type is not supported or if the file does not contain any information
    /// </returns>
    static public MusicTag ReadTag(string strFile)
    {
      ITag reader = null;
      int prio = -1;
      if (strFile == null) return null;
      foreach (ITag tmpReader in m_readers)
      {
        if (tmpReader.SupportsFile(strFile) && tmpReader.Priority > prio)
        {
          prio = tmpReader.Priority;
          Type t = tmpReader.GetType();
          object newObj = (object)Activator.CreateInstance(t);
          reader = (ITag)newObj;
        }
      }
      if (reader != null)
      {
        try
        {
          if (reader.SupportsFile(strFile))
          {
            MusicTag musicTag = new MusicTag();
            if (reader.Read(strFile))
            {             
              musicTag.Album = reader.Album;
              musicTag.Artist = reader.Artist;
              musicTag.AlbumArtist = reader.AlbumArtist;
              musicTag.Comment = reader.Comment;
              musicTag.Duration = (int)Math.Round(reader.LengthMS / 1000.00);
              musicTag.Genre = reader.Genre;
              musicTag.Title = reader.Title;
              musicTag.Track = reader.Track;
              musicTag.Year = reader.Year;
              musicTag.CoverArtImageBytes = reader.CoverArtImageBytes;
              musicTag.FileName = strFile;
              musicTag.BitRate = reader.AverageBitrate;
              musicTag.Lyrics = Utils.CleanLyrics(reader.Lyrics);
              if (musicTag.CoverArtImageBytes != null)
              {
                System.Drawing.Image img = musicTag.CoverArtImage;

                if (img == null)
                {
                  musicTag.CoverArtImageBytes = null;
                  Log.Warn("TagReader: Image probably corrupted: {0}", strFile);
                }

                else
                {
                  img.Dispose();
                  img = null;
                }
              }

              // if we didn't get a title, use the Filename without extension to prevent the file to appear as "unknown"
              if (musicTag.Title == "")
                musicTag.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);

              reader.Dispose();

              return musicTag;
            }
            else
            {
              Log.Warn("TagReader: {0} does not contain valid tags. Using Filename as Title", strFile);
              musicTag.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);
              // Set non Tag related values
              musicTag.Duration = (int)Math.Round(reader.LengthMS / 1000.00);
              musicTag.FileName = strFile;
              musicTag.BitRate = reader.AverageBitrate;
              reader.Dispose();
              return musicTag;
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("TagReader: exception on file {0}: {1}", strFile, ex.ToString());
        }
        reader.Dispose();
      }
      return null;
    }
  }
}
