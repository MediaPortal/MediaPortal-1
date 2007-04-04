using System;
using System.Collections.Generic;

namespace ProjectInfinity
{
  /// <summary>
  /// The Service Scope class.  It is used to keep track of the scope of services.
  /// The moment you create a new ServiceScope instance, any service instances you add
  /// to it will be automtically used by code that is called while the the ServiceScope
  /// instance remains in scope (i.e. is not Disposed)
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>A service scope is only valid in the same thread it was created.</b> 
  /// </para>
  /// <para>A ServiceScope is really some kind of repository that holds a reference
  /// to services that other parts of PI could need.</para><para>Instead of making
  /// this class a static with static properties for all types of services we choose
  /// to create a mechanism that is more flexible.  This implementation can contain
  /// all kinds of services (including the ones we can't imagine right now).</para>
  /// <para>Another advantage of this implemtentation is that you can create different
  /// ServiceScope instances that will be "stacked" upon one another.  This way
  /// you can (temporarily) override a certain service by another implementation that
  /// fits you better.  All code that you call from that moment on will automatically
  /// (if it is written correctly of course) use this new service implementation</para>
  /// </remarks>
  /// <example> This example creates a new ServiceScope and adds its own implementation
  /// of a certain service to it.
  /// <code>
  /// //SomeMethod will log to the old logger here.
  /// SomeMethod();
  /// using(new ServiceScope())
  /// {
  ///   ServiceScope.Add&lt;ILogger&gt;(new FileLogger("blabla.txt"))
  ///   {
  ///     //SomeMethod will now log to our new logger (which will log to blabla.txt)
  ///     SomeMethod();
  ///   }
  /// }
  /// .
  /// .
  /// .
  /// private void SomeMethod()
  /// {
  ///    ILogger logger = ServiceScope.Get&lt;ILogger&gt;();
  ///    logger.Debug("Logging to whatever file our calling method decides");
  /// }
  /// </code></example>
  public class ServiceScope : IDisposable
  {
    /// <summary>
    /// Pointer to the current <see cref="ServiceScope"/>.
    /// </summary>
    /// <remarks>
    /// This pointer is only static for the current thread.  If you want to pass the 
    /// context to another thread you must pass <see cref="ServiceScope.Current"/>
    /// with the delegate used to start the thread and then use
    /// <b>ServiceContect.Current = passedContect</b> to override the current context
    /// with the passed one.
    /// </remarks>
    /*[ThreadStatic]*/ private static ServiceScope current;

    /// <summary>
    /// Pointer to the previous <see cref="ServiceScope"/>.  We need this pointer 
    /// to be able to restore the previous ServiceScope when the <see cref="Dispose()"/>
    /// method is called, and to ask it for services that we do not contain ourselves.
    /// </summary>
    private ServiceScope oldInstance;

    /// <summary>
    /// Holds the list of services.
    /// </summary>
    private Dictionary<Type, object> services;

    /// <summary>
    /// Keeps track whether the instance is already disposed
    /// </summary>
    private bool isDisposed = false;

    /// <summary>
    /// Gets or sets the current <see cref="ServiceScope"/>
    /// </summary>
    public static ServiceScope Current
    {
      get
      {
        if (current == null)
        {
          current = new ServiceScope();
        }
        return current;
      }
      set { current = value; }
    }

    /// <summary>
    /// Creates a new <see cref="ServiceScope"/> instance and initialize it.
    /// </summary>
    public ServiceScope()
    {
      oldInstance = current;
      services = new Dictionary<Type, object>();
      current = this;
    }

    ~ServiceScope()
    {
      Dispose(false);
    }

    /// <summary>
    /// Adds a new Service to the <see cref="ServiceScope"/>
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of service to add.</typeparam>
    /// <param name="service">The service implementation to add.</param>
    public static void Add<T>(T service)
    {
      Current.AddService(service);
    }

    public static void Remove<T>()
    {
      Current.RemoveService<T>();
    }

    /// <summary>
    /// Gets a service from the current <see cref="ServiceScope"/>
    /// </summary>
    /// <typeparam name="T">the type of the service to get.  This is typically
    /// (but not necessarily) an interface</typeparam>
    /// <returns>the service implementation.</returns>
    /// <exception cref="ServiceNotFoundException">when the requested service type is not found.</exception>
    public static T Get<T>()
    {
      return Current.GetService<T>(true);
    }

    /// <summary>
    /// Gets a service from the current <see cref="ServiceScope"/>
    /// </summary>
    /// <typeparam name="T">the type of the service to get.  This is typically
    /// (but not necessarily) an interface</typeparam>
    /// <param name="throwIfNotFound">a <b>bool</b> indicating whether to throw a
    /// <see cref="ServiceNotFoundException"/> when the requested service is not found</param>
    /// <returns>the service implementation or <b>null</b> if the service is not available
    /// and <paramref name="throwIfNotFound"/> is false.</returns>
    /// <exception cref="ServiceNotFoundException">when <paramref="throwIfNotFound"/>
    /// is <b>true</b> andthe requested service type is not found.</exception>
    public static T Get<T>(bool throwIfNotFound)
    {
      return Current.GetService<T>(throwIfNotFound);
    }

    private void AddService<T>(T service)
    {
      services.Add(typeof (T), service);
    }

    private void RemoveService<T>()
    {
      services.Remove(typeof (T));
    }

    private T GetService<T>(bool throwIfNotFound)
    {
      Type type = typeof (T);
      if (services.ContainsKey(type))
      {
        return (T) services[type];
      }
      if (oldInstance == null)
      {
        if (throwIfNotFound)
        {
          throw new ServiceNotFoundException(type);
        }
        object o = null;
        return (T) (o);
      }
      return oldInstance.GetService<T>(throwIfNotFound);
    }

    #region IDisposable implementation

    protected virtual void Dispose(bool alsoManaged)
    {
      if (isDisposed) //already disposed?
      {
        return;
      }
      if (alsoManaged)
      {
        current = oldInstance; //set current scope to previous one
      }
      isDisposed = true;
    }

    /// <summary>
    /// Restores the previous service context.
    /// </summary>
    /// <remarks>
    /// Use the using keyword to automatically call this method when the 
    /// service context goes out of scope.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      //Tell the CLR not to call the finalizer, preventing the Dispose(bool)
      //method to be called a second time when this instance is destroyed.
      GC.SuppressFinalize(this);
    }

    #endregion
  }
}