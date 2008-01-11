#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using MediaPortal.ServiceImplementations;

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
      _instance.Add<MediaPortal.Threading.IThreadPool>(
        new ServiceCreatorCallback<MediaPortal.Threading.IThreadPool>(ThreadPoolServiceRequested));
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

    private static MediaPortal.Threading.IThreadPool ThreadPoolServiceRequested(ServiceProvider services)
    {
      MediaPortal.Threading.ThreadPool pool = new MediaPortal.Threading.ThreadPool();
      pool.ErrorLog += new MediaPortal.Threading.LoggerDelegate(_instance.Get<ILog>().Error);
      pool.WarnLog += new MediaPortal.Threading.LoggerDelegate(_instance.Get<ILog>().Warn);
      pool.InfoLog += new MediaPortal.Threading.LoggerDelegate(_instance.Get<ILog>().Info);
      pool.DebugLog += new MediaPortal.Threading.LoggerDelegate(_instance.Get<ILog>().Debug);
      services.Add<MediaPortal.Threading.IThreadPool>(pool);
      return pool;
    }
    #endregion
  }
}