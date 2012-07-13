using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MediaPortal.Common.Utils
{
  public class Singleton<T> where T : class
  {
    private static class SingletonCreator
    {
      static SingletonCreator() { }

      internal static readonly T Instance =
          typeof(T).InvokeMember(typeof(T).Name,
                                  BindingFlags.CreateInstance |
                                  BindingFlags.Instance |
                                  BindingFlags.Public |
                                  BindingFlags.NonPublic,
                                  null, null, null) as T;
    }

    public static T Instance
    {
      get
      {
        try
        {
          return SingletonCreator.Instance;
        }
        catch (Exception ex)
        {
#if DEBUG
          System.Diagnostics.Debugger.Launch();
#endif
          //TODO gibman: log here once log4net is introduced                    
        }
        return null;
      }
    }
  }

}
