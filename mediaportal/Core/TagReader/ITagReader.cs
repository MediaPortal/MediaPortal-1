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
using System.Drawing;

namespace MediaPortal.TagReader
{
	/// <summary>
	/// Interface for tagreaders
	/// Tag readers are a special kind of plugins
	/// They can read media files (like .mp3) and extract any information from those files
	/// Currently there are tag readers for mp3, wma and other kinds of media
	/// Tag reader plugins are placed in the folder plugins/tagreaders
	/// and should implement this interface
	/// </summary>
  public class ITagReader
  {
    public ITagReader()
    {
    }

    public virtual int Priority
    {
      get { return 0; }
    }
		/// <summary>
		/// This method is called by mediaportal when it needs information about a media file
		/// The method should look if it can read the media tags for the given file and return
		/// true if it understands how to handle it
		/// </summary>
		/// <param name="strFileName">filename of the media file</param>
		/// <returns>true: this plugin can read tags from this file
		/// false: this plugin does not support the current file</returns>
    public virtual bool SupportsFile(string strFileName)
    {
      return false;
    }

		/// <summary>
		/// This method is called by mediaportal when it needs the tag information about the given file
		/// Before this function is called, Mediaportal will first check if the plugin supports the media file
		/// by calling the SupportsFile() function
		/// </summary>
		/// <param name="strFileName">filename of media file</param>
		/// <returns>
		/// true: plugin has read all information of the file
		/// false: plugin was unable to read information from the file
		/// </returns>
    public virtual bool ReadTag(string strFileName)
    {
      return false;
    }

		/// <summary>
		/// This method is called by mediaportal after ReadTag() is called to retrieve the
		/// information read. The plugin should return a valid MusicTag instance with
		/// all information available about the file read
		/// </summary>
    public virtual MusicTag Tag
    {
      get { return null;}
    }

    /// <summary>
    /// </summary>
    public virtual byte[] Image
    {
      get { return null;}
    }
  }
}
