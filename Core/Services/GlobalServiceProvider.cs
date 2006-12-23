#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
    private static readonly ServiceProvider _instance;

    static GlobalServiceProvider()
    {
      _instance = new ServiceProvider();
      _instance.Add<ILog>(new ServiceCreatorCallback<ILog>(LogServiceRequested));
      _instance.Add<MediaPortal.Threading.IThreadPool>(
        new ServiceCreatorCallback<MediaPortal.Threading.IThreadPool>(ThreadPoolServiceRequested));
    }

    /// <summary>
    /// Gets the implementation of the requested service type.
    /// </summary>
    /// <typeparam name="T">The type of service to request.</typeparam>
    /// <returns>The service implementation</returns>
    public static T Get<T>()
    {
      return _instance.Get<T>();
    }

    public static ServiceProvider Instance
    {
      get { return _instance; }
    }

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

    public static void Replace<T>(T service)
    {
      _instance.Replace<T>(service);
    }

    public static void Add<T>(T service)
    {
      _instance.Add<T>(service);
    }
  }
}