using System;
using System.Collections;
using System.Reflection;
using MediaPortal.GUI.Library;

namespace MediaPortal.Subtitle
{
    /// <summary>
    /// 
    /// </summary>
    public class SubReader
    {
      static ArrayList m_readers=new ArrayList();
		
      static SubReader()
      {	
        Log.Write("loading subtitle plugins");
        string[] strFiles=System.IO.Directory.GetFiles(@"plugins\subtitle", "*.dll");
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
                    if (t.IsSubclassOf (typeof(ISubtitleReader)))
                    {
                      
                      Log.Write("  found plugin:{0} in {1}",t.ToString(), strFile);
                      object newObj=(object)Activator.CreateInstance(t);
                      ISubtitleReader reader=(ISubtitleReader)newObj;
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

      static public SubTitles ReadTag(string strFile)
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
