using System;

namespace MediaPortal.Common.Utils
{
  /// <summary>
  /// Static wrapper class for the single global instance of the ServiceProvider 
  /// </summary>
  /// <remarks>
  /// This global service provider instance is initialized with a number of
  /// default services.
  /// </remarks>
  public class GlobalServiceProvider : Singleton<ServiceProvider>
  {        
    #region Public Methods

    /// <summary>
    /// Gets the implementation of the requested service type.
    /// </summary>
    /// <typeparam name="T">The type of service to request.</typeparam>
    /// <returns>The service implementation</returns>
    public static T Get<T>()
    {
      return Instance.Get<T>();
    }

    /// <summary>
    /// Replaces the specified service.
    /// </summary>
    /// <param name="service">The new service.</param>
    public static void Replace<T>(T service)
    {
      Instance.Replace<T>(service);
    }

    /// <summary>
    /// Adds the specified service.
    /// </summary>
    /// <param name="service">The service.</param>
    public static void Add<T>(T service)
    {
      Instance.Add<T>(service);
    }

    /// <summary>
    /// Adds the specified service.
    /// </summary>
    /// <param name="serviceInterface"></param>
    /// <param name="service">The service.</param>
    public static void Add<T>(Type serviceInterface, T service)
    {
      Instance.Add(serviceInterface, service);
    }

    /// <summary>
    /// Determines whether this instance is registered.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance is registered; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRegistered<T>()
    {
      return Instance.IsRegistered<T>();
    }

    public static bool IsRegistered(Type serviceInterface)
    {
      return Instance.IsRegistered(serviceInterface);
    }

    #endregion

    #region Private Methods

    /*private static ILog LogServiceRequested(ServiceProvider services)
    {
      ILog log = new LogImpl();
      services.Add<ILog>(log);
      return log;

    }

    private static IThreadPool ThreadPoolServiceRequested(ServiceProvider services)
    {
      ThreadPool pool = new ThreadPool();
      pool.ErrorLog += new LoggerDelegate(_instance.Get<ILog>().Error);
      pool.WarnLog += new LoggerDelegate(_instance.Get<ILog>().Warn);
      pool.InfoLog += new LoggerDelegate(_instance.Get<ILog>().Info);
      pool.DebugLog += new LoggerDelegate(_instance.Get<ILog>().Debug);
      services.Add<IThreadPool>(pool);
      return pool;
    }
    */
    #endregion

    public static void Remove(Type serviceInterface)
    {
      Instance.Remove(serviceInterface);
    }

    public static void Remove<T>()
    {
      Instance.Remove<T>();
    }
  }
}
