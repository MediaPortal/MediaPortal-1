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
