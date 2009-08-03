/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System;
using System.Collections.Generic;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// The service provider
  /// </summary>
  public class ServiceProvider
  {
    private readonly Dictionary<Type, object> services;

    /// <summary>
    /// Constructor
    /// </summary>
    public ServiceProvider()
    {
      services = new Dictionary<Type, object>();
    }

    /// <summary>
    /// This method returns true if the service is registered
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>true if service is registered</returns>
    public bool IsRegistered<T>()
    {
      return services.ContainsKey(typeof (T));
    }

    /// <summary>
    /// Register a new service to the service provider
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="service">service</param>
    public void Add<T>(object service)
    {
      // Make sure service implements type
      Type t = typeof (T);
      services.Add(t, service);
    }

    /// <summary>
    /// returns the service
    /// </summary>
    /// <typeparam name="T">service type</typeparam>
    /// <returns>service itself</returns>
    /// <remarks>if service is not registered an exception is thrown</remarks>
    public T Get<T>()
    {
      Type t = typeof (T);
      if (services.ContainsKey(t))
      {
        return (T) services[t];
      }
      throw new ArgumentException(String.Format("Service {0} is not registered", t));
    }

    /// <summary>
    /// returns the service
    /// </summary>
    /// <typeparam name="T">service type</typeparam>
    /// <returns>service itself</returns>
    /// <remarks>if service is not registered a suitable default is returned</remarks>
    public T TryGet<T>()
    {
      Type t = typeof(T);
      if (services.ContainsKey(t))
      {
        return (T)services[t];
      }
      return default(T);
    }


    /// <summary>
    /// removes a service from the service provider
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    public void Remove<T>()
    {
      Type t = typeof (T);
      if (services.ContainsKey(t))
      {
        services.Remove(t);
      }
    }

    /// <summary>
    /// Replaces an instance of the service 
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="service">new instance of the service</param>
    public void Replace<T>(object service)
    {
      services.Remove(typeof (T));
      services.Add(typeof (T), service);
    }
  }
}