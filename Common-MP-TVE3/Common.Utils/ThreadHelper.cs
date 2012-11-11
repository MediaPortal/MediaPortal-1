using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mediaportal.Common.Utils
{
  /// <summary>
  /// ThreadHelper functions.
  /// </summary>
  public static class ThreadHelper
  {
    private static ParallelOptions _parallelismOptions;

    /// <summary>
    /// use the SERVER_PARALLELISM_ENBABLED to enable or disable parallelism (threads) on the server to aid in debugging.
    /// default: true
    /// disabling it will only work when running in debug mode.
    /// </summary>
    private const bool PARALLELISM_ENABLED = true;

    static ThreadHelper()
    {
      _parallelismOptions = new ParallelOptions();

#if debug
            if (!PARALLELISM_ENABLED)
            {
                _parallelOptions.MaxDegreeOfParallelism = 1;
            }                               
#endif
    }
   
    ///<summary>
    /// ThreadHelper.ParallelInvoke encapsulated with a try catch, throwing the 1st possible exception.
    ///</summary>
    public static void ParallelInvoke(params Action[] actions)
    {
      try
      {
        Parallel.Invoke(_parallelismOptions, actions);
      }
      catch (AggregateException e)
      {
        if (e.InnerExceptions != null && e.InnerExceptions.Count > 0)
        {
          Exception cause = e.InnerExceptions.FirstOrDefault();
          throw cause;
        }
      }
    }

    public static void WaitAndHandleExceptions(this Task task)
    {
      try
      {
        task.Wait();
      }
      catch (AggregateException e)
      {
        if (e.InnerExceptions != null && e.InnerExceptions.Count > 0)
        {
          Exception cause = e.InnerExceptions.FirstOrDefault();
          throw cause;
        }
      }
    }
  }
}
