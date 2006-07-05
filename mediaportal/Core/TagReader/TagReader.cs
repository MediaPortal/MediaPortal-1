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
using MediaPortal.Utils.Services;

namespace MediaPortal.TagReader
{
	/// <summary>
	/// This class will manage all tagreader plugins
	/// See the ITagReader.cs for more information about tagreader plugins
	/// It will load all tagreader plugins and when Mediaportal wants information for a given music file
	/// it will check which tagreader plugin supports it and ask it to read the information
	/// which is then returned to mediaportal
	/// </summary>
	public class TagReader
	{
    static ArrayList m_readers=new ArrayList();
    static ILog _log;
		
		/// <summary>
		/// Constructor
		/// This will load all tagreader plugins from plugins/tagreaders
		/// </summary>
    static TagReader()
		{
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();

      _log.Info("Loading tag reader plugins");
      string[] strFiles=System.IO.Directory.GetFiles(@"plugins\tagreaders", "*.dll");
      foreach (string strFile in strFiles)
      {
        try
        {
          Assembly assem = Assembly.LoadFrom(strFile);
          if (assem!=null)
          {
            Type[] types = assem.GetExportedTypes();

            foreach (Type t in types)
            {
              try
              {
                if (t.IsClass)
                {
                  if (t.IsSubclassOf (typeof(ITagReader)))
                  {
                    object newObj=(object)Activator.CreateInstance(t);
                    _log.Info("  found plugin:{0} in {1}",t.ToString(), strFile);
                    ITagReader reader=(ITagReader)newObj;
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
        catch (Exception )
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
      ITagReader reader = null;
      int prio = -1;
      if (strFile == null) return null;
      foreach (ITagReader tmpReader in m_readers)
      {
        if (tmpReader.SupportsFile(strFile) && tmpReader.Priority > prio)
        {
          prio = tmpReader.Priority;
          reader = tmpReader;
        }
      }
      if (reader!=null)
      {
        try
        {
          if (reader.SupportsFile(strFile))
          {
            if (reader.ReadTag(strFile))
            {
              MusicTag newTag = new MusicTag(reader.Tag);
              return newTag;
            }
          }
        }
        catch(Exception ex)
        { 
          _log.Info("Tag reader generated exception:{0}",ex.ToString());
        }
      }
      return null;
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
    static public MusicTag ReadTag(string strFile, ref byte[] imageBytes )
    {
      ITagReader reader = null;
      int prio = -1;
      if (strFile == null) return null;
      foreach (ITagReader tmpReader in m_readers)
      {
        if (tmpReader.SupportsFile(strFile) && tmpReader.Priority > prio)
        {
          prio = tmpReader.Priority;
          reader = tmpReader;
        }
      }
      if (reader != null)
      {
        try
        {
          if (reader.SupportsFile(strFile))
          {
            if (reader.ReadTag(strFile))
            {
              MusicTag newTag = new MusicTag(reader.Tag);
              imageBytes = reader.Image;
              return newTag;
            }
          }
        }
        catch(Exception ex)
        { 
          _log.Info("Tag reader generated exception:{0}",ex.ToString());
        }
      }
      return null;
    }
  }
}
