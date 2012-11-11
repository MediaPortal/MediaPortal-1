using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace MediaPortal.Common.Utils
{
  public delegate T ServiceCreatorCallback<T>(ServiceProvider provider);

  public class ServiceProvider
  {
    #region Variables

    private readonly IDictionary<Type, object> _services;
    private readonly object _servicesLock = new object();

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
    /// </summary>
    public ServiceProvider()
    {
      _services = new Dictionary<Type, object>();
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
      lock (_servicesLock)
      {
        return _services.ContainsKey(typeof(T));
      }
    }


    /// <summary>
    /// Determines whether this instance is registered.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance is registered; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRegistered(Type t)
    {
      lock (_servicesLock)
      {
        return _services.ContainsKey(t);  
      }      
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
      Type t = typeof(T);
      object o = null;

      lock (_servicesLock)
      {
        if (_services.TryGetValue(t, out o))
        {
          if (!(o is ServiceCreatorCallback<T>))
          {
            throw new ArgumentException(string.Format("A service of type {0} is already present", t.ToString()));
          }
          _services[t] = service;
          return;
        }
        _services.Add(t, service);
      }
    }

    public void Add<T>(Type serviceInterface, T service)
    {      
      object o = null;
      lock (_servicesLock)
      {
        if (_services.TryGetValue(serviceInterface, out o))
        {
          /*if (!(o is ServiceCreatorCallback<T>))
          {
            throw new ArgumentException(string.Format("A service of type {0} is already present", t.ToString()));
          }*/
          _services[serviceInterface] = service;
          return;
        }
        _services.Add(serviceInterface, service);
      }
    }

    /// <summary>
    /// Registes a new service to the ServiceProvider. 
    /// </summary>
    /// <typeparam name="T">The type of the service to register.</typeparam>
    /// <param name="callback">The <see cref="ServiceCreatorCallback<T>"/> to call to get to the service instance.</param>
    public void Add<T>(ServiceCreatorCallback<T> callback)
    {
      Type t = typeof(T);
      lock (_servicesLock)
      {
        if (_services.ContainsKey(t))
        {
          throw new ArgumentException(string.Format("A service of type {0} is already present", t.ToString()));
        }
        _services.Add(t, callback);
      }
    }

    /// <summary>
    /// Gets the requested service instance.
    /// </summary>
    /// <typeparam name="T">The type of the service to return</typeparam>
    /// <returns>The service implementation.</returns>
    public T Get<T>()
    {
      Type t = typeof(T);
      object o = null;
      lock (_servicesLock)
      {
        if (_services.TryGetValue(t, out o))
        {
          ServiceCreatorCallback<T> s = o as ServiceCreatorCallback<T>;
          if (s != null)
          {
            return s(this);
          }
          return (T) _services[t];
        }
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
      Type t = typeof(T);
      Remove(t);
    }

    public void Remove(Type t)
    {
      lock (_servicesLock)
      {
        if (_services.ContainsKey(t))
        {
          _services.Remove(t);
        }
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

    public bool HasServices
    {
      get
      {
        lock (_servicesLock)
        {
          return _services.Count() > 0;
        }
      }
    }

    #endregion

   
  }
}
