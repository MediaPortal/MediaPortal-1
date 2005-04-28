using System;
using System.Collections;
using System.Reflection;
using System.Drawing;
using MediaPortal.GUI.Library;

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
		
		/// <summary>
		/// Constructor
		/// This will load all tagreader plugins from plugins/tagreaders
		/// </summary>
    static TagReader()
		{	
      Log.Write("Loading tag reader plugins");
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
                    Log.Write("  found plugin:{0} in {1}",t.ToString(), strFile);
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
          Log.Write("Tag reader generated exception:{0}",ex.ToString());
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
      foreach (ITagReader reader in m_readers)
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
          Log.Write("Tag reader generated exception:{0}",ex.ToString());
        }
      }
      return null;
    }
  }
}
