#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using MediaPortal.ServiceImplementations;
using MediaPortal.Threading;

namespace MediaPortal.Services
{
  /// <summary>
  /// Static wrapper class for the single global instance of the ServiceProvider 
  /// </summary>
  /// <remarks>
  /// This global service provider instance is initialized with a number of
  /// default services.
  /// </remarks>
  public static class GlobalServiceProvider
  {
    #region Variables

    private static readonly ServiceProvider _instance;

    #endregion

    #region Constructors/Destructors

    static GlobalServiceProvider()
    {
      _instance = new ServiceProvider();
      _instance.Add<ILog>(new ServiceCreatorCallback<ILog>(LogServiceRequested));
      _instance.Add<IThreadPool>(
        new ServiceCreatorCallback<IThreadPool>(ThreadPoolServiceRequested));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the ServiceProvider instance.
    /// </summary>
    /// <value>The instance.</value>
    public static ServiceProvider Instance
    {
      get { return _instance; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the implementation of the requested service type.
    /// </summary>
    /// <typeparam name="T">The type of service to request.</typeparam>
    /// <returns>The service implementation</returns>
    public static T Get<T>()
    {
      return _instance.Get<T>();
    }

    /// <summary>
    /// Replaces the specified service.
    /// </summary>
    /// <param name="service">The new service.</param>
    public static void Replace<T>(T service)
    {
      _instance.Replace<T>(service);
    }

    /// <summary>
    /// Adds the specified service.
    /// </summary>
    /// <param name="service">The service.</param>
    public static void Add<T>(T service)
    {
      _instance.Add<T>(service);
    }

    /// <summary>
    /// Determines whether this instance is registered.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance is registered; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRegistered<T>()
    {
      return _instance.IsRegistered<T>();
    }

    #endregion

    #region Private Methods

    private static ILog LogServiceRequested(ServiceProvider services)
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

    #endregion
  }
}