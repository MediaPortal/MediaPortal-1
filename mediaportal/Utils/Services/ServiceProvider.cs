#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;

namespace MediaPortal.Services
{
  public class ServiceProvider
  {
    #region Variables

    private Dictionary<Type, object> services;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
    /// </summary>
    public ServiceProvider()
    {
      services = new Dictionary<Type, object>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Determines whether this instance is registered.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance is registered; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRegistered<T>()
    {
      return services.ContainsKey(typeof (T));
    }

    /// <summary>
    /// Adds a new service to the ServiceProvider.
    /// </summary>
    /// <typeparam name="T">The type of the service to add</typeparam>
    /// <param name="service">The service implementation to add</param>
    /// <exception cref="ArgumentException">the added service is already present</exception>
    public void Add<T>(T service)
    {
      // Make sure service implements type
      Type t = typeof (T);
      if (services.ContainsKey(t))
      {
        object o = services[t];
        if (!(o is ServiceCreatorCallback<T>))
        {
          throw new ArgumentException(string.Format("A service of type {0} is already present", t.ToString()));
        }
        services[t] = service;
        return;
      }
      services.Add(t, service);
    }

    /// <summary>
    /// Registes a new service to the ServiceProvider. 
    /// </summary>
    /// <typeparam name="T">The type of the service to register.</typeparam>
    /// <param name="callback">The <see cref="ServiceCreatorCallback<T>"/> to call to get to the service instance.</param>
    public void Add<T>(ServiceCreatorCallback<T> callback)
    {
      Type t = typeof (T);
      if (services.ContainsKey(t))
      {
        throw new ArgumentException(string.Format("A service of type {0} is already present", t.ToString()));
      }
      services.Add(t, callback);
    }

    /// <summary>
    /// Gets the requested service instance.
    /// </summary>
    /// <typeparam name="T">The type of the service to return</typeparam>
    /// <returns>The service implementation.</returns>
    public T Get<T>()
    {
      Type t = typeof (T);
      if (services.ContainsKey(t))
      {
        object o = services[t];
        ServiceCreatorCallback<T> s = o as ServiceCreatorCallback<T>;
        if (s != null)
        {
          return s(this);
        }
        return (T)services[t];
      }
      return default(T);
    }

    //public Y Get<T,Y>(string instanceName) where T: INamedInstanceService
    //{
    //    return Get<T>().GetInstance<Y>(instanceName);
    //}

    /// <summary>
    /// Removes the specified service from the ServiceProvider.
    /// </summary>
    /// <typeparam name="T">The type of the service to remove.</typeparam>
    public void Remove<T>()
    {
      Type t = typeof (T);
      if (services.ContainsKey(t))
      {
        services.Remove(t);
      }
    }

    /// <summary>
    /// Replaces the specified service with the new implementation
    /// </summary>
    /// <typeparam name="T">The type of the service to replace.</typeparam>
    /// <param name="service">The new implementation.</param>
    public void Replace<T>(T service)
    {
      Remove<T>();
      Add<T>(service);
    }

    /// <summary>
    /// Replaces the specified service.
    /// </summary>
    /// <typeparam name="T">The type of the service to replace.</typeparam>
    /// <param name="callback">The <see cref="ServiceCreatorCallback<T>"/> to call to get to the service instance.</param>
    public void Replace<T>(ServiceCreatorCallback<T> callback)
    {
      Remove<T>();
      Add<T>(callback);
    }

    #endregion
  }
}