using System;
using System.Collections;
using System.Reflection;
using MediaPortal.GUI.Library;
namespace MediaPortal.TagReader
{
	/// <summary>
	/// 
	/// </summary>
	public class TagReader
	{
    static ArrayList m_readers=new ArrayList();
		
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

    static public MusicTag ReadTag(string strFile)
    {
      foreach (ITagReader reader in m_readers)
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
      return null;
    }

	}
}
