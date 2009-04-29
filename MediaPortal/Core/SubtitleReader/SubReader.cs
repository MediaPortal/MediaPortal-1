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
using System.IO;
using System.Reflection;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.Subtitle
{
  /// <summary>
  /// 
  /// </summary>
  public class SubReader
  {
    private static ArrayList m_readers = new ArrayList();

    static SubReader()
    {
      Log.Info("loading subtitle plugins");
      string[] strFiles = Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "subtitle"), "*.dll");
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
                  if (t.IsSubclassOf(typeof (ISubtitleReader)))
                  {
                    Log.Info("  found plugin:{0} in {1}", t.ToString(), strFile);
                    object newObj = (object) Activator.CreateInstance(t);
                    ISubtitleReader reader = (ISubtitleReader) newObj;
                    m_readers.Add(reader);
                  }
                }
              }
              catch (NullReferenceException)
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

    public static SubTitles ReadTag(string strFile)
    {
      foreach (ISubtitleReader reader in m_readers)
      {
        if (reader.SupportsFile(strFile))
        {
          if (reader.ReadSubtitles(strFile))
          {
            SubTitles newTag = new SubTitles(reader.Subs);
            return newTag;
          }
        }
      }
      return null;
    }
  }
}